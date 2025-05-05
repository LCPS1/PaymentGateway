using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace PaymentGateway.Application.Payments.Commands.ProcessPayment
{
    public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
    {
        public ProcessPaymentCommandValidator()
        {
            RuleFor(x => x.MerchantId)
                .NotEmpty()
                .WithMessage("Merchant ID is required");
                
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero");
                
            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required")
                .Length(3)
                .WithMessage("Currency code must be 3 characters (ISO 4217 format)")
                .Matches("^[A-Z]{3}$")
                .WithMessage("Currency must be in ISO 4217 format (e.g., USD, EUR, GBP)");
                
            RuleFor(x => x.CardNumber)
                .NotEmpty()
                .WithMessage("Card number is required")
                .Matches(@"^[0-9]{13,19}$")
                .WithMessage("Card number must be between 13 and 19 digits");
                
            RuleFor(x => x.CardHolderName)
                .NotEmpty()
                .WithMessage("Cardholder name is required")
                .MaximumLength(100)
                .WithMessage("Cardholder name cannot exceed 100 characters");
                
            RuleFor(x => x.ExpiryMonth)
                .InclusiveBetween(1, 12)
                .WithMessage("Expiry month must be between 1 and 12");
                
            RuleFor(x => x.ExpiryYear)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Year)
                .WithMessage("Expiry year must be current or future year")
                .LessThanOrEqualTo(DateTime.UtcNow.Year + 20)
                .WithMessage("Expiry year cannot be more than 20 years in the future");
                
            RuleFor(x => x.CVV)
                .NotEmpty()
                .WithMessage("CVV is required")
                .Matches(@"^\d{3,4}$")
                .WithMessage("CVV must be 3 or 4 digits");
                
            RuleFor(x => x.IdempotencyKey)
                .MaximumLength(100)
                .WithMessage("Idempotency key cannot exceed 100 characters");
                
            // Validate expiry date is not in the past
            RuleFor(x => x)
                .Must(cmd => !IsExpiryDateInPast(cmd.ExpiryMonth, cmd.ExpiryYear))
                .WithMessage("Card has expired")
                .When(cmd => cmd.ExpiryMonth >= 1 && cmd.ExpiryMonth <= 12 && cmd.ExpiryYear >= DateTime.UtcNow.Year);
        }
        
        private bool IsExpiryDateInPast(int month, int year)
        {
            var now = DateTime.UtcNow;
            return year < now.Year || (year == now.Year && month < now.Month);
        }
    }
}