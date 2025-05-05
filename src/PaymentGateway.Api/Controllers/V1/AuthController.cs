using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.Authentication.Commands;
using PaymentGateway.Application.Authentication.Queries;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Contracts.Authentication.Requests;

namespace PaymentGateway.Api.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class AuthController : ApiController
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;
        private readonly ICurrentMerchantService _currentMerchantService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ISender mediator,
            IMapper mapper,
            ICurrentMerchantService currentMerchantService,
            ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _currentMerchantService = currentMerchantService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] AuthenticationRequest request)
        {
            _logger.LogInformation("Processing authentication request");
            
            var command = new AuthenticateMerchantCommand 
            { 
                ApiKey = request.ApiKey, 
                ApiSecret = request.ApiSecret 
            };
            
            var authResult = await _mediator.Send(command);
            
            return authResult.Match(
                response => Ok(response),
                errors => Problem(errors));
        }

        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMerchantInfo()
        {
            var merchantId = _currentMerchantService.GetCurrentMerchantId();
            if (merchantId == null || merchantId == Guid.Empty)
            {
                return Unauthorized(new { message = "Invalid merchant credentials" });
            }
            
            _logger.LogInformation("Getting merchant info for ID: {MerchantId}", merchantId);
            
            var query = new GetMerchantInfoQuery
            {
                MerchantId = merchantId.Value
            };
            
            var result = await _mediator.Send(query);
            
            return result.Match(
                response => Ok(response),
                errors => Problem(errors));
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RegisterMerchant([FromBody] CreateMerchantRequest request)
        {
            _logger.LogInformation("Processing merchant registration request");
            
            var command = _mapper.Map<CreateMerchantCommand>(request);
            
            var result = await _mediator.Send(command);
            
            return result.Match(
                response => Ok(response),
                errors => Problem(errors));
        }
        
    }
}