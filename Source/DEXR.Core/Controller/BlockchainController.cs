using DEXR.Core.Configuration;
using DEXR.Core.Const;
using DEXR.Core.Model;
using DEXR.Core.Services;
using System;

namespace DEXR.Core.Controller
{
    public class BlockchainController : ControllerBase
    {
        public GenericResponse CreateNewChain(string ownerAddress)
        {
            try
            {
                DataServices dataService = new DataServices();

                if (dataService.HasLocalChainData())
                    throw new Exception("Chain folder is not empty. Please delete the existing chain folder first before creating a new chain.");

                ApplicationLog.Info("Creating new chain...");

                dataService.CreateGenesisBlock(
                    ownerAddress,
                    Settings.NewChainSetup.NativeTokenName,
                    Settings.NewChainSetup.NativeTokenSymbol,
                    Settings.NewChainSetup.InitialSupply,
                    Settings.NewChainSetup.Decimals);

                ApplicationLog.Info("Chain created successfully.");

                return new GenericResponse("Chain created successfully.", ResponseCodes.Success, ResponseMessages.Success);
            }
            catch (ValidationException vex)
            {
                ApplicationLog.Warn("ValidationException [" + vex.Code + "]: " + vex.Message);
                return new GenericResponse(null, vex.Code, vex.Message);
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
        }

        public GenericResponse DeleteLocalChain()
        {
            try
            {
                DataServices dataService = new DataServices();

                if(dataService.HasLocalChainData())
                    dataService.DeleteLocalChain();
                
                ApplicationLog.Info("Local chain data deleted successfully.");

                return new GenericResponse(null, ResponseCodes.Success, ResponseMessages.Success);
            }
            catch (ValidationException vex)
            {
                ApplicationLog.Warn("ValidationException [" + vex.Code + "]: " + vex.Message);
                return new GenericResponse(null, vex.Code, vex.Message);
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
        }
        
        public GenericResponse GenerateWalletAddress()
        {
            try
            {
                DataServices dataService = new DataServices();
                var result = dataService.GenerateWalletAddress();

                return new GenericResponse(result, ResponseCodes.Success, ResponseMessages.Success);
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
        }

        public GenericResponse RebuildIndex()
        {
            try
            {
                IndexServices indexServices = new IndexServices();
                indexServices.DeleteIndexForAllBlocks();
                indexServices.UpdateIndexForAllBlocks();

                return new GenericResponse(null, ResponseCodes.Success, ResponseMessages.Success);
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
        }
    }
}
