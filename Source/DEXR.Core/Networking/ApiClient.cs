using DEXR.Core.Const;
using DEXR.Core.Model;
using DEXR.Core.Model.Requests;
using DEXR.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace DEXR.Core.Networking
{
    public class ApiClient
    {
        private string host;

        public ApiClient(string hostUrl)
        {
            host = hostUrl;
        }

        public List<Node> GetNodes()
        {
            try
            {
                string endpoint = host + "/consensus/nodes";
                string responseData = SendGetRequest(endpoint);
                if (!string.IsNullOrEmpty(responseData))
                {
                    return JsonConvert.DeserializeObject<List<Node>>(responseData);
                }
                else
                {
                    return new List<Node>();
                }
            }
            catch
            {
                return null;
            }
        }

        public int GetChainHeight()
        {
            try
            {
                string endpoint = host + "/view/height";
                string responseData = SendGetRequest(endpoint);
                if (!string.IsNullOrEmpty(responseData))
                {
                    return JsonConvert.DeserializeObject<int>(responseData);
                }
                else
                {
                    throw new Exception("GetChainHeight: Response data is empty.");
                }
            }
            catch
            {
                FlagNodeAsDisconnected();
                throw;
            }
        }

        public List<Block> GetBlocks(int fromIndex, int toIndex)
        {
            try
            {
                ViewBlocksRequest data = new ViewBlocksRequest()
                {
                    FromIndex = fromIndex,
                    ToIndex = toIndex
                };

                string endpoint = host + "/view/blocks";
                string responseData = SendPostRequest(endpoint, JsonConvert.SerializeObject(data));
                if (!string.IsNullOrEmpty(responseData))
                {
                    return JsonConvert.DeserializeObject<List<Block>>(responseData);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        
        public void AnnounceRegisterNode(string serverAddress, string walletAddress, string signature)
        {
            try
            {
                AnnounceRegisterNodeRequest data = new AnnounceRegisterNodeRequest()
                {
                    ServerAddress = serverAddress,
                    WalletAddress = walletAddress,
                    Signature = signature
                };

                string endpoint = host + "/consensus/announce-register-node";
                string responseData = this.SendPostRequest(endpoint, JsonConvert.SerializeObject(data));
            }
            catch
            {
            }
        }
        
        public void AnnounceNewBlock(BlockHeader blockHeader)
        {
            try
            {
                AnnounceNewBlockRequest data = new AnnounceNewBlockRequest()
                {
                    NewBlockHeader = blockHeader
                };

                string endpoint = host + "/consensus/announce-new-block";
                string responseData = this.SendPostRequest(endpoint, JsonConvert.SerializeObject(data, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
            }
            catch
            {
            }
        }

        #region Generic Methods
        
        private void FlagNodeAsDisconnected()
        {
            var node = ApplicationState.ConnectedNodes.Where(x => x.ServerAddress == host).FirstOrDefault();
            if (node != null)
                ApplicationState.DisconnectedNodes.Add(node);
        }

        public string SendGetRequest(string endpoint)
        {
            string jsonResponse = string.Empty;
            string result = null;

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.Accept] = "application/json";
                    client.Headers["sender"] = JsonConvert.SerializeObject(ApplicationState.Node);   //TODO: Future enhancement: sign sender to prevent impersonation from other nodes
                    jsonResponse = client.DownloadString(new Uri(endpoint));
                }

                var response = JsonConvert.DeserializeObject<GenericResponse>(jsonResponse);
                if (response.Code != ResponseCodes.Success)
                {
                    throw new Exception(response.Code + ":" + response.Message);
                }
                else
                {
                    if (response.Data != null)
                        result = response.Data.ToString();
                }

                return result;
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                throw;
            }
        }

        public string SendPostRequest(string endpoint, string data, Dictionary<string, string> additionalHeaders = null)
        {
            string jsonResponse = string.Empty;
            string result = null;

            try
            {
                using (WebClient client = new WebClient())
                {
                    //Create headers
                    client.Headers[HttpRequestHeader.Accept] = "application/json";
                    client.Headers["sender"] = JsonConvert.SerializeObject(ApplicationState.Node); //TODO: Future enhancement: sign sender to prevent impersonation from other nodes

                    if (additionalHeaders != null)
                    {
                        foreach(var item in additionalHeaders)
                        {
                            client.Headers[item.Key] = item.Value;
                        }
                    }

                    //Post Request
                    jsonResponse = client.UploadString(new Uri(endpoint), data);
                }

                var response = JsonConvert.DeserializeObject<GenericResponse>(jsonResponse);
                if (response.Code != ResponseCodes.Success)
                {
                    throw new Exception(response.Code + ":" + response.Message);
                }
                else
                {
                    if (response.Data != null)
                        result = response.Data.ToString();
                }

                return result;
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                throw;
            }
        }
        
        #endregion

    }
}
