using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using PaymentGateway.Domain.Common.Entities;
using PaymentGateway.Domain.Common.Errors;

namespace PaymentGateway.Domain.Merchants.Entities
{
   public class Merchant : Entity
{
    public string Name { get; private set; }
    public string ApiKey { get; private set; }
    public string ApiSecret { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    private Merchant(Guid id, string name, string apiKey, string apiSecret, bool isActive) 
        : base(id)
    {
        Name = name;
        ApiKey = apiKey;
        ApiSecret = apiSecret;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }
    
    // For EF Core
    private Merchant() : base() { }
    
    // Factory method with validation
    public static ErrorOr<Merchant> Create(Guid id, string name, string apiKey, string apiSecret, bool isActive = true)
    {
        // Validate inputs
        if (id == Guid.Empty)
            return Errors.Merchant.InvalidId;
            
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Merchant.InvalidName;
            
        if (string.IsNullOrWhiteSpace(apiKey))
            return Errors.Merchant.InvalidApiKey;
            
        if (string.IsNullOrWhiteSpace(apiSecret))
            return Errors.Merchant.InvalidApiSecret;
            
        return new Merchant(id, name, apiKey, apiSecret, isActive);
    }
    
    // Deactivate a merchant
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    // Activate a merchant
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    // Update merchant details
    public ErrorOr<Success> UpdateDetails(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Merchant.InvalidName;
            
        Name = name;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success;
    }
    
    // Update API credentials
    public ErrorOr<Success> UpdateApiCredentials(string apiKey, string apiSecret)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return Errors.Merchant.InvalidApiKey;
            
        if (string.IsNullOrWhiteSpace(apiSecret))
            return Errors.Merchant.InvalidApiSecret;
            
        ApiKey = apiKey;
        ApiSecret = apiSecret;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success;
    }
}
}