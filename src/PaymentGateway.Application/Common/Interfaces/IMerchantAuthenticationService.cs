using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using PaymentGateway.Contracts.Authentication.Responses;


namespace PaymentGateway.Application.Common.Interfaces
{
    public interface IMerchantAuthenticationService
    {
        Task<ErrorOr<AuthenticationResponse>> AuthenticateAsync(string apiKey, string apiSecret);
        Task<ErrorOr<CreateMerchantResponse>> CreateMerchantAsync(string name);
        Task<ErrorOr<ApiCredentialsResponse>> GenerateApiCredentialAsync(string merchantId);
    }
}