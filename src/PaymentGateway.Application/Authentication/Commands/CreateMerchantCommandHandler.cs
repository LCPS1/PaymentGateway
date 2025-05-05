using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Contracts.Authentication.Responses;
using PaymentGateway.Domain.Core.Repositories;
using PaymentGateway.Domain.Merchants.Entities;

namespace PaymentGateway.Application.Authentication.Commands
{
    public class CreateMerchantCommandHandler : IRequestHandler<CreateMerchantCommand, ErrorOr<CreateMerchantResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateMerchantCommandHandler> _logger;

        public CreateMerchantCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateMerchantCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ErrorOr<CreateMerchantResponse>> Handle(
            CreateMerchantCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new merchant with name: {MerchantName}", request.Name);
                
                var merchantId = Guid.NewGuid();
                // Generate API key and secret
                var apiKey = GenerateApiKey();
                var apiSecret = GenerateApiSecret();
                
                // Create merchant
                var merchant = Merchant.Create(
                    merchantId,
                    request.Name,
                    apiKey,
                    apiSecret);
                    
                if (merchant.IsError)
                    return merchant.Errors;
                    
                // Persist the merchant
                await _unitOfWork.Merchants.AddAsync(merchant.Value);
                await _unitOfWork.CompleteAsync();
                
                // Map to response
                var response = _mapper.Map<CreateMerchantResponse>(merchant.Value);
                
                _logger.LogInformation("Merchant created successfully with ID: {MerchantId}", merchant.Value.Id);
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating merchant");
                return Error.Unexpected(description: "An unexpected error occurred");
            }
        }
        
        private string GenerateApiKey()
        {
            return GenerateRandomString(32);
        }
        
        private string GenerateApiSecret()
        {
            return GenerateRandomString(64);
        }
        
        private string GenerateRandomString(int length)
        {
            var randomBytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");
        }
    }
}
