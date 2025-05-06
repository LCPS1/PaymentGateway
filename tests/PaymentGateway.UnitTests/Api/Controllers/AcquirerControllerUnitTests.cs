using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentGateway.Api.Common.Extensions;
using PaymentGateway.Api.Controllers.V1;

namespace PaymentGateway.UnitTests.Api.Controllers
{
    public class AcquirerControllerUnitTests
    {
        private readonly Mock<ILogger<AcquirerController>> _mockLogger;
        private readonly AcquirerController _controller;
        
        public AcquirerControllerUnitTests()
        {
            _mockLogger = new Mock<ILogger<AcquirerController>>();
            _controller = new AcquirerController(_mockLogger.Object);
        }
        
        [Fact]
        public void ProcessPayment_WhenCalled_ReturnsOkResult()
        {
            // Arrange
            var paymentRequest = new { Amount = 100, CardNumber = "4111111111111111" };
            
            // Act
            var result = _controller.ProcessPayment(paymentRequest);
            
            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
        
        [Fact]
        public void ProcessPayment_WhenSuccess_ReturnsCorrectResponseStructure()
        {
            // Arrange
            var paymentRequest = new { Amount = 100 };
            
            // Force success by setting Random to always return 0 (less than 80)
            SetRandomValueForController(_controller, 0);
            
            // Act
            var result = _controller.ProcessPayment(paymentRequest) as OkObjectResult;
            dynamic response = result.Value;
            
            // Assert
            Assert.NotNull(result);
            Assert.True(response.Success);
            Assert.NotNull(response.Reference);
            Assert.StartsWith("ACQ_", (string)response.Reference);
            Assert.Null(response.ErrorMessage);
        }
        
        [Fact]
        public void ProcessPayment_WhenDeclined_ReturnsCorrectResponseStructure()
        {
            // Arrange
            var paymentRequest = new { Amount = 100 };
            
            // Force failure by setting Random to always return 90 (greater than 80)
            SetRandomValueForController(_controller, 90);
            
            // Act
            var result = _controller.ProcessPayment(paymentRequest) as OkObjectResult;
            dynamic response = result.Value;
            
            // Assert
            Assert.NotNull(result);
            Assert.False(response.Success);
            Assert.Null(response.Reference);
            Assert.NotNull(response.ErrorMessage);
        }
        
        [Fact]
        public void ProcessPayment_LogsInformation()
        {
            // Arrange
            var paymentRequest = new { Amount = 100 };
            
            // Act
            _controller.ProcessPayment(paymentRequest);
            
            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("received payment request")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
                
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("approved payment") || 
                        v.ToString().Contains("declined payment")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        
        [Fact]
        public void DeclineReasonExtension_ReturnsNonEmptyString()
        {
            // Act
            string reason = DeclineReasonExtension.GetRandomDeclineReason();
            
            // Assert
            Assert.NotNull(reason);
            Assert.NotEmpty(reason);
        }
        
        /// <summary>
        /// Helper method to set a controlled random value for testing
        /// Uses reflection to replace the private Random field
        /// </summary>
        private void SetRandomValueForController(AcquirerController controller, int valueToReturn)
        {
            // Create a mock Random that returns a specific value
            var mockRandom = new Mock<Random>();
            mockRandom.Setup(r => r.Next(It.IsAny<int>())).Returns(valueToReturn);
            
            // Use reflection to replace the private _random field
            var fieldInfo = typeof(AcquirerController).GetField("_random", 
                BindingFlags.NonPublic | BindingFlags.Instance);
                
            fieldInfo?.SetValue(controller, mockRandom.Object);
        }
    }
}