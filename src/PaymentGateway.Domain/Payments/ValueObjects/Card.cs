using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using PaymentGateway.Domain.Common.ValueObjects;
using PaymentGateway.Domain.Common.Errors;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace PaymentGateway.Domain.Payments.ValueObjects
{
    public class Card : ValueObject
    {
        public string CardHolderName { get; }
        public string CardNumberHash { get; }
        public string LastFourDigits { get; }
        public int ExpiryMonth { get; }
        public int ExpiryYear { get; }
        public string CardBrand { get; } 
        
        private Card(string cardHolderName, string cardNumberHash, string lastFourDigits, 
                int expiryMonth, int expiryYear, string cardBrand)
        {
            CardHolderName = cardHolderName;
            CardNumberHash = cardNumberHash;
            LastFourDigits = lastFourDigits;
            ExpiryMonth = expiryMonth;
            ExpiryYear = expiryYear;
            CardBrand = cardBrand;
        }
        
        public static ErrorOr<Card> Create(string cardNumber, string cardHolderName, 
                                        int expiryMonth, int expiryYear, string cvv)
        {
            // Validate cardholder name
            if (string.IsNullOrWhiteSpace(cardHolderName))
                return Errors.Card.EmptyCardholderName;
                
            // Validate card number
            if (!IsValidCardNumber(cardNumber))
                return Errors.Card.InvalidCardNumber;
                
            // Validate expiry date
            if (!IsValidExpiryDate(expiryMonth, expiryYear))
                return Errors.Card.InvalidExpiryDate;
                
            // Validate CVV
            if (!IsValidCVV(cvv))
                return Errors.Card.InvalidCVV;
            
            // Get card brand
            var cardBrand = DetermineCardBrand(cardNumber);
            
            // Get last four digits
            var lastFour = cardNumber.Length > 4 ? cardNumber.Substring(cardNumber.Length - 4) : cardNumber;
            
            // Hash card number for storage
            var cardNumberHash = HashCardNumber(cardNumber);
            
            return new Card(
                cardHolderName,
                cardNumberHash,
                lastFour,
                expiryMonth,
                expiryYear,
                cardBrand);
        }
        
        private static bool IsValidCardNumber(string cardNumber)
        {
            // Remove spaces and dashes
            cardNumber = Regex.Replace(cardNumber, @"[\s-]", "");
            
            // Check if it contains only digits
            if (!Regex.IsMatch(cardNumber, @"^\d+$"))
                return false;
                
            // Check length (most cards are 13-19 digits)
            if (cardNumber.Length < 13 || cardNumber.Length > 19)
                return false;
                
            // Implement Luhn algorithm
            int sum = 0;
            bool alternate = false;
            
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = int.Parse(cardNumber[i].ToString());
                
                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }
                
                sum += digit;
                alternate = !alternate;
            }
            
            return sum % 10 == 0;
        }
        
        private static bool IsValidExpiryDate(int month, int year)
        {
            if (month < 1 || month > 12)
                return false;
                
            var now = DateTime.UtcNow;
            var currentYear = now.Year;
            var currentMonth = now.Month;
            
            return (year > currentYear) || 
                (year == currentYear && month >= currentMonth);
        }
        
        private static bool IsValidCVV(string cvv)
        {
            return !string.IsNullOrEmpty(cvv) && Regex.IsMatch(cvv, @"^\d{3,4}$");
        }
        
        private static string HashCardNumber(string cardNumber)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(cardNumber));
            return Convert.ToBase64String(hashedBytes);
        }
        
        private static string DetermineCardBrand(string cardNumber)
        {
            // Clean the card number
            cardNumber = Regex.Replace(cardNumber, @"[\s-]", "");
            
            // Visa
            if (Regex.IsMatch(cardNumber, @"^4\d{12}(\d{3})?$"))
                return "Visa";
                
            // MasterCard
            if (Regex.IsMatch(cardNumber, @"^(5[1-5]\d{4}|222[1-9]\d{2}|22[3-9]\d{3}|2[3-6]\d{4}|27[01]\d{3}|2720\d{2})\d{10}$"))
                return "MasterCard";
                
            // American Express
            if (Regex.IsMatch(cardNumber, @"^3[47]\d{13}$"))
                return "AmericanExpress";
                
            // Discover
            if (Regex.IsMatch(cardNumber, @"^6(?:011|5\d{2})\d{12}$"))
                return "Discover";
                
            // Unknown
            return "Unknown";
        }
        public string GetMaskedCardNumber()
        {
            return $"**** **** **** {LastFourDigits}";
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return CardHolderName;
            yield return CardNumberHash;
            yield return LastFourDigits;
            yield return ExpiryMonth;
            yield return ExpiryYear;
            yield return CardBrand;
        }
    }
}