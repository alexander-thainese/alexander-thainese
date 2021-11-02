using CMT.BL.Core;
using CMT.BO;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace CMT.Handlers
{
    public class CMTExceptionHandler : ExceptionHandler
    {
        private static CMTLogger logger = new CMTLogger();

        private class ErrorInformation
        {
            public string Message { get; set; }
            public DateTime ErrorDate { get; set; }
        }

        public override void Handle(ExceptionHandlerContext context)
        {

            //Access-Control-Allow-Origin
            if (context.Exception is UserFriendlyException)
            {
                UserFriendlyException exception = context.Exception as UserFriendlyException;
                HttpResponseMessage response = context.Request.CreateResponse(exception.HttpErrorCode, new ErrorInformation { Message = exception.Message, ErrorDate = DateTime.UtcNow });
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Result = new ResponseMessageResult(response);
            }
            else if (!ApplicationSettings.ReturnErrorMessage)
            {
                HttpResponseMessage response = context.Request.CreateResponse(HttpStatusCode.InternalServerError, new ErrorInformation { Message = "We apologize but an unexpected error occured. Please try again later.", ErrorDate = DateTime.UtcNow });
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Result = new ResponseMessageResult(response);
            }
            logger.LogError(GetType(), context.Exception);
            // LogEventInfo info = new LogEventInfo(LogLevel.Error, this.GetType().Name, context.Exception.Message);
            // info.Exception = context.Exception;
            // info.Properties["StackTrace"] = context.Exception.StackTrace;
            // info.Properties["DateTime"] = DateTime.Now.ToUniversalTime();

            // var requestContext = HttpContext.Current;
            // if (requestContext != null)
            // {
            //     info.Properties["UserAgentString"] = requestContext.Request.UserAgent;

            //     info.Properties["FormData"] = requestContext.Request.Params.ToString();
            //     info.Properties["UserName"] = requestContext.User == null ? string.Empty : requestContext.User.Identity.Name;
            //     info.Properties["VirtualPath"] = requestContext.Request.Path;
            //     info.Properties["QueryString"] = requestContext.Request.QueryString.ToString();

            // }

            //logger.Log(info);


        }
        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            return true;
        }
    }
}