using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Contracts.Authentication.Responses;

namespace PaymentGateway.Application.Authentication.Commands
{
    public class AuthenticateMerchantCommandHandler 
        : IRequestHandler<AuthenticateMerchantCommand, ErrorOr<AuthenticationResponse>>
    {
        private readonly IMerchantAuthenticationService _authService;
        private readonly ILogger<AuthenticateMerchantCommandHandler> _logger;

        public AuthenticateMerchantCommandHandler(
            IMerchantAuthenticationService authService,
            ILogger<AuthenticateMerchantCommandHandler> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<ErrorOr<AuthenticationResponse>> Handle(
            AuthenticateMerchantCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling merchant authentication for API key: {ApiKey}", request.ApiKey);
            
            // Delegate to the authentication service
            var result = await _authService.AuthenticateAsync(request.ApiKey, request.ApiSecret);
            
            if (result.IsError)
            {
                _logger.LogWarning("Authentication failed for API key: {ApiKey}", request.ApiKey);
            }
            else
            {
                _logger.LogInformation("Authentication successful for merchant: {MerchantId}", result.Value.MerchantId);
            }
            
            return result;
        }
    }
}