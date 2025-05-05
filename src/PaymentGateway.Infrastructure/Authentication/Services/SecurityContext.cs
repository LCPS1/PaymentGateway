using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PaymentGateway.Infrastructure.Authentication.Services
{
    public class SecurityContext : CurrentMerchantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public SecurityContext(IHttpContextAccessor httpContextAccessor) 
            : base(httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        /// <summary>
        /// Gets the current ClaimsPrincipal user
        /// </summary>
        public ClaimsPrincipal? GetCurrentUser()
        {
            return _httpContextAccessor.HttpContext?.User;
        }
        
        /// <summary>
        /// Gets the IP address of the current user
        /// </summary>
        public string? GetCurrentUserIp()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }
    }
}