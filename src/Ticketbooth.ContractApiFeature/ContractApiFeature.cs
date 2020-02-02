using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Stratis.Bitcoin.Builder.Feature;
using System.Threading.Tasks;

namespace Ticketbooth.ContractApiFeature
{
    public class ContractApiFeature : FullNodeFeature
    {
        private readonly ILogger<ContractApiFeature> _logger;
        private readonly IWebHostBuilder _webHostBuilder;

        private IWebHost _webHost;

        public ContractApiFeature(ILogger<ContractApiFeature> logger, IWebHostBuilder webHostBuilder)
        {
            _logger = logger;
            _webHostBuilder = webHostBuilder;
        }

        public override async Task InitializeAsync()
        {
            _logger.LogInformation("API starting...");
            _webHost = _webHostBuilder
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();
            await _webHost.StartAsync();
        }

        public override void Dispose()
        {
            _webHost.StopAsync().Wait();
        }
    }
}
