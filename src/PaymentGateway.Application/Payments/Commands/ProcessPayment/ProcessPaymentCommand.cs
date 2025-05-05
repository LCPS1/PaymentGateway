using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using MediatR;
using PaymentGateway.Contracts.Payments.Responses;

namespace PaymentGateway.Application.Payments.Commands.ProcessPayment
{
    public record ProcessPaymentCommand : IRequest<ErrorOr<PaymentResponse>>
    {
        public Guid MerchantId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string CardNumber { get; init; } = string.Empty;
        public string CardHolderName { get; init; } = string.Empty;
        public int ExpiryMonth { get; init; }
        public int ExpiryYear { get; init; }
        public string CVV { get; init; } = string.Empty;
        public string? IdempotencyKey { get; init; }

    }
}