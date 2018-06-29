using DEXR.Core.Configuration;
using DEXR.Core.Consensus;
using DEXR.Core.Const;
using DEXR.Core.Data;
using DEXR.Core.Model;
using DEXR.Core.Models;
using DEXR.Core.Networking;
using DEXR.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEXR.Core.Services
{
    public class ConsensusServices : ServicesBase
    {
        public void AddConsensusNodes(List<Node> nodes)
        {
            foreach (var node in nodes)
                AddConsensusNode(node);
        }
        public void AddConsensusNode(Node node)
        {
            //Validate address
            Uri uriValidation;
            bool isValidUrl = Uri.TryCreate(node.ServerAddress, UriKind.Absolute, out uriValidation)
                && (uriValidation.Scheme == Uri.UriSchemeHttp || uriValidation.Scheme == Uri.UriSchemeHttps);

            if (isValidUrl)
            {
                var existingNodeServerAddress = ApplicationState.ConnectedNodes.Where(x => x.ServerAddress == node.ServerAddress).FirstOrDefault();
                if(existingNodeServerAddress != null)
                {
                    ApplicationState.ConnectedNodes.Remove(existingNodeServerAddress);
                }
                var existingNodeWalletAddress = ApplicationState.ConnectedNodes.Where(x => x.WalletAddress == node.WalletAddress).FirstOrDefault();
                if (existingNodeWalletAddress != null)
                {
                    ApplicationState.ConnectedNodes.Remove(existingNodeWalletAddress);
                }

                if (ApplicationState.ConnectedNodes.Where(x => x.ServerAddress == node.ServerAddress).Count() == 0 &&
                    ApplicationState.ConnectedNodes.Where(x => x.WalletAddress == node.WalletAddress).Count() == 0)
                {
                    ApplicationState.ConnectedNodes.Add(node);

                    //Update proof of stake
                    UpdateProofOfStake();
                }
            }
        }
        
        public void RegisterSelfOnNetwork(string walletAddress, string signature, bool broadcast)
        {
            ApplicationLog.Info("Registering self on the network.");

            //Broadcast to other nodes
            if(broadcast)
            {
                var nodes = ApplicationState.ConnectedNodesExceptSelf;
                Parallel.ForEach(nodes, new ParallelOptions { MaxDegreeOfParallelism = ConstantConfig.BroadcastThreadCount }, networkNode =>
                {
                    ApiClient api = new ApiClient(networkNode.ServerAddress);
                    api.AnnounceRegisterNode(ApplicationState.ServerAddress, walletAddress, signature);
                });
            }

            //Register self in local nodes cache

            //Get wallet balance
            decimal walletBalance = 0;
            IndexServices indexServices = new IndexServices();
            var nativeToken = indexServices.TokenIndex.GetNative();
            if(nativeToken != null) //Native token would be null if chain has not yet been sync / no local chain
            {
                var walletIndex = indexServices.BalanceIndex.Get(walletAddress, indexServices.TokenIndex.GetNative().Symbol);
                if (walletIndex != null)
                    walletBalance = walletIndex.Balance;
            }

            //Add node to local consensus ledger
            Node node = new Node()
            {
                ServerAddress = ApplicationState.ServerAddress,
                WalletAddress = walletAddress,
                Signature = signature,
                Balance = walletBalance
            };

            AddConsensusNode(node);
            ApplicationState.Node = node;
        }
        
        public async void NextEpoch()
        {
            try
            {
                ApplicationLog.Info("Beginning new epoch iteration");

                ApplicationState.ConsensusState = ConsensusStates.BeginningEpoch;

                ApplicationState.CurrentSpeaker = null;
                NodeChainHeight longestChainNode = null;
                
                if(ApplicationState.DisconnectedNodes.Count > 0)
                {
                    RemoveDisconnectedNodes();
                    //TODO: subsequent epoch can retrieve network nodes from other nodes instead of from seed
                    RetrieveNetworkNodes();
                }

                if (IsChainUpToDate(out longestChainNode))
                {
                    ApplicationState.IsChainUpToDate = true;
                    ApplicationState.LiveEpochCount++;
                    SelectSpeaker();
                }
                else
                {
                    ApplicationState.IsChainUpToDate = false;
                    SyncChain(longestChainNode);

                    ApplicationState.LiveEpochCount = 0;
                    ApplicationState.PendingRecords.Clear();

                    RegisterSelfOnNetwork(ApplicationState.Node.WalletAddress, ApplicationState.Node.Signature, false);

                    NextEpoch();
                }
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                ApplicationLog.Info("Retry in 30 seconds.");
                await Task.Delay(30 * 1000);
                NextEpoch();
            }
        }

        public bool IsChainUpToDate(out NodeChainHeight longestChainNode)
        {
            longestChainNode = null;
            bool isUpToDate = false;

            int margin = 1;
            Node longestNode = null;
            int longestChain = GetLongestChain(out longestNode);

            if(longestChain == -1)
            {
                throw new ValidationException(ResponseCodes.NoChainExistOnNetwork, "Failed to sync chain: " + ResponseMessages.NoChainExistOnNetwork);
            }

            //Compare with self height
            ViewerServices viewerServices = new ViewerServices();
            int localHeight = viewerServices.GetChainHeight();

            if (longestChain > (localHeight + margin))
            {
                isUpToDate = false;
            }
            else
            {
                isUpToDate = true;
            }

            longestChainNode = new NodeChainHeight();
            longestChainNode.Address = longestNode.ServerAddress;
            longestChainNode.ChainHeight = longestChain;

            return isUpToDate;
        }

        public void SyncChain(NodeChainHeight longestChainNode)
        {
            ApplicationLog.Info("Downloading chain.");

            ApplicationState.ConsensusState = ConsensusStates.SyncingChain;

            ViewerServices viewerServices = new ViewerServices();
            int localHeight = viewerServices.GetChainHeight();

            if (localHeight < 0)
                localHeight = 0;

            //TODO: Future enhancement to download from different nodes in parellel
            int currentHeight = localHeight;
            int lastHeight = longestChainNode.ChainHeight;

            while (currentHeight <= lastHeight)
            {
                var blocks = GetBlocks(longestChainNode.Address, currentHeight, currentHeight + 10);
                foreach (var block in blocks)
                {
                    AddBlockToChain(block);
                    currentHeight++;
                }
            }
        }

        public void SelectSpeaker()
        {
            ApplicationState.ConsensusState = ConsensusStates.SelectingSpeaker;

            //Draw speaker from consensus pool
            var speakerNode = DrawSpeaker();

            ApplicationLog.Info(string.Format("Speaker IP: {0}", speakerNode.ServerAddress));

            //Set to state
            ApplicationState.CurrentSpeaker = speakerNode;

            if(ApplicationState.Node.ServerAddress == ApplicationState.CurrentSpeaker.ServerAddress)
            {
                //This node is the speaker
                CreateAndAnnounceBlockHeaderAsync();
            }
            else
            {
                //Timeout configuration on WaitingForSpeaker
                ApplicationState.SpeakerTimeoutTimer = new System.Timers.Timer();
                ApplicationState.SpeakerTimeoutTimer.Interval = 20 * 1000;
                ApplicationState.SpeakerTimeoutTimer.Elapsed += SpeakerTimeoutTimer_Elapsed;
                ApplicationState.SpeakerTimeoutTimer.Start();
            }
            
            ApplicationState.ConsensusState = ConsensusStates.WaitingForSpeaker;
        }

        private void SpeakerTimeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(ApplicationState.ConsensusState == ConsensusStates.WaitingForSpeaker)
                NextEpoch();
        }
        
        public void RetrieveNetworkNodes()
        {
            ApplicationLog.Info("Updating nodes addresses.");

            ApplicationState.ConsensusState = ConsensusStates.UpdatingNodes;

            //Retrieve entire list of network nodes from seed
            foreach (var seed in Settings.ProtocolConfiguration.SeedList)
            {
                ApiClient api = new ApiClient(seed);
                var nodes = api.GetNodes();
                if(nodes != null)
                {
                    //Update nodes to local memory
                    AddConsensusNodes(nodes);
                    break;
                }
            }

            //TODO: fallback if failed to retrieve from seed, get from connected nodes
        }

        public async void CreateAndAnnounceBlockHeaderAsync()
        {
            ApplicationState.ConsensusState = ConsensusStates.CreatingBlock;

            //Get last block timestamp
            DataServices dataService = new DataServices();
            var lastBlock = dataService.LastBlock;
            if (lastBlock == null)
                throw new Exception("Not last block available.");

            int lastBlockTimestamp = lastBlock.Header.Timestamp;

            //Calculate next block timestamp
            int nextBlockTime = lastBlockTimestamp + ConstantConfig.BlockInterval;
            int currentTime = DateTimeUtility.ToUnixTime(DateTime.UtcNow);

            //Wait until next block time
            if (nextBlockTime > currentTime)
            {
                int secondsToWait = nextBlockTime - currentTime;
                ApplicationLog.Info("Time until next block (seconds): " + secondsToWait);

                await Task.Delay(secondsToWait * 1000);
            }

            ApplicationLog.Info("Creating new block header");

            //Create new block
            var header = dataService.CreateBlockHeader();

            //Broadcast new block to network
            var nodes = ApplicationState.ConnectedNodes;
            Parallel.ForEach(nodes, new ParallelOptions { MaxDegreeOfParallelism = ConstantConfig.BroadcastThreadCount }, networkNode =>
            {
                ApiClient api = new ApiClient(networkNode.ServerAddress);
                api.AnnounceNewBlock(header);
            });
        }
        
        private int GetLongestChain(out Node longestNode)
        {
            ApplicationLog.Info("Getting longest chain.");

            int longestChain = -1;
            Node longestNetworkNode = null;
            
            //TODO: In order not to overload the node with longest chain, get a list of nodes
            //with longest chain and randomly select one to server

            var nodes = ApplicationState.ConnectedNodes;
            Parallel.ForEach(nodes, new ParallelOptions { MaxDegreeOfParallelism = ConstantConfig.BroadcastThreadCount }, networkNode =>
            {
                ApiClient api = new ApiClient(networkNode.ServerAddress);
                int height = api.GetChainHeight();

                if (height > longestChain)
                {
                    longestChain = height;
                    longestNetworkNode = networkNode;
                }
            });

            longestNode = longestNetworkNode;

            return longestChain;
        }

        private List<Block> GetBlocks(string node, int fromIndex, int toIndex)
        {
            ApiClient api = new ApiClient(node);
            return api.GetBlocks(fromIndex, toIndex);
        }

        private void AddBlockToChain(Block block)
        {
            DataServices dataServices = new DataServices();
            dataServices.SaveBlockToChain(block);
        }

        private void Echo(string endpoint, string data)
        {
            try
            {
                ApplicationLog.Info("Relaying echo.");

                var nodes = ApplicationState.ConnectedNodes;
                Parallel.ForEach(nodes, new ParallelOptions { MaxDegreeOfParallelism = ConstantConfig.BroadcastThreadCount }, networkNode =>
                {
                    ApiClient api = new ApiClient(networkNode.ServerAddress);
                    var response = api.SendPostRequest(endpoint, data);
                });
            }
            catch
            {
                throw;
            }
        }

        private void UpdateProofOfStake()
        {
            //Get genesis block
            ViewerServices viewerServices = new ViewerServices();
            var genesisBlock = viewerServices.GetBlock(0);
            if(genesisBlock != null)
            {
                List<NodeStake> stakes = new List<NodeStake>();

                foreach (var node in ApplicationState.ConnectedNodes)
                {
                    stakes.Add(new NodeStake()
                    {
                        WalletAddress = node.WalletAddress,
                        Balance = node.Balance
                    });
                }

                ApplicationState.PosPool.UpdateNodes(stakes, genesisBlock.Header.Timestamp, 15);
            }
        }

        private Node DrawSpeaker()
        {
            ApplicationLog.Info("Drawing speaker from proof of stake pool.");

            var drawResult = ApplicationState.PosPool.DrawSpeaker();
            return ApplicationState.ConnectedNodes.Where(x => x.WalletAddress == drawResult.WalletAddress).FirstOrDefault();
        }

        private void RemoveDisconnectedNodes()
        {
            if (ApplicationState.DisconnectedNodes.Count > 0)
            {
                ApplicationLog.Info("Removing disconnected nodes.");

                foreach (var disconnectedNode in ApplicationState.DisconnectedNodes)
                {
                    if(disconnectedNode.ServerAddress != ApplicationState.Node.ServerAddress)
                    {
                        var removeNode = ApplicationState.ConnectedNodes.Where(x => x.ServerAddress == disconnectedNode.ServerAddress).FirstOrDefault();
                        if(removeNode != null)
                            ApplicationState.ConnectedNodes.Remove(removeNode);
                    }
                }

                ApplicationState.DisconnectedNodes.Clear();
            }
        }
    }
}
