using DEXR.Core.Configuration;
using DEXR.Core.Controller;
using DEXR.Core.Model.Requests;
using DEXR.HttpServer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DEXR.Core.Networking
{
    internal class ApiController : ApiControllerBase
    {
        // GET: /
        public static HttpResponse HomeIndex(HttpRequest request)
        {
            GenericController controller = new GenericController();
            var response = controller.GetWelcome();

            return CreateHttpResponse(response);
        }
        
        // POST: /sign
        public static HttpResponse Sign(HttpRequest request)
        {
            SignRequest requestData = JsonConvert.DeserializeObject<SignRequest>(request.Content);

            //string content = request.Content.Substring(request.Content.LastIndexOf("\"Content\":") + 1);
            //content = content.Replace("Content\":", "");
            //content = content.Substring(0, content.Length - 1);

            GenericController controller = new GenericController();
            //var response = controller.Sign(requestData.PrivateKey, content);
            var response = controller.Sign(requestData.PrivateKey, JsonConvert.SerializeObject(requestData.Content));

            return CreateHttpResponse(response);
        }
        
        #region Viewer API

        // GET: /view/chainheight
        public static HttpResponse ViewChainHeight(HttpRequest request)
        {
            ViewerController controller = new ViewerController();
            var response = controller.GetChainHeight();

            return CreateHttpResponse(response);
        }

        // GET: /view/pending
        public static HttpResponse ViewPending(HttpRequest request)
        {
            ViewerController controller = new ViewerController();
            var response = controller.GetPending();

            return CreateHttpResponse(response);
        }
        
        // GET: /view/tokens
        public static HttpResponse ViewAllTokens(HttpRequest request)
        {
            ViewerController controller = new ViewerController();
            var response = controller.GetAllTokens();

            return CreateHttpResponse(response);
        }

        // POST: /view/wallet-balance
        public static HttpResponse ViewWalletBalance(HttpRequest request)
        {
            ViewWalletRequest requestData = JsonConvert.DeserializeObject<ViewWalletRequest>(request.Content);

            ViewerController controller = new ViewerController();
            var response = controller.GetWalletBalance(requestData.Address, requestData.TokenSymbol);

            return CreateHttpResponse(response);
        }

        // POST: /view/orders
        public static HttpResponse ViewOrders(HttpRequest request)
        {
            ViewOrdersRequest requestData = JsonConvert.DeserializeObject<ViewOrdersRequest>(request.Content);

            ViewerController controller = new ViewerController();
            var response = controller.GetOrders(requestData.TradingPair);

            return CreateHttpResponse(response);
        }

        // POST: /view/block
        public static HttpResponse ViewBlocks(HttpRequest request)
        {
            ViewBlocksRequest requestData = JsonConvert.DeserializeObject<ViewBlocksRequest>(request.Content);

            ViewerController controller = new ViewerController();
            var response = controller.GetBlocks(requestData.FromIndex, requestData.ToIndex);

            return CreateHttpResponse(response);
        }

        // GET: /view/network-fee
        public static HttpResponse ViewNetworkFee(HttpRequest request)
        {
            ViewerController controller = new ViewerController();
            var response = controller.GetNetworkFee();

            return CreateHttpResponse(response);
        }

        #endregion

        #region Transactions API

        // POST: /transaction/create-token
        public static HttpResponse AddToken(HttpRequest request)
        {
            TransactionCreateTokenRequest requestData = JsonConvert.DeserializeObject<TransactionCreateTokenRequest>(request.Content);

            TransactionController controller = new TransactionController();
            var response = controller.CreateToken(requestData);
            
            Echo(request);

            return CreateHttpResponse(response);
        }

        // POST: /transaction/transfer
        public static HttpResponse AddTransfer(HttpRequest request)
        {
            TransactionTransferRequest requestData = JsonConvert.DeserializeObject<TransactionTransferRequest>(request.Content);

            TransactionController controller = new TransactionController();
            var response = controller.Transfer(requestData);

            Echo(request);

            return CreateHttpResponse(response);
        }

        // POST: /transaction/limit-order
        public static HttpResponse AddOrderLimit(HttpRequest request)
        {
            TransactionOrderLimitRequest requestData = JsonConvert.DeserializeObject<TransactionOrderLimitRequest>(request.Content);

            TransactionController controller = new TransactionController();
            var response = controller.AddOrderLimit(requestData);

            Echo(request);

            return CreateHttpResponse(response);
        }

        // POST: /transaction/market-order
        public static HttpResponse AddOrderMarket(HttpRequest request)
        {
            TransactionOrderMarketRequest requestData = JsonConvert.DeserializeObject<TransactionOrderMarketRequest>(request.Content);

            TransactionController controller = new TransactionController();
            var response = controller.AddOrderMarket(requestData);

            Echo(request);

            return CreateHttpResponse(response);
        }

        // POST: /transaction/cancel-order
        public static HttpResponse CancelOrder(HttpRequest request)
        {
            TransactionCancelOrderRequest requestData = JsonConvert.DeserializeObject<TransactionCancelOrderRequest>(request.Content);

            TransactionController controller = new TransactionController();
            var response = controller.CancelOrder(requestData);

            Echo(request);

            return CreateHttpResponse(response);
        }

        #endregion

        #region Consensus API

        // GET: /consensus/nodes
        public static HttpResponse GetNodes(HttpRequest request)
        {
            ConsensusController controller = new ConsensusController();
            var response = controller.GetConnectedNodes();

            return CreateHttpResponse(response);
        }
        
        // POST: /consensus/announce-register-node
        public static HttpResponse RegisterNodeAnnouncement(HttpRequest request)
        {
            AnnounceRegisterNodeRequest requestData = JsonConvert.DeserializeObject<AnnounceRegisterNodeRequest>(request.Content);

            ConsensusController controller = new ConsensusController(GetCallerNode(request));
            var response = controller.RegisterNodeAnnouncement(requestData.ServerAddress, requestData.WalletAddress, requestData.Signature);

            return CreateHttpResponse(response);
        }
        
        // POST: /consensus/announce-new-block
        public static HttpResponse NewBlockAnnouncement(HttpRequest request)
        {
            AnnounceNewBlockRequest requestData = JsonConvert.DeserializeObject<AnnounceNewBlockRequest>(request.Content, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            ConsensusController controller = new ConsensusController(GetCallerNode(request));
            var response = controller.NewBlockAnnouncement(requestData.NewBlockHeader);

            return CreateHttpResponse(response);
        }
        
        #endregion

        private static void Echo(HttpRequest request)
        {
            try
            {
                //If request is already an echo, don't re-echo
                if (request.Headers.ContainsKey("echo"))
                {
                    return;
                }

                //Set header to identity that this message is an echo
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("echo", "1");

                //Echo to all other nodes in network
                var nodes = ApplicationState.ConnectedNodesExceptSelf;
                Parallel.ForEach(nodes, new ParallelOptions { MaxDegreeOfParallelism = ConstantConfig.BroadcastThreadCount }, networkNode =>
                {
                    ApiClient api = new ApiClient(networkNode.ServerAddress);
                    api.SendPostRequest(request.Url, request.Content, headers);
                });
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
            }
        }

    }
}
