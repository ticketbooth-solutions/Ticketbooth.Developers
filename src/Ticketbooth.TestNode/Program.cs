﻿using NBitcoin.Protocol;
using Stratis.Bitcoin;
using Stratis.Bitcoin.Builder;
using Stratis.Bitcoin.Configuration;
using Stratis.Bitcoin.Features.BlockStore;
using Stratis.Bitcoin.Features.MemoryPool;
using Stratis.Bitcoin.Features.SmartContracts;
using Stratis.Bitcoin.Features.SmartContracts.PoA;
using Stratis.Bitcoin.Features.SmartContracts.Wallet;
using Stratis.Bitcoin.Utilities;
using Stratis.Sidechains.Networks;
using System;
using System.Threading.Tasks;
using Ticketbooth.Api;

namespace Ticketbooth.TestNode
{
    class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        public static async Task MainAsync(string[] args)
        {
            try
            {
                var nodeSettings = new NodeSettings(
                    network: CirrusNetwork.NetworksSelector.Testnet(),
                    protocolVersion: ProtocolVersion.CIRRUS_VERSION,
                    args: args)
                {
                    MinProtocolVersion = ProtocolVersion.ALT_PROTOCOL_VERSION
                };

                IFullNode node = GetSideChainFullNode(nodeSettings);

                if (node != null)
                    await node.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was a problem initializing the node. Details: '{0}'", ex.Message);
            }
        }

        private static IFullNode GetSideChainFullNode(NodeSettings nodeSettings)
        {
            IFullNodeBuilder nodeBuilder = new FullNodeBuilder()
                .UseNodeSettings(nodeSettings)
                .UseBlockStore()
                .UseMempool()
                .AddSmartContracts(options =>
                {
                    options.UseReflectionExecutor();
                    options.UsePoAWhitelistedContracts();
                })
                .UseSmartContractPoAConsensus()
                .UseSmartContractPoAMining()
                .UseSmartContractWallet()
                //.UseDiagnosticFeature()
                .AddTicketboothApi();

            //if (nodeSettings.EnableSignalR)
            //{
            //    nodeBuilder.AddSignalR(options =>
            //    {
            //        options.EventsToHandle = new[]
            //        {
            //            (IClientEvent) new BlockConnectedClientEvent(),
            //            new TransactionReceivedClientEvent()
            //        };

            //        options.ClientEventBroadcasters = new[]
            //        {
            //            (Broadcaster: typeof(WalletInfoBroadcaster),
            //                ClientEventBroadcasterSettings: new ClientEventBroadcasterSettings
            //                {
            //                    BroadcastFrequencySeconds = 5
            //                })
            //        };
            //    });
            //}

            return nodeBuilder.Build();
        }
    }
}
