namespace Ticketbooth.ApiDemo.Models
{
    public class MiniWallet
    {
        public MiniWallet(string name, string password, params string[] addresses)
        {
            Name = name;
            Password = password;
            Addresses = addresses;
        }

        public string Name { get; }

        public string Account => "account 0";

        public string Password { get; }

        public string[] Addresses { get; }
    }
}
