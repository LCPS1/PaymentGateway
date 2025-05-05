using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using MediatR;
using PaymentGateway.Contracts.Authentication.Responses;

namespace PaymentGateway.Application.Authentication.Queries
{
    public record GetMerchantInfoQuery : IRequest<ErrorOr<MerchantInfoResponse>>
    {
        public Guid MerchantId { get; init; } 
    }
}