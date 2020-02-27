using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBitcoin;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ticketbooth.ApiDemo.Services.Background
{
    public class FullNodeMonitor : IHostedService, IDisposable
    {
        private readonly ILogger<FullNodeMonitor> _logger;
        private readonly ChainIndexer _chainIndexer;
        private readonly IHubContext<DemoHub> _demoHubAccessor;

        private int _height;
        private Timer _timer;

        public FullNodeMonitor(ChainIndexer chainIndexer, IHubContext<DemoHub> demoHubAccessor, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FullNodeMonitor>();
            _chainIndexer = chainIndexer;
            _demoHubAccessor = demoHubAccessor;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Full node monitor started.");

            _timer = new Timer(PollFullNodeAsync, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));

            return Task.CompletedTask;
        }

        private async void PollFullNodeAsync(object state)
        {
            if (_height < _chainIndexer.Height)
            {
                _logger.LogDebug("Sending consensus update notification");
                _height = _chainIndexer.Height;
                await _demoHubAccessor.Clients.All.SendAsync("consensusUpdated");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Full node monitor stopped.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
