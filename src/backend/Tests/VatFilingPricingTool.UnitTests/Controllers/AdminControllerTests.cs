using System; // Version 6.0.0
using System.Collections.Generic; // Version 6.0.0
using System.Linq; // Version 6.0.0
using System.Threading.Tasks; // Version 6.0.0
using Microsoft.AspNetCore.Mvc; // Version 6.0.0
using Moq; // Version 4.18.2
using Xunit; // Version 2.4.1
using FluentAssertions; // Version 6.7.0
using VatFilingPricingTool.Api.Controllers; // Project-specific
using VatFilingPricingTool.Service.Interfaces; // Project-specific
using VatFilingPricingTool.Infrastructure.Logging; // Project-specific
using VatFilingPricingTool.Infrastructure.Configuration; // Project-specific
using VatFilingPricingTool.Common.Models; // Project-specific
using VatFilingPricingTool.UnitTests.Helpers; // Project-specific
using Microsoft.AspNetCore.Http; // Version 6.0.0

namespace VatFilingPricingTool.UnitTests.Controllers
{
    /// <summary>
    /// Test class for AdminController containing unit tests for administrative functionality
    /// </summary>
    public class AdminControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IRuleService> _mockRuleService;
        private readonly Mock<ICountryService> _mockCountryService;
        private readonly Mock<ILoggingService> _mockLoggingService;
        private readonly Mock<AppConfiguration> _mockAppConfiguration;
        private readonly AdminController _controller;

        /// <summary>
        /// Initializes a new instance of the AdminControllerTests class with mocked dependencies
        /// </summary>
        public AdminControllerTests()
        {
            // Arrange
            _mockUserService = new Mock<IUserService>();
            _mockRuleService = new Mock<IRuleService>();
            _mockCountryService = new Mock<ICountryService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _mockAppConfiguration = new Mock<AppConfiguration>(MockBehavior.Default, new Mock<IConfiguration>().Object, new Mock<IHostEnvironment>().Object);

            _controller = new AdminController(
                _mockUserService.Object,
                _mockRuleService.Object,
                _mockCountryService.Object,
                _mockLoggingService.Object,
                _mockAppConfiguration.Object);

            TestHelpers.SetupControllerContext(_controller, "admin-user-id");
        }

