using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LevelDB;

namespace DEXR.Core.Data
{
    public class DBClient
    {
        private string dbFolder;

        public DBClient(string folderName)
        {
            dbFolder = folderName;
        }
        
        public void Put(string key, string value)
        {
            using (var database = new DB(dbFolder, new Options() { CreateIfMissing = true, BloomFilter = new BloomFilterPolicy(10) }))
            {
                database.Put(key, value);
            }
        }

        public string Get(string key)
        {
            using (var database = new DB(dbFolder, new Options() { CreateIfMissing = true, BloomFilter = new BloomFilterPolicy(10) }))
            {
                return database.Get(key);
            }
        }

        public void Delete(string key)
        {
            using (var database = new DB(dbFolder, new Options() { CreateIfMissing = true, BloomFilter = new BloomFilterPolicy(10) }))
            {
                database.Delete(key);
            }
        }
    }
}
