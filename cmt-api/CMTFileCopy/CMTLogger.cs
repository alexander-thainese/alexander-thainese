
using NLog;
using System;

namespace CMTFileCopy
{
    public class CMTLogger
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        public void LogAction(string message, Type caller)
        {
            LogEventInfo info = new LogEventInfo(LogLevel.Info, caller.Name, message);

            info.Properties["DateTime"] = DateTime.Now.ToUniversalTime();
            logger.Log(info);
        }


        public void LogError(Type caller, Exception e)
        {
            LogEventInfo info = new LogEventInfo(LogLevel.Error, caller.Name, e.Message);
            info.Exception = e;
            info.Properties["StackTrace"] = e.ToString();
            info.Properties["DateTime"] = DateTime.Now.ToUniversalTime();

            logger.Log(info);
        }
    }
}