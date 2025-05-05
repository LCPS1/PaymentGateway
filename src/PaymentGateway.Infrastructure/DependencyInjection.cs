using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PaymentGateway.Domain.Core.Repositories;
using PaymentGateway.Domain.Merchants.Repositories;
using PaymentGateway.Domain.Payments.Repositories;
using PaymentGateway.Domain.Payments.Services;
using PaymentGateway.Infrastructure.Authentication.Models;
using PaymentGateway.Infrastructure.Cache;
using PaymentGateway.Infrastructure.Cache.Options;
using PaymentGateway.Infrastructure.Payments.Options;
using PaymentGateway.Infrastructure.Payments.Services;
using PaymentGateway.Infrastructure.Persistence;
using PaymentGateway.Infrastructure.Persistence.Repositories;
using Polly;
using Polly.Extensions.Http;
using System.Security.Claims;
using PaymentGateway.Infrastructure.Authentication.Services;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Infrastructure.Common.Extensions;

namespace PaymentGateway.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Add database - using SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            });
            

            // Add repositories
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IMerchantRepository, MerchantRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Register HTTP context accessor for security
            services.AddHttpContextAccessor();
            
            // Register security services
            services.AddScoped<ICurrentMerchantService, CurrentMerchantService>();
            
            // Register settings from configuration
            services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
            services.Configure<AcquirerSettings>(configuration.GetSection(nameof(AcquirerSettings)));
            services.Configure<CacheSettings>(configuration.GetSection(nameof(CacheSettings)));
            
            // Add Redis cache
            services.AddStackExchangeRedisCache(options =>
            {
                var cacheSettings = configuration.GetSection(nameof(CacheSettings)).Get<CacheSettings>();
                options.Configuration = cacheSettings?.ConnectionString ?? configuration.GetConnectionString("Redis");
                options.InstanceName = cacheSettings?.InstanceName ?? "PaymentGateway:";
            });
            
            services.AddScoped<ICacheService, RedisCacheService>();
            
            // Add authentication service
            services.AddScoped<IMerchantAuthenticationService, MerchantAuthenticationService>();
            
            // Configure JWT authentication
            ConfigureAuthentication(services, configuration);
            
            // Configure HttpClient for Acquirer Service
            ConfigureAcquirerService(services, configuration);
            
            return services;
        }
        
        private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>() 
                ?? new JwtSettings();
                
            if (string.IsNullOrEmpty(jwtSettings.Secret))
            {
                throw new InvalidOperationException("JWT Secret is not configured");
            }
                
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                    ValidateIssuer = !string.IsNullOrEmpty(jwtSettings.Issuer),
                    ValidateAudience = !string.IsNullOrEmpty(jwtSettings.Audience),
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                
                // Configure event handlers for logging and debugging
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogError(context.Exception, "Authentication failed: {Message}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogInformation("Token validated for subject: {Subject}", 
                            context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogWarning("Authentication challenge issued: {Path}", context.Request.Path);
                        return Task.CompletedTask;
                    }
                };
            });
            
            // Add authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("MerchantOnly", policy =>
                    policy.RequireClaim("merchant_id"));
                    
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("admin"));
            });
        }
        
        private static void ConfigureAcquirerService(IServiceCollection services, IConfiguration configuration)
        {
            var acquirerSettings = configuration.GetSection(nameof(AcquirerSettings)).Get<AcquirerSettings>() 
                ?? new AcquirerSettings();
                
            services.AddHttpClient<IAcquirerService, AcquirerService>(client =>
            {
                client.BaseAddress = new Uri(acquirerSettings.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(acquirerSettings.TimeoutSeconds);
                
                // Add default headers if needed
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "PaymentGateway/1.0");
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());
        }

        // Retry policy for transient errors and 429 Too Many Requests
       private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    3, 
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30));
        }


        public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
            
            try
            {
                logger.LogInformation("Starting database initialization");
                
                // Get the DbContext
                var dbContext = services.GetRequiredService<ApplicationDbContext>();
                
                // IMPORTANT: First delete any existing database to ensure clean schema
                logger.LogInformation("Ensuring clean database schema by recreating the database");
                await dbContext.Database.EnsureDeletedAsync();
                
                // Create the database with tables
                logger.LogInformation("Creating database schema");
                await dbContext.Database.EnsureCreatedAsync();
                
                // Force connection to close and reopen to ensure schema changes are committed
                await dbContext.Database.CloseConnectionAsync();
                
                // Verify tables exist with direct SQL
                var connection = dbContext.Database.GetDbConnection();
                await connection.OpenAsync();
                
                try
                {
                    // Check both tables exist
                    using var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME IN ('Merchants', 'Payments')";
                        
                    var tableCount = Convert.ToInt32(await command.ExecuteScalarAsync());
                    logger.LogInformation("Found {tableCount}/2 required tables", tableCount);
                    

                    
                    if (tableCount != 2)
                    {
                        throw new Exception($"Database schema creation failed - found {tableCount}/2 required tables");
                    }
                }
                finally
                {
                    await connection.CloseAsync();
                }
                
                // Now seed the data
                logger.LogInformation("Seeding database");
                await DatabaseInitializer.SeedDirectAsync(dbContext, services);
                
                logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database initialization");
                throw;
            }
        }
    }
}