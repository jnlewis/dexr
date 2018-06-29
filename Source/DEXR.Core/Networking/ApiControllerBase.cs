using DEXR.Core.Model;
using DEXR.HttpServer.Models;
using Newtonsoft.Json;

namespace DEXR.Core.Networking
{
    public class ApiControllerBase
    {
        protected static HttpResponse CreateHttpResponse(object response)
        {
            var res = new HttpResponse()
            {
                ContentAsUTF8 = JsonConvert.SerializeObject(response),
                ReasonPhrase = "OK",
                StatusCode = "200"
            };

            res.Headers.Add("Content-Type", "application/json");

            return res;
        }

        protected static Node GetCallerNode(HttpRequest request)
        {
            if(request.Headers.ContainsKey("sender"))
            {
                return JsonConvert.DeserializeObject<Node>(request.Headers["sender"]);
            }
            else
            {
                return null;
            }
        }
    }
}
