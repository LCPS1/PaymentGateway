using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaymentGateway.Infrastructure.Payments.Models
{
    public class AcquirerPaymentResponse
    {
        public bool Success { get; set; }
        
        public string? Reference { get; set; }
        
        public string? ErrorMessage { get; set; }
    }
}