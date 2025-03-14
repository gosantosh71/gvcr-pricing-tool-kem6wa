using System; // Version 6.0.0 - Core .NET functionality
using System.Collections.Generic; // Version 6.0.0 - For collection types like List and Dictionary
using System.Linq; // Version 6.0.0 - For LINQ operations on collections
using System.Threading.Tasks; // Version 6.0.0 - For Task-based asynchronous operations
using VatFilingPricingTool.Service.Interfaces; // Interface that this class implements
using VatFilingPricingTool.Data.Repositories.Interfaces; // Repository for accessing rule data
using VatFilingPricingTool.Domain.Rules; // Engine for validating and evaluating rule expressions
using VatFilingPricingTool.Domain.Entities; // Domain entity for VAT rules
using VatFilingPricingTool.Domain.ValueObjects; // Value object for country codes
using VatFilingPricingTool.Domain.Enums; // Enum defining types of VAT rules
using VatFilingPricingTool.Common.Models; // Generic result wrapper for operation outcomes
using VatFilingPricingTool.Service.Models; // Service layer model for VAT rules with mapping functionality
using VatFilingPricingTool.Contracts.V1.Responses; // Response model for rule data
using VatFilingPricingTool.Contracts.V1.Requests; // Request model for rule data
using VatFilingPricingTool.Service.Helpers; // Helper for validating input data

namespace VatFilingPricingTool.Service.Implementations
{
    /// <summary>
    /// Service implementation for managing VAT filing pricing rules
    /// </summary>
    public class RuleService : IRuleService
    {
        private readonly IRuleRepository _ruleRepository;
        private readonly IRuleEngine _ruleEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleService"/> class with required dependencies
        /// </summary>
        /// <param name="ruleRepository">The repository for accessing rule data</param>
        /// <param name="ruleEngine">The engine for validating and evaluating rule expressions</param>
        public RuleService(IRuleRepository ruleRepository, IRuleEngine ruleEngine)
        {
            // Validate that ruleRepository is not null
            if (ruleRepository == null)
            {
                throw new ArgumentNullException(nameof(ruleRepository));
            }

            // Validate that ruleEngine is not null
            if (ruleEngine == null)
            {
                throw new ArgumentNullException(nameof(ruleEngine));
            }

            // Assign ruleRepository to _ruleRepository
            _ruleRepository = ruleRepository;

            // Assign ruleEngine to _ruleEngine
            _ruleEngine = ruleEngine;
        }

        /// <summary>
        /// Retrieves a specific rule by its unique identifier
        /// </summary>
        /// <param name="ruleId">The unique identifier of the rule to retrieve</param>
        /// <returns>A result containing the rule data or error information</returns>
        public async Task<Result<RuleResponse>> GetRuleByIdAsync(string ruleId)
        {
            // Validate that ruleId is not null or empty
            if (string.IsNullOrEmpty(ruleId))
            {
                return Result<RuleResponse>.Failure("Rule ID cannot be null or empty", Common.Constants.ErrorCodes.General.BadRequest);
            }

            // Call _ruleRepository.GetByIdAsync to retrieve the rule
            var rule = await _ruleRepository.GetByIdAsync(ruleId);

            // If rule is null, return Result.Failure with appropriate error message
            if (rule == null)
            {
                return Result<RuleResponse>.Failure("Rule not found", Common.Constants.ErrorCodes.Rule.RuleNotFound);
            }

            // Create a service model from the domain entity using RuleModel.FromDomain
            var ruleModel = RuleModel.FromDomain(rule);

            // Convert the service model to a contract model using ToContract
            var contractModel = ruleModel.ToContract();

            // Create a RuleResponse from the contract model
            var ruleResponse = new RuleResponse
            {
                RuleId = contractModel.RuleId,
                CountryCode = contractModel.CountryCode,
                RuleType = contractModel.RuleType,
                Name = contractModel.Name,
                Description = contractModel.Description,
                Expression = contractModel.Expression,
                EffectiveFrom = contractModel.EffectiveFrom,
                EffectiveTo = contractModel.EffectiveTo,
                Priority = contractModel.Priority,
                IsActive = contractModel.IsActive,
            };

            // Return Result.Success with the RuleResponse
            return Result<RuleResponse>.Success(ruleResponse);
        }

