using Microsoft.AspNetCore.SignalR;
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
        private readonly IReceiptRepository _receiptRepository;
        private readonly Network _network;
        private readonly WalletService _walletService;

        public DemoHub(IReceiptRepository receiptRepository, Network network, WalletService walletService)
        {
            _receiptRepository = receiptRepository;
            _network = network;
            _walletService = walletService;
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
            catch (FormatException)
            {
            }

            var response = new ReceiptResponse(receipt, _network);
            return Task.FromResult(response);
        }
    }
}
