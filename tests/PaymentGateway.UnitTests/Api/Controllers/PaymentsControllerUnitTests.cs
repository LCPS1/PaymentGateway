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
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Application.Payments.Commands.ProcessPayment;
using PaymentGateway.Application.Payments.Queries.GetPaymentStatus;
using PaymentGateway.Contracts.Payments.Requests;
using PaymentGateway.Contracts.Payments.Responses;

namespace PaymentGateway.UnitTests.Api.Controllers
{
    public class PaymentsControllerUnitTests
    {
        private readonly Mock<ISender> _mockMediator;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICurrentMerchantService> _mockCurrentMerchantService;
        private readonly Mock<ILogger<PaymentsController>> _mockLogger;
        private readonly PaymentsController _controller;
        private readonly Guid _validMerchantId = Guid.NewGuid();
        
        public PaymentsControllerUnitTests()
        {
            _mockMediator = new Mock<ISender>();
            _mockMapper = new Mock<IMapper>();
            _mockCurrentMerchantService = new Mock<ICurrentMerchantService>();
            _mockLogger = new Mock<ILogger<PaymentsController>>();
            
            _controller = new PaymentsController(
                _mockMediator.Object,
                _mockMapper.Object,
                _mockCurrentMerchantService.Object,
                _mockLogger.Object);
        }
        
        #region ProcessPayment Tests
        
