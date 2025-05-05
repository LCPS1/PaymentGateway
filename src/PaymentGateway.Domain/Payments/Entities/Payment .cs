using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentGateway.Domain.Common.Entities;
using PaymentGateway.Domain.Payments.ValueObjects;
using PaymentGateway.Domain.Common.Errors;
using ErrorOr;
using PaymentGateway.Domain.Payments.Events;

namespace PaymentGateway.Domain.Payments.Entities
{
    public class Payment : Entity
    {
        public Guid MerchantId { get; private set; }
        public Money Amount { get; private set; }
        public Card Card { get; private set; }
        public PaymentStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public string? AcquirerReference { get; private set; }
        public string? IdempotencyKey { get; private set; }
        
        private readonly List<object> _domainEvents = new();
        public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();
        
        // Private constructor - used by factory method
        private Payment(Guid id, Guid merchantId, Money amount, Card card, string? idempotencyKey) 
            : base(id)
        {
            MerchantId = merchantId;
            Amount = amount;
            Card = card;
            Status = PaymentStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            IdempotencyKey = idempotencyKey;
            
            _domainEvents.Add(new PaymentCreatedEvent(id, merchantId));
        }
        
        // For EF Core
        private Payment() : base() { }
        
        // Factory method that validates inputs
        public static ErrorOr<Payment> Create(
            Guid id, 
            Guid merchantId, 
            Money amount, 
            Card card, 
            string? idempotencyKey)
        {
            // Validate inputs
            if (id == Guid.Empty)
                return Errors.Payment.InvalidId;
                
            if (merchantId == Guid.Empty)
                return Errors.Payment.InvalidMerchantId;
                
            // Validate idempotency key if provided
            if (idempotencyKey != null)
            {
                if (string.IsNullOrWhiteSpace(idempotencyKey))
                    return Errors.Payment.InvalidIdempotencyKey;
                    
                if (idempotencyKey.Length > 100) // Set a reasonable limit
                    return Errors.Payment.InvalidIdempotencyKey;
            }
            
            return new Payment(id, merchantId, amount, card, idempotencyKey);
        }
        
        // Mark a payment as successfully processed
        public ErrorOr<Success> MarkAsSuccessful(string acquirerReference)
        {
            if (string.IsNullOrWhiteSpace(acquirerReference))
                return Error.Validation("Payment.InvalidAcquirerReference", "Acquirer reference cannot be empty");
                
            Status = PaymentStatus.Successful;
            ProcessedAt = DateTime.UtcNow;
            AcquirerReference = acquirerReference;
            
            _domainEvents.Add(new PaymentSucceededEvent(Id, MerchantId, AcquirerReference));
            
            return Result.Success;
        }
        
        // Mark a payment as failed
        public void MarkAsFailed()
        {
            Status = PaymentStatus.Failed;
            ProcessedAt = DateTime.UtcNow;
            
            _domainEvents.Add(new PaymentFailedEvent(Id, MerchantId));
        }
        
        // Clear domain events (typically after they've been dispatched)
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }

}