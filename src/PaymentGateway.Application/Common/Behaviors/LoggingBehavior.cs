using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PaymentGateway.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request, 
            RequestHandlerDelegate<TResponse> next, 
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            
            _logger.LogInformation("Handling {RequestName}", requestName);
            
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var response = await next();
                
                stopwatch.Stop();
                
                _logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds}ms", 
                    requestName, stopwatch.ElapsedMilliseconds);
                    
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(ex, "Error handling {RequestName} after {ElapsedMilliseconds}ms", 
                    requestName, stopwatch.ElapsedMilliseconds);
                    
                throw;
            }
        }
    }
}