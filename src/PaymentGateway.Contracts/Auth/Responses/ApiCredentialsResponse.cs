using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Contracts.Authentication.Responses
{
    public class ApiCredentialsResponse
    {
    public string ApiKey { get; init; } = string.Empty;

    public string ApiSecret { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
    }
}