using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using PaymentGateway.Contracts.Authentication.Responses;
using PaymentGateway.Contracts.Enums;
using PaymentGateway.Contracts.Payments.Responses;
using PaymentGateway.Domain.Merchants.Entities;
using PaymentGateway.Domain.Payments.Entities;

namespace PaymentGateway.Application.Mappings
{
public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Payment mappings
        config.NewConfig<Payment, PaymentResponse>()
            .Map(dest => dest.PaymentId, src => src.Id)
            .Map(dest => dest.Amount, src => src.Amount.Amount)
            .Map(dest => dest.Currency, src => src.Amount.Currency)
            .Map(dest => dest.CardLast4, src => src.Card.LastFourDigits) 
            .Map(dest => dest.CardBrand, src => src.Card.CardBrand)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Reference, src => src.AcquirerReference) 
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
    
        // Use the enum type for Status
        config.NewConfig<Payment, PaymentStatusResponse>()
            .Map(dest => dest.PaymentId, src => src.Id)
            .Map(dest => dest.Amount, src => src.Amount.Amount)
            .Map(dest => dest.Currency, src => src.Amount.Currency)
            .Map(dest => dest.CardLast4, src => src.Card.LastFourDigits)  
            .Map(dest => dest.CardBrand, src => src.Card.CardBrand)
            .Map(dest => dest.Status, src => src.Status.ToString())  
            .Map(dest => dest.Reference, src => src.AcquirerReference)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        // Merchant mappings
        config.NewConfig<Merchant, CreateMerchantResponse>()
            .Map(dest => dest.MerchantId, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.ApiKey, src => src.ApiKey)
            .Map(dest => dest.ApiSecret, src => src.ApiSecret)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
            
        config.NewConfig<Merchant, MerchantInfoResponse>()
            .Map(dest => dest.MerchantId, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
        }
    }
}