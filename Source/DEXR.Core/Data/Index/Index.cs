using System;
using System.Collections.Generic;

namespace DEXR.Core.Data.Index
{
    public class Index
    {
        protected Database database;

        protected void Init(Database db)
        {
            database = db;
        }

        protected void AddToKeyMap(string mapName, string value)
        {
            List<string> keys = database.GetKeyMap(mapName);
            if (keys == null)
            {
                keys = new List<string>();
                keys.Add(value);
                database.SaveKeyMap(mapName, keys);
            }
            else
            {
                if (!keys.Contains(value))
                {
                    keys.Add(value);
                    database.SaveKeyMap(mapName, keys);
                }
            }
        }

    }
}
