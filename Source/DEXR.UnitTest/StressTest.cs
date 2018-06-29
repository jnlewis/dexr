using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using DEXR.Core.Model.Requests;
using DEXR.Core.Cryptography;
using Newtonsoft.Json;
using System.Net;
using DEXR.Core.Model;
using DEXR.Core;
using DEXR.Core.Const;
using DEXR.Core.Utility;
using System.Diagnostics;

namespace DEXR.UnitTest
{
    public class StressTest
    {
        StressTestOptions _options;
        List<StressTestNode> nodes;

        public StressTest(StressTestOptions options)
        {
            _options = options;
        }

        public void RunEmitTransactions()
        {
            try
            {
                int successCount = 0;
                int failedCount = 0;

                Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 4 }, delegate (int i)
                {
                    bool isSuccessful = SendTransaction("http://localhost:8080", (DateTimeUtility.CurrentUnixTimeUTC() + i));

                    if (isSuccessful)
                    {
                        successCount++;
                        Console.WriteLine("Successfully sent transaction " + (i + 1).ToString());
                    }
                    else
                    {
                        failedCount++;
                    }
                });
                
                Console.WriteLine(string.Format("Total Success: {0}, Total Failed: {1}", successCount, failedCount));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Run()
        {
            /*
             * Prerequisite: 
             * Dexr application folder must be ready and configured to seed node.
             * Application must have chain already created
             
             1. Spawn clone nodes
                - Duplicate application folder
                - Launch exe with param "dexr.exe startConsensus port=N"
             2. Start up seed node from ST config
            3. Call seed node API to get list of nodes on network
            4. Create N tx every second and split equally to parallel send to network nodes
            5. Timer to consolidate logs from all nodes
             */

            //Prepare nodes
            PrepareNodes();

            //Clone nodes - Duplicate application folder
            foreach (var node in nodes)
            {
                CopyDirectory(_options.ApplicationFolder, node.ApplicationFolder);
            }

            //Start up seed node
            StartNode(nodes[0]);

            //Wait for seed node to load completely
            System.Threading.Thread.Sleep(5000);

            //Start up clone nodes
            for (int i = 1; i < _options.NumberOfClones + 1; i++)
            {
                StartNode(nodes[i]);
            }

            //Wait for clone nodes to load completely
            System.Threading.Thread.Sleep(5000);

            //Start sending transactions
            StartEmitter();
        }

        private void PrepareNodes()
        {
            nodes = new List<StressTestNode>();
            nodes.Add(new StressTestNode() { ApplicationFolder = _options.ApplicationFolder + "_0", Port = _options.SeedNodePort });
            for (int i = 1; i < _options.NumberOfClones + 1; i++)
            {
                nodes.Add(new StressTestNode() { ApplicationFolder = _options.ApplicationFolder + "_" + i, Port = (_options.CloneStartPort + i - 1) });
            }
        }

        private void StartNode(StressTestNode node)
        {
            string args = "StartConsensus port=" + node.Port;
            var proc = System.Diagnostics.Process.Start(node.ApplicationFolder + "/DEXR.exe", args);
            node.Process = proc;
            node.Status = "Started";
        }

        private void StopNode(StressTestNode node)
        {
            if(node.Status == "Started")
            {
                node.Process.CloseMainWindow();
                node.Process.Close();
                node.Status = "Stopped";
            }
        }

        private void CopyDirectory(string sourcePath, string destinationPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
        }

        private void StartEmitter()
        {
            System.Timers.Timer emitTxTimer = new System.Timers.Timer();
            emitTxTimer.Elapsed += EmitTxTimer_Elapsed;
            emitTxTimer.Interval = _options.IntervalInSeconds * 1000;
            emitTxTimer.Enabled = true;
        }

        private void EmitTxTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                //Send transactions
                for(int i=0; i<_options.TxPerInterval; i++)
                {
                    //Parallel

                    //Round robin
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public bool SendTransaction(string nodeAddress, long nonce)
        {
            bool isSuccessful = false;

            try
            {
                //Test send native token from owner wallet to another wallet
                TransactionTransferRequest data = new TransactionTransferRequest();
                data.Body = new TransactionTransferRequestBody()
                {
                    Nonce = nonce,
                    TokenSymbol = "DXR",
                    Sender = "N4ue7ZC1ciuiDjn9QgKHH5etHnVAjhRsJtwnyVLH7TqBBmaobL7ZtZm7MD8YhZgyvkw89BBkyzWJwd8CGfTxDLNR",
                    ToAddress = "SK8mWrHYb5LFznZxi7XThogYHdS4ZCkrd7182dUuS7G2vzkTgEKoCPbhrBLfdPujxnTS9JPhBcwaYqrKxcnqA2dZ",
                    Amount = "1"
                };
                //data.Signature = KeySignature.Sign(this.PrivateKey, JsonConvert.SerializeObject(data.Body));

                Node sender = new Node() { ServerAddress = "http://localhost:8080" };
                string endpoint = nodeAddress + "/transaction/transfer";
                string responseData = this.SendPostRequest(sender, endpoint, JsonConvert.SerializeObject(data));

                isSuccessful = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                isSuccessful = false;
            }

            return isSuccessful;
        }
        
        #region Private Methods

        private string SendGetRequest(Node sender, string endpoint)
        {
            string jsonResponse = string.Empty;
            string result = null;

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.Accept] = "application/json";
                    client.Headers["sender"] = JsonConvert.SerializeObject(sender);
                    jsonResponse = client.DownloadString(new Uri(endpoint));
                }

                var response = JsonConvert.DeserializeObject<GenericResponse>(jsonResponse);
                if (response.Code != ResponseCodes.Success)
                {
                    throw new Exception(response.Code + ":" + response.Message);
                }
                else
                {
                    if (response.Data != null)
                        result = response.Data.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }

        private string SendPostRequest(Node sender, string endpoint, string data)
        {
            string jsonResponse = string.Empty;
            string result = null;

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.Accept] = "application/json";
                    client.Headers["sender"] = JsonConvert.SerializeObject(sender);
                    jsonResponse = client.UploadString(new Uri(endpoint), data);
                }

                var response = JsonConvert.DeserializeObject<GenericResponse>(jsonResponse);
                if (response.Code != ResponseCodes.Success)
                {
                    throw new Exception(response.Code + ":" + response.Message);
                }
                else
                {
                    if (response.Data != null)
                        result = response.Data.ToString();
                }
            }
            catch
            {
                throw;
            }

            return result;
        }

        #endregion
        
    }
}
