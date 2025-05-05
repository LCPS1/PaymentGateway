using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace PaymentGateway.Application.Authentication.Commands
{
   public class AuthenticateMerchantCommandValidator : AbstractValidator<AuthenticateMerchantCommand>
    {
        public AuthenticateMerchantCommandValidator()
        {
            RuleFor(x => x.ApiKey)
                .NotEmpty().WithMessage("API key is required")
                .MaximumLength(100).WithMessage("API key cannot exceed 100 characters");
                
            RuleFor(x => x.ApiSecret)
                .NotEmpty().WithMessage("API secret is required")
                .MaximumLength(200).WithMessage("API secret cannot exceed 200 characters");
        }
    }
}