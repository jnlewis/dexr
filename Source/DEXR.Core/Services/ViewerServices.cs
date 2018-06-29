using DEXR.Core.Configuration;
using DEXR.Core.Const;
using DEXR.Core.Data;
using DEXR.Core.Model;
using DEXR.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEXR.Core.Services
{
    public class ViewerServices : ServicesBase
    {
        private Database database;
        
        public ViewerServices()
        {
            database = new Database(Settings.ProtocolConfiguration.ChainFolderName);
        }

        public int GetChainHeight()
        {
            DataServices dataServices = new DataServices();
            var lastBlock = dataServices.LastBlock;
            if(lastBlock != null)
            {
                return lastBlock.Header.Index;
            }
            else
            {
                return -1;
            }
        }

        public List<Transaction> GetPending()
        {
            return ApplicationState.PendingRecords;
        }

        public Block GetBlock(int index)
        {
            return database.GetBlock(index);
        }
        public List<Block> GetBlocks(int fromIndex, int toIndex)
        {
            List<Block> blocks = new List<Block>();

            for(int i=fromIndex; i<=toIndex; i++)
            {
                var block = database.GetBlock(i);
                if(block != null)
                {
                    blocks.Add(block);
                }
                else
                {
                    break;
                }
            }

            return blocks;
        }
        public List<Block> GetAllBlocks()
        {
            List<Block> blocks = new List<Block>();

            Block currentBlock = database.GetBlock(0);
            while (currentBlock != null)
            {
                blocks.Add(currentBlock);
                currentBlock = database.GetBlock(currentBlock.Header.Index + 1);
            }

            return blocks;
        }

        /// <summary>
        /// Returns the network fee based on number of transactions in the latest block
        /// </summary>
        public long GetNetworkFee()
        {
            DataServices dataServices = new DataServices();
            var lastBlock = dataServices.LastBlock;
            
            if (lastBlock != null)
            {
                IndexServices indexServices = new IndexServices();
                var nativeToken = indexServices.TokenIndex.GetNative();

                var multiplier = Convert.ToInt64(Math.Floor(Convert.ToDecimal(lastBlock.Header.TransactionCount / ConstantConfig.NetworkFeeUnit)));
                var rate = ToFactoredPrice(ConstantConfig.NetworkFeeRate, nativeToken.Decimals);
                return rate * multiplier;
            }
            else
            {
                return 0;
            }
        }

        public decimal? GetWalletBalance(string address, string tokenSymbol)
        {
            decimal? balance = null;

            IndexServices indexService = new IndexServices();
            var index = indexService.BalanceIndex.Get(address, tokenSymbol);
            if(index != null)
            {

                balance = FromFactoredPrice(index.Balance, indexService.TokenIndex.Get(tokenSymbol).Decimals);
            }

            return balance;
        }
    }
}
