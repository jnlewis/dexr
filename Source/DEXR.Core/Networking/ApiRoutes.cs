using DEXR.HttpServer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DEXR.Core.Networking
{
    internal static class ApiRoutes
    {
        public static List<Route> GET
        {
            get
            {
                return new List<Route>()
                {
                    //new Route()
                    //{
                    //    Callable = ApiController.HomeIndex,
                    //    UrlRegex = "^\\/$",
                    //    Method = "GET"
                    //},
                    new Route() { Method = "GET",  Url = "/", Callable = ApiController.HomeIndex },
                    new Route() { Method = "POST", Url = "/sign", Callable = ApiController.Sign },
                    new Route() { Method = "GET",  Url = "/view/height", Callable = ApiController.ViewChainHeight },
                    new Route() { Method = "GET",  Url = "/view/pending-transactions", Callable = ApiController.ViewPending },
                    new Route() { Method = "POST", Url = "/view/blocks", Callable = ApiController.ViewBlocks },
                    new Route() { Method = "GET",  Url = "/view/tokens", Callable = ApiController.ViewAllTokens },
                    new Route() { Method = "POST", Url = "/view/orders", Callable = ApiController.ViewOrders },
                    new Route() { Method = "POST", Url = "/view/wallet-balance", Callable = ApiController.ViewWalletBalance },
                    new Route() { Method = "GET",  Url = "/view/network-fee", Callable = ApiController.ViewNetworkFee },
                    new Route() { Method = "POST", Url = "/transaction/create-token", Callable = ApiController.AddToken },
                    new Route() { Method = "POST", Url = "/transaction/transfer", Callable = ApiController.AddTransfer },
                    new Route() { Method = "POST", Url = "/transaction/limit-order", Callable = ApiController.AddOrderLimit },
                    new Route() { Method = "POST", Url = "/transaction/market-order", Callable = ApiController.AddOrderMarket },
                    new Route() { Method = "POST", Url = "/transaction/cancel-order", Callable = ApiController.CancelOrder },
                    new Route() { Method = "GET",  Url = "/consensus/nodes", Callable = ApiController.GetNodes },
                    new Route() { Method = "POST", Url = "/consensus/announce-register-node", Callable = ApiController.RegisterNodeAnnouncement },
                    new Route() { Method = "POST", Url = "/consensus/announce-new-block", Callable = ApiController.NewBlockAnnouncement }
                };
            }
        }
        
    }
}
