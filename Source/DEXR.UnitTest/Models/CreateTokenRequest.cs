using System;

namespace DEXR.UnitTest.Model
{
    public class CreateTokenRequest
    {
        public CreateTokenRequestBody Body { get; set; }
        public string Signature { get; set; }
    }
    public class CreateTokenRequestBody
    {
        public long Nonce { get; set; }
        public string Fee { get; set; }

        public string Symbol { get; set; }
        public string Name { get; set; }
        public long TotalSupply { get; set; }
        public short Decimals { get; set; }
        public string Owner { get; set; }
    }
}
