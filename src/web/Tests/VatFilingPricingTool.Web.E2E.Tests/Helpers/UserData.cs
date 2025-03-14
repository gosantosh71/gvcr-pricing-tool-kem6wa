using System; // System v6.0.0
using VatFilingPricingTool.Web.E2E.Tests.Helpers;

namespace VatFilingPricingTool.Web.E2E.Tests.Helpers
{
    /// <summary>
    /// Represents a user account for testing purposes with credentials and role information
    /// </summary>
    public class TestUser
    {
        /// <summary>
        /// The email address of the test user
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The password of the test user
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The first name of the test user
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of the test user
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The role assigned to the test user
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Initializes a new instance of the TestUser class
        /// </summary>
        /// <param name="email">The email address of the test user</param>
        /// <param name="password">The password of the test user</param>
        /// <param name="firstName">The first name of the test user</param>
        /// <param name="lastName">The last name of the test user</param>
        /// <param name="role">The role assigned to the test user</param>
        public TestUser(string email, string password, string firstName, string lastName, string role)
        {
            Email = email;
            Password = password;
            FirstName = firstName;
            LastName = lastName;
            Role = role;
        }

        /// <summary>
        /// Gets the full name of the user by combining first and last name
        /// </summary>
        /// <returns>The full name of the user</returns>
        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }

    /// <summary>
    /// Provides methods to retrieve predefined test users for different roles
    /// </summary>
    public class UserData
    {
        /// <summary>
        /// Initializes a new instance of the UserData class
        /// </summary>
        public UserData()
        {
            // No initialization required
        }

        /// <summary>
        /// Gets a standard user account for testing
        /// </summary>
        /// <returns>A standard user with Customer role</returns>
        public TestUser GetStandardUser()
        {
            return new TestUser(
                TestSettings.TestUserEmail,
                TestSettings.TestUserPassword,
                "John",
                "Smith",
                "Customer");
        }

        /// <summary>
        /// Gets an administrator user account for testing
        /// </summary>
        /// <returns>An admin user with Administrator role</returns>
        public TestUser GetAdminUser()
        {
            return new TestUser(
                TestSettings.AdminUserEmail,
                TestSettings.AdminUserPassword,
                "Admin",
                "User",
                "Administrator");
        }

        /// <summary>
        /// Gets an accountant user account for testing
        /// </summary>
        /// <returns>An accountant user with Accountant role</returns>
        public TestUser GetAccountantUser()
        {
            return new TestUser(
                TestSettings.AccountantUserEmail,
                TestSettings.AccountantUserPassword,
                "Accountant",
                "User",
                "Accountant");
        }

        /// <summary>
        /// Gets a pricing administrator user account for testing
        /// </summary>
        /// <returns>A pricing admin user with PricingAdministrator role</returns>
        public TestUser GetPricingAdminUser()
        {
            return new TestUser(
                TestSettings.PricingAdminUserEmail,
                TestSettings.PricingAdminUserPassword,
                "Pricing",
                "Admin",
                "PricingAdministrator");
        }

        /// <summary>
        /// Gets an invalid user account for negative testing
        /// </summary>
        /// <returns>An invalid user with incorrect credentials</returns>
        public TestUser GetInvalidUser()
        {
            return new TestUser(
                "invalid@example.com",
                "WrongPassword123!",
                "Invalid",
                "User",
                "None");
        }
    }
}