using DEXR.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace DEXR.Core.Data.Index
{
    public class IndexToken
    {
        private Database database;

        public IndexToken(Database db)
        {
            database = db;
        }

        public void AddToIndex(TransactionToken record)
        {
            if (record == null)
                return;

            var tokenIndex = new TokenIndexItem()
            {
                Name = record.Name,
                Symbol = record.Symbol,
                Decimals = record.Decimals
            };
            
            database.SaveTokenIndex(tokenIndex);
            IndexTokenCache.AddOrUpdate(tokenIndex);

            //Update key map
            List<string> symbols = database.GetKeyMap("token");
            if(symbols == null)
            {
                //First token in block is native
                database.SaveNativeTokenIndex(tokenIndex);

                symbols = new List<string>();
                symbols.Add(record.Symbol);
                database.SaveKeyMap("token", symbols);
            }
            else
            {
                if(!symbols.Contains(record.Symbol))
                {
                    symbols.Add(record.Symbol);
                    database.SaveKeyMap("token", symbols);
                }
            }
        }

        public void DeleteIndex(TransactionToken record)
        {
            database.DeleteTokenIndex(record.Symbol);
            database.DeleteKeyMap("token");
            IndexTokenCache.Remove(record.Symbol);
        }

        public List<TokenIndexItem> GetAll()
        {
            List<TokenIndexItem> result = new List<TokenIndexItem>();

            List<string> symbols = database.GetKeyMap("token");
            if(symbols != null)
            {
                foreach (var symbol in symbols)
                {
                    result.Add(Get(symbol));
                }
            }

            return result;
        }

        public TokenIndexItem Get(string symbol)
        {
            var cachedIndex = IndexTokenCache.Get(symbol);
            if (cachedIndex != null)
                return cachedIndex;

            var index = database.GetTokenIndex(symbol);
            if(index != null)
                IndexTokenCache.AddOrUpdate(index);
            return index;
        }

        public TokenIndexItem GetNative()
        {
            var cachedIndex = IndexNativeTokenCache.Get();
            if (cachedIndex != null)
                return cachedIndex;

            var index = database.GetNativeTokenIndex();
            if (index != null)
                IndexNativeTokenCache.AddOrUpdate(index);
            return index;
        }
    }
    public class TokenIndexItem
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public short Decimals { get; set; }
    }

    public static class IndexTokenCache
    {
        private static List<TokenIndexItem> cache;

        public static void AddOrUpdate(TokenIndexItem item)
        {
            if (cache == null)
            {
                cache = new List<TokenIndexItem>();
                cache.Add(item);
            }
            else
            {
                var find = cache.Where(x =>
                    x.Symbol == item.Symbol)
                    .FirstOrDefault();

                if (find == null)
                {
                    cache.Add(item);
                }
                else
                {
                    cache.Remove(find);
                    cache.Add(item);
                }
            }
        }

        public static TokenIndexItem Get(string symbol)
        {
            if (cache == null)
                return null;

            return cache.Where(x =>
                    x.Symbol == symbol)
                    .FirstOrDefault();
        }

        public static void Remove(string symbol)
        {
            var find = cache.Where(x =>
                    x.Symbol == symbol)
                    .FirstOrDefault();

            if (find != null)
            {
                cache.Remove(find);
            }
        }
    }

    public static class IndexNativeTokenCache
    {
        private static TokenIndexItem cache;

        public static void AddOrUpdate(TokenIndexItem item)
        {
            cache = item;
        }

        public static TokenIndexItem Get()
        {
            return cache;
        }

        public static void Remove()
        {
            cache = null;
        }
    }
}
