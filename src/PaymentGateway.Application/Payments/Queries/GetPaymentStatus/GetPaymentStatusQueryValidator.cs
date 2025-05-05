using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace PaymentGateway.Application.Payments.Queries.GetPaymentStatus
{
    public class GetPaymentStatusQueryValidator : AbstractValidator<GetPaymentStatusQuery>
    {
        public GetPaymentStatusQueryValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty()
                .WithMessage("Payment ID is required");
                
            RuleFor(x => x.MerchantId)
                .NotEmpty()
                .WithMessage("Merchant ID is required")
                .Must(id => id != Guid.Empty) // Add check for empty Guid
                .WithMessage("Merchant ID cannot be empty");
        }
    }
}