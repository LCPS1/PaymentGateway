using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaymentGateway.Contracts.Authentication.Requests
{
    public class CreateMerchantRequest
    {
        public string Name { get; init; } = string.Empty;
    }
}