using System.Collections.Generic; // version 6.0.0
using System.Linq; // version 6.0.0
using System.Net; // version 6.0.0
using Microsoft.AspNetCore.Http; // version 6.0.0
using Microsoft.AspNetCore.Mvc; // version 6.0.0
using Microsoft.AspNetCore.Mvc.Filters; // version 6.0.0
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Common.Models;

namespace VatFilingPricingTool.Api.Filters
{
    /// <summary>
    /// ASP.NET Core action filter that validates model state and returns standardized API responses for validation failures.
    /// This filter ensures consistent validation handling across all API endpoints by intercepting requests with invalid models
    /// before they reach controller actions.
    /// </summary>
    public class ValidationFilter : ActionFilterAttribute
    {
        /// <summary>
        /// Default constructor for the ValidationFilter
        /// </summary>
        public ValidationFilter()
        {
        }

        /// <summary>
        /// Executes before the controller action and validates the model state.
        /// If model state is invalid, the action execution is prevented and a standardized error response is returned.
        /// </summary>
        /// <param name="context">The action executing context.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                // Extract validation errors from model state
                var validationErrors = context.ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors)
                    .Select(x => string.IsNullOrEmpty(x.ErrorMessage) ? "Unknown error" : x.ErrorMessage)
                    .ToList();

                // Create an error response with validation errors
                var response = ApiResponse.CreateError(
                    message: "Validation failed. Please check the request data.",
                    errorCode: ErrorCodes.General.ValidationError,
                    statusCode: (int)HttpStatusCode.BadRequest)
                    .AddValidationErrors(validationErrors);

                // Set the result to prevent action execution
                context.Result = new ObjectResult(response)
                {
                    StatusCode = response.StatusCode
                };
            }
            else
            {
                // If model state is valid, continue with action execution
                base.OnActionExecuting(context);
            }
        }
    }
}