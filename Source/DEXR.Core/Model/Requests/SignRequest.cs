using System;

namespace DEXR.Core.Model.Requests
{
    public class SignRequest
    {
        public string PrivateKey { get; set; }
        public object Content { get; set; }
    }
}
