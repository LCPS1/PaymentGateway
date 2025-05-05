using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Infrastructure.Authentication.Models;
using PaymentGateway.Domain.Merchants.Entities;
using PaymentGateway.Domain.Common.Errors;
using PaymentGateway.Domain.Core.Repositories;
using PaymentGateway.Contracts.Authentication.Responses;


namespace PaymentGateway.Infrastructure.Authentication.Services
{
public class MerchantAuthenticationService : IMerchantAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<MerchantAuthenticationService> _logger;

    public MerchantAuthenticationService(
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings,
        ILogger<MerchantAuthenticationService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<ErrorOr<AuthenticationResponse>> AuthenticateAsync(string apiKey, string apiSecret)
    {
        try
        {
            _logger.LogInformation("Authenticating merchant with API key: {ApiKey}", apiKey);
            
            // Find merchant by API key
            var merchant = await _unitOfWork.Merchants.GetByApiKeyAsync(apiKey);
            
            // Check if merchant exists
            if (merchant is null)
            {
                _logger.LogWarning("Authentication failed: merchant not found for API key: {ApiKey}", apiKey);
                return Errors.Merchant.Unauthorized;
            }
            
            // Check if merchant is active
            if (!merchant.IsActive)
            {
                _logger.LogWarning("Authentication failed: merchant {MerchantId} is inactive", merchant.Id);
                return Errors.Merchant.Inactive;
            }
            
            // Verify API secret
            if (merchant.ApiSecret != apiSecret)
            {
                _logger.LogWarning("Authentication failed: invalid API secret for merchant {MerchantId}", merchant.Id);
                return Errors.Merchant.Unauthorized;
            }
            
            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            
            var expiryTime = DateTime.UtcNow.AddHours(_jwtSettings.ExpiryHours);
            
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, merchant.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim("merchant_id", merchant.Id.ToString()),
                new Claim("merchant_name", merchant.Name)
            };
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiryTime,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            _logger.LogInformation("Authentication successful for merchant {MerchantId}", merchant.Id);
            
            return new AuthenticationResponse
            {
                Token = tokenString,
                MerchantId = merchant.Id.ToString(),
                MerchantName = merchant.Name,
                ExpiresAt = expiryTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during merchant authentication");
            return Error.Unexpected(description: "An unexpected error occurred during authentication");
        }
    }

    public async Task<ErrorOr<CreateMerchantResponse>> CreateMerchantAsync(string name)
    {
        try
        {
            _logger.LogInformation("Creating new merchant: {Name}", name);
            
            // Generate API credentials
            var apiKey = GenerateApiKey();
            var apiSecret = GenerateApiSecret();
            
            // Create new merchant using domain factory method
            var merchantId = Guid.NewGuid();
            var merchantResult = Merchant.Create(
                merchantId,
                name,
                apiKey,
                apiSecret);
                
            if (merchantResult.IsError)
            {
                _logger.LogWarning("Failed to create merchant: {Errors}", 
                    string.Join(", ", merchantResult.Errors));
                return merchantResult.Errors;
            }
            
            // Save to repository through unit of work
            await _unitOfWork.Merchants.AddAsync(merchantResult.Value);
            await _unitOfWork.CompleteAsync();
            
            _logger.LogInformation("Merchant created successfully: {MerchantId}", merchantId);
            
            // Return response
            return new CreateMerchantResponse
            {
                MerchantId = merchantId,
                Name = name,
                ApiKey = apiKey,
                ApiSecret = apiSecret,
                CreatedAt = merchantResult.Value.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating merchant");
            return Error.Unexpected(description: "An unexpected error occurred while creating the merchant");
        }
    }

        public async Task<ErrorOr<ApiCredentialsResponse>> GenerateApiCredentialAsync(string merchantId)
        {
            try
            {
                _logger.LogInformation("Generating new API credentials for merchant: {MerchantId}", merchantId);
                
                // Validate merchant ID
                if (!Guid.TryParse(merchantId, out var merchantGuid))
                {
                    return Error.Validation(description: "Invalid merchant ID format");
                }
                
                // Find merchant through unit of work
                var merchant = await _unitOfWork.Merchants.GetByIdAsync(merchantGuid);
                
                if (merchant == null)
                {
                    _logger.LogWarning("Merchant not found: {MerchantId}", merchantId);
                    return Errors.Merchant.NotFound;
                }
                
                // Generate new credentials
                var apiKey = GenerateApiKey();
                var apiSecret = GenerateApiSecret();
                
                // Update merchant using domain method
                var updateResult = merchant.UpdateApiCredentials(apiKey, apiSecret);
                
                if (updateResult.IsError)
                {
                    _logger.LogWarning("Failed to update API credentials: {Errors}", 
                        string.Join(", ", updateResult.Errors));
                    return updateResult.Errors;
                }
                
                // Save changes through unit of work
                await _unitOfWork.Merchants.AddAsync(merchant);
                await _unitOfWork.CompleteAsync();
                
                _logger.LogInformation("API credentials regenerated for merchant: {MerchantId}", merchantId);
                
                return new ApiCredentialsResponse
                {
                    ApiKey = apiKey,
                    ApiSecret = apiSecret,
                    Message = "New API credentials have been generated. Please store these securely as the API secret cannot be retrieved later."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating API credentials for merchant {MerchantId}", merchantId);
                return Error.Unexpected(description: "An unexpected error occurred while generating API credentials");
            }
        }


    
    #region Helper Methods
    
    private string GenerateApiKey()
    {
        return $"pk_{Guid.NewGuid().ToString("N")}";
    }
    
    private string GenerateApiSecret()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32]; // 256 bits
        rng.GetBytes(bytes);
        return $"sk_{Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "")}";
    }
    
    #endregion
}
}