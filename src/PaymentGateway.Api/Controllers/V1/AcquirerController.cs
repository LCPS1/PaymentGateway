using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Common.Extensions;

namespace PaymentGateway.Api.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [AllowAnonymous]
    public class AcquirerController : ApiController
    {
        private readonly ILogger<AcquirerController> _logger;
        private readonly Random _random = new Random();

        public AcquirerController(ILogger<AcquirerController> logger)
        {
            _logger = logger;
        }

        [HttpPost("payments")]
        public IActionResult ProcessPayment([FromBody] object request)
        {
            _logger.LogInformation("Acquirer received payment request");
            
            // Simulate random success/failure (80% success rate)
            bool isSuccess = _random.Next(100) < 80;
            
            if (isSuccess)
            {
                _logger.LogInformation("Acquirer approved payment");
                return Ok(new
                {
                    Success = true,
                    Reference = $"ACQ_{Guid.NewGuid():N}",
                    ErrorMessage = (string)null
                });
            }
            else
            {
                _logger.LogInformation("Acquirer declined payment");
                return Ok(new
                {
                    Success = false,
                    Reference = (string)null,
                    ErrorMessage = DeclineReasonExtension.GetRandomDeclineReason()
                });
            }
        }
        

    }
}