#nullable enable
using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using System.Linq; // System.Linq package version 6.0.0
using Microsoft.AspNetCore.Mvc; // Microsoft.AspNetCore.Mvc package version 6.0.0
using Xunit; // Xunit package version 2.4.2
using Moq; // Moq package version 4.18.2
using FluentAssertions; // FluentAssertions package version 6.7.0
using VatFilingPricingTool.Api.Controllers;
using VatFilingPricingTool.Service.Interfaces;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Contracts.V1.Responses;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.UnitTests.Helpers;

namespace VatFilingPricingTool.UnitTests.Controllers
{
    /// <summary>
    /// Contains unit tests for the RuleController class
    /// </summary>
    public class RuleControllerTests
    {
        private readonly Mock<IRuleService> _mockRuleService;
        private readonly RuleController _controller;

        /// <summary>
        /// Initializes a new instance of the RuleControllerTests class
        /// </summary>
        public RuleControllerTests()
        {
            _mockRuleService = new Mock<IRuleService>();
            _controller = new RuleController(_mockRuleService.Object);
            TestHelpers.SetupControllerContext(_controller, "test-user-id");
        }

        /// <summary>
        /// Tests that GetRuleAsync returns a rule when a valid ID is provided
        /// </summary>
        [Fact]
        public async Task GetRuleAsync_WithValidId_ReturnsRule()
        {
            // Arrange
            string ruleId = Guid.NewGuid().ToString();
            var mockRule = new RuleResponse { RuleId = ruleId, CountryCode = "GB", Name = "Test Rule" };
            _mockRuleService.Setup(x => x.GetRuleByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(Result<RuleResponse>.Success(mockRule));

            // Act
            var request = new GetRuleRequest { RuleId = ruleId };
            var result = await _controller.GetRuleAsync(request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var response = okResult.Value as ApiResponse<RuleResponse>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().BeEquivalentTo(mockRule);
        }

        /// <summary>
        /// Tests that GetRuleAsync returns NotFound when an invalid ID is provided
        /// </summary>
        [Fact]
        public async Task GetRuleAsync_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            string ruleId = Guid.NewGuid().ToString();
            _mockRuleService.Setup(x => x.GetRuleByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(Result<RuleResponse>.Failure("Rule not found", "RULE-001"));

            // Act
            var request = new GetRuleRequest { RuleId = ruleId };
            var result = await _controller.GetRuleAsync(request);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();

            var response = notFoundResult.Value as ApiResponse;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be("Rule not found");
        }

        /// <summary>
        /// Tests that GetRulesByCountryAsync returns rules when a valid request is provided
        /// </summary>
        [Fact]
        public async Task GetRulesByCountryAsync_WithValidRequest_ReturnsRules()
        {
            // Arrange
            var request = new GetRulesByCountryRequest { CountryCode = "GB", PageNumber = 1, PageSize = 10 };
            var mockRules = new RulesResponse { Items = MockData.GetMockRules().Select(r => new RuleResponse { RuleId = r.RuleId, CountryCode = r.CountryCode, Name = r.Name }).ToList(), PageNumber = 1, TotalCount = 2 };
            _mockRuleService.Setup(x => x.GetRulesAsync(It.IsAny<GetRulesRequest>()))
                .ReturnsAsync(Result<RulesResponse>.Success(mockRules));

            // Act
            var result = await _controller.GetRulesByCountryAsync(request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var response = okResult.Value as ApiResponse<RulesResponse>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().BeEquivalentTo(mockRules);
        }

        /// <summary>
        /// Tests that GetRulesByCountryAsync returns BadRequest when an invalid request is provided
        /// </summary>
        [Fact]
        public async Task GetRulesByCountryAsync_WithInvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new GetRulesByCountryRequest { CountryCode = null, PageNumber = 0, PageSize = 101 };
            _mockRuleService.Setup(x => x.GetRulesAsync(It.IsAny<GetRulesRequest>()))
                .ReturnsAsync(Result<RulesResponse>.Failure("Invalid request", "GENERAL-007"));

            // Act
            var result = await _controller.GetRulesByCountryAsync(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();

            var response = badRequestResult.Value as ApiResponse<RulesResponse>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be("Invalid request");
        }

        /// <summary>
        /// Tests that GetRuleSummariesAsync returns rule summaries when a valid request is provided
        /// </summary>
        [Fact]
        public async Task GetRuleSummariesAsync_WithValidRequest_ReturnsSummaries()
        {
            // Arrange
            var request = new GetRuleSummariesRequest { CountryCode = "GB", ActiveOnly = true };
            var mockSummaries = MockData.GetMockRules().Select(r => new RuleSummaryResponse { RuleId = r.RuleId, Name = r.Name, RuleType = r.Type }).ToList();
            _mockRuleService.Setup(x => x.GetRuleSummariesAsync(It.IsAny<string>(), It.IsAny<RuleType?>(), It.IsAny<bool>()))
                .ReturnsAsync(Result<List<RuleSummaryResponse>>.Success(mockSummaries));

            // Act
            var result = await _controller.GetRuleSummariesAsync(request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var response = okResult.Value as ApiResponse<List<RuleSummaryResponse>>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Count.Should().Be(mockSummaries.Count);
        }

        /// <summary>
        /// Tests that CreateRuleAsync returns Created when a valid request is provided
        /// </summary>
        [Fact]
        public async Task CreateRuleAsync_WithValidRequest_ReturnsCreated()
        {
            // Arrange
            var request = new CreateRuleRequest { CountryCode = "GB", RuleType = RuleType.VatRate, Name = "New Rule", Expression = "basePrice * 0.2" };
            var mockResponse = new CreateRuleResponse { RuleId = Guid.NewGuid().ToString(), Success = true };
            _mockRuleService.Setup(x => x.CreateRuleAsync(It.IsAny<CreateRuleRequest>()))
                .ReturnsAsync(Result<CreateRuleResponse>.Success(mockResponse));

            // Act
            var result = await _controller.CreateRuleAsync(request);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();

            var response = createdResult.Value as ApiResponse<CreateRuleResponse>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.RuleId.Should().Be(mockResponse.RuleId);
        }

        /// <summary>
        /// Tests that CreateRuleAsync returns BadRequest when an invalid request is provided
        /// </summary>
        [Fact]
        public async Task CreateRuleAsync_WithInvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateRuleRequest { CountryCode = null, RuleType = RuleType.VatRate, Name = null, Expression = null };
            _mockRuleService.Setup(x => x.CreateRuleAsync(It.IsAny<CreateRuleRequest>()))
                .ReturnsAsync(Result<CreateRuleResponse>.Failure("Invalid request", "GENERAL-007"));

            // Act
            var result = await _controller.CreateRuleAsync(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();

            var response = badRequestResult.Value as ApiResponse<CreateRuleResponse>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be("Invalid request");
        }

        /// <summary>
        /// Tests that UpdateRuleAsync returns Ok when a valid request is provided
        /// </summary>
        [Fact]
        public async Task UpdateRuleAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new UpdateRuleRequest { RuleId = Guid.NewGuid().ToString(), Name = "Updated Rule", Expression = "basePrice * 0.25" };
            var mockResponse = new UpdateRuleResponse { RuleId = request.RuleId, Success = true };
            _mockRuleService.Setup(x => x.UpdateRuleAsync(It.IsAny<UpdateRuleRequest>()))
                .ReturnsAsync(Result<UpdateRuleResponse>.Success(mockResponse));

            // Act
            var result = await _controller.UpdateRuleAsync(request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var response = okResult.Value as ApiResponse<UpdateRuleResponse>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.RuleId.Should().Be(request.RuleId);
        }

        /// <summary>
        /// Tests that UpdateRuleAsync returns BadRequest when an invalid request is provided
        /// </summary>
        [Fact]
        public async Task UpdateRuleAsync_WithInvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateRuleRequest { RuleId = null, Name = null, Expression = null };
            _mockRuleService.Setup(x => x.UpdateRuleAsync(It.IsAny<UpdateRuleRequest>()))
                .ReturnsAsync(Result<UpdateRuleResponse>.Failure("Invalid request", "GENERAL-007"));

            // Act
            var result = await _controller.UpdateRuleAsync(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();

            var response = badRequestResult.Value as ApiResponse<UpdateRuleResponse>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be("Invalid request");
        }

        /// <summary>
        /// Tests that DeleteRuleAsync returns Ok when a valid ID is provided
        /// </summary>
        [Fact]
        public async Task DeleteRuleAsync_WithValidId_ReturnsOk()
        {
            // Arrange
            string ruleId = Guid.NewGuid().ToString();
            var mockResponse = new DeleteRuleResponse { RuleId = ruleId, Success = true };
            _mockRuleService.Setup(x => x.DeleteRuleAsync(It.IsAny<string>()))
                .ReturnsAsync(Result<DeleteRuleResponse>.Success(mockResponse));

            // Act
            var request = new DeleteRuleRequest { RuleId = ruleId };
            var result = await _controller.DeleteRuleAsync(request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var response = okResult.Value as ApiResponse<DeleteRuleResponse>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.RuleId.Should().Be(ruleId);
        }

        /// <summary>
        /// Tests that DeleteRuleAsync returns NotFound when an invalid ID is provided
        /// </summary>
        [Fact]
        public async Task DeleteRuleAsync_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            string ruleId = Guid.NewGuid().ToString();
            _mockRuleService.Setup(x => x.DeleteRuleAsync(It.IsAny<string>()))
                .ReturnsAsync(Result<DeleteRuleResponse>.Failure("Rule not found", "RULE-001"));

            // Act
            var request = new DeleteRuleRequest { RuleId = ruleId };
            var result = await _controller.DeleteRuleAsync(request);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();

            var response = notFoundResult.Value as ApiResponse<DeleteRuleResponse>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be("Rule not found");
        }

        /// <summary>
        /// Tests that ValidateRuleExpressionAsync returns validation results when a valid expression is provided
        /// </summary>
        [Fact]
        public async Task ValidateRuleExpressionAsync_WithValidExpression_ReturnsValidationResult()
        {
            // Arrange
            var request = new ValidateRuleExpressionRequest { Expression = "basePrice * 0.2", Parameters = new List<Contracts.V1.Models.RuleParameterModel>() };
            var mockResponse = new ValidateRuleExpressionResponse { IsValid = true, EvaluationResult = 100m };
            _mockRuleService.Setup(x => x.ValidateRuleExpressionAsync(It.IsAny<ValidateRuleExpressionRequest>()))
                .ReturnsAsync(Result<ValidateRuleExpressionResponse>.Success(mockResponse));

            // Act
            var result = await _controller.ValidateRuleExpressionAsync(request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var response = okResult.Value as ApiResponse<ValidateRuleExpressionResponse>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.IsValid.Should().BeTrue();
            response.Data.EvaluationResult.Should().Be(100m);
        }

        /// <summary>
        /// Tests that ValidateRuleExpressionAsync returns BadRequest when an invalid expression is provided
        /// </summary>
        [Fact]
        public async Task ValidateRuleExpressionAsync_WithInvalidExpression_ReturnsBadRequest()
        {
            // Arrange
            var request = new ValidateRuleExpressionRequest { Expression = "invalid expression", Parameters = new List<Contracts.V1.Models.RuleParameterModel>() };
            _mockRuleService.Setup(x => x.ValidateRuleExpressionAsync(It.IsAny<ValidateRuleExpressionRequest>()))
                .ReturnsAsync(Result<ValidateRuleExpressionResponse>.Failure("Invalid expression", "RULE-003"));

            // Act
            var result = await _controller.ValidateRuleExpressionAsync(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();

            var response = badRequestResult.Value as ApiResponse<ValidateRuleExpressionResponse>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be("Invalid expression");
        }

        /// <summary>
        /// Tests that ImportRulesAsync returns import results when a valid request is provided
        /// </summary>
        [Fact]
        public async Task ImportRulesAsync_WithValidRequest_ReturnsImportResults()
        {
            // Arrange
            var request = new ImportRulesRequest { Rules = new List<Contracts.V1.Models.RuleModel>(), OverwriteExisting = true };
            var mockResponse = new ImportRulesResponse { TotalRules = 10, ImportedRules = 8, FailedRules = 2, Success = true };
            _mockRuleService.Setup(x => x.ImportRulesAsync(It.IsAny<ImportRulesRequest>()))
                .ReturnsAsync(Result<ImportRulesResponse>.Success(mockResponse));

            // Act
            var result = await _controller.ImportRulesAsync(request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var response = okResult.Value as ApiResponse<ImportRulesResponse>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.TotalRules.Should().Be(10);
            response.Data.ImportedRules.Should().Be(8);
            response.Data.FailedRules.Should().Be(2);
        }

        /// <summary>
        /// Tests that ImportRulesAsync returns BadRequest when an invalid request is provided
        /// </summary>
        [Fact]
        public async Task ImportRulesAsync_WithInvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new ImportRulesRequest { Rules = null, OverwriteExisting = true };
            _mockRuleService.Setup(x => x.ImportRulesAsync(It.IsAny<ImportRulesRequest>()))
                .ReturnsAsync(Result<ImportRulesResponse>.Failure("Invalid request", "GENERAL-007"));

            // Act
            var result = await _controller.ImportRulesAsync(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();

            var response = badRequestResult.Value as ApiResponse<ImportRulesResponse>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be("Invalid request");
        }

        /// <summary>
        /// Tests that ExportRulesAsync returns a file result when a valid request is provided
        /// </summary>
        [Fact]
        public async Task ExportRulesAsync_WithValidRequest_ReturnsFileResult()
        {
            // Arrange
            string countryCode = "GB";
            RuleType? ruleType = RuleType.VatRate;
            bool activeOnly = true;
            string format = "json";
            byte[] mockData = { 0x01, 0x02, 0x03 };

            _mockRuleService.Setup(x => x.ExportRulesAsync(countryCode, ruleType, activeOnly, format))
                .ReturnsAsync(Result<byte[]>.Success(mockData));

            // Act
            var result = await _controller.ExportRulesAsync(countryCode, ruleType, activeOnly, format);

            // Assert
            result.Should().BeOfType<FileContentResult>();
            var fileResult = result as FileContentResult;
            fileResult.Should().NotBeNull();
            fileResult.FileContents.Should().BeEquivalentTo(mockData);
            fileResult.ContentType.Should().Be("application/json");
        }

        /// <summary>
        /// Tests that ExportRulesAsync returns BadRequest when an invalid request is provided
        /// </summary>
        [Fact]
        public async Task ExportRulesAsync_WithInvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            string countryCode = null;
            RuleType? ruleType = RuleType.VatRate;
            bool activeOnly = true;
            string format = "json";

            _mockRuleService.Setup(x => x.ExportRulesAsync(countryCode, ruleType, activeOnly, format))
                .ReturnsAsync(Result<byte[]>.Failure("Invalid request", "GENERAL-007"));

            // Act
            var result = await _controller.ExportRulesAsync(countryCode, ruleType, activeOnly, format);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();

            var response = badRequestResult.Value as ApiResponse<byte[]>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be("Invalid request");
        }
    }
}