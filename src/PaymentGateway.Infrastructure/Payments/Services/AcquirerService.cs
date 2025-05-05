using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.Domain.Payments.Entities;
using PaymentGateway.Domain.Payments.Models;
using PaymentGateway.Domain.Payments.Services;
using PaymentGateway.Infrastructure.Payments.Models;
using PaymentGateway.Infrastructure.Payments.Options;
using Polly;
using Polly.Retry;

namespace PaymentGateway.Infrastructure.Payments.Services
{
    public class AcquirerService : IAcquirerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AcquirerService> _logger;
        private readonly AcquirerSettings _settings;

        public AcquirerService(
            HttpClient httpClient, 
            IOptions<AcquirerSettings> settings,
            ILogger<AcquirerService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AcquirerResponse> ProcessPaymentAsync(Payment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            try
            {
                _logger.LogInformation("Processing payment {PaymentId} with acquirer", payment.Id);
                
                // Map to acquirer request model
                var request = new AcquirerPaymentRequest
                {
                    Amount = payment.Amount.Amount,
                    Currency = payment.Amount.Currency,
                    CardNumber = payment.Card.GetMaskedCardNumber(),
                    CardHolderName = payment.Card.CardHolderName,
                    ExpiryMonth = payment.Card.ExpiryMonth,
                    ExpiryYear = payment.Card.ExpiryYear,
                    MerchantReference = payment.Id.ToString()
                };

                var requestUri = GetAcquirerUri();
                _logger.LogInformation("Sending payment request to: {PaymentUrl}", requestUri);

                // Configure request message directly for better control
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
                
                // Add content
                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Add headers
                if (!string.IsNullOrEmpty(_settings.ApiKey))
                {
                    httpRequest.Headers.Add("X-API-Key", _settings.ApiKey);
                }
                
                // Send request with timeout
                using var cts = new System.Threading.CancellationTokenSource(
                    TimeSpan.FromSeconds(_settings.TimeoutSeconds));
                    
                var response = await _httpClient.SendAsync(httpRequest, cts.Token);
                
                // Process response
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    try
                    {
                        var acquirerResponse = JsonSerializer.Deserialize<AcquirerPaymentResponse>(
                            responseContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        
                        if (acquirerResponse != null)
                        {
                            if (acquirerResponse.Success)
                            {
                                _logger.LogInformation("Payment {PaymentId} processed successfully with reference {Reference}", 
                                    payment.Id, acquirerResponse.Reference);
                            }
                            else
                            {
                                _logger.LogWarning("Payment {PaymentId} was declined: {ErrorMessage}", 
                                    payment.Id, acquirerResponse.ErrorMessage);
                            }
                            
                            return new AcquirerResponse(
                                acquirerResponse.Success,
                                acquirerResponse.Reference,
                                acquirerResponse.ErrorMessage);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize acquirer response for payment {PaymentId}: {Response}", 
                            payment.Id, responseContent);
                    }
                    
                    return new AcquirerResponse(false, null, "Invalid response from payment processor");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Acquirer returned error {StatusCode} for payment {PaymentId}: {ErrorContent}", 
                        (int)response.StatusCode, payment.Id, errorContent);
                        
                    return new AcquirerResponse(
                        false, 
                        null, 
                        $"Payment processor error: {response.StatusCode}");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Timeout processing payment {PaymentId} with acquirer", payment.Id);
                return new AcquirerResponse(false, null, "Payment processing timed out");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error processing payment {PaymentId} with acquirer", payment.Id);
                return new AcquirerResponse(false, null, "Payment processor unavailable");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing payment {PaymentId} with acquirer", payment.Id);
                return new AcquirerResponse(false, null, "Payment processing failed");
            }
        }
   
   
        private Uri GetAcquirerUri()
        {
            try
            {
                if (string.IsNullOrEmpty(_settings.BaseUrl))
                {
                    _logger.LogWarning("Acquirer BaseUrl is empty - using internal endpoint");
                    return new Uri("http://localhost:8080/api/v1/acquirer/payments");
                }

                var baseUri = new Uri(_settings.BaseUrl);
                return new Uri(baseUri, _settings.PaymentEndpoint);
            }
            catch (UriFormatException ex)
            {
                _logger.LogError(ex, "Invalid URI format in acquirer settings");
                return new Uri("http://localhost:8080/api/v1/acquirer/payments");
            }   
        }   
    }
}
