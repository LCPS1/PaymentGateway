using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaymentGateway.Contracts.Authentication.Requests
{
    public class AuthenticationRequest
    {
        public string ApiKey { get; init; } = string.Empty;
        
        public string ApiSecret { get; init; } = string.Empty;
    }
}