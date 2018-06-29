using DEXR.Core.Configuration;
using DEXR.Core.Services;
using System;
using DEXR.Core.Model;
using DEXR.Core.Const;
using DEXR.Core.Cryptography;

namespace DEXR.Core.Controller
{
    public class GenericController
    {
        public GenericResponse GetWelcome()
        {
            try
            {
                return new GenericResponse(null, ResponseCodes.Success, "Welcome to DEXR. Please view documentation for API usage.");
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
        }

        public GenericResponse Sign(string privateKey, string content)
        {
            try
            {
                string signature = KeySignature.Sign(privateKey, content);
                
                return new GenericResponse(signature, ResponseCodes.Success, ResponseMessages.Success);
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
        }
    }
}
