using Microsoft.Extensions.Logging;
using Moq;
using NBitcoin;
using NUnit.Framework;
using Stratis.Bitcoin.Features.Wallet;
using Stratis.Bitcoin.Features.Wallet.Interfaces;
using Stratis.Bitcoin.Utilities;
using System;
using Ticketbooth.ApiDemo.Models;
using Ticketbooth.ApiDemo.Services;

namespace Ticketbooth.ApiDemo.Tests.Services
{
    public class WalletServiceTests
    {
        private Mock<ILogger<WalletService>> _logger;
        private Mock<IDateTimeProvider> _dateTimeProvider;
        private Mock<IWalletManager> _walletManager;
        private Mock<IWalletSyncManager> _walletSyncManager;

        private WalletService _walletService;

        [SetUp]
        public void SetUp()
        {
            var loggerFactory = new Mock<ILoggerFactory>();
            _logger = new Mock<ILogger<WalletService>>();
            loggerFactory.Setup(callTo => callTo.CreateLogger(It.IsAny<string>())).Returns(_logger.Object);
            _dateTimeProvider = new Mock<IDateTimeProvider>();
            _walletManager = new Mock<IWalletManager>();
            _walletSyncManager = new Mock<IWalletSyncManager>();

            _walletService = new WalletService(_dateTimeProvider.Object, loggerFactory.Object, _walletManager.Object, _walletSyncManager.Object);
        }

        [Test]
        public void CreateWallet_ThrowsWalletException_ReturnsFalse()
        {
            // Arrange
            var createWalletDetails = new CreateWalletDetails { Name = "Wallet One", Password = "hunter2" };

            _walletManager
                .Setup(callTo => callTo.CreateWallet(createWalletDetails.Password, createWalletDetails.Name, createWalletDetails.Password, It.IsAny<Mnemonic>()))
                .Throws(new WalletException("Something went wrong"));

            // Act
            var created = _walletService.CreateWallet(createWalletDetails);

            // Assert
            Assert.That(created, Is.False);
        }

        [Test]
        public void CreateWallet_ThrowsNothing_ReturnsTrue()
        {
            // Arrange
            var createWalletDetails = new CreateWalletDetails { Name = "Wallet One", Password = "hunter2" };

            _walletManager
                .Setup(callTo => callTo.CreateWallet(createWalletDetails.Password, createWalletDetails.Name, createWalletDetails.Password, It.IsAny<Mnemonic>()))
                .Returns(new Mnemonic(Wordlist.English, WordCount.Twelve));

            // Act
            var created = _walletService.CreateWallet(createWalletDetails);

            // Assert
            Assert.That(created, Is.True);
        }

        [Test]
        public void CreateWallet_ThrowsNotSupportedException_ReturnsFalse()
        {
            // Arrange
            var createWalletDetails = new CreateWalletDetails { Name = "Wallet One", Password = "hunter2" };

            _walletManager
                .Setup(callTo => callTo.CreateWallet(createWalletDetails.Password, createWalletDetails.Name, createWalletDetails.Password, It.IsAny<Mnemonic>()))
                .Throws<NotSupportedException>();

            // Act
            var created = _walletService.CreateWallet(createWalletDetails);

            // Assert
            Assert.That(created, Is.False);
        }

        [Test]
        public void GenerateAddresses_WalletManagerThrows_ReturnsNull()
        {
            // Arrange
            var walletName = "Wallet One";
            var accountName = "account 0";
            var count = 3;

            _walletManager.Setup(callTo => callTo.GetUnusedAddresses(It.IsAny<WalletAccountReference>(), count, It.IsAny<bool>())).Throws<Exception>();

            // Act
            var addresses = _walletService.GenerateAddresses(walletName, accountName, count);

            // Assert
            Assert.That(addresses, Is.Null);
        }

        [Test]
        public void GenerateAddresses_RetrievesFrom_WalletManager()
        {
            // Arrange
            var walletName = "Wallet One";
            var accountName = "account 0";
            var count = 3;

            var hdAddressesResult = new HdAddress[]
            {
                new HdAddress { Address = "tU9vpG8bfQxCQxGgHSjFYdb936jddYGrhm" },
                new HdAddress { Address = "tNuiVJiEhvbQgXu4P32S4TnAVEG3kgLnu8" },
                new HdAddress { Address = "tP1ysN5xxUiFkgXp3FyfsRk6KmCvuMk7DQ" },
            };

            _walletManager.Setup(callTo => callTo.GetUnusedAddresses(It.IsAny<WalletAccountReference>(), count, It.IsAny<bool>())).Returns(hdAddressesResult);

            // Act
            var addresses = _walletService.GenerateAddresses(walletName, accountName, count);

            // Assert
            Assert.That(addresses, Is.Unique);
        }

        [Test]
        public void GetBalance_Returns_ConfirmedBalance()
        {
            // Arrange
            var address = "tNuiVJiEhvbQgXu4P32S4TnAVEG3kgLnu8";
            var addressBalance = new AddressBalance
            {
                Address = address,
                AmountConfirmed = new Money(500_000_000),
                AmountUnconfirmed = new Money(100_000_000),
                CoinType = CoinType.Testnet
            };

            _walletManager.Setup(callTo => callTo.GetAddressBalance(address)).Returns(addressBalance);

            // Act
            var balance = _walletService.GetBalance(address);

            // Assert
            Assert.That(balance.Satoshi, Is.EqualTo(500_000_000));
        }
    }
}
