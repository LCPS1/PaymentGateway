using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Infrastructure.Payments.Options
{
    public class AcquirerSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string PaymentEndpoint { get; set; } = "api/payments";
        public string ApiKey { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
    }
}