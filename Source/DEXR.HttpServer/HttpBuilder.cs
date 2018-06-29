using DEXR.HttpServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEXR.HttpServer
{
    class HttpBuilder
    {
        public static HttpResponse InternalServerError()
        {
            return new HttpResponse()
            {
                ReasonPhrase = "InternalServerError",
                StatusCode = "500",
                ContentAsUTF8 = "{ \"Message\" : \"Internal Server Error\" }"
            };
        }

        public static HttpResponse NotFound()
        {
            return new HttpResponse()
            {
                ReasonPhrase = "NotFound",
                StatusCode = "404",
                ContentAsUTF8 = "{ \"Message\" : \"Endpoint Not Found\" }"
            };
        }
    }
}
