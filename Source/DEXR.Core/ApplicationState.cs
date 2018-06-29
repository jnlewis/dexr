using DEXR.Core.Consensus;
using DEXR.Core.Data;
using DEXR.Core.Model;
using DEXR.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DEXR.Core
{
    //TODO: Consider using write locks for properties here as some may be accessed concurrently

    public static class ApplicationState
    {
        public static System.Timers.Timer SpeakerTimeoutTimer { get; set; }

        public static DBClient DBClient { get; set; }
        public static string ServerState { get; set; }
        public static string ServerAddress { get; set; }
        public static string ConsensusState { get; set; }

        public static List<Transaction> PendingRecords { get; set; }
        
        public static List<Node> ConnectedNodes { get; set; }
        public static List<Node> ConnectedNodesExceptSelf
        {
            get { return ConnectedNodes.Where(x => x.ServerAddress != Node.ServerAddress).ToList(); }
        }
        public static List<Node> DisconnectedNodes { get; set; }

        public static Node CurrentSpeaker { get; set; }
        public static Node Node { get; set; }
        public static int LiveEpochCount { get; set; }
        
        public static ProofOfStake PosPool { get; set; }
        public static bool IsChainUpToDate { get; set; }

        public static void PendingRecordsAdd(Transaction transaction)
        {
            if (PendingRecords == null)
                PendingRecords = new List<Transaction>();

            transaction.Type = transaction.GetType().Name;
            PendingRecords.Add(transaction);
        }
        public static void PendingRecordsAddRange(IEnumerable<Transaction> transactions)
        {
            if (PendingRecords == null)
                PendingRecords = new List<Transaction>();

            foreach (var item in transactions)
            {
                item.Type = item.GetType().Name;
            }
            PendingRecords.AddRange(transactions);
        }
    }
}
