using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace PaymentGateway.Application.Authentication.Commands
{
    public class CreateMerchantCommandValidator : AbstractValidator<CreateMerchantCommand>
    {
        public CreateMerchantCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Merchant name is required")
                .MaximumLength(100).WithMessage("Merchant name cannot exceed 100 characters");
        }
    }
}