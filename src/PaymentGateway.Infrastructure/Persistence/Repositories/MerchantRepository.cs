using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentGateway.Domain.Merchants.Entities;
using PaymentGateway.Domain.Merchants.Repositories;
using Microsoft.EntityFrameworkCore;

namespace PaymentGateway.Infrastructure.Persistence.Repositories
{
    public class MerchantRepository : Repository<Merchant>, IMerchantRepository
    {
        public MerchantRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Merchant?> GetByApiKeyAsync(string apiKey)
        {
            return await _context.Merchants
                .Where(m => m.ApiKey == apiKey)
                .FirstOrDefaultAsync();
        }
        
        public async Task<bool> ExistsAsync(string apiKey)
        {
            return await _context.Merchants
                .AnyAsync(m => m.ApiKey == apiKey);
        }
    }
}