using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentGateway.Api.Controllers.V1;
using PaymentGateway.Application.Authentication.Commands;
using PaymentGateway.Application.Authentication.Queries;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Contracts.Authentication.Requests;
using PaymentGateway.Contracts.Authentication.Responses;

namespace PaymentGateway.UnitTests.Api.Controllers
{
    public class AuthControllerUnitTests
    {
         private readonly Mock<ISender> _mockMediator;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICurrentMerchantService> _mockCurrentMerchantService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _controller;
        private readonly Guid _validMerchantId = Guid.NewGuid();
        
        public AuthControllerUnitTests()
        {
            _mockMediator = new Mock<ISender>();
            _mockMapper = new Mock<IMapper>();
            _mockCurrentMerchantService = new Mock<ICurrentMerchantService>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            
            _controller = new AuthController(
                _mockMediator.Object,
                _mockMapper.Object,
                _mockCurrentMerchantService.Object,
                _mockLogger.Object);
        }
        
        #region Login Tests
        
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var request = new AuthenticationRequest 
            { 
                ApiKey = "valid-api-key", 
                ApiSecret = "valid-api-secret" 
            };
            
            var authResponse = new AuthenticationResponse
            {
                MerchantId = _validMerchantId.ToString(),
                Token = "valid-jwt-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
            
            _mockMediator.Setup(x => x.Send(
                    It.Is<AuthenticateMerchantCommand>(c => 
                        c.ApiKey == request.ApiKey && 
                        c.ApiSecret == request.ApiSecret),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ErrorOr<AuthenticationResponse>>(authResponse));
            
            // Act
            var result = await _controller.Login(request);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<AuthenticationResponse>(okResult.Value);
            Assert.Equal(authResponse.MerchantId, returnValue.MerchantId);
            Assert.Equal(authResponse.Token, returnValue.Token);
        }
        
        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var request = new AuthenticationRequest 
            { 
                ApiKey = "invalid-api-key", 
                ApiSecret = "invalid-api-secret" 
            };
            
            var errors = new List<Error> 
            { 
                Error.Unauthorized("Auth.InvalidCredentials", "Invalid API credentials") 
            };
            
            _mockMediator.Setup(x => x.Send(
                    It.IsAny<AuthenticateMerchantCommand>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ErrorOr<AuthenticationResponse>>(errors));
            
            // Act
            var result = await _controller.Login(request);
            
            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);
        }
        
        #endregion
        
        #region GetMerchantInfo Tests
        
        [Fact]
        public async Task GetMerchantInfo_WithValidMerchantId_ReturnsOkWithMerchantInfo()
        {
            // Arrange
            var merchantInfo = new MerchantInfoResponse
            {
                MerchantId = _validMerchantId.ToString(),
                Name = "Test Merchant",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };
            
            _mockCurrentMerchantService.Setup(x => x.GetCurrentMerchantId())
                .Returns(_validMerchantId);
                
            _mockMediator.Setup(x => x.Send(
                    It.Is<GetMerchantInfoQuery>(q => q.MerchantId == _validMerchantId),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ErrorOr<MerchantInfoResponse>>(merchantInfo));
            
            // Act
            var result = await _controller.GetMerchantInfo();
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<MerchantInfoResponse>(okResult.Value);
            Assert.Equal(merchantInfo.MerchantId, returnValue.MerchantId);
            Assert.Equal(merchantInfo.Name, returnValue.Name);
        }
        
        [Fact]
        public async Task GetMerchantInfo_WithNullMerchantId_ReturnsUnauthorized()
        {
            // Arrange
            _mockCurrentMerchantService.Setup(x => x.GetCurrentMerchantId())
                .Returns((Guid?)null);
            
            // Act
            var result = await _controller.GetMerchantInfo();
            
            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value);
        }
        
        [Fact]
        public async Task GetMerchantInfo_WithEmptyMerchantId_ReturnsUnauthorized()
        {
            // Arrange
            _mockCurrentMerchantService.Setup(x => x.GetCurrentMerchantId())
                .Returns(Guid.Empty);
            
            // Act
            var result = await _controller.GetMerchantInfo();
            
            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value);
        }
        
        [Fact]
        public async Task GetMerchantInfo_WithNonExistentMerchant_ReturnsNotFound()
        {
            // Arrange
            var errors = new List<Error> 
            { 
                Error.NotFound("Merchant.NotFound", "Merchant not found") 
            };
            
            _mockCurrentMerchantService.Setup(x => x.GetCurrentMerchantId())
                .Returns(_validMerchantId);
                
            _mockMediator.Setup(x => x.Send(
                    It.IsAny<GetMerchantInfoQuery>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ErrorOr<MerchantInfoResponse>>(errors));
            
            // Act
            var result = await _controller.GetMerchantInfo();
            
            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }
        
        #endregion
            
    }
    
}