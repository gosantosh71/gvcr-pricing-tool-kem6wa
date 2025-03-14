using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq; // System.Linq package version 6.0.0
using System.Security.Claims; // System.Security.Claims package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using Microsoft.AspNetCore.Authorization; // Microsoft.AspNetCore.Authorization package version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using VatFilingPricingTool.Domain.Enums; // For UserRole enum
using VatFilingPricingTool.Web.Models; // For UserModel class

namespace VatFilingPricingTool.Web.Handlers
{
    /// <summary>
    /// Implements a custom authorization handler for the VAT Filing Pricing Tool web application
    /// that enforces role-based access control and resource-specific authorization policies.
    /// </summary>
    public class CustomAuthorizationHandler : IAuthorizationHandler
    {
        private readonly ILogger<CustomAuthorizationHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging authorization decisions.</param>
        public CustomAuthorizationHandler(ILogger<CustomAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles authorization requirements by evaluating them against the user's claims.
        /// </summary>
        /// <param name="context">The authorization context containing requirements and user.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            _logger.LogInformation("Evaluating authorization requirements");

            // Extract the user from the context
            var user = context.User;

            // If the user is not authenticated, return without success
            if (!user.Identity.IsAuthenticated)
            {
                _logger.LogInformation("User is not authenticated");
                return;
            }

            // Handle each requirement
            foreach (var requirement in context.Requirements)
            {
                if (requirement is RoleRequirement roleRequirement)
                {
                    HandleRoleRequirement(context, roleRequirement);
                }
                else if (requirement is ResourceOwnerRequirement resourceRequirement)
                {
                    HandleResourceOwnerRequirement(context, resourceRequirement);
                }
                else if (requirement is PermissionRequirement permissionRequirement)
                {
                    HandlePermissionRequirement(context, permissionRequirement);
                }
            }

            _logger.LogInformation("Authorization evaluation completed");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Handles role-based authorization requirements.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The role requirement to evaluate.</param>
        private void HandleRoleRequirement(AuthorizationHandlerContext context, RoleRequirement requirement)
        {
            var userRoles = GetUserRoles(context.User);
            
            // If the user has the Administrator role, they can access anything
            if (userRoles.Contains(UserRole.Administrator))
            {
                context.Succeed(requirement);
                _logger.LogInformation("User has Administrator role, granting access");
                return;
            }

            // Check if the user has any of the required roles
            var hasRequiredRole = userRoles.Any(role => requirement.AllowedRoles.Contains(role));
            
            if (hasRequiredRole)
            {
                context.Succeed(requirement);
                _logger.LogInformation("User has required role, granting access");
            }
            else
            {
                _logger.LogInformation("User does not have required role, access denied");
            }
        }

        /// <summary>
        /// Handles resource ownership authorization requirements.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The resource owner requirement to evaluate.</param>
        private void HandleResourceOwnerRequirement(AuthorizationHandlerContext context, ResourceOwnerRequirement requirement)
        {
            var userId = GetUserId(context.User);
            
            // If the user is the owner of the resource, grant access
            if (userId == requirement.ResourceOwnerId)
            {
                context.Succeed(requirement);
                _logger.LogInformation("User is the resource owner, granting access");
                return;
            }
            
            // If the user has the Administrator role, they can access any resource
            var userRoles = GetUserRoles(context.User);
            if (userRoles.Contains(UserRole.Administrator))
            {
                context.Succeed(requirement);
                _logger.LogInformation("User has Administrator role, granting access to resource");
                return;
            }
            
            _logger.LogInformation("User is not the resource owner, access denied");
        }

        /// <summary>
        /// Handles permission-based authorization requirements.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The permission requirement to evaluate.</param>
        private void HandlePermissionRequirement(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var userRoles = GetUserRoles(context.User);
            
            // Administrators have all permissions
            if (userRoles.Contains(UserRole.Administrator))
            {
                context.Succeed(requirement);
                _logger.LogInformation("User has Administrator role, granting permission");
                return;
            }
            
            // Handle specific permissions based on user roles
            var permission = requirement.Permission;
            
            // PricingAdministrator can access pricing-related features
            if (userRoles.Contains(UserRole.PricingAdministrator) && 
                (permission.StartsWith("pricing") || permission.StartsWith("rule")))
            {
                context.Succeed(requirement);
                _logger.LogInformation("User has PricingAdministrator role, granting pricing permission");
                return;
            }
            
            // Accountants can access calculation and reporting features
            if (userRoles.Contains(UserRole.Accountant) && 
                (permission.StartsWith("calculation") || permission.StartsWith("report")))
            {
                context.Succeed(requirement);
                _logger.LogInformation("User has Accountant role, granting calculation/report permission");
                return;
            }
            
            // Customers can only access their own data
            if (userRoles.Contains(UserRole.Customer) && 
                permission.StartsWith("view") && permission.Contains("own"))
            {
                context.Succeed(requirement);
                _logger.LogInformation("User has Customer role, granting view own data permission");
                return;
            }
            
            _logger.LogInformation("User does not have required permission, access denied");
        }

        /// <summary>
        /// Extracts the user roles from the ClaimsPrincipal.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal representing the user.</param>
        /// <returns>A list of user roles.</returns>
        private List<UserRole> GetUserRoles(ClaimsPrincipal user)
        {
            var roles = new List<UserRole>();
            var roleClaims = user.FindAll(ClaimTypes.Role);
            
            foreach (var claim in roleClaims)
            {
                if (Enum.TryParse<UserRole>(claim.Value, out var role))
                {
                    roles.Add(role);
                }
            }
            
            return roles;
        }

        /// <summary>
        /// Extracts the user ID from the ClaimsPrincipal.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal representing the user.</param>
        /// <returns>The user ID or null if not found.</returns>
        private string GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }

    /// <summary>
    /// Represents a role-based authorization requirement that allows access based on user roles.
    /// </summary>
    public class RoleRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets the list of roles that are allowed to access the resource.
        /// </summary>
        public List<UserRole> AllowedRoles { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleRequirement"/> class.
        /// </summary>
        /// <param name="allowedRoles">The roles that are allowed to access the resource.</param>
        public RoleRequirement(params UserRole[] allowedRoles)
        {
            AllowedRoles = new List<UserRole>();
            if (allowedRoles != null)
            {
                AllowedRoles.AddRange(allowedRoles);
            }
        }
    }

    /// <summary>
    /// Represents a resource ownership authorization requirement that allows access
    /// based on whether the user owns the resource.
    /// </summary>
    public class ResourceOwnerRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets the ID of the resource owner.
        /// </summary>
        public string ResourceOwnerId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceOwnerRequirement"/> class.
        /// </summary>
        /// <param name="resourceOwnerId">The ID of the resource owner.</param>
        public ResourceOwnerRequirement(string resourceOwnerId)
        {
            ResourceOwnerId = resourceOwnerId;
        }
    }

    /// <summary>
    /// Represents a permission-based authorization requirement that allows access
    /// based on whether the user has the required permission.
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets the permission that is required to access the resource.
        /// </summary>
        public string Permission { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionRequirement"/> class.
        /// </summary>
        /// <param name="permission">The permission that is required to access the resource.</param>
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}