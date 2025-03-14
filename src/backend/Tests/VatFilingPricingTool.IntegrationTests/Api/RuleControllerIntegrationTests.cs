using FluentAssertions; // FluentAssertions, Version=6.2.0
using System.Collections.Generic; // System.Collections.Generic, Version=6.0.0
using System.Net.Http; // System.Net.Http, Version=6.0.0
using System.Net.Http.Json; // System.Net.Http.Json, Version=6.0.0
using System.Threading.Tasks; // System.Threading.Tasks, Version=6.0.0
using VatFilingPricingTool.Common.Constants.ApiRoutes; // Centralized API route definitions for rule endpoints
using VatFilingPricingTool.Contracts.V1.Requests.RuleRequests; // Request model for retrieving a specific rule
using VatFilingPricingTool.Contracts.V1.Responses.RuleResponses; // Response model for rule information
using VatFilingPricingTool.IntegrationTests.TestServer; // Base class for integration tests providing common setup and utility methods
using Xunit; // Testing framework for assertions and test lifecycle
using VatFilingPricingTool.Contracts.V1.Models; // Data model for VAT filing pricing rules
using VatFilingPricingTool.Domain.Enums; // User role enumeration for authentication testing
using VatFilingPricingTool.Common.Models.ApiResponse; // Generic API response wrapper for handling test responses

namespace VatFilingPricingTool.IntegrationTests.Api
{
    /// <summary>
    /// Integration tests for the Rule Controller API endpoints, verifying the correct behavior of rule-related operations
    /// </summary>
    public class RuleControllerIntegrationTests : IntegrationTestBase
    {
        /// <summary>
        /// Initializes a new instance of the RuleControllerIntegrationTests class
        /// </summary>
        public RuleControllerIntegrationTests()
        {
            // LD1: Call base constructor to set up the test environment
        }

