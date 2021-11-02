using System;
using System.Collections.Generic;

namespace CMT.BL.Core
{
    public class ValidationException : Exception
    {

        public ValidationException(List<string> errors)
        {
            Errors = errors;
        }

        public List<string> Errors { get; set; }
    }
}
