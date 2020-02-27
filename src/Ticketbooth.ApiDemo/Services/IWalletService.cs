using NBitcoin;
using Ticketbooth.ApiDemo.Models;

namespace Ticketbooth.ApiDemo.Services
{
    public interface IWalletService
    {
        bool CreateWallet(CreateWalletDetails createWalletDetails);

        string[] GenerateAddresses(string walletName, string accountName, int count);

        Money GetBalance(string address);
    }
}
