using DEXR.Core.Const;
using DEXR.Core.Cryptography;
using DEXR.Core.Model;
using DEXR.Core.Model.Requests;
using DEXR.Core.Models;
using DEXR.Core.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DEXR.UnitTest
{
    public class SimulationTest
    {
        private const string nodeServerAddress = "http://localhost:8080";

        public async void Run()
        {
            try
            {
                Log("Running simulation...");

                ////Note: To enable API for viewing blockchain only but not starting consensus
                //RunApiServer();

                //Note: Delete and re-create a new chain. Also creates test wallets
                RunNewChain();

                //Note: Start consensus and chain extention. Will also enable API for viewing blockchain.
                RunConsensus();

                //Note: Send transactions for creating new tokens
                await Task.Delay(17 * 1000);
                RunCreateTokens();

                //await Task.Delay(17 * 1000);
                //ShowTokens();
                //ShowBalanceForDXR();

                await Task.Delay(20 * 1000);
                RunTransfer();

                await Task.Delay(18 * 1000);
                ShowBalanceForDXR();

                //await Task.Delay(15 * 1000);

                //await Task.Delay(5 * 1000);
                //RunTradePartialLimitFill();

                //await Task.Delay(5 * 1000);
                //RunTradeSingleOrderFill();

                //await Task.Delay(5 * 1000);
                //RunTransfer2();

                //await Task.Delay(20 * 1000);

                //await Task.Delay(5 * 1000);
                //RunTradePartialMarketFill();
            }
            catch (Exception ex)
            {
                Log("Error: " + ex.Message);
            }
        }

        private void RunApiServer()
        {
            try
            {
                Log("RunApiServer");

                DEXR.Commands.Run("start server");
            }
            catch
            {
                throw;
            }
        }

        private void RunNewChain()
        {
            try
            {
                Log("RunNewChain");

                DEXR.Commands.Run("open wallet wallet_owner.json");

                DEXR.Commands.Run("delete chain");
                DEXR.Commands.Run("create new chain");
                DEXR.Commands.Run("create wallet walletA.json");
                DEXR.Commands.Run("create wallet walletB.json");
                DEXR.Commands.Run("create wallet walletC.json");
            }
            catch
            {
                throw;
            }
        }

        private async void RunConsensus()
        {
            try
            {
                Log("RunConsensus");

                DEXR.Commands.Run("open wallet wallet_owner.json");

                await Task.Delay(1 * 1000);
                DEXR.Commands.Run("start consensus");

                //Give it a few seconds for consensus to startup
                await Task.Delay(5 * 1000);
            }
            catch
            {
                throw;
            }
        }

        private void RunRebuildIndex()
        {
            try
            {
                Log("RunRebuildIndex");

                DEXR.Commands.Run("start server");
            }
            catch
            {
                throw;
            }
        }

        private async void RunCreateTokens()
        {
            try
            {
                Log("RunCreateTokens");

                var walletOwner = ReadWallet("wallet_owner.json");
                var walletA = ReadWallet("walletA.json");
                var walletB = ReadWallet("walletB.json");
                var walletC = ReadWallet("walletC.json");

                bool success = true;

                await Task.Delay(1 * 1000);
                Log("Creating token BTC");
                success = SendTxCreateToken("BTC", "Bitcoin", 25000000, 8, walletA.PublicKey, walletA.PrivateKey);
                if (!success)
                    throw new Exception("Failed. Simulation halted.");

                await Task.Delay(1 * 1000);
                Log("Creating token ETH");
                success = SendTxCreateToken("ETH", "Ethereum", 50000000, 8, walletB.PublicKey, walletB.PrivateKey);
                if (!success)
                    throw new Exception("Failed. Simulation halted.");

                await Task.Delay(1 * 1000);
                Log("Creating token NEO");
                success = SendTxCreateToken("NEO", "Neo", 100000000, 8, walletC.PublicKey, walletC.PrivateKey);
                if (!success)
                    throw new Exception("Failed. Simulation halted.");

                Log("Waiting for confirmation block creation 15s.");
            }
            catch
            {
                throw;
            }
        }

        private async void RunTransfer()
        {
            try
            {
                Log("RunTransfer");

                var walletOwner = ReadWallet("wallet_owner.json");
                var walletA = ReadWallet("walletA.json");
                var walletB = ReadWallet("walletB.json");
                var walletC = ReadWallet("walletC.json");

                bool success = true;

                await Task.Delay(1 * 1000);
                Log("Transfering DXR from Owner to A");
                success = SendTxTransfer("DXR", walletOwner.PublicKey, walletA.PublicKey, "100000", "35", walletOwner.PrivateKey);
                if (!success)
                    throw new Exception("Failed. Simulation halted.");

                await Task.Delay(1 * 1000);
                Log("Transfering DXR from Owner to B");
                success = SendTxTransfer("DXR", walletOwner.PublicKey, walletB.PublicKey, "80000", "35", walletOwner.PrivateKey);
                if (!success)
                    throw new Exception("Failed. Simulation halted.");

                await Task.Delay(1 * 1000);
                Log("Transfering DXR from Owner to C");
                success = SendTxTransfer("DXR", walletOwner.PublicKey, walletC.PublicKey, "50000", "35", walletOwner.PrivateKey);
                if (!success)
                    throw new Exception("Failed. Simulation halted.");

                Log("Waiting for confirmation block creation 15s.");
            }
            catch
            {
                throw;
            }
        }

        private async void RunTransfer2()
        {
            try
            {
                Log("RunTransfer");

                var walletOwner = ReadWallet("wallet_owner.json");
                var walletA = ReadWallet("walletA.json");
                var walletB = ReadWallet("walletB.json");
                var walletC = ReadWallet("walletC.json");

                bool success = true;

                await Task.Delay(1 * 1000);
                Log("Transfering ETH from B to A and C");
                success = SendTxTransfer("ETH", walletB.PublicKey, walletA.PublicKey, "25000", "35", walletB.PrivateKey);
                if (!success)
                    throw new Exception("Failed. Simulation halted.");

                await Task.Delay(1 * 1000);
                Log("Transfering ETH from B to A and C");
                success = SendTxTransfer("ETH", walletB.PublicKey, walletC.PublicKey, "25000", "35", walletB.PrivateKey);
                if (!success)
                    throw new Exception("Failed. Simulation halted.");


                await Task.Delay(1 * 1000);
                Log("Transfering NEO from C to A and B");
                success = SendTxTransfer("ETH", walletC.PublicKey, walletA.PublicKey, "25000", "35", walletC.PrivateKey);
                if (!success)
                    throw new Exception("Failed. Simulation halted.");

                await Task.Delay(1 * 1000);
                Log("Transfering NEO from C to A and B");
                success = SendTxTransfer("ETH", walletC.PublicKey, walletB.PublicKey, "25000", "35", walletC.PrivateKey);
                if (!success)
                    throw new Exception("Failed. Simulation halted.");


                Log("Waiting for confirmation block creation 15s.");
            }
            catch
            {
                throw;
            }
        }

        private async void RunTradePartialLimitFill()
        {
            try
            {
                Log("RunTrading");

                var walletA = ReadWallet("walletA.json");
                var walletB = ReadWallet("walletB.json");
                var walletC = ReadWallet("walletC.json");


                //WalletB place buy limit order BTC/ETH for 50 BTC at 10 ETH each (selling 500 ETH)
                await Task.Delay(1 * 1000);
                Log("WalletB place buy limit order BTC/ETH for 50 BTC at 10 ETH each");
                SendTxOrderLimit("BTC/ETH", "Buy", "50", "10", 240, walletB.PublicKey, "5", walletB.PrivateKey);

                //Wait for block creation to confirm order
                Log("Waiting for confirmation block creation 15s.");
                await Task.Delay(20 * 1000);

                Log("Displaying orders BTC/ETH");
                ShowOrders("BTC/ETH");


                //WalletA place sell market order BTC/ETH for 25 BTC (buys 250 ETH from walletB 50% order fill)
                await Task.Delay(5 * 1000);
                Log("WalletA place sell market order BTC/ETH for 25 BTC");
                SendTxOrderMarket("BTC/ETH", "Sell", "25", walletA.PublicKey, "5", walletA.PrivateKey);

                //Wait for block creation to confirm order
                Log("Waiting for confirmation block creation 15s.");
                await Task.Delay(20 * 1000);

                Log("Displaying orders BTC/ETH");
                ShowOrders("BTC/ETH");
            }
            catch
            {
                throw;
            }
        }

        private async void RunTradeSingleOrderFill()
        {
            try
            {
                Log("RunTrading");

                var walletA = ReadWallet("walletA.json");
                var walletB = ReadWallet("walletB.json");
                var walletC = ReadWallet("walletC.json");

                //WalletB place sell limit order BTC/ETH of 25 BTC at 10 ETH each (buying 250 ETH)
                await Task.Delay(1 * 1000);
                Log("WalletB place sell limit order BTC/ETH of 25 BTC at 10 ETH each");
                SendTxOrderLimit("BTC/ETH", "Sell", "25", "10", 240, walletB.PublicKey, "5", walletB.PrivateKey);

                //Wait for block creation to confirm order
                Log("Waiting for confirmation block creation 15s.");
                await Task.Delay(20 * 1000);

                Log("Displaying orders BTC/ETH");
                ShowOrders("BTC/ETH");
                ShowAllBalances();

                //WalletA place buy market order BTC/ETH for 25 BTC at market price (sells 250 ETH to walletB 100% order fill)
                await Task.Delay(5 * 1000);
                Log("WalletA place buy market order BTC/ETH for 25 BTC");
                SendTxOrderMarket("BTC/ETH", "Buy", "25", walletA.PublicKey, "5", walletA.PrivateKey);

                //Wait for block creation to confirm order
                Log("Waiting for confirmation block creation 15s.");
                await Task.Delay(20 * 1000);

                Log("Displaying orders BTC/ETH");
                ShowOrders("BTC/ETH");

                ShowAllBalances();
            }
            catch
            {
                throw;
            }
        }

        private async void RunTradePartialMarketFill()
        {
            try
            {
                Log("RunTrading");

                var walletA = ReadWallet("walletA.json");
                var walletB = ReadWallet("walletB.json");
                var walletC = ReadWallet("walletC.json");

                //WalletA place sell limit order BTC/ETH for 10 BTC at 10 ETH each

                //WalletB place sell limit order BTC/ETH for 20 BTC at 12 ETH each

                //WalletC place buy market order BTC/ETH of 50 BTC at market price
                //(buys 10 BTC sells 100 ETH to WalletA 100% order fill)
                //(buys 20 BTC sells 240 ETH to WalletB 100% order fill)
                //(WalletC market order is partially filled)
            }
            catch
            {
                throw;
            }
        }

        private async void RunTradeFullMarketFill()
        {
            try
            {
                Log("RunTrading");

                var walletA = ReadWallet("walletA.json");
                var walletB = ReadWallet("walletB.json");
                var walletC = ReadWallet("walletC.json");

                //WalletA place sell limit order BTC/ETH for 10 BTC at 10 ETH each

                //WalletB place sell limit order BTC/ETH for 20 BTC at 12 ETH each

                //WalletC place buy market order BTC/ETH of 15 BTC at market price
                //(buys 10 BTC sells 100 ETH to WalletA 100% order fill)
                //(buys 5 BTC sells 120 ETH to WalletB 100% order fill)
                //(WalletC market order is completely filled)

            }
            catch
            {
                throw;
            }
        }


        #region Helper Methods

        private WalletConfig ReadWallet(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new Exception("Could not find wallet file: " + fileName);
            }

            var jsonConfig = File.ReadAllText(fileName);
            var walletFile = JsonConvert.DeserializeObject<WalletConfig>(jsonConfig);

            return walletFile;
        }

        private void ShowTokens()
        {
            Log("Displaying tokens in blockchain.");

            string endpoint = nodeServerAddress + "/view/tokens";
            string responseData = this.SendGetRequest(endpoint);
            Log("Response:" + responseData);
        }

        private void ShowBalance(string symbol, string address)
        {
            ViewWalletRequest data = new ViewWalletRequest();
            data.Address = address;
            data.TokenSymbol = symbol;
            
            string endpoint = nodeServerAddress + "/view/wallet-balance";
            string responseData = this.SendPostRequest(endpoint, JsonConvert.SerializeObject(data));
            Log("Response:" + responseData);
        }
        private void ShowAllBalances()
        {
            ShowBalanceForDXR();
            ShowBalanceForBTC();
            ShowBalanceForETH();
            ShowBalanceForNEO();
        }
        private void ShowBalanceForDXR()
        {
            Log("ShowBalanceForDXR");
            try
            {
                var walletOwner = ReadWallet("wallet_owner.json");
                var walletA = ReadWallet("walletA.json");
                var walletB = ReadWallet("walletB.json");
                var walletC = ReadWallet("walletC.json");

                Log("Showing balance for walletOwner:");
                ShowBalance("DXR", walletOwner.PublicKey);

                Log("Showing balance for walletA:");
                ShowBalance("DXR", walletA.PublicKey);

                Log("Showing balance for walletB:");
                ShowBalance("DXR", walletB.PublicKey);

                Log("Showing balance for walletC:");
                ShowBalance("DXR", walletC.PublicKey);
            }
            catch
            {
                throw;
            }
        }
        private void ShowBalanceForBTC()
        {
            Log("ShowBalanceForBTC");
            try
            {
                var walletOwner = ReadWallet("wallet_owner.json");
                var walletA = ReadWallet("walletA.json");
                var walletB = ReadWallet("walletB.json");
                var walletC = ReadWallet("walletC.json");

                Log("Showing balance for walletOwner:");
                ShowBalance("BTC", walletOwner.PublicKey);

                Log("Showing balance for walletA:");
                ShowBalance("BTC", walletA.PublicKey);

                Log("Showing balance for walletB:");
                ShowBalance("BTC", walletB.PublicKey);

                Log("Showing balance for walletC:");
                ShowBalance("BTC", walletC.PublicKey);
            }
            catch
            {
                throw;
            }
        }
        private void ShowBalanceForETH()
        {
            Log("ShowBalanceForETH");
            try
            {
                var walletOwner = ReadWallet("wallet_owner.json");
                var walletA = ReadWallet("walletA.json");
                var walletB = ReadWallet("walletB.json");
                var walletC = ReadWallet("walletC.json");

                Log("Showing balance for walletOwner:");
                ShowBalance("ETH", walletOwner.PublicKey);

                Log("Showing balance for walletA:");
                ShowBalance("ETH", walletA.PublicKey);

                Log("Showing balance for walletB:");
                ShowBalance("ETH", walletB.PublicKey);

                Log("Showing balance for walletC:");
                ShowBalance("ETH", walletC.PublicKey);
            }
            catch
            {
                throw;
            }
        }
        private void ShowBalanceForNEO()
        {
            Log("ShowBalanceForNEO");
            try
            {
                var walletOwner = ReadWallet("wallet_owner.json");
                var walletA = ReadWallet("walletA.json");
                var walletB = ReadWallet("walletB.json");
                var walletC = ReadWallet("walletC.json");

                Log("Showing balance for walletOwner:");
                ShowBalance("NEO", walletOwner.PublicKey);

                Log("Showing balance for walletA:");
                ShowBalance("NEO", walletA.PublicKey);

                Log("Showing balance for walletB:");
                ShowBalance("NEO", walletB.PublicKey);

                Log("Showing balance for walletC:");
                ShowBalance("NEO", walletC.PublicKey);
            }
            catch
            {
                throw;
            }
        }

        private bool SendTxCreateToken(string symbol, string name, long totalSupply, short decimals, string owner, string privateKey)
        {
            try
            {
                TransactionCreateTokenRequest data = new TransactionCreateTokenRequest();
                data.Body = new TransactionCreateTokenRequestBody();
                data.Body.Nonce = DateTimeUtility.CurrentUnixTimeUTC();
                data.Body.Fee = "0";
                data.Body.Symbol = symbol;
                data.Body.Name = name;
                data.Body.TotalSupply = totalSupply;
                data.Body.Decimals = decimals;
                data.Body.Owner = owner;
                data.Signature = KeySignature.Sign(privateKey, JsonConvert.SerializeObject(data.Body));
                
                string endpoint = nodeServerAddress + "/transaction/create-token";
                Log("Api:" + endpoint);
                string responseData = this.SendPostRequest(endpoint, JsonConvert.SerializeObject(data));
                Log("Response:" + responseData);

                return true;
            }
            catch(Exception ex)
            {
                Log("Failed: " + ex.Message);
                return false;
            }
        }

        private bool SendTxTransfer(string symbol, string sender, string toAddress, string amount, string fee, string privateKey)
        {
            try
            {
                TransactionTransferRequest data = new TransactionTransferRequest();
                data.Body = new TransactionTransferRequestBody();
                data.Body.Nonce = DateTimeUtility.CurrentUnixTimeUTC();
                data.Body.Fee = fee;
                data.Body.TokenSymbol = symbol;
                data.Body.Sender = sender;
                data.Body.ToAddress = toAddress;
                data.Body.Amount = amount;
                data.Signature = KeySignature.Sign(privateKey, JsonConvert.SerializeObject(data.Body));
                
                string endpoint = nodeServerAddress + "/transaction/transfer";
                Log("Api:" + endpoint);
                string responseData = this.SendPostRequest(endpoint, JsonConvert.SerializeObject(data));
                Log("Response:" + responseData);

                return true;
            }
            catch (Exception ex)
            {
                Log("Failed: " + ex.Message);
                return false;
            }
        }

        private bool SendTxOrderLimit(string symbol, string side, string amount, string price, int expiryBlocks, string sender, string fee, string privateKey)
        {
            try
            {
                TransactionOrderLimitRequest data = new TransactionOrderLimitRequest();
                data.Body = new TransactionOrderLimitRequestBody();
                data.Body.Nonce = DateTimeUtility.CurrentUnixTimeUTC();
                data.Body.Fee = fee;
                data.Body.PairSymbol = symbol;
                data.Body.Side = side;
                data.Body.Price = price;
                data.Body.Amount = amount;
                data.Body.ExpiryBlocks = expiryBlocks;
                data.Body.Owner = sender;
                data.Signature = KeySignature.Sign(privateKey, JsonConvert.SerializeObject(data.Body));
                
                string endpoint = nodeServerAddress + "/transaction/limit-order";
                Log("Api:" + endpoint);
                string responseData = this.SendPostRequest(endpoint, JsonConvert.SerializeObject(data));
                Log("Response:" + responseData);

                return true;
            }
            catch (Exception ex)
            {
                Log("Failed: " + ex.Message);
                return false;
            }
        }

        private bool SendTxOrderMarket(string symbol, string side, string amount, string sender, string fee, string privateKey)
        {
            try
            {
                TransactionOrderMarketRequest data = new TransactionOrderMarketRequest();
                data.Body = new TransactionOrderMarketRequestBody();
                data.Body.Nonce = DateTimeUtility.CurrentUnixTimeUTC();
                data.Body.Fee = fee;
                data.Body.PairSymbol = symbol;
                data.Body.Side = side;
                data.Body.Amount = amount;
                data.Body.Owner = sender;
                data.Signature = KeySignature.Sign(privateKey, JsonConvert.SerializeObject(data.Body));
                
                string endpoint = nodeServerAddress + "/transaction/market-order";
                Log("Api:" + endpoint);
                string responseData = this.SendPostRequest(endpoint, JsonConvert.SerializeObject(data));
                Log("Response:" + responseData);

                return true;
            }
            catch (Exception ex)
            {
                Log("Failed: " + ex.Message);
                return false;
            }
        }

        private void ShowOrders(string tradingPair)
        {
            Log("Showing orders " + tradingPair);

            ViewOrdersRequest data = new ViewOrdersRequest();
            data.TradingPair = tradingPair;

            string endpoint = nodeServerAddress + "/view/orders";
            Log("Api:" + endpoint);
            string responseData = this.SendPostRequest(endpoint, JsonConvert.SerializeObject(data));
            Log("Response:" + responseData);
        }

        #endregion

        #region Web Requests

        private string SendGetRequest(string endpoint)
        {
            string jsonResponse = string.Empty;
            string result = null;

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.Accept] = "application/json";
                    jsonResponse = client.DownloadString(new Uri(endpoint));
                }

                var response = JsonConvert.DeserializeObject<GenericResponse>(jsonResponse);
                if (response.Code != ResponseCodes.Success)
                {
                    throw new Exception(response.Code + ":" + response.Message);
                }
                else
                {
                    //if (response.Data != null)
                    //    result = response.Data.ToString();
                    result = jsonResponse;
                }
            }
            catch
            {
                throw;
            }

            return result;
        }

        private string SendPostRequest(string endpoint, string data)
        {
            string jsonResponse = string.Empty;
            string result = null;

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.Accept] = "application/json";
                    jsonResponse = client.UploadString(new Uri(endpoint), data);
                }

                var response = JsonConvert.DeserializeObject<GenericResponse>(jsonResponse);
                if (response.Code != ResponseCodes.Success)
                {
                    throw new Exception(response.Code + ":" + response.Message);
                }
                else
                {
                    //if (response.Data != null)
                    //    result = response.Data.ToString();
                    result = jsonResponse;
                }
            }
            catch
            {
                throw;
            }

            return result;
        }

        #endregion

        private static void Log(string message)
        {
            string log = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + message;

            Console.WriteLine(log);
            ApplicationLog.Info(log);
        }
    }
}
