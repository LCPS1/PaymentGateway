using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;

namespace PaymentGateway.Domain.Common.Errors
{
    public static partial class Errors
    {
        public static class Payment
        {
            public static Error InvalidId => Error.Validation(
                code: "Payment.InvalidId", 
                description: "Payment ID cannot be empty");
                
            public static Error InvalidMerchantId => Error.Validation(
                code: "Payment.InvalidMerchantId", 
                description: "Merchant ID cannot be empty");
                
            public static Error InvalidIdempotencyKey => Error.Validation(
                code: "Payment.InvalidIdempotencyKey", 
                description: "Idempotency key is invalid");
                
            public static Error NotFound => Error.NotFound(
                code: "Payment.NotFound",
                description: "The payment does not exist");
                
            public static Error Unauthorized => Error.Unauthorized(
                code: "Payment.Unauthorized",
                description: "The payment does not belong to the merchant");
                
            public static Error InvalidAmount => Error.Validation(
                code: "Payment.InvalidAmount",
                description: "The payment amount must be greater than zero");

            public static Error InvalidCurrency => Error.Validation(
                code: "Payment.InvalidCurrency",
                description: "The payment currency is invalid");
                
            public static Error ProcessingFailed => Error.Failure(
                code: "Payment.ProcessingFailed",
                description: "The payment could not be processed");
                
            public static Error AcquirerUnavailable => Error.Failure(
                code: "Payment.AcquirerUnavailable",
                description: "The payment processor is currently unavailable");
                
            public static Error DuplicateIdempotencyKey => Error.Conflict(
                code: "Payment.DuplicateIdempotencyKey",
                description: "A payment with this idempotency key already exists");
        }
    

        public static class Card
        {
            public static Error InvalidCardNumber => Error.Validation(
                code: "Card.InvalidCardNumber",
                description: "The card number is invalid");
                
            public static Error EmptyCardholderName => Error.Validation(
                code: "Card.EmptyCardholderName",
                description: "The cardholder name cannot be empty");
                
            public static Error InvalidExpiryDate => Error.Validation(
                code: "Card.InvalidExpiryDate",
                description: "The card expiry date is invalid or the card has expired");
                
            public static Error InvalidCVV => Error.Validation(
                code: "Card.InvalidCVV",
                description: "The card security code (CVV) is invalid");
        }

        public static class Merchant
        {
            public static Error InvalidId => Error.Validation(
                code: "Merchant.InvalidId", 
                description: "Merchant ID cannot be empty");
                
            public static Error InvalidName => Error.Validation(
                code: "Merchant.InvalidName", 
                description: "Merchant name cannot be empty");
                
            public static Error InvalidApiKey => Error.Validation(
                code: "Merchant.InvalidApiKey", 
                description: "Merchant API key cannot be empty");
                
            public static Error InvalidApiSecret => Error.Validation(
                code: "Merchant.InvalidApiSecret", 
                description: "Merchant API secret cannot be empty");
                
            public static Error NotFound => Error.NotFound(
                code: "Merchant.NotFound",
                description: "The merchant does not exist");
                
            public static Error Inactive => Error.Validation(
                code: "Merchant.Inactive",
                description: "The merchant account is inactive");
                
            public static Error Unauthorized => Error.Unauthorized(
                code: "Merchant.Unauthorized",
                description: "Invalid merchant credentials");
                
            public static Error DuplicateApiKey => Error.Conflict(
                code: "Merchant.DuplicateApiKey",
                description: "A merchant with this API key already exists");
        }
    }
}