using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Domain.Payments.Events
{
    public record PaymentSucceededEvent(Guid PaymentId, Guid MerchantId, string? AcquirerReference);
}