using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Contracts.Payments.Responses;
using PaymentGateway.Domain.Core.Repositories;
using PaymentGateway.Domain.Payments.Services;
using PaymentGateway.Domain.Payments.ValueObjects;
using PaymentGateway.Domain.Common.Errors;
using PaymentGateway.Domain.Payments.Entities;

namespace PaymentGateway.Application.Payments.Commands.ProcessPayment
{
    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, ErrorOr<PaymentResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAcquirerService _acquirerService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProcessPaymentCommandHandler> _logger;

        public ProcessPaymentCommandHandler(
            IUnitOfWork unitOfWork,
            IAcquirerService acquirerService,
            IMapper mapper,
            ILogger<ProcessPaymentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _acquirerService = acquirerService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ErrorOr<PaymentResponse>> Handle(
            ProcessPaymentCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing payment request from merchant {MerchantId}", request.MerchantId);
                
                // Check for duplicate idempotency key if provided
                if (!string.IsNullOrEmpty(request.IdempotencyKey))
                {
                    var existingPayment = await _unitOfWork.Payments.GetByIdempotencyKeyAsync(
                        request.MerchantId, 
                        request.IdempotencyKey);
                        
                    if (existingPayment != null)
                    {
                        _logger.LogInformation("Found existing payment with idempotency key {IdempotencyKey}", 
                            request.IdempotencyKey);
                        return _mapper.Map<PaymentResponse>(existingPayment);
                    }
                }
                
                // Create Money value object
                var money = Money.Create(request.Amount, request.Currency);
                if (money.IsError)
                    return money.Errors;
                    
                // Create Card value object
                var card = Card.Create(
                    request.CardNumber, 
                    request.CardHolderName, 
                    request.ExpiryMonth, 
                    request.ExpiryYear, 
                    request.CVV);
                    
                if (card.IsError)
                    return card.Errors;
                    
                var paymentId = Guid.NewGuid();    
                // Create Payment entity
                var paymentResult = Payment.Create(
                    paymentId,
                    request.MerchantId,
                    money.Value,
                    card.Value,
                    request.IdempotencyKey);
                    
                if (paymentResult.IsError)
                    return paymentResult.Errors;
                    
                var payment = paymentResult.Value;
                
                // Process with acquirer
                try
                {
                    var acquirerResponse = await _acquirerService.ProcessPaymentAsync(payment);
                    
                    if (acquirerResponse.Success)
                    {
                        var markResult = payment.MarkAsSuccessful(acquirerResponse.Reference!);
                        if (markResult.IsError)
                            return markResult.Errors;
                    }
                    else
                    {
                        payment.MarkAsFailed();
                        return Errors.Payment.AcquirerUnavailable;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing payment with acquirer");
                    payment.MarkAsFailed();
                    return Errors.Payment.AcquirerUnavailable;
                }
                
                // Persist the payment
                await _unitOfWork.Payments.AddAsync(payment);
                await _unitOfWork.CompleteAsync();
                
                // Map to response
                var response = _mapper.Map<PaymentResponse>(payment);
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing payment");
                return Error.Unexpected(description: "An unexpected error occurred");
            }
        }
    }
}