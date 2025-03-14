using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Contracts.V1.Models
{
    /// <summary>
    /// Contract model representing a user in the VAT Filing Pricing Tool system with 
    /// authentication details, profile information, and role-based permissions.
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Email address of the user, used for authentication and communications.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// User's first name.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        /// <summary>
        /// Collection of roles assigned to the user, determining their permissions in the system.
        /// </summary>
        [Required]
        public List<UserRole> Roles { get; set; }

        /// <summary>
        /// Date and time when the user account was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date and time of the user's most recent login, or null if they have never logged in.
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Indicates whether the user account is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The name of the company that the user belongs to.
        /// </summary>
        [StringLength(200)]
        public string CompanyName { get; set; }

        /// <summary>
        /// Contact phone number for the user.
        /// </summary>
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Default constructor for the UserModel.
        /// Initializes the Roles collection, sets CreatedDate to current UTC time,
        /// and sets the user as active by default.
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
            return $"{FirstName} {LastName}";
        }

        /// <summary>
        /// Checks if the user has a specific role.
        /// </summary>
        /// <param name="role">The role to check for.</param>
        /// <returns>True if the user has the specified role, otherwise false.</returns>
        public bool HasRole(UserRole role)
        {
            return Roles?.Contains(role) ?? false;
        }

        /// <summary>
        /// Checks if the user has administrator privileges.
        /// </summary>
        /// <returns>True if the user has the Administrator role, otherwise false.</returns>
        public bool IsAdmin()
        {
            return HasRole(UserRole.Administrator);
        }
    }
}