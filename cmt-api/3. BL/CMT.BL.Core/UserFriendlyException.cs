using System;
using System.Net;

namespace CMT.BL.Core
{
    public class UserFriendlyException : Exception
    {
        public UserFriendlyException()
        {

        }
        public UserFriendlyException(string message, HttpStatusCode httpErrorCode) : base(message)
        {
            HttpErrorCode = httpErrorCode;
        }

        public UserFriendlyException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        public HttpStatusCode HttpErrorCode { get; private set; }
    }
}
