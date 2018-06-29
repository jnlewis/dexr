using System;
using System.Collections.Generic;
using System.Linq;

namespace DEXR.Core.Consensus
{
    public class ProofOfStake
    {
        private List<NodeStake> _nodes = null;
        private List<string> _distributedSchedule = null;
        private int _genesisTimestamp = -1;
        private int _blockDuration = -1;

        public ProofOfStake()
        {
        }

        public void UpdateNodes(List<NodeStake> nodes, int genesisTimestmap, int blockDuration)
        {
            _nodes = nodes;
            _genesisTimestamp = genesisTimestmap;
            _blockDuration = blockDuration;

            Update(nodes);
        }

        public NodeStake DrawSpeaker()
        {
            if (_distributedSchedule == null)
                throw new Exception("Unable to draw speaker. Distributed schedule is not ready. Call UpdateNodes first.");

            int epochSinceGenesis = Convert.ToInt32((CurrentUnixTimeUTC() - _genesisTimestamp) / _blockDuration);
            int index = epochSinceGenesis % _distributedSchedule.Count;
            return _nodes.Where(x => x.WalletAddress == _distributedSchedule[index]).FirstOrDefault();
        }

        #region Private Methods

        private void Update(List<NodeStake> nodes)
        {
            //Calculate the percentage balance of each node
            Dictionary<string, int> nodeStakesPercent = ComputeStakePercentages(nodes);

            //Expand nodes by percentage share
            List<List<string>> expandedNodes = ExpandNodes(nodeStakesPercent);

            //Evently distribute expanded nodes
            _distributedSchedule = MergeEvenly(expandedNodes);
        }

        private Dictionary<string, int> ComputeStakePercentages(List<NodeStake> nodes)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            var total = nodes.Sum(x => x.Balance);
            if (total == 0)
            {
                //no stakes
                foreach (var node in nodes)
                {
                    result.Add(node.WalletAddress, 1);
                }
            }
            else
            {
                foreach (var node in nodes)
                {
                    var percent = Convert.ToInt32(Math.Ceiling(((decimal)node.Balance / (decimal)total) * 100));
                    result.Add(node.WalletAddress, percent);
                }
            }

            return result;
        }

        private List<List<string>> ExpandNodes(Dictionary<string, int> stakePercentages)
        {
            List<List<string>> result = new List<List<string>>();

            var sortedStakePercentages = stakePercentages.OrderByDescending(x => x.Value).ToList();
            foreach(var item in sortedStakePercentages)
            {
                List<string> nodeAddressList = new List<string>();

                for (int i = 0; i < item.Value; i++)
                {
                    nodeAddressList.Add(item.Key);
                }

                result.Add(nodeAddressList);
            }

            return result;
        }

        private List<string> MergeEvenly(List<List<string>> expandedNodes)
        {
            List<string> result = new List<string>();

            foreach(var list in expandedNodes)
            {
                result = ListMerge.MergeWithEvenDistribution(result, list);
            }

            return result;
        }

        private int CurrentUnixTimeUTC()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        #endregion
    }

    public struct NodeStake
    {
        public string WalletAddress;
        public decimal Balance;
    }
}
