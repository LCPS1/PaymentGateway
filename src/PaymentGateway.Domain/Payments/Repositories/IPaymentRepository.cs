using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentGateway.Domain.Core.Repositories;
using PaymentGateway.Domain.Payments.Entities;

namespace PaymentGateway.Domain.Payments.Repositories
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<Payment?> GetByIdempotencyKeyAsync(Guid merchantId, string idempotencyKey);
        Task<IEnumerable<Payment>> GetByMerchantIdAsync(Guid merchantId);
    }
}