        /// <summary>
        /// Retrieves a paginated list of rules with optional filtering
        /// </summary>
        /// <param name="request">The request containing filter and pagination parameters</param>
        /// <returns>A result containing paginated rules data or error information</returns>
        public async Task<Result<RulesResponse>> GetRulesAsync(GetRulesRequest request)
        {
            // Validate that request is not null
            if (request == null)
            {
                return Result<RulesResponse>.Failure("Request cannot be null", Common.Constants.ErrorCodes.General.BadRequest);
            }

            // Validate that request.CountryCode is valid using ValidationHelper
            if (!string.IsNullOrEmpty(request.CountryCode) && !ValidationHelper.IsValidCountryCode(request.CountryCode))
            {
                return Result<RulesResponse>.Failure("Invalid country code", Common.Constants.ErrorCodes.Country.InvalidCountryCode);
            }

            CountryCode countryCode = null;
            if (!string.IsNullOrEmpty(request.CountryCode))
            {
                // Create a CountryCode value object from the request.CountryCode
                countryCode = CountryCode.Create(request.CountryCode);
            }

            // Call _ruleRepository.GetPagedRulesAsync with pagination and filter parameters
            var (Rules, TotalCount) = await _ruleRepository.GetPagedRulesAsync(request.PageNumber, request.PageSize, countryCode, request.RuleType, request.ActiveOnly);

            // Create a RulesResponse with the retrieved data
            var rulesResponse = new RulesResponse
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = TotalCount,
                TotalPages = (int)Math.Ceiling((double)TotalCount / request.PageSize),
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < (int)Math.Ceiling((double)TotalCount / request.PageSize),
                Items = Rules.Select(rule =>
                {
                    var ruleModel = RuleModel.FromDomain(rule);
                    return new RuleResponse
                    {
                        RuleId = ruleModel.RuleId,
                        CountryCode = ruleModel.CountryCode,
                        RuleType = ruleModel.Type,
                        Name = ruleModel.Name,
                        Description = ruleModel.Description,
                        Expression = ruleModel.Expression,
                        EffectiveFrom = ruleModel.EffectiveFrom,
                        EffectiveTo = ruleModel.EffectiveTo,
                        Priority = ruleModel.Priority,
                        IsActive = ruleModel.IsActive
                    };
                }).ToList()
            };

            // Return Result.Success with the RulesResponse
            return Result<RulesResponse>.Success(rulesResponse);
        }

        /// <summary>
        /// Retrieves a simplified list of rules for dropdown selection components
        /// </summary>
        /// <param name="countryCode">Optional country code to filter rules by</param>
        /// <param name="ruleType">Optional rule type to filter by</param>
        /// <param name="activeOnly">Whether to return only active rules</param>
        /// <returns>A result containing simplified rule information or error information</returns>
        public async Task<Result<List<RuleSummaryResponse>>> GetRuleSummariesAsync(string countryCode, RuleType? ruleType, bool activeOnly)
        {
            // Validate that countryCode is valid using ValidationHelper
            if (!string.IsNullOrEmpty(countryCode) && !ValidationHelper.IsValidCountryCode(countryCode))
            {
                return Result<List<RuleSummaryResponse>>.Failure("Invalid country code", Common.Constants.ErrorCodes.Country.InvalidCountryCode);
            }

            CountryCode countryCodeValue = null;
            if (!string.IsNullOrEmpty(countryCode))
            {
                // Create a CountryCode value object from the countryCode parameter
                countryCodeValue = CountryCode.Create(countryCode);
            }

            // Initialize a list to store rule summaries
            var ruleSummaries = new List<RuleSummaryResponse>();
            IEnumerable<Rule> rules;

            // Retrieve rules based on parameters:
            if (ruleType.HasValue)
            {
                // If ruleType has value, call _ruleRepository.GetRulesByTypeAsync
                rules = await _ruleRepository.GetRulesByTypeAsync(ruleType.Value);
            }
            else
            {
                // Otherwise, call _ruleRepository.GetRulesByCountryAsync
                rules = await _ruleRepository.GetRulesByCountryAsync(countryCodeValue);
            }

            // Map each rule to a RuleSummaryResponse with basic information
            foreach (var rule in rules)
            {
                ruleSummaries.Add(new RuleSummaryResponse
                {
                    RuleId = rule.RuleId,
                    Name = rule.Name,
                    RuleType = rule.Type,
                    CountryCode = rule.CountryCode.Value,
                    IsActive = rule.IsActive
                });
            }

            // Return Result.Success with the list of RuleSummaryResponse objects
            return Result<List<RuleSummaryResponse>>.Success(ruleSummaries);
        }

