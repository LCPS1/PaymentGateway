using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Domain.Payments.Events
{
    public record PaymentFailedEvent(Guid PaymentId, Guid MerchantId);
}