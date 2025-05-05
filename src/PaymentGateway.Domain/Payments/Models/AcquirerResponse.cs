using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Domain.Payments.Models
{
    public class AcquirerResponse
    {
        public bool Success { get; }
        public string? Reference { get; }
        public string? ErrorMessage { get; }
        
        public AcquirerResponse(bool success, string? reference, string? errorMessage = null)
        {
            Success = success;
            Reference = reference;
            ErrorMessage = errorMessage;
        }
    }
}