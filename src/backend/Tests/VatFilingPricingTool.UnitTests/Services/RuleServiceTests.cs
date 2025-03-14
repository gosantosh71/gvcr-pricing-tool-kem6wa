#nullable enable
using System; // Version 6.0.0 - Core .NET functionality
using System.Collections.Generic; // Version 6.0.0 - For collection types like List and IEnumerable
using System.Linq; // Version 6.0.0 - For LINQ operations on collections
using System.Threading.Tasks; // Version 6.0.0 - For Task-based asynchronous operations
using Xunit; // Version 2.4.2 - Testing framework
using Moq; // Version 4.18.2 - Mocking framework for dependencies
using FluentAssertions; // Version 6.7.0 - Fluent assertions for test validation
using VatFilingPricingTool.Service.Interfaces; // Interface that this class implements
using VatFilingPricingTool.Service.Implementations; // Implementation being tested
using VatFilingPricingTool.Data.Repositories.Interfaces; // Repository for accessing rule data
using VatFilingPricingTool.Domain.Rules; // Engine for validating and evaluating rule expressions
using VatFilingPricingTool.Domain.Entities; // Domain entity for VAT rules
using VatFilingPricingTool.Domain.Enums; // Enum defining types of VAT rules
using VatFilingPricingTool.Common.Models; // Generic result wrapper for operation outcomes
using VatFilingPricingTool.Contracts.V1.Responses; // Response model for rule data
using VatFilingPricingTool.Contracts.V1.Requests; // Request model for rule data
using VatFilingPricingTool.UnitTests.Helpers; // Provides mock data for tests

namespace VatFilingPricingTool.UnitTests.Services
{
    /// <summary>
    /// Contains unit tests for the RuleService class
    /// </summary>
    public class RuleServiceTests
    {
        private readonly Mock<IRuleRepository> _mockRuleRepository;
        private readonly Mock<IRuleEngine> _mockRuleEngine;
        private readonly IRuleService _ruleService;

        /// <summary>
        /// Initializes a new instance of the RuleServiceTests class with mocked dependencies
        /// </summary>
        public RuleServiceTests()
        {
            _mockRuleRepository = new Mock<IRuleRepository>();
            _mockRuleEngine = new Mock<IRuleEngine>();
            _ruleService = new RuleService(_mockRuleRepository.Object, _mockRuleEngine.Object);
        }

        /// <summary>
        /// Tests that GetRuleByIdAsync returns a rule when it exists
        /// </summary>
        [Fact]
        public async Task GetRuleByIdAsync_ExistingRule_ReturnsRule()
        {
            // Arrange: Create a mock rule with a known ID
            var ruleId = "test-rule-id";
            var mockRule = Rule.Create("GB", RuleType.VatRate, "Test Rule", "basePrice * 0.2", DateTime.UtcNow);
            typeof(Rule).GetProperty("RuleId")!.SetValue(mockRule, ruleId);

            // Arrange: Setup _mockRuleRepository.GetByIdAsync to return the mock rule
            _mockRuleRepository.Setup(repo => repo.GetByIdAsync(ruleId))
                .ReturnsAsync(mockRule);

            // Act: Call _ruleService.GetRuleByIdAsync with the rule ID
            var result = await _ruleService.GetRuleByIdAsync(ruleId);

            // Assert: Verify the result is successful
            result.IsSuccess.Should().BeTrue();

            // Assert: Verify the returned rule has the expected ID
            result.Value!.RuleId.Should().Be(ruleId);

            // Assert: Verify _mockRuleRepository.GetByIdAsync was called once with the correct ID
            _mockRuleRepository.Verify(repo => repo.GetByIdAsync(ruleId), Times.Once);
        }

        /// <summary>
        /// Tests that GetRuleByIdAsync returns a failure when the rule doesn't exist
        /// </summary>
        [Fact]
        public async Task GetRuleByIdAsync_NonExistingRule_ReturnsFailure()
        {
            // Arrange: Setup _mockRuleRepository.GetByIdAsync to return null
            _mockRuleRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Rule)null!);

            // Act: Call _ruleService.GetRuleByIdAsync with a non-existing rule ID
            var result = await _ruleService.GetRuleByIdAsync("non-existing-id");

            // Assert: Verify the result is a failure
            result.IsSuccess.Should().BeFalse();

            // Assert: Verify the error message indicates the rule was not found
            result.ErrorMessage.Should().Be("Rule not found");

