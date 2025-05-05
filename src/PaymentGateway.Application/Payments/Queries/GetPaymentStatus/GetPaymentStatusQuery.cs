using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using MediatR;
using PaymentGateway.Contracts.Payments.Responses;

namespace PaymentGateway.Application.Payments.Queries.GetPaymentStatus
{
    public record GetPaymentStatusQuery : IRequest<ErrorOr<PaymentStatusResponse>>
    {
        public Guid PaymentId { get; init; }
        public Guid MerchantId { get; init; } 
    }

}