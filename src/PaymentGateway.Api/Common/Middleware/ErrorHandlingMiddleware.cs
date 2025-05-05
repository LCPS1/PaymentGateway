using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using PaymentGateway.Application.Common.Exceptions;

namespace PaymentGateway.Api.Common.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);
            
            var errorDetails = new Dictionary<string, string[]>();
            
            // Handle different validation exception types
            if (exception is ApiValidationException apiValidationEx)
            {
                errorDetails = apiValidationEx.Errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            else if (exception is FluentValidation.ValidationException fluentValidationEx)
            {
                errorDetails = fluentValidationEx.Errors
                    .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                    .ToDictionary(g => g.Key, g => g.ToArray());
            }
            else if (exception is System.ComponentModel.DataAnnotations.ValidationException dataAnnotationsEx)
            {
                // Simple case for DataAnnotations.ValidationException which doesn't have detailed errors by default
                errorDetails.Add("Error", new[] { dataAnnotationsEx.Message });
            }
            
            var response = new
            {
                title = GetTitle(exception),
                status = statusCode,
                detail = GetErrorMessage(exception),
                errors = errorDetails
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ApiValidationException => StatusCodes.Status400BadRequest,
                FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
                System.ComponentModel.DataAnnotations.ValidationException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                ForbiddenAccessException => StatusCodes.Status403Forbidden,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private static string GetTitle(Exception exception)
        {
            return exception switch
            {
                ApplicationException applicationException => applicationException.Message,
                ApiValidationException => "Validation Failure",
                FluentValidation.ValidationException => "Validation Failure",
                System.ComponentModel.DataAnnotations.ValidationException => "Validation Failure",
                NotFoundException => "Resource Not Found",
                ForbiddenAccessException => "Forbidden",
                UnauthorizedAccessException => "Unauthorized",
                _ => "Server Error"
            };
        }

        private static string GetErrorMessage(Exception exception)
        {
            return exception switch
            {
                ApiValidationException => "One or more validation errors occurred.",
                FluentValidation.ValidationException => "One or more validation errors occurred.",
                _ => exception.Message
            };
        }
    }
}