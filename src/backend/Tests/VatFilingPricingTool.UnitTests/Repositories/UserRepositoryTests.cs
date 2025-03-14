#nullable enable
using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq; // System.Linq package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using Microsoft.EntityFrameworkCore; // Microsoft.EntityFrameworkCore package version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using Moq; // Moq package version 4.18.2
using Xunit; // Xunit package version 2.4.2
using FluentAssertions; // FluentAssertions package version 6.7.0
using VatFilingPricingTool.Data.Context;
using VatFilingPricingTool.Data.Repositories.Implementations;
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.UnitTests.Helpers;

namespace VatFilingPricingTool.UnitTests.Repositories
{
    /// <summary>
    /// Test class for the UserRepository implementation
    /// </summary>
    public class UserRepositoryTests
    {
        private readonly Mock<IVatFilingDbContext> _mockContext;
        private readonly Mock<DbSet<User>> _mockUserDbSet;
        private readonly Mock<ILogger<UserRepository>> _mockLogger;
        private readonly List<User> _users;
        private readonly UserRepository _repository;

        /// <summary>
        /// Initializes a new instance of the UserRepositoryTests class
        /// </summary>
        public UserRepositoryTests()
        {
            // Arrange
            _mockContext = new Mock<IVatFilingDbContext>();
            _mockUserDbSet = new Mock<DbSet<User>>();
            _mockLogger = new Mock<ILogger<UserRepository>>();
            _users = MockData.GetMockUsers();

            // Setup mock DbSet with _users data using Entity Framework Core testing extensions
            _mockUserDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(_users.AsQueryable().Provider);
            _mockUserDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(_users.AsQueryable().Expression);
            _mockUserDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(_users.AsQueryable().ElementType);
            _mockUserDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(_users.AsQueryable().GetEnumerator());

            // Setup _mockContext.Users to return _mockUserDbSet.Object
            _mockContext.Setup(c => c.Users).Returns(_mockUserDbSet.Object);

            // Setup _mockContext.SaveChangesAsync() to return Task.FromResult(1)
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Initialize _repository = new UserRepository(_mockContext.Object, _mockLogger.Object)
            _repository = new UserRepository(_mockContext.Object, _mockLogger.Object);
        }

        /// <summary>
        /// Tests that GetByEmailAsync returns the correct user when a valid email is provided
        /// </summary>
        [Fact]
        public async Task GetByEmailAsync_WithValidEmail_ReturnsUser()
        {
            // Arrange: Get a test user from _users
            var testUser = _users.First();

            // Act: Call _repository.GetByEmailAsync with the test user's email
            var result = await _repository.GetByEmailAsync(testUser.Email);

            // Assert: Verify that the returned user is not null
            result.Should().NotBeNull();

            // Assert: Verify that the returned user's email matches the test user's email
            result.Email.Should().Be(testUser.Email);
        }

