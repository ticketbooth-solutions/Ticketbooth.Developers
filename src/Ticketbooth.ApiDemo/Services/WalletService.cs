using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Features.Wallet;
using Stratis.Bitcoin.Features.Wallet.Interfaces;
using Stratis.Bitcoin.Utilities;
using System;
using System.Linq;
using Ticketbooth.ApiDemo.Models;

namespace Ticketbooth.ApiDemo.Services
{
    public class WalletService
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<WalletService> _logger;
        private readonly IWalletManager _walletManager;
        private readonly IWalletSyncManager _walletSyncManager;

        public WalletService(
            IDateTimeProvider dateTimeProvider,
            ILoggerFactory loggerFactory,
            IWalletManager walletManager,
            IWalletSyncManager walletSyncManager)
        {
            _dateTimeProvider = dateTimeProvider;
            _logger = loggerFactory.CreateLogger<WalletService>();
            _walletManager = walletManager;
            _walletSyncManager = walletSyncManager;
        }

        public bool CreateWallet(CreateWalletDetails createWalletDetails)
        {
            try
            {
                _walletManager.CreateWallet(createWalletDetails.Password, createWalletDetails.Name, createWalletDetails.Password);
                _walletSyncManager.SyncFromDate(_dateTimeProvider.GetUtcNow());
                _logger.LogInformation($"Created wallet {createWalletDetails.Name}.");
                return true;
            }
            catch (WalletException)
            {
                _logger.LogDebug($"Wallet {createWalletDetails.Name} already exists.");
                return false;
            }
            catch (NotSupportedException e)
            {
                _logger.LogError(e.Message);
                return false;
            }
        }

        public string[] GenerateAddresses(string walletName, string accountName, int count)
        {
            try
            {
                var results = _walletManager.GetUnusedAddresses(new WalletAccountReference(walletName, accountName), count);
                return results.Select(result => result.Address).ToArray();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }

        public Money GetBalance(string address)
        {
            return _walletManager.GetAddressBalance(address).AmountConfirmed;
        }
    }
}
