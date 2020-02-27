using Microsoft.Extensions.Logging;
using Moq;
using NBitcoin;
using NUnit.Framework;
using Stratis.Bitcoin.Features.SmartContracts.Models;
using Stratis.Sidechains.Networks;
using Stratis.SmartContracts.Core.Receipts;
using System;
using System.Linq;
using System.Threading.Tasks;
using Ticketbooth.ApiDemo.Services;
using Ticketbooth.ApiDemo.Services.Background;

namespace Ticketbooth.ApiDemo.Tests.Services.Background
{
    public class DemoHubTests
    {
        private Mock<ILogger<DemoHub>> _logger;
        private Mock<IReceiptRepository> _receiptRepository;
        private Mock<IWalletService> _walletService;

        private DemoHub _demoHub;

        [SetUp]
        public void SetUp()
        {
            var loggerFactory = new Mock<ILoggerFactory>();
            _logger = new Mock<ILogger<DemoHub>>();
            loggerFactory.Setup(callTo => callTo.CreateLogger(It.IsAny<string>())).Returns(_logger.Object);
            _receiptRepository = new Mock<IReceiptRepository>();
            _walletService = new Mock<IWalletService>();
            var network = CirrusNetwork.NetworksSelector.Testnet();

            _demoHub = new DemoHub(loggerFactory.Object, _receiptRepository.Object, _walletService.Object, network);
        }

        [Test]
        public async Task UpdateBalances_RetrievesBalances_ForEachAddress()
        {
            // Arrange
            var addresses = new string[] { "tU9vpG8bfQxCQxGgHSjFYdb936jddYGrhm", "tNuiVJiEhvbQgXu4P32S4TnAVEG3kgLnu8", "tP1ysN5xxUiFkgXp3FyfsRk6KmCvuMk7DQ" };

            _walletService.Setup(callTo => callTo.GetBalance(It.IsAny<string>())).Returns(new Money(200_000_000));

            // Act
            var balances = await _demoHub.UpdateBalances(addresses);

            // Assert
            _walletService.Verify(callTo => callTo.GetBalance(It.Is<string>(address => addresses.Contains(address))), Times.Exactly(addresses.Length));
        }

        [Test]
        public async Task UpdateBalances_Result_MapsCorrectly()
        {
            // Arrange
            var addressOne = "tU9vpG8bfQxCQxGgHSjFYdb936jddYGrhm";
            var addressTwo = "tNuiVJiEhvbQgXu4P32S4TnAVEG3kgLnu8";
            var addressThree = "tP1ysN5xxUiFkgXp3FyfsRk6KmCvuMk7DQ";
            var addresses = new string[] { addressOne, addressTwo, addressThree };

            _walletService.Setup(callTo => callTo.GetBalance(addressOne)).Returns(new Money(0));
            _walletService.Setup(callTo => callTo.GetBalance(addressTwo)).Returns(new Money(100_000_000));
            _walletService.Setup(callTo => callTo.GetBalance(addressThree)).Returns(new Money(200_000_000));

            // Act
            var balances = await _demoHub.UpdateBalances(addresses);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(balances.First(item => item.Address.Equals(addressOne)).Balance, Is.EqualTo(0));
                Assert.That(balances.First(item => item.Address.Equals(addressTwo)).Balance, Is.EqualTo(100_000_000));
                Assert.That(balances.First(item => item.Address.Equals(addressThree)).Balance, Is.EqualTo(200_000_000));
            });
        }

        [Test]
        public async Task PollTransaction_InvalidHash_ReturnsNull()
        {
            // Arrange
            var hash = "not_a_valid_hash";

            // Act
            var result = await _demoHub.PollTransaction(hash);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PollTransaction_ValidHashFound_ReturnsReceiptResponse()
        {
            // Arrange
            var hash = "c80b5075fcd6d0d58e235b86556834d7dc71be48e8ee5940071431963b9f47d6";
            var receipt = new Receipt(new uint256(), 100_000, Array.Empty<Log>(), new uint256(hash), new uint160(), new uint160(), new uint160(), true, "x", string.Empty, 100, 100, "Hello Ticketbooth")
            {
                BlockHash = new uint256()
            };

            _receiptRepository.Setup(callTo => callTo.Retrieve(new uint256(hash))).Returns(receipt);

            // Act
            var result = await _demoHub.PollTransaction(hash);

            // Assert
            Assert.That(result, Is.TypeOf<ReceiptResponse>());
        }
    }
}
