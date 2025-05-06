using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace PaymentGateway.IntegrationTests.Api.Controllers
{
    public class AcquirerControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        
        public AcquirerControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }
        
        [Fact]
        public async Task ProcessPayment_ReturnsSuccessStatusCode()
        {
            // Arrange
            var request = new
            {
                Amount = 100,
                Currency = "USD",
                CardNumber = "4111111111111111",
                CardHolder = "Test User",
                ExpiryMonth = 12,
                ExpiryYear = 2030,
                CVV = "123"
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/acquirer/payments", request);
            
            // Assert
            response.EnsureSuccessStatusCode();
        }
        
        [Fact]
        public async Task ProcessPayment_ReturnsCorrectResponseStructure()
        {
            // Arrange
            var request = new
            {
                Amount = 100,
                Currency = "USD",
                CardNumber = "4111111111111111"
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/acquirer/payments", request);
            var content = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<AcquirerResponse>(content, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            // Assert
            Assert.NotNull(responseObj);
            
            if (responseObj.Success)
            {
                Assert.NotNull(responseObj.Reference);
                Assert.StartsWith("ACQ_", responseObj.Reference);
                Assert.Null(responseObj.ErrorMessage);
            }
            else
            {
                Assert.Null(responseObj.Reference);
                Assert.NotNull(responseObj.ErrorMessage);
                Assert.NotEmpty(responseObj.ErrorMessage);
            }
        }
        
        [Fact]
        public async Task ProcessPayment_HandlesEmptyRequest()
        {
            // Arrange
            var emptyRequest = new { };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/acquirer/payments", emptyRequest);
            
            // Assert
            response.EnsureSuccessStatusCode();
        }
        
        [Fact]
        public async Task ProcessPayment_ReturnsJsonContent()
        {
            // Arrange
            var request = new { Amount = 100 };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/acquirer/payments", request);
            
            // Assert
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        }
        
        private class AcquirerResponse
        {
            public bool Success { get; set; }
            public string? Reference { get; set; }
            public string? ErrorMessage { get; set; }
        }
    }
}