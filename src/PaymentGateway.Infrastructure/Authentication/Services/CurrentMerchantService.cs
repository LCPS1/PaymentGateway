using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Infrastructure.Common.Extensions;

namespace PaymentGateway.Infrastructure.Authentication.Services
{
    public class CurrentMerchantService : ICurrentMerchantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentMerchantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetCurrentMerchantId()
        {
            return _httpContextAccessor.HttpContext?.GetMerchantIdFromClaims();
        }

        public string? GetCurrentMerchantName()
        {
            return _httpContextAccessor.HttpContext?.User
                .FindFirstValue("merchant_name");
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }
}