using DEXR.Core.Configuration;
using DEXR.Core.Services;
using System;
using DEXR.Core.Model;
using DEXR.Core.Const;
using System.Linq;

namespace DEXR.Core.Controller
{
    public class ViewerController
    {
        public GenericResponse GetChainHeight()
        {
            try
            {
                ViewerServices viewerService = new ViewerServices();
                int chainHeight = viewerService.GetChainHeight();

                return new GenericResponse(chainHeight, ResponseCodes.Success, ResponseMessages.Success);
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

        public GenericResponse GetPending()
        {
            try
            {
                ViewerServices viewerService = new ViewerServices();
                var pending = viewerService.GetPending();

                return new GenericResponse(pending, ResponseCodes.Success, ResponseMessages.Success);
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
        
        public GenericResponse GetBlocks(int fromIndex, int toIndex)
        {
            try
            {
                if (toIndex < fromIndex)
                    return new GenericResponse(null, ResponseCodes.ViewBlocksInvalidIndex, ResponseMessages.ViewBlocksInvalidIndex);

                if ((toIndex - fromIndex) > 100)
                    return new GenericResponse(null, ResponseCodes.ViewBlocksTooMany, ResponseMessages.ViewBlocksTooMany);

                ViewerServices viewerService = new ViewerServices();
                var blocks = viewerService.GetBlocks(fromIndex, toIndex);

                return new GenericResponse(blocks, ResponseCodes.Success, ResponseMessages.Success);
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
        
        public GenericResponse GetAllTokens()
        {
            try
            {
                IndexServices indexService = new IndexServices();
                var index = indexService.TokenIndex.GetAll();

                return new GenericResponse(index, ResponseCodes.Success, ResponseMessages.Success);
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
        }
        
        public GenericResponse GetWalletBalance(string address, string tokenSymbol)
        {
            try
            {
                ViewerServices viewerServices = new ViewerServices();

                decimal? balance = viewerServices.GetWalletBalance(address, tokenSymbol);

                return new GenericResponse(balance, ResponseCodes.Success, ResponseMessages.Success);
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

        public GenericResponse GetOrders(string tradingPair)
        {
            try
            {
                IndexServices indexServices = new IndexServices();
                var orders = indexServices.OrderIndex.GetByTradingPair(tradingPair);
                if(orders != null)
                {
                    orders = orders.OrderByDescending(x => x.Price).ToList();
                    return new GenericResponse(orders, ResponseCodes.Success, ResponseMessages.Success);
                }
                else
                {
                    return new GenericResponse(null, ResponseCodes.Success, ResponseMessages.Success);
                }
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

        public GenericResponse GetNetworkFee()
        {
            try
            {
                ViewerServices viewerService = new ViewerServices();
                long networkFee = viewerService.GetNetworkFee();

                return new GenericResponse(networkFee, ResponseCodes.Success, ResponseMessages.Success);
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

    }
}
