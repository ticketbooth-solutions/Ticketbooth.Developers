using Microsoft.Extensions.DependencyInjection;
using Stratis.Bitcoin.Builder;

namespace Ticketbooth.ContractApiFeature
{
    public static class FullNodeBuilderExtension
    {
        public static IFullNodeBuilder UseContractApi(this IFullNodeBuilder fullNodeBuilder)
        {
            fullNodeBuilder.ConfigureFeature(features =>
            {
                features
                .AddFeature<ContractApiFeature>()
                .FeatureServices(services =>
                {
                    services.AddSingleton(fullNodeBuilder);
                });
            });

            return fullNodeBuilder;
        }
    }
}
