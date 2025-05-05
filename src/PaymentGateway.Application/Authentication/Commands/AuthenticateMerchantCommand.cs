using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using MediatR;
using PaymentGateway.Contracts.Authentication.Responses;

namespace PaymentGateway.Application.Authentication.Commands
{
   public record AuthenticateMerchantCommand : IRequest<ErrorOr<AuthenticationResponse>>
    {
        public string ApiKey { get; init; } = string.Empty;
        
        public string ApiSecret { get; init; } = string.Empty;
    }
}