using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using PaymentGateway.Domain.Common.ValueObjects;
using PaymentGateway.Domain.Common.Errors;
using PaymentGateway.Domain.Common.Constants;


namespace PaymentGateway.Domain.Payments.ValueObjects
{
    public class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }
        
        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }
        
        public static ErrorOr<Money> Create(decimal amount, string currency)
        {
            // Validate amount
            if (amount <= 0)
                return Errors.Payment.InvalidAmount;
                
            // Validate currency
            if (string.IsNullOrWhiteSpace(currency) || !CurrencyCodes.IsValid(currency))
                return Errors.Payment.InvalidCurrency;
                
            return new Money(amount, currency.ToUpper());
        }
        
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
        
        public override string ToString()
        {
            return $"{Amount} {Currency}";
        }
    }

}