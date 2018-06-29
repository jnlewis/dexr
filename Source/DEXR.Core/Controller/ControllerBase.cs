using DEXR.Core.Configuration;
using DEXR.Core.Const;
using DEXR.Core.Cryptography;
using Newtonsoft.Json;

namespace DEXR.Core.Controller
{
    public class ControllerBase
    {
        protected void VerifySignature(object content, string signature, string publicKey)
        {
            if(ConstantConfig.EnableSignatureVerification)
            {
                bool isValidSignature = KeySignature.VerifySignature(publicKey, JsonConvert.SerializeObject(content), signature);
                if (!isValidSignature)
                {
                    throw new ValidationException(ResponseCodes.InvalidSignature, ResponseMessages.InvalidSignature);
                }
            }
        }

    }
}
