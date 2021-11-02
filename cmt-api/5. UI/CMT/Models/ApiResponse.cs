using System.Collections.Generic;

namespace CMT
{

    public class ApiResponse<T>
    {
        public T Result { get; set; }
        public bool IsSucceed { get; set; }
        public List<string> Errors { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(bool isSucceed, List<string> errors)
        {
            IsSucceed = isSucceed;
            Errors = errors;
        }

        public ApiResponse(bool isSucceed, List<string> errors, T result)
            : this(isSucceed, errors)
        {
            Result = result;
        }
    }
}
