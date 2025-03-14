using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Service.Models
{
    /// <summary>
    /// Service model representing a user in the VAT Filing Pricing Tool system with authentication details, 
    /// profile information, and role-based permissions.
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Email address of the user, also used for authentication.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// First name of the user.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the user.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Collection of roles assigned to the user, determining their permissions.
        /// </summary>
        public List<UserRole> Roles { get; set; }

        /// <summary>
        /// Date and time when the user account was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date and time of the user's last login, null if they haven't logged in yet.
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Indicates whether the user account is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Name of the company the user is associated with.
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Contact phone number for the user.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Azure Active Directory Object ID for users authenticated through Azure AD.
        /// </summary>
        public string AzureAdObjectId { get; set; }

        /// <summary>
        /// Default constructor for the UserModel.
        /// </summary>
        public UserModel()
        {
            Roles = new List<UserRole>();
            CreatedDate = DateTime.UtcNow;
            IsActive = true;
        }

        /// <summary>
        /// Returns the full name of the user by combining first and last name.
        /// </summary>
        /// <returns>The full name of the user.</returns>
        public string GetFullName()
        {
            return FirstName + " " + LastName;
        }

        /// <summary>
        /// Checks if the user has a specific role.
        /// </summary>
        /// <param name="role">The role to check for.</param>
        /// <returns>True if the user has the specified role, otherwise false.</returns>
        public bool HasRole(UserRole role)
        {
            return Roles.Contains(role);
        }

        /// <summary>
        /// Checks if the user has administrator privileges.
        /// </summary>
        /// <returns>True if the user has the Administrator role, otherwise false.</returns>
        public bool IsAdmin()
        {
            return Roles.Contains(UserRole.Administrator);
        }

        /// <summary>
        /// Checks if the user has pricing administrator privileges.
        /// </summary>
        /// <returns>True if the user has the PricingAdministrator role, otherwise false.</returns>
        public bool IsPricingAdmin()
        {
            return Roles.Contains(UserRole.PricingAdministrator);
        }

        /// <summary>
        /// Checks if the user has accountant privileges.
        /// </summary>
        /// <returns>True if the user has the Accountant role, otherwise false.</returns>
        public bool IsAccountant()
        {
            return Roles.Contains(UserRole.Accountant);
        }

        /// <summary>
        /// Adds a role to the user if they don't already have it.
        /// </summary>
        /// <param name="role">The role to add.</param>
        public void AddRole(UserRole role)
        {
            if (!Roles.Contains(role))
            {
                Roles.Add(role);
            }
        }

        /// <summary>
        /// Removes a role from the user if they have it.
        /// </summary>
        /// <param name="role">The role to remove.</param>
        public void RemoveRole(UserRole role)
        {
            if (Roles.Contains(role))
            {
                Roles.Remove(role);
            }
        }

        /// <summary>
        /// Creates a UserModel from a domain User entity.
        /// </summary>
        /// <param name="user">The domain User entity to convert.</param>
        /// <returns>A new UserModel populated with data from the domain entity.</returns>
        public static UserModel FromDomain(User user)
        {
            // Validate that user is not null
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Create a new UserModel instance
            var userModel = new UserModel();

            // Set properties from the domain entity
            userModel.UserId = user.UserId;
            userModel.Email = user.Email;
            userModel.FirstName = user.FirstName;
            userModel.LastName = user.LastName;
            userModel.Roles.Add(user.Role); // Add user.Role to Roles list
            userModel.CreatedDate = user.CreatedDate;
            userModel.LastLoginDate = user.LastLoginDate;
            userModel.IsActive = user.IsActive;
            userModel.AzureAdObjectId = user.AzureAdObjectId;
            userModel.CompanyName = user.CompanyName;
            userModel.PhoneNumber = user.PhoneNumber;

            // Return the populated UserModel
            return userModel;
        }
    }
}