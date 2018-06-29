using System;
using System.Threading;
using DEXR.Core.Networking;
using DEXR.HttpServer;

namespace DEXR.Core
{
    internal class Server
    {
        public int Port { get; }
        public string State { get; set; }

        public Server(int port)
        {
            this.Port = port;
        }

        public void Start()
        {
            HttpWebServer httpServer = new HttpWebServer(this.Port, ApiRoutes.GET);

            Thread thread = new Thread(new ThreadStart(httpServer.Listen));
            thread.Start();
        }
    }
}
