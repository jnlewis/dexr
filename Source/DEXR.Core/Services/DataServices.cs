using DEXR.Core.Configuration;
using DEXR.Core.Const;
using DEXR.Core.Cryptography;
using DEXR.Core.Data;
using DEXR.Core.Model;
using DEXR.Core.Models;
using DEXR.Core.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DEXR.Core.Services
{
    public class DataServices : ServicesBase
    {
        private Database database;

        public DataServices()
        {
            database = new Database(Settings.ProtocolConfiguration.ChainFolderName);
        }

        public Block LastBlock
        {
            get
            {
                int lastBlockIndex = database.GetLastBlockIndex();
                if (lastBlockIndex != -1)
                {
                    return database.GetBlock(lastBlockIndex);
                }
                else
                {
                    return null;
                }
            }
        }

        public void CreateGenesisBlock(
            string ownerAddress,
            string nativeTokenName,
            string nativeTokenSymbol,
            long initialSupply,
            short decimals)
        {
            if (LastBlock != null || (ApplicationState.PendingRecords != null && ApplicationState.PendingRecords.Count > 0))
                throw new Exception("Error CreateGenesisBlock: Chain already exist.");

            List<Transaction> transactions = new List<Transaction>();

            //Deploy native token to genesis block
            TransactionToken nativeToken = new TransactionToken();
            nativeToken.Name = nativeTokenName;
            nativeToken.Symbol = nativeTokenSymbol;
            nativeToken.Decimals = decimals;
            nativeToken.TransactionId = GenerateTransactionId(nativeToken);
            transactions.Add(nativeToken);

            //Transfer native token supply to owner address
            TransactionTransfer transfer = new TransactionTransfer();
            transfer.TokenSymbol = nativeTokenSymbol;
            transfer.FromAddress = null;
            transfer.ToAddress = ownerAddress;
            transfer.Amount = ToFactoredPrice(initialSupply, decimals);
            transfer.TransactionId = GenerateTransactionId(transfer);
            transactions.Add(transfer);

            ApplicationState.PendingRecordsAddRange(transactions);

            //Create genesis block
            var header = CreateBlockHeader(true);
            var block = CreateBlockAndClearPendingTx(header);
            
            //Push genesis block to blockchain
            database.SaveBlock(block);

            //Update index
            IndexServices indexServices = new IndexServices();
            indexServices.UpdateIndex(block);
        }
        
        public void ClearPendingTxUpToId(string transactionId)
        {
            if (string.IsNullOrEmpty(transactionId))
                return;

            int skip = 1;
            foreach(var tx in ApplicationState.PendingRecords)
            {
                if(tx.TransactionId == transactionId)
                {
                    break;
                }
                else
                {
                    skip++;
                }
            }

            var selectedTx = ApplicationState.PendingRecords.Skip(skip).ToList();
            ApplicationState.PendingRecords.Clear();
            ApplicationState.PendingRecordsAddRange(selectedTx);
        }

        public List<Transaction> GetPendingTxUpTo(string transactionId)
        {
            List<Transaction> result = new List<Transaction>();

            if (string.IsNullOrEmpty(transactionId))
                return result;

            foreach (var tx in ApplicationState.PendingRecords)
            {
                result.Add(tx);

                if (tx.TransactionId == transactionId)
                    break;
            }
            return result;
        }

        public BlockHeader CreateBlockHeader(bool isGenesisBlock = false)
        {
            var transactions = ApplicationState.PendingRecords.ToList();
            
            BlockHeader header = new BlockHeader();

            if(isGenesisBlock)
            {
                header.Index = 0;
                header.PreviousHash = null;
            }
            else
            {
                var lastBlock = LastBlock;
                if (lastBlock == null)
                    throw new Exception("Not last block available.");

                header.Index = lastBlock.Header.Index + 1;
                header.PreviousHash = lastBlock.Header.Hash;
            }

            header.Timestamp = DateTimeUtility.ToUnixTime(DateTime.UtcNow);
            header.TransactionCount = transactions.Count;
            if (transactions.Count > 0)
                header.LastTransactionId = transactions[transactions.Count - 1].TransactionId;
            header.Hash = HashBlockHeaderAndTransactions(header, transactions);

            return header;
        }

        public Block CreateBlockAndClearPendingTx(BlockHeader header)
        {
            var transactions = GetPendingTxUpTo(header.LastTransactionId);
            ClearPendingTxUpToId(header.LastTransactionId);
            
            Block block = new Block();
            block.Header = header;
            block.Transactions = transactions;
            return block;
        }

        public Block AttachBlockFee(Block block, Node speaker)
        {
            if (block.Transactions == null)
                throw new Exception("AttachBlockFee: Block transactions is null.");

            if (block.Transactions.Count > 0 && 
                block.Transactions.Last() is TransactionBlockFee)
            {
                throw new Exception("Fee already attached to this block");
            }
            
            TransactionBlockFee blockFeeTx = new TransactionBlockFee();
            blockFeeTx.Nonce = block.Header.Index;    //Nonce of block fee is block index for block fee to standardize across all nodes without network transfer
            blockFeeTx.TotalFee = block.Transactions.Sum(x => x.Fee);
            blockFeeTx.ToAddress = speaker.WalletAddress;
            blockFeeTx.TransactionId = GenerateTransactionId(blockFeeTx);
            blockFeeTx.Type = blockFeeTx.GetType().Name;

            block.Transactions.Add(blockFeeTx);
            block.Header.TransactionCount += 1;
            block.Header.Hash = HashBlockHeaderAndTransactions(block.Header, block.Transactions);

            return block;
        }
        
        public void SaveBlockToChain(Block block)
        {
            //Validate block hash
            var previousBlock = database.GetBlock(block.Header.Index - 1);
            if(previousBlock.Header.Hash != block.Header.PreviousHash)
            {
                throw new ValidationException(ResponseCodes.InvalidHash, ResponseMessages.InvalidHash);
            }

            //Push the new block to blockchain
            database.SaveBlock(block);

            //Update index
            IndexServices indexService = new IndexServices();
            indexService.UpdateIndex(block);
        }
        
        public bool HasLocalChainData()
        {
            if (Directory.Exists(Settings.ProtocolConfiguration.ChainFolderName) &&
                Directory.GetFiles(Settings.ProtocolConfiguration.ChainFolderName).Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //public bool ValidateLocalChain()
        //{
        //    bool isValid = true;
        //    int currentIndex = -1;
        
        //    Block currentBlock = database.GetBlock(0);
        //    while (currentBlock != null)
        //    {
        //        currentIndex = currentBlock.Header.Index;
        //        Console.Write("\rValidating block {0}", currentIndex);

        //        if (currentBlock.Header.Index == 0)
        //        {
        //            isValid = IsBlockValid(currentBlock, null);
        //        }
        //        else
        //        {
        //            isValid = IsBlockValid(currentBlock, database.GetBlock(currentBlock.Header.Index - 1));
        //        }
        //        if (!isValid)
        //        {
        //            ApplicationLog.Warn("Block invalid at block index " + currentIndex);
        //            break;
        //        }

        //        //Fetch next block
        //        currentBlock = database.GetBlock(currentIndex + 1);
        //    }
        //    return isValid;
        //}

        public void DeleteLocalChain()
        {
            Directory.Delete(Settings.ProtocolConfiguration.ChainFolderName, true);
        }

        /// <summary>
        /// Creates a SHA-256 hash of a Block
        /// </summary>
        public string HashBlock(Block block)
        {
            return HashBlockHeaderAndTransactions(block.Header, block.Transactions);
        }
        public string HashBlockHeaderAndTransactions(BlockHeader header, List<Transaction> transactions)
        {
            header.Hash = null;
            string headerHash = HashUtility.SHA256(JsonConvert.SerializeObject(header));
            string txHash = HashBlockTransactions(transactions);
            return HashUtility.SHA256(headerHash + txHash);
        }
        public string HashBlockTransactions(List<Transaction> transactions)
        {
            if (transactions == null || transactions.Count == 0)
                return string.Empty;

            List<string> hashes = transactions.Select(x => x.TransactionId).ToList();
            MerkleTree tree = new MerkleTree(hashes);
            return tree.Root.Hash;
        }

        public bool ValidateBlocks(List<Block> blocks)
        {
            bool isValid = false;

            for (int i = 0; i < blocks.Count; i++)
            {
                if (i == 0)
                    continue;

                isValid = IsBlockValid(blocks[i], blocks[i - 1]);
                if (!isValid)
                    break;
            }

            return isValid;
        }

        public bool IsBlockValid(Block currentBlock, Block previousBlock)
        {
            if (previousBlock == null)
                return true;

            if (previousBlock.Header.Index + 1 != currentBlock.Header.Index)
                return false;

            if (previousBlock.Header.Hash != currentBlock.Header.PreviousHash)
                return false;

            if (HashBlock(currentBlock) != currentBlock.Header.Hash)
                return false;

            return true;
        }
        
        public WalletAddress GenerateWalletAddress()
        {
            var keys = KeySignature.GenerateKeyPairs();
            WalletAddress wallet = new WalletAddress()
            {
                Address = keys.PublicKey,
                PrivateKey = keys.PrivateKey
            };

            return wallet;
        }

    }
}
