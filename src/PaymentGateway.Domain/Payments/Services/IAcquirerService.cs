using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentGateway.Domain.Payments.Entities;
using PaymentGateway.Domain.Payments.Models;

namespace PaymentGateway.Domain.Payments.Services
{
   public interface IAcquirerService
    {
        Task<AcquirerResponse> ProcessPaymentAsync(Payment payment);
    }
}