            // Assert: Verify _mockRuleRepository.GetByIdAsync was called once with the correct ID
            _mockRuleRepository.Verify(repo => repo.GetByIdAsync("non-existing-id"), Times.Once);
        }

        /// <summary>
        /// Tests that GetRulesAsync returns a paginated list of rules
        /// </summary>
        [Fact]
        public async Task GetRulesAsync_ValidRequest_ReturnsRules()
        {
            // Arrange: Create a list of mock rules
            var mockRules = MockData.GetMockRules();
            var totalCount = mockRules.Count;
            var pageNumber = 1;
            var pageSize = 10;

            // Arrange: Setup _mockRuleRepository.GetPagedRulesAsync to return the mock rules with a total count
            _mockRuleRepository.Setup(repo => repo.GetPagedRulesAsync(pageNumber, pageSize, It.IsAny<CountryCode>(), It.IsAny<RuleType?>(), It.IsAny<bool>()))
                .ReturnsAsync((new List<Rule>(mockRules.Take(pageSize)), totalCount));

            // Arrange: Create a valid GetRulesRequest with pagination and filter parameters
            var request = new GetRulesRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                CountryCode = "GB",
                ActiveOnly = true
            };

            // Act: Call _ruleService.GetRulesAsync with the request
            var result = await _ruleService.GetRulesAsync(request);

            // Assert: Verify the result is successful
            result.IsSuccess.Should().BeTrue();

            // Assert: Verify the returned rules match the expected count
            result.Value!.Items.Count.Should().Be(mockRules.Take(pageSize).Count());

            // Assert: Verify the pagination properties are set correctly
            result.Value.PageNumber.Should().Be(pageNumber);
            result.Value.PageSize.Should().Be(pageSize);
            result.Value.TotalCount.Should().Be(totalCount);
            result.Value.TotalPages.Should().Be((int)Math.Ceiling((double)totalCount / pageSize));
            result.Value.HasPreviousPage.Should().BeFalse();
            result.Value.HasNextPage.Should().Be(totalCount > pageSize);

            // Assert: Verify _mockRuleRepository.GetPagedRulesAsync was called once with the correct parameters
            _mockRuleRepository.Verify(repo => repo.GetPagedRulesAsync(pageNumber, pageSize, It.IsAny<CountryCode>(), It.IsAny<RuleType?>(), It.IsAny<bool>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetRuleSummariesAsync returns simplified rule information
        /// </summary>
        [Fact]
        public async Task GetRuleSummariesAsync_ValidParameters_ReturnsSummaries()
        {
            // Arrange: Create a list of mock rules for a specific country
            var mockRules = MockData.GetMockRules().Where(r => r.CountryCode.Value == "GB").ToList();

            // Arrange: Setup _mockRuleRepository.GetRulesByCountryAsync to return the mock rules
            _mockRuleRepository.Setup(repo => repo.GetRulesByCountryAsync(It.IsAny<CountryCode>()))
                .ReturnsAsync(mockRules);

            // Act: Call _ruleService.GetRuleSummariesAsync with country code, rule type, and activeOnly parameters
            var result = await _ruleService.GetRuleSummariesAsync("GB", null, true);

            // Assert: Verify the result is successful
            result.IsSuccess.Should().BeTrue();

            // Assert: Verify the returned summaries match the expected count
            result.Value!.Count.Should().Be(mockRules.Count);

            // Assert: Verify each summary contains the expected basic information
            foreach (var summary in result.Value!)
            {
                mockRules.Any(r => r.RuleId == summary.RuleId &&
                                    r.Name == summary.Name &&
                                    r.Type == summary.RuleType &&
                                    r.CountryCode.Value == summary.CountryCode &&
                                    r.IsActive == summary.IsActive).Should().BeTrue();
            }

            // Assert: Verify _mockRuleRepository.GetRulesByCountryAsync was called once with the correct parameters
            _mockRuleRepository.Verify(repo => repo.GetRulesByCountryAsync(It.IsAny<CountryCode>()), Times.Once);
        }

        /// <summary>
        /// Tests that CreateRuleAsync creates a new rule with valid input
        /// </summary>
        [Fact]
        public async Task CreateRuleAsync_ValidRequest_CreatesRule()
        {
            // Arrange: Create a valid CreateRuleRequest with required fields
            var request = new CreateRuleRequest
            {
                CountryCode = "GB",
                RuleType = RuleType.VatRate,
                Name = "Test Rule",
                Expression = "basePrice * 0.2",
                EffectiveFrom = DateTime.UtcNow,
                Priority = 100,
                IsActive = true
            };

            // Arrange: Setup _mockRuleRepository.CreateAsync to return a new rule with the expected properties
            _mockRuleRepository.Setup(repo => repo.CreateAsync(It.IsAny<Rule>()))
                .ReturnsAsync((Rule rule) =>
                {
                    typeof(Rule).GetProperty("RuleId")!.SetValue(rule, Guid.NewGuid().ToString());
                    return rule;
                });

            // Act: Call _ruleService.CreateRuleAsync with the request
            var result = await _ruleService.CreateRuleAsync(request);

            // Assert: Verify the result is successful
            result.IsSuccess.Should().BeTrue();

            // Assert: Verify the response contains the expected rule ID and name
            result.Value!.RuleId.Should().NotBeNullOrEmpty();
            result.Value.CountryCode.Should().Be(request.CountryCode);
            result.Value.Name.Should().Be(request.Name);

            // Assert: Verify _mockRuleRepository.CreateAsync was called once with a rule matching the request
            _mockRuleRepository.Verify(repo => repo.CreateAsync(It.Is<Rule>(r =>
                r.CountryCode.Value == request.CountryCode &&
                r.Type == request.RuleType &&
                r.Name == request.Name &&
                r.Expression == request.Expression &&
                r.EffectiveFrom == request.EffectiveFrom &&
                r.Priority == request.Priority &&
                r.IsActive == request.IsActive
            )), Times.Once);
        }

        /// <summary>
        /// Tests that CreateRuleAsync returns validation errors for invalid input
        /// </summary>
        [Fact]
        public async Task CreateRuleAsync_InvalidRequest_ReturnsValidationError()
        {
            // Arrange: Create an invalid CreateRuleRequest (missing required fields or invalid values)
            var request = new CreateRuleRequest
            {
                CountryCode = "Invalid", // Invalid country code
                RuleType = RuleType.VatRate,
                Name = "", // Missing name
                Expression = "basePrice * 0.2",
                EffectiveFrom = DateTime.UtcNow.AddDays(-1), // Invalid date
                Priority = 1001, // Invalid priority
                IsActive = true
            };

            // Act: Call _ruleService.CreateRuleAsync with the invalid request
            var result = await _ruleService.CreateRuleAsync(request);

            // Assert: Verify the result is a failure
            result.IsSuccess.Should().BeFalse();

            // Assert: Verify the error message indicates validation errors
            result.ErrorMessage.Should().Be("Invalid rule data");

            // Assert: Verify _mockRuleRepository.CreateAsync was not called
            _mockRuleRepository.Verify(repo => repo.CreateAsync(It.IsAny<Rule>()), Times.Never);
        }

        /// <summary>
        /// Tests that UpdateRuleAsync updates an existing rule
        /// </summary>
        [Fact]
        public async Task UpdateRuleAsync_ExistingRule_UpdatesRule()
        {
            // Arrange: Create an existing rule with known properties
            var ruleId = "existing-rule-id";
            var existingRule = Rule.Create("GB", RuleType.VatRate, "Original Name", "basePrice * 0.2", DateTime.UtcNow);
            typeof(Rule).GetProperty("RuleId")!.SetValue(existingRule, ruleId);

            // Arrange: Setup _mockRuleRepository.ExistsByIdAsync to return true
            _mockRuleRepository.Setup(repo => repo.ExistsByIdAsync(ruleId))
                .ReturnsAsync(true);

            // Arrange: Setup _mockRuleRepository.GetByIdAsync to return the existing rule
            _mockRuleRepository.Setup(repo => repo.GetByIdAsync(ruleId))
                .ReturnsAsync(existingRule);

            // Arrange: Setup _mockRuleRepository.UpdateAsync to return the updated rule
            _mockRuleRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Rule>()))
                .Returns(Task.CompletedTask);

            // Arrange: Create a valid UpdateRuleRequest with updated values
            var request = new UpdateRuleRequest
            {
                RuleId = ruleId,
                Name = "Updated Name",
                Description = "Updated Description",
                Expression = "basePrice * 0.3",
                EffectiveFrom = DateTime.UtcNow.AddDays(1),
                Priority = 200,
                IsActive = false
            };

            // Act: Call _ruleService.UpdateRuleAsync with the request
            var result = await _ruleService.UpdateRuleAsync(request);

            // Assert: Verify the result is successful
            result.IsSuccess.Should().BeTrue();

            // Assert: Verify the response contains the expected rule ID and updated name
            result.Value!.RuleId.Should().Be(ruleId);
            result.Value.Name.Should().Be(request.Name);

            // Assert: Verify _mockRuleRepository.UpdateAsync was called once with a rule containing the updated values
            _mockRuleRepository.Verify(repo => repo.UpdateAsync(It.Is<Rule>(r =>
                r.RuleId == request.RuleId &&
                r.Name == request.Name &&
                r.Description == request.Description &&
                r.Expression == request.Expression &&
                r.EffectiveFrom == request.EffectiveFrom &&
                r.Priority == request.Priority &&
                r.IsActive == request.IsActive
            )), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateRuleAsync returns a failure when the rule doesn't exist
        /// </summary>
        [Fact]
        public async Task UpdateRuleAsync_NonExistingRule_ReturnsFailure()
        {
            // Arrange: Setup _mockRuleRepository.ExistsByIdAsync to return false
            _mockRuleRepository.Setup(repo => repo.ExistsByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Arrange: Create an UpdateRuleRequest with a non-existing rule ID
            var request = new UpdateRuleRequest
            {
                RuleId = "non-existing-rule-id",
                Name = "Updated Name",
                Description = "Updated Description",
                Expression = "basePrice * 0.3",
                EffectiveFrom = DateTime.UtcNow.AddDays(1),
                Priority = 200,
                IsActive = false
            };

            // Act: Call _ruleService.UpdateRuleAsync with the request
            var result = await _ruleService.UpdateRuleAsync(request);

            // Assert: Verify the result is a failure
            result.IsSuccess.Should().BeFalse();

            // Assert: Verify the error message indicates the rule was not found
            result.ErrorMessage.Should().Be("Rule not found");

            // Assert: Verify _mockRuleRepository.UpdateAsync was not called
            _mockRuleRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Rule>()), Times.Never);
        }

        /// <summary>
        /// Tests that DeleteRuleAsync deletes an existing rule
        /// </summary>
        [Fact]
        public async Task DeleteRuleAsync_ExistingRule_DeletesRule()
        {
            // Arrange: Setup _mockRuleRepository.ExistsByIdAsync to return true
            _mockRuleRepository.Setup(repo => repo.ExistsByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Arrange: Setup _mockRuleRepository.DeleteAsync to complete successfully
            _mockRuleRepository.Setup(repo => repo.DeleteAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act: Call _ruleService.DeleteRuleAsync with an existing rule ID
            var result = await _ruleService.DeleteRuleAsync("existing-rule-id");

            // Assert: Verify the result is successful
            result.IsSuccess.Should().BeTrue();

            // Assert: Verify the response contains the expected rule ID
            result.Value!.RuleId.Should().Be("existing-rule-id");

            // Assert: Verify _mockRuleRepository.DeleteAsync was called once with the correct ID
            _mockRuleRepository.Verify(repo => repo.DeleteAsync("existing-rule-id"), Times.Once);
        }

        /// <summary>
        /// Tests that DeleteRuleAsync returns a failure when the rule doesn't exist
        /// </summary>
        [Fact]
        public async Task DeleteRuleAsync_NonExistingRule_ReturnsFailure()
        {
            // Arrange: Setup _mockRuleRepository.ExistsByIdAsync to return false
            _mockRuleRepository.Setup(repo => repo.ExistsByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act: Call _ruleService.DeleteRuleAsync with a non-existing rule ID
            var result = await _ruleService.DeleteRuleAsync("non-existing-rule-id");

            // Assert: Verify the result is a failure
            result.IsSuccess.Should().BeFalse();

            // Assert: Verify the error message indicates the rule was not found
            result.ErrorMessage.Should().Be("Rule not found");

            // Assert: Verify _mockRuleRepository.DeleteAsync was not called
            _mockRuleRepository.Verify(repo => repo.DeleteAsync(It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that ValidateRuleExpressionAsync validates a syntactically correct expression
        /// </summary>
        [Fact]
        public async Task ValidateRuleExpressionAsync_ValidExpression_ReturnsSuccess()
        {
            // Arrange: Setup _mockRuleEngine.ValidateRuleExpression to return true
            _mockRuleEngine.Setup(engine => engine.ValidateRuleExpression(It.IsAny<string>()))
                .Returns(true);

            // Arrange: Create a ValidateRuleExpressionRequest with a valid expression
            var request = new ValidateRuleExpressionRequest
            {
                Expression = "basePrice * 0.2"
            };

            // Act: Call _ruleService.ValidateRuleExpressionAsync with the request
            var result = await _ruleService.ValidateRuleExpressionAsync(request);

            // Assert: Verify the result is successful
            result.IsSuccess.Should().BeTrue();

            // Assert: Verify the response indicates the expression is valid
            result.Value!.IsValid.Should().BeTrue();

            // Assert: Verify _mockRuleEngine.ValidateRuleExpression was called once with the correct expression
            _mockRuleEngine.Verify(engine => engine.ValidateRuleExpression(request.Expression), Times.Once);
        }

        /// <summary>
        /// Tests that ValidateRuleExpressionAsync identifies a syntactically incorrect expression
        /// </summary>
        [Fact]
        public async Task ValidateRuleExpressionAsync_InvalidExpression_ReturnsFailure()
        {
            // Arrange: Setup _mockRuleEngine.ValidateRuleExpression to return false
            _mockRuleEngine.Setup(engine => engine.ValidateRuleExpression(It.IsAny<string>()))
                .Returns(false);

            // Arrange: Create a ValidateRuleExpressionRequest with an invalid expression
            var request = new ValidateRuleExpressionRequest
            {
                Expression = "basePrice ** 0.2" // Invalid operator
            };

            // Act: Call _ruleService.ValidateRuleExpressionAsync with the request
            var result = await _ruleService.ValidateRuleExpressionAsync(request);

            // Assert: Verify the result is successful (the validation itself succeeded)
            result.IsSuccess.Should().BeTrue();

            // Assert: Verify the response indicates the expression is invalid
            result.Value!.IsValid.Should().BeFalse();

            // Assert: Verify _mockRuleEngine.ValidateRuleExpression was called once with the correct expression
            _mockRuleEngine.Verify(engine => engine.ValidateRuleExpression(request.Expression), Times.Once);
        }

        /// <summary>
        /// Tests that ImportRulesAsync imports valid rules
        /// </summary>
        [Fact]
        public async Task ImportRulesAsync_ValidRules_ImportsRules()
        {
            // Arrange: Create a list of valid rule contracts for import
            var ruleContracts = new List<Contracts.V1.Models.RuleModel>
            {
                new Contracts.V1.Models.RuleModel { RuleId = "rule1", CountryCode = "GB", RuleType = RuleType.VatRate, Name = "Rule 1", Expression = "basePrice * 0.2", EffectiveFrom = DateTime.UtcNow, Priority = 100, IsActive = true },
                new Contracts.V1.Models.RuleModel { RuleId = "rule2", CountryCode = "DE", RuleType = RuleType.Threshold, Name = "Rule 2", Expression = "transactionVolume > 1000 ? basePrice * 1.1 : basePrice", EffectiveFrom = DateTime.UtcNow, Priority = 200, IsActive = true }
            };

            // Arrange: Setup _mockRuleRepository.ExistsByIdAsync to return false (rules don't exist yet)
            _mockRuleRepository.Setup(repo => repo.ExistsByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Arrange: Setup _mockRuleRepository.CreateAsync to return new rules
            _mockRuleRepository.Setup(repo => repo.CreateAsync(It.IsAny<Rule>()))
                .ReturnsAsync((Rule rule) => rule);

            // Arrange: Create an ImportRulesRequest with the rule contracts
            var request = new ImportRulesRequest { Rules = ruleContracts };

            // Act: Call _ruleService.ImportRulesAsync with the request
            var result = await _ruleService.ImportRulesAsync(request);

            // Assert: Verify the result is successful
            result.IsSuccess.Should().BeTrue();

            // Assert: Verify the response indicates all rules were imported successfully
            result.Value!.ImportedRules.Should().Be(ruleContracts.Count);
            result.Value.FailedRules.Should().Be(0);

            // Assert: Verify _mockRuleRepository.CreateAsync was called once for each rule
            _mockRuleRepository.Verify(repo => repo.CreateAsync(It.IsAny<Rule>()), Times.Exactly(ruleContracts.Count));
        }

        /// <summary>
        /// Tests that ImportRulesAsync reports failures for invalid rules
        /// </summary>
        [Fact]
        public async Task ImportRulesAsync_InvalidRules_ReportsFailures()
        {
            // Arrange: Create a list of rule contracts with some invalid rules
            var ruleContracts = new List<Contracts.V1.Models.RuleModel>
            {
                new Contracts.V1.Models.RuleModel { RuleId = "rule1", CountryCode = "GB", RuleType = RuleType.VatRate, Name = "Rule 1", Expression = "basePrice * 0.2", EffectiveFrom = DateTime.UtcNow, Priority = 100, IsActive = true },
                new Contracts.V1.Models.RuleModel { RuleId = "rule2", CountryCode = "DE", RuleType = RuleType.Threshold, Name = "", Expression = "transactionVolume > 1000 ? basePrice * 1.1 : basePrice", EffectiveFrom = DateTime.UtcNow, Priority = 200, IsActive = true }, // Invalid: Missing Name
                new Contracts.V1.Models.RuleModel { RuleId = "rule3", CountryCode = "FR", RuleType = RuleType.Complexity, Name = "Rule 3", Expression = "invalid expression", EffectiveFrom = DateTime.UtcNow, Priority = 300, IsActive = true } // Invalid: Invalid Expression
            };

            // Arrange: Create an ImportRulesRequest with the rule contracts
            var request = new ImportRulesRequest { Rules = ruleContracts };

            // Act: Call _ruleService.ImportRulesAsync with the request
            var result = await _ruleService.ImportRulesAsync(request);

            // Assert: Verify the result is successful (the import operation itself succeeded)
            result.IsSuccess.Should().BeTrue();

            // Assert: Verify the response indicates some rules failed to import
            result.Value!.ImportedRules.Should().Be(1);
            result.Value.FailedRules.Should().Be(2);

            // Assert: Verify the response contains error messages for the invalid rules
            result.Value.Errors.Count.Should().Be(2);

            // Assert: Verify _mockRuleRepository.CreateAsync was called only for valid rules
            _mockRuleRepository.Verify(repo => repo.CreateAsync(It.IsAny<Rule>()), Times.Exactly(1));
        }

        /// <summary>
        /// Tests that ExportRulesAsync exports rules in the requested format
        /// </summary>
        [Fact]
        public async Task ExportRulesAsync_ExistingRules_ExportsRules()
        {
            // Arrange: Create a list of mock rules for a specific country
            var mockRules = MockData.GetMockRules().Where(r => r.CountryCode.Value == "GB").ToList();

            // Arrange: Setup _mockRuleRepository.GetRulesByCountryAsync to return the mock rules
            _mockRuleRepository.Setup(repo => repo.GetRulesByCountryAsync(It.IsAny<CountryCode>()))
                .ReturnsAsync(mockRules);

            // Act: Call _ruleService.ExportRulesAsync with country code, rule type, activeOnly, and format parameters
            var result = await _ruleService.ExportRulesAsync("GB", null, true, "json");

            // Assert: Verify the result is successful
            result.IsSuccess.Should().BeTrue();

            // Assert: Verify the result contains non-empty byte array data
            result.Value.Should().NotBeNullOrEmpty();

            // Assert: Verify _mockRuleRepository.GetRulesByCountryAsync was called once with the correct parameters
            _mockRuleRepository.Verify(repo => repo.GetRulesByCountryAsync(It.IsAny<CountryCode>()), Times.Once);
        }

        /// <summary>
        /// Tests that ExportRulesAsync returns a failure when no rules are found
        /// </summary>
        [Fact]
        public async Task ExportRulesAsync_NoRules_ReturnsFailure()
        {
            // Arrange: Setup _mockRuleRepository.GetRulesByCountryAsync to return an empty list
            _mockRuleRepository.Setup(repo => repo.GetRulesByCountryAsync(It.IsAny<CountryCode>()))
                .ReturnsAsync(new List<Rule>());

            // Act: Call _ruleService.ExportRulesAsync with country code, rule type, activeOnly, and format parameters
            var result = await _ruleService.ExportRulesAsync("GB", null, true, "json");

            // Assert: Verify the result is a failure
            result.IsSuccess.Should().BeFalse();

            // Assert: Verify the error message indicates no rules were found
            result.ErrorMessage.Should().Be("No rules found for the specified criteria");

            // Assert: Verify _mockRuleRepository.GetRulesByCountryAsync was called once with the correct parameters
            _mockRuleRepository.Verify(repo => repo.GetRulesByCountryAsync(It.IsAny<CountryCode>()), Times.Once);
        }
    }
}
#nullable disable