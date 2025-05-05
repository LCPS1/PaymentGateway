using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentGateway.Domain.Payments.Entities;
using PaymentGateway.Domain.Payments.Repositories;
using Microsoft.EntityFrameworkCore;


namespace PaymentGateway.Infrastructure.Persistence.Repositories
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<Payment?> GetByIdempotencyKeyAsync(Guid merchantId, string idempotencyKey)
        {
            return await _context.Payments
                .Where(p => p.MerchantId == merchantId && p.IdempotencyKey == idempotencyKey)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Payment>> GetByMerchantIdAsync(Guid merchantId)
        {
            return await _context.Payments
                .Where(p => p.MerchantId == merchantId)
                .ToListAsync();
        }
    }
}