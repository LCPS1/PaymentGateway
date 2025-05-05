using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Contracts.Payments.Responses;
using PaymentGateway.Domain.Common.Errors;
using PaymentGateway.Domain.Core.Repositories;


namespace PaymentGateway.Application.Payments.Queries.GetPaymentStatus
{
    public class GetPaymentStatusQueryHandler : IRequestHandler<GetPaymentStatusQuery, ErrorOr<PaymentStatusResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPaymentStatusQueryHandler> _logger;

        public GetPaymentStatusQueryHandler(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            IMapper mapper,
            ILogger<GetPaymentStatusQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ErrorOr<PaymentStatusResponse>> Handle(
            GetPaymentStatusQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting payment status for payment {PaymentId} by merchant {MerchantId}", 
                    request.PaymentId, request.MerchantId);
                
                // Try to get from cache first
                var cacheKey = $"payment:{request.PaymentId}";
                var cachedResponse = await _cacheService.GetAsync<PaymentStatusResponse>(cacheKey);
                
                if (cachedResponse != null)
                {
                    _logger.LogInformation("Payment {PaymentId} status found in cache", request.PaymentId);
                    return cachedResponse;
                }
                
                // Get payment by ID from database
                var payment = await _unitOfWork.Payments.GetByIdAsync(request.PaymentId);
                
                // Check if payment exists
                if (payment is null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found", request.PaymentId);
                    return Errors.Payment.NotFound;
                }
                    
                // Verify merchant can access this payment
                if (payment.MerchantId.ToString() != request.MerchantId.ToString())
                {
                    _logger.LogWarning("Merchant {MerchantId} not authorized to access payment {PaymentId}", 
                        request.MerchantId, request.PaymentId);
                    return Errors.Payment.Unauthorized;
                }
                                    
                // Map to response
                var response = _mapper.Map<PaymentStatusResponse>(payment);
                
                // Store in cache
                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment status for payment {PaymentId}", request.PaymentId);
                return Error.Unexpected(description: "An unexpected error occurred");
            }
        }
    }
}