        /// <summary>
        /// Creates a new VAT filing pricing rule
        /// </summary>
        /// <param name="request">The request containing the rule details</param>
        /// <returns>A result containing the created rule's information</returns>
        public async Task<Result<CreateRuleResponse>> CreateRuleAsync(CreateRuleRequest request)
        {
            // Validate that request is not null
            if (request == null)
            {
                return Result<CreateRuleResponse>.Failure("Request cannot be null", Common.Constants.ErrorCodes.General.BadRequest);
            }

            // Create a service model from the request using RuleModel.FromContract
            var ruleModel = RuleModel.FromContract(request);

            // Validate the service model using IsValid
            if (!ruleModel.IsValid())
            {
                return Result<CreateRuleResponse>.Failure("Invalid rule data", Common.Constants.ErrorCodes.Rule.RuleValidationFailed);
            }

            // Convert the service model to a domain entity using ToDomain
            var rule = ruleModel.ToDomain();

            // Call _ruleRepository.CreateAsync to persist the rule
            await _ruleRepository.CreateAsync(rule);

            // Create a CreateRuleResponse with the rule ID, country code, and name
            var createRuleResponse = new CreateRuleResponse
            {
                RuleId = rule.RuleId,
                CountryCode = rule.CountryCode.Value,
                Name = rule.Name,
                Success = true,
                Message = "Rule created successfully"
            };

            // Return Result.Success with the CreateRuleResponse
            return Result<CreateRuleResponse>.Success(createRuleResponse);
        }

        /// <summary>
        /// Updates an existing VAT filing pricing rule
        /// </summary>
        /// <param name="request">The request containing the updated rule details</param>
        /// <returns>A result containing the updated rule's information</returns>
        public async Task<Result<UpdateRuleResponse>> UpdateRuleAsync(UpdateRuleRequest request)
        {
            // Validate that request is not null
            if (request == null)
            {
                return Result<UpdateRuleResponse>.Failure("Request cannot be null", Common.Constants.ErrorCodes.General.BadRequest);
            }

            // Validate that request.RuleId is not null or empty
            if (string.IsNullOrEmpty(request.RuleId))
            {
                return Result<UpdateRuleResponse>.Failure("Rule ID cannot be null or empty", Common.Constants.ErrorCodes.General.BadRequest);
            }

            // Check if rule exists using _ruleRepository.ExistsByIdAsync
            if (!await _ruleRepository.ExistsByIdAsync(request.RuleId))
            {
                // If rule doesn't exist, return Result.Failure with appropriate error message
                return Result<UpdateRuleResponse>.Failure("Rule not found", Common.Constants.ErrorCodes.Rule.RuleNotFound);
            }

            // Retrieve the existing rule using _ruleRepository.GetByIdAsync
            var existingRule = await _ruleRepository.GetByIdAsync(request.RuleId);

            // Create a service model from the request using RuleModel.FromContract
            var ruleModel = RuleModel.FromContract(request);

            // Validate the service model using IsValid
            if (!ruleModel.IsValid())
            {
                return Result<UpdateRuleResponse>.Failure("Invalid rule data", Common.Constants.ErrorCodes.Rule.RuleValidationFailed);
            }

            // Update the existing rule with new values:
            existingRule.UpdateName(request.Name);
            existingRule.UpdateDescription(request.Description);
            existingRule.UpdateExpression(request.Expression);
            existingRule.UpdateEffectiveDates(request.EffectiveFrom, request.EffectiveTo);
            existingRule.UpdatePriority(request.Priority);
            existingRule.SetActive(request.IsActive);

            // Call _ruleRepository.UpdateAsync to persist the changes
            await _ruleRepository.UpdateAsync(existingRule);

            // Create an UpdateRuleResponse with the rule ID, name, and last updated timestamp
            var updateRuleResponse = new UpdateRuleResponse
            {
                RuleId = existingRule.RuleId,
                Name = existingRule.Name,
                LastUpdated = existingRule.LastUpdated,
                Success = true,
                Message = "Rule updated successfully"
            };

            // Return Result.Success with the UpdateRuleResponse
            return Result<UpdateRuleResponse>.Success(updateRuleResponse);
        }

