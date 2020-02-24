using Microsoft.AspNetCore.Mvc.RazorPages;
using Ticketbooth.ApiDemo.Models;
using Ticketbooth.ApiDemo.Services;

namespace Ticketbooth.ApiDemo.Pages
{
    public class IndexModel : PageModel
    {
        private readonly WalletService _walletService;

        public IndexModel(WalletService walletService)
        {
            _walletService = walletService;
        }

        public CreateWalletDetails CreateWalletDetails { get; set; }

        public MiniWallet Wallet { get; private set; }

        public string ErrorMessage { get; private set; }

        public void OnGet()
        {
            CreateWalletDetails = new CreateWalletDetails();
        }

        public void OnPost(CreateWalletDetails createWalletDetails)
        {
            CreateWalletDetails = createWalletDetails;
            var walletCreated = _walletService.CreateWallet(createWalletDetails);
            if (walletCreated)
            {
                var addresses = _walletService.GenerateAddresses(createWalletDetails.Name, "account 0", 3);
                Wallet = new MiniWallet(createWalletDetails.Name, createWalletDetails.Password, addresses);
            }
            else
            {
                ErrorMessage = "Wallet already exists";
            }
        }
    }
}
