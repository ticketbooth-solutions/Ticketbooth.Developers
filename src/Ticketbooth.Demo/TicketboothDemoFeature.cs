using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stratis.Bitcoin;
using Stratis.Bitcoin.Builder;
using Stratis.Bitcoin.Builder.Feature;
using System;
using System.Net;
using System.Threading.Tasks;
using Ticketbooth.ApiDemo;

namespace Ticketbooth.Demo
{
    public class TicketboothDemoFeature : FullNodeFeature
    {
        private readonly FullNode _fullNode;
        private readonly IFullNodeBuilder _fullNodeBuilder;
        private readonly IOptions<TicketboothDemoOptions> _ticketboothDemoOptions;
        private readonly ILogger<TicketboothDemoFeature> _logger;
        private IWebHost _webHost;

        public TicketboothDemoFeature(
            FullNode fullNode,
            IFullNodeBuilder fullNodeBuilder,
            ILoggerFactory loggerFactory,
            IOptions<TicketboothDemoOptions> ticketboothDemoOptions)
        {
            _fullNode = fullNode;
            _fullNodeBuilder = fullNodeBuilder;
            _ticketboothDemoOptions = ticketboothDemoOptions;
            _logger = loggerFactory.CreateLogger<TicketboothDemoFeature>();
        }

        public override async Task InitializeAsync()
        {
            _logger.LogInformation("Demo starting...");

            _webHost = WebHost.CreateDefaultBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, _ticketboothDemoOptions.Value.HttpsPort, config =>
                    {
                        config.UseHttps();
                    });
                })
                .ConfigureServices(services =>
                {
                    // copies all the services defined for the full node.
                    // also copies over singleton instances already defined
                    foreach (ServiceDescriptor service in _fullNodeBuilder.Services)
                    {
                        // open types can't be singletons
                        if (service.ServiceType.IsGenericType || service.Lifetime == ServiceLifetime.Scoped)
                        {
                            services.Add(service);
                            continue;
                        }

                        var serviceObj = _fullNode.Services.ServiceProvider.GetService(service.ServiceType);
                        if (serviceObj != null && service.Lifetime == ServiceLifetime.Singleton && service.ImplementationInstance == null)
                        {
                            services.AddSingleton(service.ServiceType, serviceObj);
                        }
                        else
                        {
                            services.Add(service);
                        }
                    }
                })
                .UseStartup<Startup>()
                .Build();
            await _webHost.StartAsync();

            _logger.LogInformation($"Demo running on port {_ticketboothDemoOptions.Value.HttpsPort}");
        }

        public override void Dispose()
        {
            _logger.LogInformation($"Demo stopping on port {_ticketboothDemoOptions.Value.HttpsPort}");
            _webHost.StopAsync().Wait();
        }
    }

    /// <summary>
    /// Options for the Ticketbooth demo
    /// </summary>
    public sealed class TicketboothDemoOptions
    {
        /// <summary>
        /// The port to access the API. Set to 40100 by default.
        /// </summary>
        public int HttpsPort { get; set; } = 40100;
    }

    /// <summary>
    /// A class providing extension methods for to configure Ticketbooth demo full node extension.
    /// </summary>
    public static class TicketboothApiDemoSetupExtensions
    {
        /// <summary>
        /// Adds the Ticketbooth demo to the full node.
        /// </summary>
        /// <param name="fullNodeBuilder">Full node builder</param>
        /// <param name="setupAction">Configuration options</param>
        /// <returns>The full node builder</returns>
        public static IFullNodeBuilder RunTicketboothDemo(this IFullNodeBuilder fullNodeBuilder, Action<TicketboothDemoOptions> setupAction = null)
        {
            return fullNodeBuilder.ConfigureFeature(features =>
            {
                features
                    .AddFeature<TicketboothDemoFeature>()
                    .DependOn<Api.TicketboothApiFeature>()
                    .FeatureServices(services =>
                    {
                        services.AddOptions<TicketboothDemoOptions>();

                        if (setupAction != null)
                        {
                            services.ConfigureTicketboothDemo(setupAction);
                        }

                        services.AddSingleton(fullNodeBuilder);
                    });
            });
        }

        /// <summary>
        /// Configures the Ticketbooth demo.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="setupAction">Configuration options</param>
        public static void ConfigureTicketboothDemo(this IServiceCollection services, Action<TicketboothDemoOptions> setupAction = null)
        {
            services.Configure(setupAction);
        }
    }
}
