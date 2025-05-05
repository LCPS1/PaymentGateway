using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Common.Interfaces
{
    public interface ICurrentMerchantService
    {
        Guid? GetCurrentMerchantId();
        string? GetCurrentMerchantName();
        bool IsAuthenticated();
    }
}