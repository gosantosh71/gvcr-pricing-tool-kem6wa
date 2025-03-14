using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq; // System.Linq package version 6.0.0
using VatFilingPricingTool.Api.Models.Requests;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Common.Validation;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Api.Validators
{
    /// <summary>
    /// Validates user-related request models to ensure they contain valid data before processing
    /// according to business rules and data integrity requirements.
    /// </summary>
    public class UserRequestValidator
    {
        /// <summary>
        /// Validates a GetUserApiRequest object against business rules.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <returns>Validation result indicating success or failure with error details.</returns>
        public Result ValidateGetUserRequest(GetUserApiRequest request)
        {
            var errors = new List<string>();

            // Validate UserId is a valid GUID
            errors.AddRange(Validators.ValidateGuid(request.UserId, nameof(request.UserId), true));

            if (errors.Any())
            {
                return Result.ValidationFailure(errors, ErrorCodes.User.UserNotFound);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates a GetUsersApiRequest object against business rules.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <returns>Validation result indicating success or failure with error details.</returns>
        public Result ValidateGetUsersRequest(GetUsersApiRequest request)
        {
            var errors = new List<string>();

            // Validate Page number (minimum 1)
            errors.AddRange(Validators.ValidateInteger(request.Page, nameof(request.Page), 1, 10000));

            // Validate PageSize (minimum 1, maximum 100)
            errors.AddRange(Validators.ValidateInteger(request.PageSize, nameof(request.PageSize), 1, 100));

            // Validate SearchTerm (optional, max length 100)
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                errors.AddRange(Validators.ValidateString(request.SearchTerm, nameof(request.SearchTerm), 0, 100, false));
            }

            // Validate RoleFilter (must be a valid UserRole enum value if provided)
            if (request.RoleFilter.HasValue)
            {
                errors.AddRange(Validators.ValidateEnum(request.RoleFilter.Value, nameof(request.RoleFilter)));
            }

            if (errors.Any())
            {
                return Result.ValidationFailure(errors, ErrorCodes.User.InvalidParameters);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates an UpdateUserProfileApiRequest object against business rules.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <returns>Validation result indicating success or failure with error details.</returns>
        public Result ValidateUpdateUserProfileRequest(UpdateUserProfileApiRequest request)
        {
            var errors = new List<string>();

            // Validate FirstName (required, 1-50 chars)
            errors.AddRange(Validators.ValidateString(request.FirstName, nameof(request.FirstName), 1, 50, true));

            // Validate LastName (required, 1-50 chars)
            errors.AddRange(Validators.ValidateString(request.LastName, nameof(request.LastName), 1, 50, true));

            // Validate CompanyName (optional, max 100 chars)
            if (!string.IsNullOrEmpty(request.CompanyName))
            {
                errors.AddRange(Validators.ValidateString(request.CompanyName, nameof(request.CompanyName), 0, 100, false));
            }

            // Validate PhoneNumber (optional, valid format)
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                errors.AddRange(Validators.ValidatePhoneNumber(request.PhoneNumber, nameof(request.PhoneNumber), false));
            }

            // Validate Email (required, valid format)
            errors.AddRange(Validators.ValidateEmail(request.Email, nameof(request.Email), true));

            if (errors.Any())
            {
                return Result.ValidationFailure(errors, ErrorCodes.User.ProfileUpdateFailed);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates an UpdateUserRolesApiRequest object against business rules.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <returns>Validation result indicating success or failure with error details.</returns>
        public Result ValidateUpdateUserRolesRequest(UpdateUserRolesApiRequest request)
        {
            var errors = new List<string>();

            // Validate UserId is a valid GUID
            errors.AddRange(Validators.ValidateGuid(request.UserId, nameof(request.UserId), true));

            // Validate Roles collection (must have at least one role)
            errors.AddRange(Validators.ValidateCollection(request.Roles, nameof(request.Roles), true, 1));

            // Validate each role is a valid UserRole enum value
            if (request.Roles != null)
            {
                foreach (var role in request.Roles)
                {
                    errors.AddRange(Validators.ValidateEnum(role, $"{nameof(request.Roles)} item"));
                }
            }

            if (errors.Any())
            {
                return Result.ValidationFailure(errors, ErrorCodes.User.InvalidRole);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates a ChangeUserStatusApiRequest object against business rules.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <returns>Validation result indicating success or failure with error details.</returns>
        public Result ValidateChangeUserStatusRequest(ChangeUserStatusApiRequest request)
        {
            var errors = new List<string>();

            // Validate UserId is a valid GUID
            errors.AddRange(Validators.ValidateGuid(request.UserId, nameof(request.UserId), true));

            // No need to validate IsActive as it's a boolean which can only be true or false

            if (errors.Any())
            {
                return Result.ValidationFailure(errors, ErrorCodes.User.UserNotFound);
            }

            return Result.Success();
        }
    }
}