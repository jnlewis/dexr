using System;
using System.IO;
using DEXR.CLI;
using DEXR.CLI.Model;
using DEXR.Core;
using DEXR.Core.Controller;
using DEXR.Core.Cryptography;
using Newtonsoft.Json;
using DEXR.Core.Const;

namespace DEXR
{
    public static class Commands
    {
        private static Wallet activeWallet;

        public static void Run(string input)
        {
            if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                RunHelp();
            }
            else if (input.StartsWith("create wallet", StringComparison.OrdinalIgnoreCase))
            {
                string fileName = input.Substring(
                    "create wallet".Length,
                    input.Length - "create wallet".Length);

                RunCreateWallet(fileName.Trim());
            }
            else if (input.StartsWith("open wallet", StringComparison.OrdinalIgnoreCase))
            {
                string fileName = input.Substring(
                    "open wallet".Length, 
                    input.Length - "open wallet".Length);

                RunOpenWallet(fileName.Trim());
            }
            else if (input.Equals("delete chain", StringComparison.OrdinalIgnoreCase))
            {
                RunDeleteChain();
            }
            else if (input.Equals("create new chain", StringComparison.OrdinalIgnoreCase))
            {
                RunCreateNewChain();
            }
            else if (input.Equals("start server", StringComparison.OrdinalIgnoreCase))
            {
                RunStartServer();
            }
            else if (input.Equals("start consensus", StringComparison.OrdinalIgnoreCase))
            {
                RunStartConsensus();
            }
            else if (input.Equals("show height", StringComparison.OrdinalIgnoreCase))
            {
                RunShowHeight();
            }
            else if (input.Equals("show tokens", StringComparison.OrdinalIgnoreCase))
            {
                RunShowTokens();
            }
            else if (input.Equals("rebuild index", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Note: This option has been deprecated. Index is built automatically on each block commit.");
                RunRebuildIndex();
            }
            else
            {
                Console.WriteLine("Unrecognized command: " + input);
            }
        }

        private static void RunHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("help");
            Console.WriteLine("create wallet <wallet.json>");
            Console.WriteLine("open wallet <wallet.json>");
            Console.WriteLine("create new chain");
            Console.WriteLine("delete chain");
            Console.WriteLine("start server");
            Console.WriteLine("start consensus");
            Console.WriteLine("show height");
            Console.WriteLine("show tokens");
            Console.WriteLine("rebuild index");
        }

        private static void RunDeleteChain()
        {
            BlockchainController controller = new BlockchainController();
            controller.DeleteLocalChain();
        }

        private static void RunCreateNewChain()
        {
            if (activeWallet == null)
            {
                Console.WriteLine("No active wallet. Open a wallet first.");
                return;
            }

            BlockchainController controller = new BlockchainController();
            controller.CreateNewChain(activeWallet.Address);
            Console.WriteLine("New chain created.");
        }

        private static void RunStartServer()
        {
            if(ApplicationState.ServerState == ServerStates.Active)
            {
                Console.WriteLine("Server already started.");
                return;
            }

            Console.WriteLine("Starting server...");
            var address = Blockchain.StartServer();
            Console.WriteLine("Server started. Listening on " + address);
        }

        private static void RunCreateWallet(string fileName)
        {
            if (!fileName.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Wallet file name must end with .json");
                return;
            }
            if (File.Exists(fileName))
            {
                Console.WriteLine("A wallet file with this name already exists.");
                return;
            }

            var keys = KeySignature.GenerateKeyPairs();
            WalletConfig wallet = new WalletConfig()
            {
                PrivateKey = keys.PrivateKey,
                PublicKey = keys.PublicKey
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(wallet, Formatting.Indented);
            File.WriteAllText(fileName, json);

            Console.WriteLine("Wallet created successfully");
        }

        private static void RunOpenWallet(string fileName)
        {
            if(!File.Exists(fileName))
            {
                Console.WriteLine("Could not find wallet file: " + fileName);
                return;
            }
            var jsonConfig = File.ReadAllText(fileName);
            var walletFile = Newtonsoft.Json.JsonConvert.DeserializeObject<WalletConfig>(jsonConfig);
            var signature = KeySignature.Sign(walletFile.PrivateKey, walletFile.PublicKey);

            activeWallet = new Wallet()
            {
                Address = walletFile.PublicKey.Trim(),
                PrivateKey = walletFile.PrivateKey.Trim(),
                Signature = signature.Trim()
            };

            Console.WriteLine("Wallet opened successfully: " + activeWallet.Address);
        }

        private static void RunStartConsensus()
        {
            if (activeWallet == null)
            {
                Console.WriteLine("No active wallet. Open a wallet first.");
                return;
            }

            if (ApplicationState.ServerState != ServerStates.Active)
            {
                RunStartServer();
            }

            ConsensusController consensus = new ConsensusController();
            var response = consensus.StartConsensus(activeWallet.Address, activeWallet.Signature);
            if (response.Code == "1")
            {
                Console.WriteLine(JsonConvert.SerializeObject(response.Data));
            }
            else
            {
                Console.WriteLine("Error: " + response.Code + ":" + response.Message);
            }
        }
        
        private static void RunShowHeight()
        {
            ViewerController viewer = new ViewerController();
            var response = viewer.GetChainHeight();
            if (response.Code == "1")
            {
                Console.WriteLine(JsonConvert.SerializeObject(response.Data));
            }
            else
            {
                Console.WriteLine(response.Message);
            }
        }
        
        private static void RunShowTokens()
        {
            ViewerController viewer = new ViewerController();
            var response = viewer.GetAllTokens();
            if(response.Code == "1")
            {
                Console.WriteLine(JsonConvert.SerializeObject(response.Data));
            }
            else
            {
                Console.WriteLine(response.Message);
            }
        }

        private static void RunRebuildIndex()
        {
            BlockchainController controller = new BlockchainController();
            var response = controller.RebuildIndex();
            if (response.Code == "1")
            {
                Console.WriteLine(JsonConvert.SerializeObject(response.Data));
            }
            else
            {
                Console.WriteLine(response.Message);
            }
        }
    }
}