        [Fact]
        public async Task GetRule_WithValidRuleId_ReturnsRule()
        {
            // LD1: Create a GetRuleRequest with a valid rule ID
            var request = new GetRuleRequest { RuleId = "GB-VAT-001" };

            // LD1: Call GetAsync to the GetById endpoint with the request
            var response = await GetAsync<RuleResponse>($"{ApiRoutes.Rule.Base}/{request.RuleId}");

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the returned rule has the expected rule ID
            response.Data.RuleId.Should().Be(request.RuleId);

            // LD1: Assert that the rule has the expected properties (name, expression, etc.)
            response.Data.Name.Should().NotBeNullOrEmpty();
            response.Data.Expression.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetRule_WithInvalidRuleId_ReturnsNotFound()
        {
            // LD1: Create a GetRuleRequest with an invalid rule ID
            var request = new GetRuleRequest { RuleId = "NonExistentRule" };

            // LD1: Call GetAsync to the GetById endpoint with the request
            var response = await GetAsync<RuleResponse>($"{ApiRoutes.Rule.Base}/{request.RuleId}");

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response contains an appropriate error message
            response.Message.Should().Contain("Rule not found");
        }

        [Fact]
        public async Task GetRulesByCountry_WithValidCountryCode_ReturnsRules()
        {
            // LD1: Create a GetRulesByCountryRequest with a valid country code
            var request = new GetRulesByCountryRequest { CountryCode = "GB" };

            // LD1: Call GetAsync to the GetByCountry endpoint with the request
            var response = await GetAsync<RulesResponse>($"{ApiRoutes.Rule.Base}/country/{request.CountryCode}");

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the returned rules list is not empty
            response.Data.Items.Should().NotBeEmpty();

            // LD1: Assert that all returned rules have the expected country code
            foreach (var rule in response.Data.Items)
            {
                rule.CountryCode.Should().Be(request.CountryCode);
            }
        }

        [Fact]
        public async Task GetRulesByCountry_WithPagination_ReturnsCorrectPage()
        {
            // LD1: Create a GetRulesByCountryRequest with specific page and page size
            var request = new GetRulesByCountryRequest { CountryCode = "GB", PageNumber = 2, PageSize = 5 };

            // LD1: Call GetAsync to the GetByCountry endpoint with the request
            var response = await GetAsync<RulesResponse>($"{ApiRoutes.Rule.Base}/country/{request.CountryCode}?pageNumber={request.PageNumber}&pageSize={request.PageSize}");

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the returned page number matches the request
            response.Data.PageNumber.Should().Be(request.PageNumber);

            // LD1: Assert that the number of items does not exceed the page size
            response.Data.Items.Count.Should().BeLessOrEqualTo(request.PageSize);
        }

        [Fact]
        public async Task GetRulesByCountry_WithRuleTypeFilter_ReturnsMatchingRules()
        {
            // LD1: Create a GetRulesByCountryRequest with a specific rule type
            var request = new GetRulesByCountryRequest { CountryCode = "GB", RuleType = RuleType.VatRate };

            // LD1: Call GetAsync to the GetByCountry endpoint with the request
            var response = await GetAsync<RulesResponse>($"{ApiRoutes.Rule.Base}/country/{request.CountryCode}?ruleType={request.RuleType}");

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that all returned rules have the specified rule type
            foreach (var rule in response.Data.Items)
            {
                rule.RuleType.Should().Be(request.RuleType);
            }
        }

        [Fact]
        public async Task GetRuleSummaries_ReturnsRuleSummaries()
        {
            // LD1: Create a GetRuleSummariesRequest with appropriate parameters
            var request = new GetRuleSummariesRequest { CountryCode = "GB", RuleType = RuleType.VatRate, ActiveOnly = true };

            // LD1: Call GetAsync to the Summaries endpoint with the request
             var response = await GetAsync<List<RuleSummaryResponse>>($"{ApiRoutes.Rule.Base}/country/{request.CountryCode}?ruleType={request.RuleType}&activeOnly={request.ActiveOnly}");

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the returned summaries list is not empty
            response.Data.Should().NotBeEmpty();

            // LD1: Assert that each summary contains the expected basic properties (rule ID, name, type)
            foreach (var summary in response.Data)
            {
                summary.RuleId.Should().NotBeNullOrEmpty();
                summary.Name.Should().NotBeNullOrEmpty();
                summary.RuleType.Should().BeOfType(typeof(RuleType));
            }
        }

        [Fact]
        public async Task CreateRule_WithValidData_CreatesRule()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create a CreateRuleRequest with valid rule data
            var request = new CreateRuleRequest
            {
                CountryCode = "GB",
                RuleType = RuleType.Threshold,
                Name = "New Test Rule",
                Description = "Test rule created by integration test",
                Expression = "basePrice * 1.1",
                EffectiveFrom = DateTime.UtcNow.AddDays(1)
            };

            // LD1: Call PostAsync to the Create endpoint with the request
            var response = await PostAsync<CreateRuleRequest, CreateRuleResponse>(ApiRoutes.Rule.Base, request, client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response indicates successful creation
            response.Data.Success.Should().BeTrue();

            // LD1: Verify the rule was created by retrieving it with GetRule
            var getResponse = await GetAsync<RuleResponse>($"{ApiRoutes.Rule.Base}/{response.Data.RuleId}");
            getResponse.Success.Should().BeTrue();
            getResponse.Data.Name.Should().Be(request.Name);
        }

        [Fact]
        public async Task CreateRule_WithInvalidData_ReturnsBadRequest()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create a CreateRuleRequest with invalid data (missing required fields)
            var request = new CreateRuleRequest { CountryCode = "GB", Expression = "basePrice * 1.1" };

            // LD1: Call PostAsync to the Create endpoint with the request
            var response = await PostAsync<CreateRuleRequest, CreateRuleResponse>(ApiRoutes.Rule.Base, request, client);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response contains validation error messages
            response.Message.Should().Contain("Validation failed");
        }

