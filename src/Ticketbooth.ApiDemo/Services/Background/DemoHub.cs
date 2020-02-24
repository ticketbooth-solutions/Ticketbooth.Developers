using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Threading.Tasks;
using Ticketbooth.ApiDemo.Models;

namespace Ticketbooth.ApiDemo.Services.Background
{
    public class DemoHub : Hub
    {
        private readonly WalletService _walletService;

        public DemoHub(WalletService walletService)
        {
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
    }
}
