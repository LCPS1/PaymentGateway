using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PaymentGateway.Api.Controllers.V1
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class ApiController : ControllerBase
    {
        protected IActionResult Problem(List<Error> errors)
        {
            if (errors.Count == 0)
            {
                return Problem();
            }

            if (errors.All(error => error.Type == ErrorType.Validation))
            {
                return ValidationProblem(errors);
            }

            if (errors.Any(error => error.Type == ErrorType.Unauthorized))
            {
                return Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    detail: errors[0].Description);
            }

            if (errors.Any(error => error.Type == ErrorType.NotFound))
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: errors[0].Description);
            }

            if (errors.Any(error => error.Type == ErrorType.Conflict))
            {
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    detail: errors[0].Description);
            }

            if (errors.Any(error => error.Type == ErrorType.Unexpected))
            {
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: errors[0].Description);
            }

            return Problem();
        }

        private IActionResult ValidationProblem(List<Error> errors)
        {
            var modelStateDictionary = new ModelStateDictionary();

            foreach (var error in errors)
            {
                modelStateDictionary.AddModelError(
                    error.Code,
                    error.Description);
            }

            return ValidationProblem(modelStateDictionary);
        }
    }
}