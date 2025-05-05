using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Domain.Common.Constants
{
    public static class CurrencyCodes
    {
        public const string USD = "USD";
        public const string EUR = "EUR";
        public const string GBP = "GBP";
        public const string CAD = "CAD";
        public const string AUD = "AUD";
        public const string JPY = "JPY";
        
        private static readonly HashSet<string> _validCurrencies = new()
        {
            USD, EUR, GBP, CAD, AUD, JPY
        };

        public static bool IsValid(string currencyCode)
        {
            return !string.IsNullOrEmpty(currencyCode) && 
                currencyCode.Length == 3 && 
                _validCurrencies.Contains(currencyCode);
        }
    }
}