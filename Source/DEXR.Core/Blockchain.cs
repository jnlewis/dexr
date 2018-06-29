using DEXR.Core.Configuration;
using DEXR.Core.Const;
using DEXR.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEXR.Core
{
    public class Blockchain
    {
        private static Server _server;
        public static string StartServer()
        {
            string protocol = null;
            string address = null;

            Initialize();

            _server = new Server(Settings.ProtocolConfiguration.ServerPort);
            _server.Start();

            if (Settings.ProtocolConfiguration.ServerHostType.ToLower() == "default")
            {
                protocol = "http";
                address = NetworkUtility.GetPublicIPAddress();
            }
            else if (Settings.ProtocolConfiguration.ServerHostType.ToLower() == "localhost")
            {
                protocol = "http";
                address = "localhost";
            }
            else if (Settings.ProtocolConfiguration.ServerHostType.ToLower() == "default_https")
            {
                protocol = "https";
                address = NetworkUtility.GetPublicIPAddress();
            }
            else if (Settings.ProtocolConfiguration.ServerHostType.ToLower() == "localhost_https")
            {
                protocol = "https";
                address = "localhost";
            }

            ApplicationState.ServerState = ServerStates.Active;
            ApplicationState.ServerAddress = protocol + "://" + address + ":" + _server.Port;

            ApplicationState.Node = new Model.Node() { ServerAddress = ApplicationState.ServerAddress };

            return ApplicationState.ServerAddress;
        }
        
        public static void StopServer()
        {
            _server = new Server(Settings.ProtocolConfiguration.ServerPort);

            ApplicationState.ServerState = ServerStates.Inactive;
            ApplicationState.ServerAddress = null;
        }
        
        private static void Initialize()
        {
            ApplicationState.Node = null;
            ApplicationState.ServerState = null;
            ApplicationState.ServerAddress = null;
            ApplicationState.ConsensusState = null;
            ApplicationState.PendingRecords = new List<Models.Transaction>();
            ApplicationState.ConnectedNodes = new List<Model.Node>();
            ApplicationState.DisconnectedNodes = new List<Model.Node>();
            ApplicationState.CurrentSpeaker = null;
            ApplicationState.PosPool = new Consensus.ProofOfStake();
            ApplicationState.IsChainUpToDate = false;
        }
    }
}
