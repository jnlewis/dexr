using DEXR.Core.Configuration;
using DEXR.Core.Data;
using DEXR.Core.Data.Index;
using DEXR.Core.Models;

namespace DEXR.Core.Services
{
    public class IndexServices : ServicesBase
    {
        private Database database;

        public int LastBlockIndex { get; private set; }

        public IndexOwner OwnerIndex { get; private set; }
        public IndexToken TokenIndex { get; private set; }
        public IndexBalance BalanceIndex { get; private set; }
        public IndexOrder OrderIndex { get; private set; }

        public IndexServices()
        {
            database = new Database(Settings.ProtocolConfiguration.ChainFolderName);
            OwnerIndex = new IndexOwner(database);
            TokenIndex = new IndexToken(database);

            string nativeTokenSymbol = null;
            if (TokenIndex.GetNative() != null)
                nativeTokenSymbol = TokenIndex.GetNative().Symbol;

            BalanceIndex = new IndexBalance(database, nativeTokenSymbol);
            OrderIndex = new IndexOrder(database);
        }

        public void UpdateIndexForAllBlocks()
        {
            ViewerServices viewerService = new ViewerServices();
            var allBlocks = viewerService.GetAllBlocks();

            foreach (var block in allBlocks)
            {
                UpdateIndex(block);
            }
        }

        public void DeleteIndexForAllBlocks()
        {
            ViewerServices viewerService = new ViewerServices();
            var allBlocks = viewerService.GetAllBlocks();

            foreach (var block in allBlocks)
            {
                DeleteIndex(block);
            }
        }

        public void UpdateIndex(Block block)
        {
            if (block.Transactions == null)
                return;

            if(block.Header.Index == 0)    //genesis block
            {
                //Create owner index
                string ownerAddress = null;
                foreach (var record in block.Transactions)
                {
                    if (record is TransactionTransfer)
                    {
                        ownerAddress = ((TransactionTransfer)record).ToAddress;
                        break;
                    }
                }
                OwnerIndex.AddToIndex(ownerAddress);
            }

            foreach (var record in block.Transactions)
            {
                if (record is TransactionToken)
                {
                    TokenIndex.AddToIndex(record as TransactionToken);
                }
                else if (record is TransactionTransfer)
                {
                    BalanceIndex.AddToIndex(record as TransactionTransfer);
                }
                else if (record is TransactionOrderLimit)
                {
                    OrderIndex.AddToIndex(record as TransactionOrderLimit);
                }
                else if (record is TransactionOrderMatch)
                {
                    OrderIndex.UpdateIndexForMatchedOrder(record as TransactionOrderMatch);
                }
                else if (record is TransactionOrderCancel)
                {
                    OrderIndex.DeleteSingleTransaction((record as TransactionOrderCancel).OrderTransactionId);
                }
                else if (record is TransactionBlockFee)
                {
                    BalanceIndex.AddToFeeIndex(record as TransactionBlockFee);
                }
            }

            //Delete expired orders from index
            OrderIndex.DeleteExpiredOrders(block.Header.Index);
        }
        
        private void DeleteIndex(Block block)
        {
            if (block.Transactions == null)
                return;

            if (block.Header.Index == 0)    //genesis block
            {
                OwnerIndex.DeleteIndex();
            }

            foreach (var record in block.Transactions)
            {
                if (record is TransactionToken)
                {
                    TokenIndex.DeleteIndex(record as TransactionToken);
                }
                else if (record is TransactionTransfer)
                {
                    BalanceIndex.DeleteIndex(record as TransactionTransfer);
                }
                else if (record is TransactionOrderLimit)
                {
                    OrderIndex.DeleteIndex(record as TransactionOrderLimit);
                }
            }
        }
    }
}
