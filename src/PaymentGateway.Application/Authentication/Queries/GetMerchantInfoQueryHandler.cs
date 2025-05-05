using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Contracts.Authentication.Responses;
using PaymentGateway.Domain.Core.Repositories;
using PaymentGateway.Domain.Common.Errors;

namespace PaymentGateway.Application.Authentication.Queries
{
    public class GetMerchantInfoQueryHandler : IRequestHandler<GetMerchantInfoQuery, ErrorOr<MerchantInfoResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetMerchantInfoQueryHandler> _logger;

        public GetMerchantInfoQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetMerchantInfoQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

       public async Task<ErrorOr<MerchantInfoResponse>> Handle(
            GetMerchantInfoQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting merchant info for merchant ID: {MerchantId}", request.MerchantId);
                
                // Get merchant - no parsing needed since MerchantId is already Guid
                var merchant = await _unitOfWork.Merchants.GetByIdAsync(request.MerchantId);
                
                if (merchant is null)
                {
                    _logger.LogWarning("Merchant not found with ID: {MerchantId}", request.MerchantId);
                    return Errors.Merchant.NotFound;
                }
                
                // Map to response
                var response = _mapper.Map<MerchantInfoResponse>(merchant);
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving merchant info for merchant ID: {MerchantId}", request.MerchantId);
                return Error.Unexpected(description: "An unexpected error occurred");
            }
        }
    }
}