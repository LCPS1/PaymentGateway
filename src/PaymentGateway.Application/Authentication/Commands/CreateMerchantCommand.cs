using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using MediatR;
using PaymentGateway.Contracts.Authentication.Responses;

namespace PaymentGateway.Application.Authentication.Commands
{
    public record CreateMerchantCommand : IRequest<ErrorOr<CreateMerchantResponse>>
    {
        public string Name { get; init; } = string.Empty;
    }
}