        /// <summary>
        /// Tests that GetByEmailAsync returns null when an invalid email is provided
        /// </summary>
        [Fact]
        public async Task GetByEmailAsync_WithInvalidEmail_ReturnsNull()
        {
            // Arrange: Create an invalid email that doesn't exist in _users
            string invalidEmail = "nonexistent@example.com";

            // Act: Call _repository.GetByEmailAsync with the invalid email
            var result = await _repository.GetByEmailAsync(invalidEmail);

            // Assert: Verify that the returned user is null
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests that GetByEmailWithRolesAsync returns the correct user with role information
        /// </summary>
        [Fact]
        public async Task GetByEmailWithRolesAsync_WithValidEmail_ReturnsUserWithRole()
        {
            // Arrange: Get a test user from _users
            var testUser = _users.First();

            // Act: Call _repository.GetByEmailWithRolesAsync with the test user's email
            var result = await _repository.GetByEmailWithRolesAsync(testUser.Email);

            // Assert: Verify that the returned user is not null
            result.Should().NotBeNull();

            // Assert: Verify that the returned user's email matches the test user's email
            result.Email.Should().Be(testUser.Email);

            // Assert: Verify that the returned user's role matches the test user's role
            result.Role.Should().Be(testUser.Role);
        }

        /// <summary>
        /// Tests that GetUsersWithRolesAsync returns all users with their roles
        /// </summary>
        [Fact]
        public async Task GetUsersWithRolesAsync_ReturnsAllUsers()
        {
            // Act: Call _repository.GetUsersWithRolesAsync
            var result = await _repository.GetUsersWithRolesAsync();

            // Assert: Verify that the returned collection is not null
            result.Should().NotBeNull();

            // Assert: Verify that the returned collection count matches _users.Count
            result.Count().Should().Be(_users.Count);

            // Assert: Verify that all users in the returned collection have the expected roles
            foreach (var user in result)
            {
                var expectedUser = _users.FirstOrDefault(u => u.UserId == user.UserId);
                expectedUser.Should().NotBeNull();
                user.Role.Should().Be(expectedUser.Role);
            }
        }

        /// <summary>
        /// Tests that GetUsersByRoleAsync returns users with the specified role
        /// </summary>
        [Fact]
        public async Task GetUsersByRoleAsync_WithValidRole_ReturnsMatchingUsers()
        {
            // Arrange: Select a role to test (e.g., UserRole.Administrator)
            UserRole testRole = UserRole.Administrator;

            // Arrange: Count how many users in _users have this role
            int expectedCount = _users.Count(u => u.Role == testRole);

            // Act: Call _repository.GetUsersByRoleAsync with the selected role
            var result = await _repository.GetUsersByRoleAsync(testRole);

            // Assert: Verify that the returned collection is not null
            result.Should().NotBeNull();

            // Assert: Verify that the returned collection count matches the expected count
            result.Count().Should().Be(expectedCount);

            // Assert: Verify that all users in the returned collection have the specified role
            foreach (var user in result)
            {
                user.Role.Should().Be(testRole);
            }
        }

        /// <summary>
        /// Tests that EmailExistsAsync returns true for an existing email
        /// </summary>
        [Fact]
        public async Task EmailExistsAsync_WithExistingEmail_ReturnsTrue()
        {
            // Arrange: Get a test user from _users
            var testUser = _users.First();

            // Act: Call _repository.EmailExistsAsync with the test user's email
            var result = await _repository.EmailExistsAsync(testUser.Email);

            // Assert: Verify that the result is true
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that EmailExistsAsync returns false for a non-existing email
        /// </summary>
        [Fact]
        public async Task EmailExistsAsync_WithNonExistingEmail_ReturnsFalse()
        {
            // Arrange: Create an email that doesn't exist in _users
            string nonExistingEmail = "nonexisting@example.com";

            // Act: Call _repository.EmailExistsAsync with the non-existing email
            var result = await _repository.EmailExistsAsync(nonExistingEmail);

            // Assert: Verify that the result is false
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that AddRoleToUserAsync updates a user's role correctly
        /// </summary>
        [Fact]
        public async Task AddRoleToUserAsync_WithValidUserIdAndRole_UpdatesUserRole()
        {
            // Arrange: Get a test user from _users with a non-Administrator role
            var testUser = _users.First(u => u.Role != UserRole.Administrator);
            
            // Arrange: Setup _mockUserDbSet.FindAsync to return the test user
            _mockUserDbSet.Setup(x => x.FindAsync(testUser.UserId)).ReturnsAsync(testUser);

            // Act: Call _repository.AddRoleToUserAsync with the test user's ID and UserRole.Administrator
            var result = await _repository.AddRoleToUserAsync(testUser.UserId, UserRole.Administrator);

            // Assert: Verify that the result is true
            result.Should().BeTrue();

            // Assert: Verify that the test user's role was updated to Administrator
            testUser.Role.Should().Be(UserRole.Administrator);

            // Assert: Verify that _mockContext.SaveChangesAsync was called once
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// Tests that AddRoleToUserAsync returns false for an invalid user ID
        /// </summary>
        [Fact]
        public async Task AddRoleToUserAsync_WithInvalidUserId_ReturnsFalse()
        {
            // Arrange: Create an invalid user ID
            string invalidUserId = "invalid-user-id";

            // Arrange: Setup _mockUserDbSet.FindAsync to return null
            _mockUserDbSet.Setup(x => x.FindAsync(invalidUserId)).ReturnsAsync((User)null);

            // Act: Call _repository.AddRoleToUserAsync with the invalid user ID and any role
            var result = await _repository.AddRoleToUserAsync(invalidUserId, UserRole.Administrator);

            // Assert: Verify that the result is false
            result.Should().BeFalse();

            // Assert: Verify that _mockContext.SaveChangesAsync was not called
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
        }

        /// <summary>
        /// Tests that RemoveRoleFromUserAsync updates a user's role correctly
        /// </summary>
        [Fact]
        public async Task RemoveRoleFromUserAsync_WithValidUserIdAndRole_UpdatesUserRole()
        {
            // Arrange: Get a test user from _users with Administrator role
            var testUser = _users.First(u => u.Role == UserRole.Administrator);

            // Arrange: Setup _mockUserDbSet.FindAsync to return the test user
            _mockUserDbSet.Setup(x => x.FindAsync(testUser.UserId)).ReturnsAsync(testUser);

            // Act: Call _repository.RemoveRoleFromUserAsync with the test user's ID and UserRole.Administrator
            var result = await _repository.RemoveRoleFromUserAsync(testUser.UserId, UserRole.Administrator);

            // Assert: Verify that the result is true
            result.Should().BeTrue();

            // Assert: Verify that the test user's role was updated to Customer (default role)
            testUser.Role.Should().Be(UserRole.Customer);

            // Assert: Verify that _mockContext.SaveChangesAsync was called once
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// Tests that RemoveRoleFromUserAsync returns false for an invalid user ID
        /// </summary>
        [Fact]
        public async Task RemoveRoleFromUserAsync_WithInvalidUserId_ReturnsFalse()
        {
            // Arrange: Create an invalid user ID
            string invalidUserId = "invalid-user-id";

            // Arrange: Setup _mockUserDbSet.FindAsync to return null
            _mockUserDbSet.Setup(x => x.FindAsync(invalidUserId)).ReturnsAsync((User)null);

            // Act: Call _repository.RemoveRoleFromUserAsync with the invalid user ID and any role
            var result = await _repository.RemoveRoleFromUserAsync(invalidUserId, UserRole.Administrator);

            // Assert: Verify that the result is false
            result.Should().BeFalse();

            // Assert: Verify that _mockContext.SaveChangesAsync was not called
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
        }

        /// <summary>
        /// Tests that UpdateUserProfileAsync updates a user's profile correctly
        /// </summary>
        [Fact]
        public async Task UpdateUserProfileAsync_WithValidData_UpdatesUserProfile()
        {
            // Arrange: Get a test user from _users
            var testUser = _users.First();

            // Arrange: Setup _mockUserDbSet.FindAsync to return the test user
            _mockUserDbSet.Setup(x => x.FindAsync(testUser.UserId)).ReturnsAsync(testUser);

            // Arrange: Create new profile data (firstName, lastName)
            string newFirstName = "UpdatedFirstName";
            string newLastName = "UpdatedLastName";

            // Act: Call _repository.UpdateUserProfileAsync with the test user's ID and new profile data
            var result = await _repository.UpdateUserProfileAsync(testUser.UserId, newFirstName, newLastName, null, null);

            // Assert: Verify that the returned user is not null
            result.Should().NotBeNull();

            // Assert: Verify that the user's FirstName and LastName were updated correctly
            result.FirstName.Should().Be(newFirstName);
            result.LastName.Should().Be(newLastName);

            // Assert: Verify that _mockContext.SaveChangesAsync was called once
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateUserProfileAsync returns null for an invalid user ID
        /// </summary>
        [Fact]
        public async Task UpdateUserProfileAsync_WithInvalidUserId_ReturnsNull()
        {
            // Arrange: Create an invalid user ID
            string invalidUserId = "invalid-user-id";

            // Arrange: Setup _mockUserDbSet.FindAsync to return null
            _mockUserDbSet.Setup(x => x.FindAsync(invalidUserId)).ReturnsAsync((User)null);

            // Act: Call _repository.UpdateUserProfileAsync with the invalid user ID and any profile data
            var result = await _repository.UpdateUserProfileAsync(invalidUserId, "NewFirstName", "NewLastName", null, null);

            // Assert: Verify that the result is null
            result.Should().BeNull();

            // Assert: Verify that _mockContext.SaveChangesAsync was not called
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
        }

        /// <summary>
        /// Tests that UpdateLastLoginDateAsync updates a user's last login date correctly
        /// </summary>
        [Fact]
        public async Task UpdateLastLoginDateAsync_WithValidUserId_UpdatesLoginDate()
        {
            // Arrange: Get a test user from _users
            var testUser = _users.First();

            // Arrange: Store the user's current LastLoginDate
            DateTime originalLastLoginDate = testUser.LastLoginDate;

            // Arrange: Setup _mockUserDbSet.FindAsync to return the test user
            _mockUserDbSet.Setup(x => x.FindAsync(testUser.UserId)).ReturnsAsync(testUser);

            // Act: Call _repository.UpdateLastLoginDateAsync with the test user's ID
            var result = await _repository.UpdateLastLoginDateAsync(testUser.UserId);

            // Assert: Verify that the result is true
            result.Should().BeTrue();

            // Assert: Verify that the user's LastLoginDate was updated to a more recent time
            testUser.LastLoginDate.Should().BeAfter(originalLastLoginDate);

            // Assert: Verify that _mockContext.SaveChangesAsync was called once
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateLastLoginDateAsync returns false for an invalid user ID
        /// </summary>
        [Fact]
        public async Task UpdateLastLoginDateAsync_WithInvalidUserId_ReturnsFalse()
        {
            // Arrange: Create an invalid user ID
            string invalidUserId = "invalid-user-id";

            // Arrange: Setup _mockUserDbSet.FindAsync to return null
            _mockUserDbSet.Setup(x => x.FindAsync(invalidUserId)).ReturnsAsync((User)null);

            // Act: Call _repository.UpdateLastLoginDateAsync with the invalid user ID
            var result = await _repository.UpdateLastLoginDateAsync(invalidUserId);

            // Assert: Verify that the result is false
            result.Should().BeFalse();

            // Assert: Verify that _mockContext.SaveChangesAsync was not called
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
        }

        /// <summary>
        /// Tests that ActivateUserAsync activates a user correctly
        /// </summary>
        [Fact]
        public async Task ActivateUserAsync_WithValidUserId_ActivatesUser()
        {
            // Arrange: Get a test user from _users
            var testUser = _users.First();

            // Arrange: Set the user's IsActive property to false
            testUser.Deactivate();

            // Arrange: Setup _mockUserDbSet.FindAsync to return the test user
            _mockUserDbSet.Setup(x => x.FindAsync(testUser.UserId)).ReturnsAsync(testUser);

            // Act: Call _repository.ActivateUserAsync with the test user's ID
            var result = await _repository.ActivateUserAsync(testUser.UserId);

            // Assert: Verify that the result is true
            result.Should().BeTrue();

            // Assert: Verify that the user's IsActive property is now true
            testUser.IsActive.Should().BeTrue();

            // Assert: Verify that _mockContext.SaveChangesAsync was called once
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// Tests that ActivateUserAsync returns false for an invalid user ID
        /// </summary>
        [Fact]
        public async Task ActivateUserAsync_WithInvalidUserId_ReturnsFalse()
        {
            // Arrange: Create an invalid user ID
            string invalidUserId = "invalid-user-id";

            // Arrange: Setup _mockUserDbSet.FindAsync to return null
            _mockUserDbSet.Setup(x => x.FindAsync(invalidUserId)).ReturnsAsync((User)null);

            // Act: Call _repository.ActivateUserAsync with the invalid user ID
            var result = await _repository.ActivateUserAsync(invalidUserId);

            // Assert: Verify that the result is false
            result.Should().BeFalse();

            // Assert: Verify that _mockContext.SaveChangesAsync was not called
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
        }

        /// <summary>
        /// Tests that DeactivateUserAsync deactivates a user correctly
        /// </summary>
        [Fact]
        public async Task DeactivateUserAsync_WithValidUserId_DeactivatesUser()
        {
            // Arrange: Get a test user from _users
            var testUser = _users.First();

            // Arrange: Set the user's IsActive property to true
            testUser.Activate();

            // Arrange: Setup _mockUserDbSet.FindAsync to return the test user
            _mockUserDbSet.Setup(x => x.FindAsync(testUser.UserId)).ReturnsAsync(testUser);

            // Act: Call _repository.DeactivateUserAsync with the test user's ID
            var result = await _repository.DeactivateUserAsync(testUser.UserId);

            // Assert: Verify that the result is true
            result.Should().BeTrue();

            // Assert: Verify that the user's IsActive property is now false
            testUser.IsActive.Should().BeFalse();

            // Assert: Verify that _mockContext.SaveChangesAsync was called once
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// Tests that DeactivateUserAsync returns false for an invalid user ID
        /// </summary>
        [Fact]
        public async Task DeactivateUserAsync_WithInvalidUserId_ReturnsFalse()
        {
            // Arrange: Create an invalid user ID
            string invalidUserId = "invalid-user-id";

            // Arrange: Setup _mockUserDbSet.FindAsync to return null
            _mockUserDbSet.Setup(x => x.FindAsync(invalidUserId)).ReturnsAsync((User)null);

            // Act: Call _repository.DeactivateUserAsync with the invalid user ID
            var result = await _repository.DeactivateUserAsync(invalidUserId);

            // Assert: Verify that the result is false
            result.Should().BeFalse();

            // Assert: Verify that _mockContext.SaveChangesAsync was not called
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
        }
    }
}
#nullable restore