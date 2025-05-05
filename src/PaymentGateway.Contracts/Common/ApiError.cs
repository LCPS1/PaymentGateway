using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaymentGateway.Contracts.Common
{
    public class ApiError
    {
        public string Message { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        
        public int StatusCode { get; set; }
    }
}