        [Fact]
        public async Task CreateRule_WithoutAdminRole_ReturnsForbidden()
        {
            // LD1: Create an authenticated client with Customer role
            var client = CreateAuthenticatedClient(UserRole.Customer);

            // LD1: Create a valid CreateRuleRequest
            var request = new CreateRuleRequest
            {
                CountryCode = "GB",
                RuleType = RuleType.Threshold,
                Name = "New Test Rule",
                Description = "Test rule created by integration test",
                Expression = "basePrice * 1.1",
                EffectiveFrom = DateTime.UtcNow.AddDays(1)
            };

            // LD1: Call PostAsync to the Create endpoint with the request
            var response = await PostAsync<CreateRuleRequest, CreateRuleResponse>(ApiRoutes.Rule.Base, request, client);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response indicates a forbidden error
            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task UpdateRule_WithValidData_UpdatesRule()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create an UpdateRuleRequest with valid updated rule data
            var request = new UpdateRuleRequest
            {
                RuleId = "GB-VAT-001",
                Name = "Updated Test Rule",
                Description = "Updated test rule by integration test",
                Expression = "basePrice * 1.2",
                IsActive = false,
                EffectiveFrom = DateTime.UtcNow.AddDays(2)
            };

            // LD1: Call PutAsync to the Update endpoint with the request
            var response = await PutAsync<UpdateRuleRequest, UpdateRuleResponse>(ApiRoutes.Rule.Base + $"/{request.RuleId}", request, client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response indicates successful update
            response.Data.Success.Should().BeTrue();

            // LD1: Verify the rule was updated by retrieving it with GetRule
            var getResponse = await GetAsync<RuleResponse>($"{ApiRoutes.Rule.Base}/{request.RuleId}");
            getResponse.Success.Should().BeTrue();
            getResponse.Data.Name.Should().Be(request.Name);
            getResponse.Data.IsActive.Should().Be(request.IsActive);
        }

        [Fact]
        public async Task UpdateRule_WithInvalidRuleId_ReturnsNotFound()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create an UpdateRuleRequest with a non-existent rule ID
            var request = new UpdateRuleRequest
            {
                RuleId = "NonExistentRule",
                Name = "Updated Test Rule",
                Description = "Updated test rule by integration test",
                Expression = "basePrice * 1.2",
                IsActive = false,
                EffectiveFrom = DateTime.UtcNow.AddDays(2)
            };

            // LD1: Call PutAsync to the Update endpoint with the request
            var response = await PutAsync<UpdateRuleRequest, UpdateRuleResponse>(ApiRoutes.Rule.Base + $"/{request.RuleId}", request, client);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response indicates a not found error
            response.Message.Should().Contain("Rule not found");
        }

        [Fact]
        public async Task DeleteRule_WithValidRuleId_DeletesRule()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create a DeleteRuleRequest with a valid rule ID
            var request = new DeleteRuleRequest { RuleId = "GB-VAT-001" };

            // LD1: Call DeleteAsync to the Delete endpoint with the request
            var response = await DeleteAsync<DeleteRuleResponse>(ApiRoutes.Rule.Base + $"/{request.RuleId}", client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response indicates successful deletion
            response.Data.Success.Should().BeTrue();

            // LD1: Verify the rule was deleted by attempting to retrieve it with GetRule
            var getResponse = await GetAsync<RuleResponse>($"{ApiRoutes.Rule.Base}/{request.RuleId}");
            getResponse.Success.Should().BeFalse();
            getResponse.Message.Should().Contain("Rule not found");
        }

        [Fact]
        public async Task DeleteRule_WithInvalidRuleId_ReturnsNotFound()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create a DeleteRuleRequest with a non-existent rule ID
            var request = new DeleteRuleRequest { RuleId = "NonExistentRule" };

            // LD1: Call DeleteAsync to the Delete endpoint with the request
            var response = await DeleteAsync<DeleteRuleResponse>(ApiRoutes.Rule.Base + $"/{request.RuleId}", client);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response indicates a not found error
            response.Message.Should().Contain("Rule not found");
        }

        [Fact]
        public async Task ValidateRuleExpression_WithValidExpression_ReturnsValidResult()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create a ValidateRuleExpressionRequest with a valid expression and sample values
            var request = new ValidateRuleExpressionRequest
            {
                Expression = "basePrice * 1.1",
                Parameters = new List<RuleParameterModel> { new RuleParameterModel { Name = "basePrice", DataType = "number", DefaultValue = "100" } },
                SampleValues = new Dictionary<string, object> { { "basePrice", 100 } }
            };

            // LD1: Call PostAsync to the Validate endpoint with the request
            var response = await PostAsync<ValidateRuleExpressionRequest, ValidateRuleExpressionResponse>(ApiRoutes.Rule.Validate, request, client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response indicates the expression is valid
            response.Data.IsValid.Should().BeTrue();

            // LD1: Assert that the evaluation result matches the expected value
            response.Data.EvaluationResult.Should().Be(110m);
        }