        [Fact]
        public async Task ProcessPayment_WithValidMerchant_SendsCorrectCommand()
        {
            // Arrange
            var request = CreateValidPaymentRequest();
            var command = new ProcessPaymentCommand
            {
                Amount = request.Amount,
                Currency = request.Currency,
                CardNumber = request.CardNumber,
                // Other properties mapped from request
                MerchantId = _validMerchantId
            };
            
            _mockCurrentMerchantService.Setup(x => x.GetCurrentMerchantId())
                .Returns(_validMerchantId);
                
            _mockMapper.Setup(x => x.Map<ProcessPaymentCommand>(request))
                .Returns(command);
                
            _mockMediator.Setup(x => x.Send(It.IsAny<ProcessPaymentCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ErrorOr<PaymentResponse>>(new PaymentResponse()));
            
            // Act
            await _controller.ProcessPayment(request);
            
            // Assert
            _mockMediator.Verify(
                x => x.Send(
                    It.Is<ProcessPaymentCommand>(c => c.MerchantId == _validMerchantId), 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ProcessPayment_WithValidMerchant_ReturnsOkResponse()
        {
            // Arrange
            var request = CreateValidPaymentRequest();
            var response = new PaymentResponse
            {
                PaymentId = Guid.NewGuid(),
                Status = "Approved",
                CardLast4 = "1111"
            };
            
            _mockCurrentMerchantService.Setup(x => x.GetCurrentMerchantId())
                .Returns(_validMerchantId);
                
            _mockMediator.Setup(x => x.Send(It.IsAny<ProcessPaymentCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ErrorOr<PaymentResponse>>(response));
            
            // Act
            var result = await _controller.ProcessPayment(request);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaymentResponse>(okResult.Value);
            Assert.Equal(response.PaymentId, returnValue.PaymentId);
        }
        
        [Fact]
        public async Task ProcessPayment_WithNullMerchantId_ReturnsUnauthorized()
        {
            // Arrange
            var request = CreateValidPaymentRequest();
            _mockCurrentMerchantService.Setup(x => x.GetCurrentMerchantId())
                .Returns((Guid?)null);
            
            // Act
            var result = await _controller.ProcessPayment(request);
            
            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
        
        [Fact]
        public async Task ProcessPayment_WithEmptyMerchantId_ReturnsUnauthorized()
        {
            // Arrange
            var request = CreateValidPaymentRequest();
            _mockCurrentMerchantService.Setup(x => x.GetCurrentMerchantId())
                .Returns(Guid.Empty);
            
            // Act
            var result = await _controller.ProcessPayment(request);
            
            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
        
        [Fact]
        public async Task ProcessPayment_WithValidationErrors_ReturnsBadRequest()
        {
            // Arrange
            var request = CreateValidPaymentRequest();
            var errors = new List<Error> 
            { 
                Error.Validation("Payment.InvalidCard", "Card number is invalid") 
            };
            
            _mockCurrentMerchantService.Setup(x => x.GetCurrentMerchantId())
                .Returns(_validMerchantId);
                
            _mockMediator.Setup(x => x.Send(It.IsAny<ProcessPaymentCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ErrorOr<PaymentResponse>>(errors));
            
            // Act
            var result = await _controller.ProcessPayment(request);
            
            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }
        
        [Fact]
        public async Task ProcessPayment_WithPaymentDeclined_ReturnsUnprocessableEntity()
        {
            // Arrange
            var request = CreateValidPaymentRequest();
            var errors = new List<Error> 
            { 
                Error.Failure("Payment.Declined", "Payment was declined by the bank") 
            };
            
            _mockCurrentMerchantService.Setup(x => x.GetCurrentMerchantId())
                .Returns(_validMerchantId);
                
            _mockMediator.Setup(x => x.Send(It.IsAny<ProcessPaymentCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ErrorOr<PaymentResponse>>(errors));
            
            // Act
            var result = await _controller.ProcessPayment(request);
            
            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);
        }
        
        #endregion
        
        #region GetPaymentStatus Tests
        
        [Fact]
        public async Task GetPaymentStatus_WithValidMerchant_SendsCorrectQuery()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            
            _mockCurrentMerchantService.Setup(x => x.GetCurrentMerchantId())
                .Returns(_validMerchantId);
                
            _mockMediator.Setup(x => x.Send(It.IsAny<GetPaymentStatusQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ErrorOr<PaymentStatusResponse>>(new PaymentStatusResponse()));
            
            // Act
            await _controller.GetPaymentStatus(paymentId);
            
            // Assert
            _mockMediator.Verify(
                x => x.Send(
                    It.Is<GetPaymentStatusQuery>(q => 
                        q.PaymentId == paymentId && 
                        q.MerchantId == _validMerchantId), 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task GetPaymentStatus_WithValidPaymentId_ReturnsOkResponse()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var response = new PaymentStatusResponse
            {
                PaymentId = paymentId,
                Status = Contracts.Enums.PaymentStatusEnum.Successful,
                CardLast4 = "1111",
                ProcessedAt = DateTime.UtcNow
            };
            
            _mockCurrentMerchantService.Setup(x => x.GetCurrentMerchantId())
                .Returns(_validMerchantId);
                
            _mockMediator.Setup(x => x.Send(It.IsAny<GetPaymentStatusQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ErrorOr<PaymentStatusResponse>>(response));
            
            // Act
            var result = await _controller.GetPaymentStatus(paymentId);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaymentStatusResponse>(okResult.Value);
            Assert.Equal(paymentId, returnValue.PaymentId);
        }
        
        [Fact]
        public async Task GetPaymentStatus_WithNullMerchantId_ReturnsUnauthorized()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            _mockCurrentMerchantService.Setup(x => x.GetCurrentMerchantId())
                .Returns((Guid?)null);
            
            // Act
            var result = await _controller.GetPaymentStatus(paymentId);
            
            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
        
        [Fact]
        public async Task GetPaymentStatus_WithNonExistentPayment_ReturnsNotFound()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var errors = new List<Error> 
            { 
                Error.NotFound("Payment.NotFound", "Payment not found") 
            };
            
            _mockCurrentMerchantService.Setup(x => x.GetCurrentMerchantId())
                .Returns(_validMerchantId);
                
            _mockMediator.Setup(x => x.Send(It.IsAny<GetPaymentStatusQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ErrorOr<PaymentStatusResponse>>(errors));
            
            // Act
            var result = await _controller.GetPaymentStatus(paymentId);
            
            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }
        
        #endregion
        
        #region Helper Methods
        
        private ProcessPaymentRequest CreateValidPaymentRequest()
        {
            return new ProcessPaymentRequest
            {
                Amount = 100,
                Currency = "USD",
                CardNumber = "4111111111111111",
                CardHolderName = "Test User",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
                CVV = "123",
                IdempotencyKey = "test-key-123"
            };
        }
        
        #endregion
    }

}