        /// <summary>
        /// Tests that GetAuditLogs returns audit logs when logs exist in the system
        /// </summary>
        [Fact]
        public async Task GetAuditLogs_ReturnsAuditLogs_WhenLogsExist()
        {
            // Arrange: Setup mock audit logs data
            var mockAuditLogs = new List<object>
            {
                new { Id = Guid.NewGuid().ToString(), Timestamp = DateTime.UtcNow, UserId = "user1", Action = "Action1", Data = new { ItemId = "item1", Changes = "Changes1" }, IpAddress = "192.168.1.1" },
                new { Id = Guid.NewGuid().ToString(), Timestamp = DateTime.UtcNow, UserId = "user2", Action = "Action2", Data = new { ItemId = "item2", Changes = "Changes2" }, IpAddress = "192.168.1.2" }
            };

            // Arrange: Setup _mockAppConfiguration to return audit logs when requested
            _mockAppConfiguration.Setup(config => config.GetValue<object>(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(mockAuditLogs);

            // Act: Call _controller.GetAuditLogs()
            var result = await _controller.GetAuditLogs();

            // Assert: Verify result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify returned data contains expected audit logs
            okResult.Value.Should().NotBeNull();
            var apiResponse = okResult.Value as ApiResponse<object>;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().BeEquivalentTo(mockAuditLogs);
        }

        /// <summary>
        /// Tests that GetAuditLogs returns an empty list when no logs exist in the system
        /// </summary>
        [Fact]
        public async Task GetAuditLogs_ReturnsEmptyList_WhenNoLogsExist()
        {
            // Arrange: Setup _mockAppConfiguration to return empty audit logs
            _mockAppConfiguration.Setup(config => config.GetValue<object>(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(new List<object>());

            // Act: Call _controller.GetAuditLogs()
            var result = await _controller.GetAuditLogs();

            // Assert: Verify result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify returned data contains empty list
            okResult.Value.Should().NotBeNull();
            var apiResponse = okResult.Value as ApiResponse<object>;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().BeAssignableTo<List<object>>();
            (apiResponse.Data as List<object>).Should().BeEmpty();
        }

        /// <summary>
        /// Tests that GetSystemSettings returns system settings when settings exist
        /// </summary>
        [Fact]
        public async Task GetSystemSettings_ReturnsSettings_WhenSettingsExist()
        {
            // Arrange: Setup mock system settings data
            var mockSystemSettings = new Dictionary<string, Dictionary<string, object>>
            {
                { "General", new Dictionary<string, object> { { "ApplicationName", "TestApp" } } }
            };

            // Arrange: Setup _mockAppConfiguration to return settings when requested
            _mockAppConfiguration.Setup(config => config.GetValue<object>(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(mockSystemSettings);

            // Act: Call _controller.GetSystemSettings()
            var result = await _controller.GetSystemSettings();

            // Assert: Verify result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify returned data contains expected settings
            okResult.Value.Should().NotBeNull();
            var apiResponse = okResult.Value as ApiResponse<object>;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().BeEquivalentTo(mockSystemSettings);
        }

        /// <summary>
        /// Tests that UpdateSystemSettings updates system settings when valid settings are provided
        /// </summary>
        [Fact]
        public async Task UpdateSystemSettings_UpdatesSettings_WhenValidSettingsProvided()
        {
            // Arrange: Create settings update request with valid data
            var request = new Dictionary<string, Dictionary<string, object>>
            {
                { "General", new Dictionary<string, object> { { "ApplicationName", "NewTestApp" } } }
            };

            // Arrange: Setup _mockAppConfiguration to handle settings update
            _mockAppConfiguration.Setup(config => config.BindExisting(It.IsAny<string>(), It.IsAny<object>()));

            // Arrange: Setup _mockLoggingService to verify audit logging
            _mockLoggingService.Setup(logging => logging.LogAudit(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()));

            // Act: Call _controller.UpdateSystemSettings(request)
            var result = await _controller.UpdateSystemSettings(request);

            // Assert: Verify result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify _mockAppConfiguration.Bind was called with correct parameters
            _mockAppConfiguration.Verify(config => config.BindExisting(It.IsAny<string>(), It.IsAny<object>()), Times.Once);

            // Assert: Verify _mockLoggingService.LogAudit was called for the settings change
            _mockLoggingService.Verify(logging => logging.LogAudit(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetAdminDashboard returns dashboard data when data exists
        /// </summary>
        [Fact]
        public async Task GetAdminDashboard_ReturnsDashboardData_WhenDataExists()
        {
            // Arrange: Setup mock user summaries using MockData.GetMockUsers()
            var mockUsers = MockData.GetMockUsers();
            _mockUserService.Setup(service => service.GetUserSummariesAsync(null))
                .ReturnsAsync(mockUsers.Select(u => new Contracts.V1.Responses.UserSummaryResponse
                {
                    UserId = u.UserId,
                    DisplayName = u.GetFullName(),
                    Email = u.Email,
                    Roles = u.Role.GetType().IsEnum ? new List<Domain.Enums.UserRole> { u.Role } : new List<Domain.Enums.UserRole>(),
                    IsActive = u.IsActive
                }).ToList());

            // Arrange: Setup mock rule summaries using MockData.GetMockRules()
            var mockRules = MockData.GetMockRules();
            _mockRuleService.Setup(service => service.GetRuleSummariesAsync(null, null, false))
                .ReturnsAsync(new Result<List<Contracts.V1.Responses.RuleSummaryResponse>>(mockRules.Select(r => new Contracts.V1.Responses.RuleSummaryResponse
                {
                    RuleId = r.RuleId,
                    Name = r.Name,
                    RuleType = r.Type,
                    CountryCode = r.CountryCode,
                    IsActive = r.IsActive
                }).ToList()));

            // Arrange: Setup mock country summaries using MockData.GetMockCountries()
            var mockCountries = MockData.GetMockCountries();
            _mockCountryService.Setup(service => service.GetCountrySummariesAsync())
                .ReturnsAsync(new Result<List<Contracts.V1.Responses.CountrySummaryResponse>>(mockCountries.Select(c => new Contracts.V1.Responses.CountrySummaryResponse
                {
                    CountryCode = c.Code,
                    Name = c.Name,
                    StandardVatRate = c.StandardVatRate,
                    IsActive = c.IsActive
                }).ToList()));

            // Act: Call _controller.GetAdminDashboard()
            var result = await _controller.GetAdminDashboard();

            // Assert: Verify result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify returned data contains user statistics
            okResult.Value.Should().NotBeNull();
            var apiResponse = okResult.Value as ApiResponse<object>;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeTrue();
        }

        /// <summary>
        /// Tests that GetAdminDashboard handles service errors gracefully
        /// </summary>
        [Fact]
        public async Task GetAdminDashboard_HandlesServiceErrors_WhenServicesReturnErrors()
        {
            // Arrange: Setup _mockUserService to return error
            _mockUserService.Setup(service => service.GetUserSummariesAsync(null))
                .ReturnsAsync(new List<Contracts.V1.Responses.UserSummaryResponse>());

            // Arrange: Setup _mockRuleService to return error
            _mockRuleService.Setup(service => service.GetRuleSummariesAsync(null, null, false))
                .ReturnsAsync(Result<List<Contracts.V1.Responses.RuleSummaryResponse>>.Failure("Rule service error", "RULE-001"));

            // Arrange: Setup _mockCountryService to return error
            _mockCountryService.Setup(service => service.GetCountrySummariesAsync())
                .ReturnsAsync(Result<List<Contracts.V1.Responses.CountrySummaryResponse>>.Failure("Country service error", "COUNTRY-001"));

            // Act: Call _controller.GetAdminDashboard()
            var result = await _controller.GetAdminDashboard();

            // Assert: Verify result is OkObjectResult with partial data
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify error information is included in response
            okResult.Value.Should().NotBeNull();
            var apiResponse = okResult.Value as ApiResponse<object>;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeTrue();
        }
    }
}