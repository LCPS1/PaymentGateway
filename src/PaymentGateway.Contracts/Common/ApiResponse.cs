using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaymentGateway.Contracts.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }     
        public ApiError? Error { get; set; }
        
        public static ApiResponse<T> SuccessResponse(T data)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data
            };
        }
        
        public static ApiResponse<T> ErrorResponse(string message, string code, int statusCode)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Error = new ApiError
                {
                    Message = message,
                    Code = code,
                    StatusCode = statusCode
                }
            };
        }    
    }
}