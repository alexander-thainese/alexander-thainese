using CMT.BL.Core;
using NLog;
using System;
using System.Web;

namespace CMT.Handlers
{
    public class CMTLogger : ISystemLogger
    {

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public void LogAction(string message, Type caller)
        {
            LogEventInfo info = new LogEventInfo(LogLevel.Info, caller.Name, message);

            info.Properties["DateTime"] = DateTime.Now.ToUniversalTime();

            HttpContext requestContext = HttpContext.Current;
            if (requestContext != null)
            {
                info.Properties["UserAgentString"] = requestContext.Request.UserAgent;

                info.Properties["FormData"] = requestContext.Request.Params.ToString();
                info.Properties["UserName"] = requestContext.User == null ? string.Empty : requestContext.User.Identity.Name;
                info.Properties["VirtualPath"] = requestContext.Request.Path;
                info.Properties["QueryString"] = requestContext.Request.QueryString.ToString();

            }

            logger.Log(info);
        }


        public void LogError(Type caller, Exception e)
        {
            LogEventInfo info = new LogEventInfo(LogLevel.Error, caller.Name, e.Message);
            info.Exception = e;
            info.Properties["StackTrace"] = e.ToString();
            info.Properties["DateTime"] = DateTime.Now.ToUniversalTime();

            HttpContext requestContext = HttpContext.Current;
            if (requestContext != null)
            {
                info.Properties["UserAgentString"] = requestContext.Request.UserAgent;

                info.Properties["FormData"] = requestContext.Request.Params.ToString();
                info.Properties["UserName"] = requestContext.User == null ? string.Empty : requestContext.User.Identity.Name;
                info.Properties["VirtualPath"] = requestContext.Request.Path;
                info.Properties["QueryString"] = requestContext.Request.QueryString.ToString();

            }

            logger.Log(info);
        }
    }
}