using DEXR.Core.Configuration;
using DEXR.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DEXR.Core.Data.Index
{
    public class IndexOwner
    {
        private Database database;

        public IndexOwner(Database db)
        {
            database = db;
        }

        public void AddToIndex(string address)
        {
            if (string.IsNullOrEmpty(address))
                return;

            var ownerIndex = new OwnerIndexItem()
            {
                Address = address
            };
            
            database.SaveOwnerIndex(ownerIndex);
            IndexOwnerCache.AddOrUpdate(ownerIndex);
        }

        public void DeleteIndex()
        {
            database.DeleteOwnerIndex();
            IndexOwnerCache.Remove();
        }

        public OwnerIndexItem Get()
        {
            var cachedIndex = IndexOwnerCache.Get();
            if (cachedIndex != null)
                return cachedIndex;

            var index = database.GetOwnerIndex();
            if (index != null)
                IndexOwnerCache.AddOrUpdate(index);
            return index;
        }
    }
    public class OwnerIndexItem
    {
        public string Address { get; set; }
    }

    public static class IndexOwnerCache
    {
        private static OwnerIndexItem cache;

        public static void AddOrUpdate(OwnerIndexItem item)
        {
            cache = item;
        }

        public static OwnerIndexItem Get()
        {
            return cache;
        }

        public static void Remove()
        {
            cache = null;
        }
    }
}
