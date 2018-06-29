using NLog;
using System;
using System.Runtime.CompilerServices;

namespace DEXR.UnitTest
{
    public static class ApplicationLog
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public static void Info(string message)
        {
            logger.Info( message);
        }
    }
}