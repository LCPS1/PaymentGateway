using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace PaymentGateway.Application.Authentication.Queries
{
    public class GetMerchantInfoQueryValidator : AbstractValidator<GetMerchantInfoQuery>
    {
        public GetMerchantInfoQueryValidator()
        {
            RuleFor(x => x.MerchantId)
                .NotEmpty().WithMessage("Merchant ID is required")
                .Must(id => id != Guid.Empty).WithMessage("Merchant ID cannot be empty");
        }
    }
}