        [Fact]
        public async Task ValidateRuleExpression_WithInvalidExpression_ReturnsInvalidResult()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create a ValidateRuleExpressionRequest with an invalid expression
            var request = new ValidateRuleExpressionRequest
            {
                Expression = "basePrice *",
                Parameters = new List<RuleParameterModel> { new RuleParameterModel { Name = "basePrice", DataType = "number", DefaultValue = "100" } },
                SampleValues = new Dictionary<string, object> { { "basePrice", 100 } }
            };

            // LD1: Call PostAsync to the Validate endpoint with the request
            var response = await PostAsync<ValidateRuleExpressionRequest, ValidateRuleExpressionResponse>(ApiRoutes.Rule.Validate, request, client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response indicates the expression is invalid
            response.Data.IsValid.Should().BeFalse();

            // LD1: Assert that the response contains an error message explaining the issue
            response.Data.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ImportRules_WithValidRules_ImportsRules()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create an ImportRulesRequest with valid rules
            var request = new ImportRulesRequest
            {
                Rules = new List<RuleModel>
                {
                    new RuleModel
                    {
                        RuleId = "NEW-RULE-001",
                        CountryCode = "GB",
                        RuleType = RuleType.VatRate,
                        Name = "New Imported Rule 1",
                        Expression = "basePrice * 0.20",
                        IsActive = true
                    },
                    new RuleModel
                    {
                        RuleId = "NEW-RULE-002",
                        CountryCode = "DE",
                        RuleType = RuleType.Threshold,
                        Name = "New Imported Rule 2",
                        Expression = "basePrice * 1.1",
                        IsActive = true
                    }
                }
            };

            // LD1: Call PostAsync to the Import endpoint with the request
            var response = await PostAsync<ImportRulesRequest, ImportRulesResponse>(ApiRoutes.Rule.Import, request, client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response indicates successful import
            response.Data.Success.Should().BeTrue();

            // LD1: Assert that the imported rules count matches the expected count
            response.Data.ImportedRules.Should().Be(request.Rules.Count);

            // LD1: Verify the rules were imported by retrieving them with GetRulesByCountry
            var getResponseGB = await GetAsync<RulesResponse>($"{ApiRoutes.Rule.Base}/country/GB");
            getResponseGB.Success.Should().BeTrue();
            getResponseGB.Data.Items.Should().Contain(r => r.RuleId == "NEW-RULE-001");

            var getResponseDE = await GetAsync<RulesResponse>($"{ApiRoutes.Rule.Base}/country/DE");
            getResponseDE.Success.Should().BeTrue();
            getResponseDE.Data.Items.Should().Contain(r => r.RuleId == "NEW-RULE-002");
        }

        [Fact]
        public async Task ImportRules_WithInvalidRules_ReturnsPartialSuccess()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create an ImportRulesRequest with a mix of valid and invalid rules
            var request = new ImportRulesRequest
            {
                Rules = new List<RuleModel>
                {
                    new RuleModel
                    {
                        RuleId = "NEW-RULE-003",
                        CountryCode = "GB",
                        RuleType = RuleType.VatRate,
                        Name = "New Imported Rule 3",
                        Expression = "basePrice * 0.20",
                        IsActive = true
                    },
                    new RuleModel // Invalid rule (missing required fields)
                    {
                        RuleId = "NEW-RULE-004",
                        CountryCode = "DE",
                        RuleType = RuleType.Threshold,
                        Name = null, // Missing Name
                        Expression = "basePrice * 1.1",
                        IsActive = true
                    }
                }
            };

            // LD1: Call PostAsync to the Import endpoint with the request
            var response = await PostAsync<ImportRulesRequest, ImportRulesResponse>(ApiRoutes.Rule.Import, request, client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response indicates partial success
            response.Data.Success.Should().BeTrue();

            // LD1: Assert that the imported rules count is less than the total rules count
            response.Data.ImportedRules.Should().BeLessThan(request.Rules.Count);

            // LD1: Assert that the response contains error information for the failed rules
            response.Data.Errors.Should().NotBeNull();
        }
    }
}