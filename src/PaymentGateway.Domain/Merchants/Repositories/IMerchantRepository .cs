using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentGateway.Domain.Core.Repositories;
using PaymentGateway.Domain.Merchants.Entities;

namespace PaymentGateway.Domain.Merchants.Repositories
{
    public interface IMerchantRepository : IRepository<Merchant>
   {
        Task<Merchant?> GetByApiKeyAsync(string apiKey);
        Task<bool> ExistsAsync(string apiKey);
   }
}