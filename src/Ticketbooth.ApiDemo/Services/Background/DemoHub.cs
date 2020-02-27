using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Features.SmartContracts.Models;
using Stratis.SmartContracts.Core.Receipts;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ticketbooth.ApiDemo.Models;

namespace Ticketbooth.ApiDemo.Services.Background
{
    public class DemoHub : Hub
    {
        private readonly ILogger<DemoHub> _logger;
        private readonly IReceiptRepository _receiptRepository;
        private readonly IWalletService _walletService;
        private readonly Network _network;

        public DemoHub(ILoggerFactory loggerFactory, IReceiptRepository receiptRepository, IWalletService walletService, Network network)
        {
            _logger = loggerFactory.CreateLogger<DemoHub>();
            _receiptRepository = receiptRepository;
            _walletService = walletService;
            _network = network;
        }

        public Task<AddressDetails[]> UpdateBalances(string[] addresses)
        {
            var addressDetails = addresses.Select(address => new AddressDetails
            {
                Address = address,
                Balance = (ulong)_walletService.GetBalance(address).Satoshi
            });

            return Task.FromResult(addressDetails.ToArray());
        }

        public Task<ReceiptResponse> PollTransaction(string hash)
        {
            Receipt receipt = null;

            try
            {
                var hashValue = new uint256(hash);

                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                while (receipt == null && !cancellationTokenSource.IsCancellationRequested)
                {
                    receipt = _receiptRepository.Retrieve(hashValue);
                }
            }
            catch (FormatException e)
            {
                _logger.LogWarning(e.Message);
            }

            var response = receipt != null ? new ReceiptResponse(receipt, _network) : null;
            return Task.FromResult(response);
        }
    }
}
