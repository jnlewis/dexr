using NLog;
using System;
using System.Runtime.CompilerServices;

namespace DEXR.Core
{
    public static class ApplicationLog
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static string ConsoleTimestamp
        {
            get { return "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]"; }
        }

        private static void Console(string message)
        {
            System.Console.WriteLine(ConsoleTimestamp + " " + message);
        }

        public static void Trace(string message, [CallerMemberName] string methodName = "")
        {
            logger.Trace("(" + methodName + ") " + message);
        }

        public static void Debug(string message, [CallerMemberName] string methodName = "")
        {
            logger.Debug("(" + methodName + ") " + message);
        }

        public static void Info(string message, [CallerMemberName] string methodName = "")
        {
            Console("(" + methodName + ") " + message);
            logger.Info("(" + methodName + ") " + message);
        }
        
        public static void Warn(string message, [CallerMemberName] string methodName = "")
        {
            Console("(" + methodName + ") " + message);
            logger.Warn("(" + methodName + ") " + message);
        }

        public static void Error(string message, [CallerMemberName] string methodName = "")
        {
            Console("(" + methodName + ") " + message);
            logger.Error("(" + methodName + ") " + message);
        }

        public static void Exception(Exception ex, [CallerMemberName] string methodName = "")
        {
            Error(ex.Message, methodName);

            Exception innerEx = ex.InnerException;
            while(innerEx != null)
            {
                Error("InnerException:" + innerEx.Message, methodName);
                innerEx = innerEx.InnerException;
            }
        }
    }
}