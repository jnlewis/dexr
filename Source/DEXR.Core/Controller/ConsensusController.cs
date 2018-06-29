using DEXR.Core.Const;
using DEXR.Core.Model;
using DEXR.Core.Models;
using DEXR.Core.Services;
using System;
using System.Linq;

namespace DEXR.Core.Controller
{
    public class ConsensusController : ControllerBase
    {
        private Node callerNode;

        public ConsensusController(Node caller = null)
        {
            if(caller == null)
            {
                callerNode = new Node();
            }
            else
            {
                callerNode = caller;
            }
        }

        public GenericResponse StartConsensus(string walletAddress, string signature)
        {
            try
            {
                if(ApplicationState.ServerState != ServerStates.Active)
                {
                    return new GenericResponse(null, ResponseCodes.ServerNotStarted, ResponseMessages.ServerNotStarted);
                }
                if (ApplicationState.ConsensusState != null && ApplicationState.ConsensusState != ConsensusStates.Inactive)
                {
                    return new GenericResponse(null, ResponseCodes.ConsensusAlreadyActive, ResponseMessages.ConsensusAlreadyActive);
                }
                
                ApplicationState.ConsensusState = ConsensusStates.Started;
                ApplicationState.LiveEpochCount = 0;

                ConsensusServices consensusService = new ConsensusServices();

                consensusService.RetrieveNetworkNodes();

                consensusService.RegisterSelfOnNetwork(walletAddress, signature, true);

                consensusService.NextEpoch();

                return new GenericResponse(null, ResponseCodes.Success, ResponseMessages.Success);
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
        }

        public GenericResponse GetConnectedNodes()
        {
            try
            {
                ApplicationLog.Info("Retrieving connected nodes.");

                var nodes = ApplicationState.ConnectedNodes;

                //Filter out nodes flagged as disconnected
                foreach (var disconnectedNode in ApplicationState.DisconnectedNodes)
                {
                    if (disconnectedNode.ServerAddress != ApplicationState.Node.ServerAddress)
                    {
                        var removeNode = nodes.Where(x => x.ServerAddress == disconnectedNode.ServerAddress).FirstOrDefault();
                        if (removeNode != null)
                            nodes.Remove(removeNode);
                    }
                }
                
                return new GenericResponse(nodes, ResponseCodes.Success, ResponseMessages.Success);
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
        }

        public GenericResponse RegisterNodeAnnouncement(string serverAddress, string walletAddress, string signature)
        {
            try
            {
                VerifySignature(walletAddress, signature, walletAddress);

                //Get wallet balance
                decimal walletBalance = 0;
                IndexServices indexServices = new IndexServices();
                var nativeToken = indexServices.TokenIndex.GetNative();
                if (nativeToken != null) //Native token would be null if chain has not yet been sync / no local chain
                {
                    var walletIndex = indexServices.BalanceIndex.Get(walletAddress, indexServices.TokenIndex.GetNative().Symbol);
                    if (walletIndex != null)
                        walletBalance = walletIndex.Balance;
                }

                //Add node to consensus
                Node node = new Node() {
                    ServerAddress = serverAddress,
                    WalletAddress = walletAddress,
                    Signature = signature,
                    Balance = walletBalance
                };

                ConsensusServices consensusService = new ConsensusServices();
                consensusService.AddConsensusNode(node);

                ApplicationLog.Info("Registered node: " + node.ServerAddress);

                return new GenericResponse(null, ResponseCodes.Success, ResponseMessages.Success);
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
        }
        
        public GenericResponse NewBlockAnnouncement(BlockHeader header)
        {
            try
            {
                if(ApplicationState.ConsensusState != ConsensusStates.WaitingForSpeaker &&
                    ApplicationState.ConsensusState != ConsensusStates.CreatingBlock)
                {
                    return new GenericResponse(null, ResponseCodes.Success, ResponseMessages.Success);
                }

                //TODO: verify sender signature
                //VerifySignature(callerNode.WalletAddress);

                ApplicationLog.Info("Received new block");

                DataServices dataServices = new DataServices();
                
                //Validate if sender is current speaker
                if (callerNode.ServerAddress != ApplicationState.CurrentSpeaker.ServerAddress &&
                    callerNode.WalletAddress != ApplicationState.CurrentSpeaker.WalletAddress)
                {
                    ApplicationLog.Info("Speaker Not Recognized");
                    return new GenericResponse(null, ResponseCodes.SpeakerNotRecognized, ResponseMessages.SpeakerNotRecognized);
                }

                //If first epoch, no need to validate as transactions will unlikely tally due to different start consensus time
                if (ApplicationState.LiveEpochCount == 1 && 
                    ApplicationState.CurrentSpeaker.ServerAddress != ApplicationState.Node.ServerAddress)
                {
                    dataServices.ClearPendingTxUpToId(header.LastTransactionId);
                    return new GenericResponse(null, ResponseCodes.Success, ResponseMessages.Success);
                }

                //Validate block transactions with local pending transactions
                //Note: If somehow this node received all transactions but not in the correct order, block will be discarded
                //and wait for correction during re-sync in subsequent epochs
                if(header.Hash != dataServices.HashBlockHeaderAndTransactions(header, dataServices.GetPendingTxUpTo(header.LastTransactionId)))
                {
                    //Pending Transaction And Block Mismatch
                    ApplicationLog.Info("Pending Transaction And Block Mismatch");
                    return new GenericResponse(null, ResponseCodes.PendingTransactionAndBlockMismatch, ResponseMessages.PendingTransactionAndBlockMismatch);
                }

                //Create Block
                var newBlock = dataServices.CreateBlockAndClearPendingTx(header);

                //Attach block fee
                newBlock = dataServices.AttachBlockFee(newBlock, ApplicationState.CurrentSpeaker);

                //Add block to chain
                dataServices.SaveBlockToChain(newBlock);
                ApplicationLog.Info("New block added to chain. Index: " + header.Index);
                
                return new GenericResponse(null, ResponseCodes.Success, ResponseMessages.Success);
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
            finally
            {
                //Resume next iteration of consensus
                ConsensusServices consensusServices = new ConsensusServices();
                consensusServices.NextEpoch();
            }
        }
        
    }
}