        /// <summary>
        /// Deletes a VAT filing pricing rule
        /// </summary>
        /// <param name="ruleId">The unique identifier of the rule to delete</param>
        /// <returns>A result containing the deletion status or error information</returns>
        public async Task<Result<DeleteRuleResponse>> DeleteRuleAsync(string ruleId)
        {
            // Validate that ruleId is not null or empty
            if (string.IsNullOrEmpty(ruleId))
            {
                return Result<DeleteRuleResponse>.Failure("Rule ID cannot be null or empty", Common.Constants.ErrorCodes.General.BadRequest);
            }

            // Check if rule exists using _ruleRepository.ExistsByIdAsync
            if (!await _ruleRepository.ExistsByIdAsync(ruleId))
            {
                // If rule doesn't exist, return Result.Failure with appropriate error message
                return Result<DeleteRuleResponse>.Failure("Rule not found", Common.Constants.ErrorCodes.Rule.RuleNotFound);
            }

            // Call _ruleRepository.DeleteAsync to delete the rule
            await _ruleRepository.DeleteAsync(ruleId);

            // Create a DeleteRuleResponse with the rule ID
            var deleteRuleResponse = new DeleteRuleResponse
            {
                RuleId = ruleId,
                Success = true,
                Message = "Rule deleted successfully"
            };

            // Return Result.Success with the DeleteRuleResponse
            return Result<DeleteRuleResponse>.Success(deleteRuleResponse);
        }

        /// <summary>
        /// Validates a rule expression for syntax and evaluates it with sample data
        /// </summary>
        /// <param name="request">The request containing the expression to validate</param>
        /// <returns>A result containing validation results or error information</returns>
        public async Task<Result<ValidateRuleExpressionResponse>> ValidateRuleExpressionAsync(ValidateRuleExpressionRequest request)
        {
            // Validate that request is not null
            if (request == null)
            {
                return Result<ValidateRuleExpressionResponse>.Failure("Request cannot be null", Common.Constants.ErrorCodes.General.BadRequest);
            }

            // Validate that request.Expression is not null or empty
            if (string.IsNullOrEmpty(request.Expression))
            {
                return Result<ValidateRuleExpressionResponse>.Failure("Expression cannot be null or empty", Common.Constants.ErrorCodes.General.BadRequest);
            }

            // Create a ValidateRuleExpressionResponse object
            var response = new ValidateRuleExpressionResponse();

            try
            {
                // Try to validate the expression syntax using _ruleEngine.ValidateRuleExpression
                bool isValidSyntax = _ruleEngine.ValidateRuleExpression(request.Expression);

                if (!isValidSyntax)
                {
                    // If syntax validation fails, set IsValid to false and add error message
                    response.IsValid = false;
                    response.Message = "Invalid expression syntax";
                }
                else
                {
                    // If syntax validation succeeds and sample values are provided, try to evaluate the expression
                    if (request.SampleValues != null && request.SampleValues.Any())
                    {
                        try
                        {
                            // Evaluate the expression with sample values
                            decimal evaluationResult = _ruleEngine.EvaluateRule(new Rule { Expression = request.Expression }, request.SampleValues);

                            // If evaluation succeeds, set IsValid to true, add success message, and include evaluation result
                            response.IsValid = true;
                            response.Message = "Expression is valid and evaluates successfully";
                            response.EvaluationResult = evaluationResult;
                        }
                        catch (Exception ex)
                        {
                            // If evaluation fails, set IsValid to false and add error message
                            response.IsValid = false;
                            response.Message = $"Expression evaluation failed: {ex.Message}";
                        }
                    }
                    else
                    {
                        // If no sample values are provided, set IsValid to true and add a message
                        response.IsValid = true;
                        response.Message = "Expression syntax is valid";
                    }
                }
            }
            catch (Exception ex)
            {
                // Catch any exceptions during validation and set IsValid to false with error message
                response.IsValid = false;
                response.Message = $"Expression validation failed: {ex.Message}";
            }

            // Return Result.Success with the ValidateRuleExpressionResponse
            return Result<ValidateRuleExpressionResponse>.Success(response);
        }

