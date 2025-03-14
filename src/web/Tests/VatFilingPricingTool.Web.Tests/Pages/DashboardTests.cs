using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Moq;
using Xunit;
using VatFilingPricingTool.Web.Pages;
using VatFilingPricingTool.Web.Services.Interfaces;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Tests.Helpers;

namespace VatFilingPricingTool.Web.Tests.Pages
{
    /// <summary>
    /// Test class for the Dashboard component containing unit tests for various scenarios
    /// </summary>
    public class DashboardTests : IDisposable
    {
        private readonly TestContext _testContext;
        private readonly Mock<IAuthService> _mockAuthService;

        /// <summary>
        /// Initializes a new instance of the DashboardTests class with test context and mocked services
        /// </summary>
        public DashboardTests()
        {
            _testContext = RenderComponent.CreateTestContext();
            _mockAuthService = RenderComponent.CreateMockService<IAuthService>(_testContext);
        }

        /// <summary>
        /// Disposes the test context after tests are complete
        /// </summary>
        public void Dispose()
        {
            _testContext.Dispose();
        }

        /// <summary>
        /// Verifies that the dashboard displays the user's name when authenticated
        /// </summary>
        [Fact]
        public async Task Dashboard_WhenAuthenticated_DisplaysUserName()
        {
            // Arrange
            var testUser = TestData.CreateTestUser("user-123", "test@example.com");
            _mockAuthService.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync(testUser);
            RenderComponent.SetupAuthenticatedUser(_testContext, "user-123", "test@example.com");

            // Act
            var cut = _testContext.RenderComponent<Dashboard>();

            // Assert
            cut.MarkupMatches(m => m.Contains("Welcome back, Test User"));
        }

        /// <summary>
        /// Verifies that the dashboard displays a generic welcome message when not authenticated
        /// </summary>
        [Fact]
        public async Task Dashboard_WhenUnauthenticated_DisplaysGenericWelcome()
        {
            // Arrange
            _mockAuthService.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync((UserModel)null);
            RenderComponent.SetupUnauthenticatedUser(_testContext);

            // Act
            var cut = _testContext.RenderComponent<Dashboard>();

            // Assert
            cut.MarkupMatches(m => m.Contains("Welcome to the VAT Filing Pricing Tool"));
        }

        /// <summary>
        /// Verifies that the dashboard displays the ActivitySummary component
        /// </summary>
        [Fact]
        public async Task Dashboard_DisplaysActivitySummaryComponent()
        {
            // Arrange
            var testUser = TestData.CreateTestUser("user-123", "test@example.com");
            _mockAuthService.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync(testUser);
            RenderComponent.SetupAuthenticatedUser(_testContext, "user-123", "test@example.com");

            // Act
            var cut = _testContext.RenderComponent<Dashboard>();

            // Assert
            cut.FindComponents<ActivitySummary>().Should().NotBeEmpty();
        }

        /// <summary>
        /// Verifies that the dashboard displays the RecentEstimates component
        /// </summary>
        [Fact]
        public async Task Dashboard_DisplaysRecentEstimatesComponent()
        {
            // Arrange
            var testUser = TestData.CreateTestUser("user-123", "test@example.com");
            _mockAuthService.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync(testUser);
            RenderComponent.SetupAuthenticatedUser(_testContext, "user-123", "test@example.com");

            // Act
            var cut = _testContext.RenderComponent<Dashboard>();

            // Assert
            cut.FindComponents<RecentEstimates>().Should().NotBeEmpty();
        }

        /// <summary>
        /// Verifies that the dashboard displays the QuickActions component
        /// </summary>
        [Fact]
        public async Task Dashboard_DisplaysQuickActionsComponent()
        {
            // Arrange
            var testUser = TestData.CreateTestUser("user-123", "test@example.com");
            _mockAuthService.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync(testUser);
            RenderComponent.SetupAuthenticatedUser(_testContext, "user-123", "test@example.com");

            // Act
            var cut = _testContext.RenderComponent<Dashboard>();

            // Assert
            cut.FindComponents<QuickActions>().Should().NotBeEmpty();
        }

        /// <summary>
        /// Verifies that the dashboard displays the NotificationList component
        /// </summary>
        [Fact]
        public async Task Dashboard_DisplaysNotificationListComponent()
        {
            // Arrange
            var testUser = TestData.CreateTestUser("user-123", "test@example.com");
            _mockAuthService.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync(testUser);
            RenderComponent.SetupAuthenticatedUser(_testContext, "user-123", "test@example.com");

            // Act
            var cut = _testContext.RenderComponent<Dashboard>();

            // Assert
            cut.FindComponents<NotificationList>().Should().NotBeEmpty();
        }

        /// <summary>
        /// Verifies that the dashboard displays the RecentActivity component
        /// </summary>
        [Fact]
        public async Task Dashboard_DisplaysRecentActivityComponent()
        {
            // Arrange
            var testUser = TestData.CreateTestUser("user-123", "test@example.com");
            _mockAuthService.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync(testUser);
            RenderComponent.SetupAuthenticatedUser(_testContext, "user-123", "test@example.com");

            // Act
            var cut = _testContext.RenderComponent<Dashboard>();

            // Assert
            cut.FindComponents<RecentActivity>().Should().NotBeEmpty();
        }

        /// <summary>
        /// Verifies that the dashboard displays an error message when the auth service throws an exception
        /// </summary>
        [Fact]
        public async Task Dashboard_WhenAuthServiceThrowsException_DisplaysErrorMessage()
        {
            // Arrange
            _mockAuthService.Setup(s => s.GetCurrentUserAsync()).ThrowsAsync(new Exception("Authentication failed"));
            RenderComponent.SetupAuthenticatedUser(_testContext, "user-123", "test@example.com");

            // Act
            var cut = _testContext.RenderComponent<Dashboard>();

            // Assert
            cut.MarkupMatches(m => m.Contains("Error loading dashboard"));
        }

        /// <summary>
        /// Verifies that the dashboard shows a loading state initially before user data is loaded
        /// </summary>
        [Fact]
        public async Task Dashboard_ShowsLoadingStateInitially()
        {
            // Arrange
            var tcs = new TaskCompletionSource<UserModel>();
            _mockAuthService.Setup(s => s.GetCurrentUserAsync()).Returns(tcs.Task);
            RenderComponent.SetupAuthenticatedUser(_testContext, "user-123", "test@example.com");

            // Act
            var cut = _testContext.RenderComponent<Dashboard>();

            // Assert - Should show loading initially
            cut.MarkupMatches(m => m.Contains("Loading dashboard"));

            // Complete the task to load the user
            var testUser = TestData.CreateTestUser("user-123", "test@example.com");
            tcs.SetResult(testUser);
            
            // Wait for the component to update
            cut.WaitForState(() => !cut.MarkupMatches(m => m.Contains("Loading dashboard")));
            
            // Assert - Should not show loading anymore
            cut.MarkupMatches(m => !m.Contains("Loading dashboard"));
        }
    }
}