using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Application.Payments.Commands.ProcessPayment;
using PaymentGateway.Application.Payments.Queries.GetPaymentStatus;
using PaymentGateway.Contracts.Payments.Requests;

namespace PaymentGateway.Api.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Policy = "MerchantOnly")]
    [Produces("application/json")]
    public class PaymentsController : ApiController
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;
        private readonly ICurrentMerchantService _currentMerchantService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            ISender mediator,
            IMapper mapper,
            ICurrentMerchantService currentMerchantService,
            ILogger<PaymentsController> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _currentMerchantService = currentMerchantService;
            _logger = logger;
        }

        /// <summary>
        /// Process a new payment transaction
        /// </summary>
        /// <param name="request">Payment details</param>
        /// <returns>Created payment with status</returns>
        /// <response code="200">Returns the payment details</response>
        /// <response code="400">If the payment request is invalid</response>
        /// <response code="401">If the merchant is not authenticated</response>
        /// <response code="422">If the payment was declined by the bank</response>
        /// <response code="503">If the payment processor is unavailable</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            _logger.LogInformation("Processing payment request");
            
            var merchantId = _currentMerchantService.GetCurrentMerchantId();
            if (merchantId == null || merchantId == Guid.Empty)
            {
                return Unauthorized(new { message = "Merchant ID not found in authenticated context" });
            }
            
            // Map request to command and set merchant ID
            var command = _mapper.Map<ProcessPaymentCommand>(request);
            command = command with { MerchantId = merchantId.Value };
            
            var result = await _mediator.Send(command);
            
            return result.Match(
                response => Ok(response),
                errors => Problem(errors));
        }

        /// <summary>
        /// Get payment status by ID
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <returns>Payment status</returns>
        /// <response code="200">Returns the payment status</response>
        /// <response code="404">If the payment is not found</response>
        /// <response code="401">If the merchant is not authenticated or not authorized to access this payment</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPaymentStatus(Guid id)
        {
            _logger.LogInformation("Getting payment status for ID: {PaymentId}", id);
            
            var merchantId = _currentMerchantService.GetCurrentMerchantId();
            if (merchantId == null || merchantId == Guid.Empty)
            {
                return Unauthorized(new { message = "Merchant ID not found in authenticated context" });
            }
            
            var query = new GetPaymentStatusQuery
            {
                PaymentId = id,
                MerchantId = merchantId.Value
            };
            
            var result = await _mediator.Send(query);
            
            return result.Match(
                response => Ok(response),
                errors => Problem(errors));
        }
    }
}