        /// <summary>
        /// Imports multiple rules from an external source
        /// </summary>
        /// <param name="request">The request containing the rules to import</param>
        /// <returns>A result containing import status or error information</returns>
        public async Task<Result<ImportRulesResponse>> ImportRulesAsync(ImportRulesRequest request)
        {
            // Validate that request is not null
            if (request == null)
            {
                return Result<ImportRulesResponse>.Failure("Request cannot be null", Common.Constants.ErrorCodes.General.BadRequest);
            }

            // Validate that request.Rules is not null and contains items
            if (request.Rules == null || !request.Rules.Any())
            {
                return Result<ImportRulesResponse>.Failure("Rules list cannot be null or empty", Common.Constants.ErrorCodes.General.BadRequest);
            }

            // Create an ImportRulesResponse object
            var response = new ImportRulesResponse
            {
                TotalRules = request.Rules.Count
            };

            // Initialize counters for imported and failed rules
            int importedRulesCount = 0;
            int failedRulesCount = 0;
            var errors = new List<string>();

            // For each rule in the request.Rules:
            foreach (var contractRule in request.Rules)
            {
                try
                {
                    // Create a service model using RuleModel.FromContract
                    var ruleModel = RuleModel.FromContract(contractRule);

                    // Validate the service model using IsValid
                    if (!ruleModel.IsValid())
                    {
                        // If validation fails, increment failed rules count and add error message
                        failedRulesCount++;
                        errors.Add($"Rule {contractRule.Name} validation failed");
                        continue;
                    }

                    // Convert to domain entity
                    var rule = ruleModel.ToDomain();

                    // Call _ruleRepository.CreateAsync to persist the rule
                    await _ruleRepository.CreateAsync(rule);

                    // On successful save, increment imported rules count
                    importedRulesCount++;
                }
                catch (Exception ex)
                {
                    // On failure, increment failed rules count and add error message
                    failedRulesCount++;
                    errors.Add($"Rule {contractRule.Name} import failed: {ex.Message}");
                }
            }

            // Set ImportedRules and FailedRules in the response
            response.ImportedRules = importedRulesCount;
            response.FailedRules = failedRulesCount;

            // Set Success based on whether any rules were successfully imported
            response.Success = importedRulesCount > 0;

            // Set appropriate message based on import results
            response.Message = response.Success ? "Rules imported successfully" : "No rules were imported";

            // Add any error messages to the Errors collection
            response.Errors = errors;

            // Return Result.Success with the ImportRulesResponse
            return Result<ImportRulesResponse>.Success(response);
        }

        /// <summary>
        /// Exports rules to a file format (JSON, CSV, Excel) for external use
        /// </summary>
        /// <param name="countryCode">Optional country code to filter rules by</param>
        /// <param name="ruleType">Optional rule type to filter by</param>
        /// <param name="activeOnly">Whether to export only active rules</param>
        /// <param name="format">The file format to export as (json, csv, xlsx)</param>
        /// <returns>A result containing the exported file data or error information</returns>
        public async Task<Result<byte[]>> ExportRulesAsync(string countryCode, RuleType? ruleType, bool activeOnly, string format)
        {
            // Validate that countryCode is valid using ValidationHelper
            if (!string.IsNullOrEmpty(countryCode) && !ValidationHelper.IsValidCountryCode(countryCode))
            {
                return Result<byte[]>.Failure("Invalid country code", Common.Constants.ErrorCodes.Country.InvalidCountryCode);
            }

            // Validate that format is one of the supported formats (json, csv, excel)
            if (string.IsNullOrEmpty(format) || (format.ToLower() != "json" && format.ToLower() != "csv" && format.ToLower() != "excel"))
            {
                return Result<byte[]>.Failure("Invalid export format. Supported formats are json, csv, and excel.", Common.Constants.ErrorCodes.General.BadRequest);
            }

            CountryCode countryCodeValue = null;
            if (!string.IsNullOrEmpty(countryCode))
            {
                // Create a CountryCode value object from the countryCode parameter
                countryCodeValue = CountryCode.Create(countryCode);
            }

            IEnumerable<Rule> rules;

            // Retrieve rules based on parameters:
            if (ruleType.HasValue)
            {
                // If ruleType has value, call _ruleRepository.GetRulesByTypeAsync
                rules = await _ruleRepository.GetRulesByTypeAsync(ruleType.Value);
            }
            else
            {
                // Otherwise, call _ruleRepository.GetRulesByCountryAsync
                rules = await _ruleRepository.GetRulesByCountryAsync(countryCodeValue);
            }

            // If no rules found, return Result.Failure with appropriate message
            if (rules == null || !rules.Any())
            {
                return Result<byte[]>.Failure("No rules found for the specified criteria", Common.Constants.ErrorCodes.Rule.RuleNotFound);
            }

            // Convert the rules to contract models using RuleModel.FromDomain and ToContract
            var contractRules = rules.Select(r => RuleModel.FromDomain(r).ToContract()).ToList();

            byte[] fileData;

            // Generate the export file based on the requested format:
            if (format.ToLower() == "json")
            {
                // For JSON, serialize the rules to JSON format
                fileData = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(contractRules);
            }
            else
            {
                // For CSV and Excel, implement the logic here
                // This is a placeholder, implement the actual CSV and Excel generation
                fileData = System.Text.Encoding.UTF8.GetBytes("CSV or Excel export not implemented yet");
            }

            // Return Result.Success with the file data as byte array
            return Result<byte[]>.Success(fileData);
        }
    }
}