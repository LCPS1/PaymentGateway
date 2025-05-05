using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentGateway.Domain.Merchants.Repositories;
using PaymentGateway.Domain.Payments.Repositories;

namespace PaymentGateway.Domain.Core.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IPaymentRepository Payments { get; }
       IMerchantRepository Merchants { get; }
        
        Task<int> CompleteAsync();
    }
}