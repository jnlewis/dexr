using System;
using System.Collections.Generic;
using DEXR.Core.Models;
using Newtonsoft.Json;
using DEXR.Core.Data.Index;

namespace DEXR.Core.Data
{
    public class Database
    {
        //private DBClient db;

        public Database(string folderName)
        {
            //db = new DBClient(folderName);

            if (ApplicationState.DBClient == null)
                ApplicationState.DBClient = new DBClient(folderName);
        }

        private void Put(string key, string value)
        {
            lock(ApplicationState.DBClient)
            {
                ApplicationState.DBClient.Put(key, value);
            }
        }

        private void Delete(string key)
        {
            lock (ApplicationState.DBClient)
            {
                ApplicationState.DBClient.Delete(key);
            }
        }

        private string Get(string key)
        {
            lock (ApplicationState.DBClient)
            {
                return ApplicationState.DBClient.Get(key);
            }
        }

        #region Save

        public void SaveSystemSettings(string key, string value)
        {
            Put(DBKeyPrefix.System + key, value);
        }

        public void SaveBlock(Block value)
        {
            Put(DBKeyPrefix.Block + value.Header.Index.ToString(), JsonConvert.SerializeObject(value, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));

            SaveSystemSettings("LastBlockIndex", value.Header.Index.ToString());
        }

        public void SaveOwnerIndex(OwnerIndexItem value)
        {
            Put(DBKeyPrefix.IndexOwner, JsonConvert.SerializeObject(value));
        }
        public void SaveNativeTokenIndex(TokenIndexItem value)
        {
            Put(DBKeyPrefix.IndexNative, JsonConvert.SerializeObject(value));
        }
        public void SaveTokenIndex(TokenIndexItem value)
        {
            Put(DBKeyPrefix.IndexToken + value.Symbol, JsonConvert.SerializeObject(value));
        }
        public void SaveOrderIndex(OrderIndexItem value)
        {
            Put(DBKeyPrefix.IndexOrder + value.TransactionId, JsonConvert.SerializeObject(value));
        }
        public void SaveBalanceIndex(BalanceIndexItem value)
        {
            Put(DBKeyPrefix.IndexBalance + value.Address + "_" + value.TokenSymbol, JsonConvert.SerializeObject(value));
        }
        public void SaveKeyMap(string type, List<string> keys)
        {
            Put(DBKeyPrefix.KeyMap + type, JsonConvert.SerializeObject(keys));
        }

        public void AddToKeyMap(string mapName, string value)
        {
            List<string> keys = GetKeyMap(mapName);
            if (keys == null)
            {
                keys = new List<string>();
                keys.Add(value);
                SaveKeyMap(mapName, keys);
            }
            else
            {
                if (!keys.Contains(value))
                {
                    keys.Add(value);
                    SaveKeyMap(mapName, keys);
                }
            }
        }

        public void UpdateToKeyMap(string mapName, string value)
        {
            List<string> keys = GetKeyMap(mapName);
            if (keys != null &&
                keys.Contains(value))
            {
                keys.Remove(value);
                SaveKeyMap(mapName, keys);
            }
        }

        #endregion

        #region Delete

        public void DeleteOwnerIndex()
        {
            Delete(DBKeyPrefix.IndexOwner);
        }
        public void DeleteTokenIndex(string symbol)
        {
            Delete(DBKeyPrefix.IndexToken + symbol);
        }
        public void DeleteOrderIndex(string transactionId)
        {
            Delete(DBKeyPrefix.IndexOrder + transactionId);
        }
        public void DeleteBalanceIndex(string address, string symbol)
        {
            Delete(DBKeyPrefix.IndexBalance + address + "_" + symbol);
        }
        public void DeleteKeyMap(string type)
        {
            Delete(DBKeyPrefix.KeyMap + type);
        }

        #endregion

        #region Retrieve

        public int GetLastBlockIndex()
        {
            var value = GetSystemSettings("LastBlockIndex");
            if (value == null)
                return -1;
            else
                return Convert.ToInt32(value);
        }

        public string GetSystemSettings(string key)
        {
            return Get(DBKeyPrefix.System + key);
        }

        public Block GetBlock(int index)
        {
            var value = Get(DBKeyPrefix.Block + index);
            if (value == null)
            {
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject<Block>(value, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            }
        }

        public OwnerIndexItem GetOwnerIndex()
        {
            var value = Get(DBKeyPrefix.IndexOwner);
            if (value == null)
                return null;
            else
                return JsonConvert.DeserializeObject<OwnerIndexItem>(value);
        }

        public TokenIndexItem GetNativeTokenIndex()
        {
            var value = Get(DBKeyPrefix.IndexNative);
            if (value == null)
                return null;
            else
                return JsonConvert.DeserializeObject<TokenIndexItem>(value);
        }

        public TokenIndexItem GetTokenIndex(string symbol)
        {
            var value = Get(DBKeyPrefix.IndexToken + symbol);
            if (value == null)
                return null;
            else
                return JsonConvert.DeserializeObject<TokenIndexItem>(value);
        }

        public OrderIndexItem GetOrderIndex(string transactionId)
        {
            var value = Get(DBKeyPrefix.IndexOrder + transactionId);
            if (value == null)
                return null;
            else
                return JsonConvert.DeserializeObject<OrderIndexItem>(value);
        }

        public BalanceIndexItem GetBalanceIndex(string address, string tokenCode)
        {
            var value = Get(DBKeyPrefix.IndexBalance + address + "_" + tokenCode);
            if (value == null)
                return null;
            else
                return JsonConvert.DeserializeObject<BalanceIndexItem>(value);
        }
        
        public List<string> GetKeyMap(string type)
        {
            var value = Get(DBKeyPrefix.KeyMap + type);
            if (value == null)
                return null;
            else
                return JsonConvert.DeserializeObject<List<string>>(value);
        }

        #endregion
    }
}
