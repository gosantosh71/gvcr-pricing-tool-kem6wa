#region

using System;
using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.Linq; // System.Linq v6.0.0
using System.Threading.Tasks; // System.Threading.Tasks v6.0.0
using VatFilingPricingTool.Common.Constants; // ErrorCodes
using VatFilingPricingTool.Common.Models; // Result, Result<T>
using VatFilingPricingTool.Contracts.V1.Requests; // CalculateRequest, GetCalculationRequest, SaveCalculationRequest, GetCalculationHistoryRequest, CompareCalculationsRequest
using VatFilingPricingTool.Contracts.V1.Responses; // CalculationResponse, SaveCalculationResponse, CalculationHistoryResponse, CalculationComparisonResponse
using VatFilingPricingTool.Data.Repositories.Interfaces; // ICalculationRepository, ICountryRepository, IRuleRepository
using VatFilingPricingTool.Domain.Entities; // Calculation, Country
using VatFilingPricingTool.Domain.Rules; // IRuleEngine
using VatFilingPricingTool.Service.Helpers; // CalculationHelper
using VatFilingPricingTool.Service.Interfaces; // IPricingService
using VatFilingPricingTool.Service.Models; // CalculationModel

#endregion

namespace VatFilingPricingTool.Service.Implementations
{
    /// <summary>
    ///     Implementation of the IPricingService interface that provides VAT filing pricing calculation
    ///     functionality
    /// </summary>
    public class PricingService : IPricingService
    {
        #region Private Members

        private readonly ICalculationRepository _calculationRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IRuleRepository _ruleRepository;
        private readonly IRuleEngine _ruleEngine;

        private readonly Dictionary<string, (string Name, decimal Cost)> _additionalServices =
            new()
            {
                { "TaxConsultancy", ("Tax Consultancy", 500m) },
                { "HistoricalDataProcessing", ("Historical Data Processing", 1000m) },
                { "ReconciliationServices", ("Reconciliation Services", 750m) }
            };

        private readonly string _defaultCurrencyCode = "EUR";

        private readonly Dictionary<string, string> _serviceTypeMapping = new()
        {
            { "StandardFiling", "StandardFiling" },
            { "ComplexFiling", "ComplexFiling" },
            { "PriorityService", "PriorityService" }
        };

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="PricingService" /> class with required dependencies
        /// </summary>
        /// <param name="calculationRepository">Repository for storing and retrieving calculation data</param>
        /// <param name="countryRepository">Repository for retrieving country data</param>
        /// <param name="ruleRepository">Repository for retrieving country-specific VAT rules</param>
        /// <param name="ruleEngine">Engine for applying country-specific VAT rules to calculations</param>
        public PricingService(
            ICalculationRepository calculationRepository,
            ICountryRepository countryRepository,
            IRuleRepository ruleRepository,
            IRuleEngine ruleEngine
        )
        {
            // Validate that all dependencies are not null
            _calculationRepository = calculationRepository ?? throw new ArgumentNullException(nameof(calculationRepository));
            _countryRepository = countryRepository ?? throw new ArgumentNullException(nameof(countryRepository));
            _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
            _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Calculates VAT filing costs based on the provided parameters
        /// </summary>
        /// <param name="request">The calculation request</param>
        /// <returns>Result containing the calculation response or error</returns>
        public async Task<Result<CalculationResponse>> CalculatePricingAsync(CalculateRequest request)
        {
            // Validate the calculation request using CalculationHelper.ValidateCalculationRequest
            CalculationHelper.ValidateCalculationRequest(request);

            // Verify that all requested countries exist using _countryRepository.ExistsByCodeAsync
            foreach (var countryCode in request.CountryCodes)
            {
                var countryExists = await _countryRepository.ExistsByCodeAsync(countryCode);
                if (!countryExists)
                    return Result<CalculationResponse>.Failure(
                        $"Country with code '{countryCode}' not found.",
                        ErrorCodes.Pricing.CountryNotSupported
                    );
            }

            // Retrieve country data for the requested countries using _countryRepository.GetCountriesByCodesAsync
            var countries = await _countryRepository.GetCountriesByCodesAsync(request.CountryCodes);

            // Get the service ID from _serviceTypeMapping based on request.ServiceType
            var serviceId = GetServiceId(request.ServiceType.ToString());

            // Create a new Calculation entity using CalculationHelper.CreateCalculationFromRequest
            var calculation = CalculationHelper.CreateCalculationFromRequest(
                request,
                "test-user", // TODO: Replace with actual user ID
                serviceId,
                _defaultCurrencyCode
            );

            // Create calculation parameters dictionary using CalculationHelper.GetCalculationParameters
            var calculationParameters = CalculationHelper.GetCalculationParameters(calculation, serviceId);

            // Calculate costs for each country using CalculationHelper.CalculateCountryCosts
            CalculationHelper.CalculateCountryCosts(calculation, countries, _ruleEngine, calculationParameters);

            // Apply volume-based discounts using CalculationHelper.ApplyVolumeDiscounts
            CalculationHelper.ApplyVolumeDiscounts(calculation);

            // Apply multi-country discounts using CalculationHelper.ApplyMultiCountryDiscounts
            CalculationHelper.ApplyMultiCountryDiscounts(calculation);

            // Add additional services if requested using CalculationHelper.AddAdditionalServices
            CalculationHelper.AddAdditionalServices(calculation, _additionalServices, request.AdditionalServices);

            // Create a CalculationModel from the entity using CalculationHelper.CreateCalculationModelFromEntity
            var calculationModel = CalculationModel.FromEntity(calculation);

            // Create a CalculationResponse from the model using CalculationHelper.CreateCalculationResponseFromModel
            var calculationResponse = CalculationHelper.CreateCalculationResponseFromModel(calculationModel);

            // Return a successful Result with the CalculationResponse
            return Result<CalculationResponse>.Success(calculationResponse);
        }

        /// <summary>
        ///     Retrieves a specific calculation by its ID
        /// </summary>
        /// <param name="request">The request containing the calculation ID</param>
        /// <returns>Result containing the calculation response or error</returns>
        public async Task<Result<CalculationResponse>> GetCalculationAsync(GetCalculationRequest request)
        {
            // Validate that request is not null
            if (request == null)
                return Result<CalculationResponse>.Failure(
                    "GetCalculationRequest cannot be null.",
                    ErrorCodes.Pricing.InvalidParameters
                );

            // Validate that request.CalculationId is not null or empty
            if (string.IsNullOrEmpty(request.CalculationId))
                return Result<CalculationResponse>.Failure(
                    "CalculationId cannot be null or empty.",
                    ErrorCodes.Pricing.InvalidParameters
                );

            // Retrieve the calculation by ID from _calculationRepository.GetByIdWithDetailsAsync
            var calculation = await _calculationRepository.GetByIdWithDetailsAsync(request.CalculationId);

            // If calculation not found, return failure result with appropriate error code
            if (calculation == null)
                return Result<CalculationResponse>.Failure(
                    $"Calculation with ID '{request.CalculationId}' not found.",
                    ErrorCodes.Pricing.CalculationNotFound
                );

            // Retrieve country data for the calculation countries using _countryRepository.GetCountriesByCodesAsync
            var countryCodes = calculation.CalculationCountries.Select(cc => cc.CountryCode.Value);
            var countries = await _countryRepository.GetCountriesByCodesAsync(countryCodes);

            // Create a CalculationModel from the entity using CalculationHelper.CreateCalculationModelFromEntity
            var calculationModel = CalculationModel.FromEntity(calculation, countries);

            // Create a CalculationResponse from the model using CalculationHelper.CreateCalculationResponseFromModel
            var calculationResponse = CalculationHelper.CreateCalculationResponseFromModel(calculationModel);

            // Return a successful Result with the CalculationResponse
            return Result<CalculationResponse>.Success(calculationResponse);
        }

        /// <summary>
        ///     Saves a calculation result for future reference
        /// </summary>
        /// <param name="request">The save calculation request</param>
        /// <param name="userId">The ID of the user saving the calculation</param>
        /// <returns>Result containing the save operation response or error</returns>
        public async Task<Result<SaveCalculationResponse>> SaveCalculationAsync(SaveCalculationRequest request,
            string userId)
        {
            // Validate that request is not null
            if (request == null)
                return Result<SaveCalculationResponse>.Failure(
                    "SaveCalculationRequest cannot be null.",
                    ErrorCodes.Pricing.InvalidParameters
                );

            // Validate that userId is not null or empty
            if (string.IsNullOrEmpty(userId))
                return Result<SaveCalculationResponse>.Failure(
                    "UserId cannot be null or empty.",
                    ErrorCodes.General.Unauthorized
                );

            // Get the service ID from _serviceTypeMapping based on request.ServiceType
            var serviceId = GetServiceId(request.ServiceType.ToString());

            // Create a new Calculation entity using Calculation.Create
            var calculation = Calculation.Create(
                userId,
                serviceId,
                request.TransactionVolume,
                request.Frequency,
                request.CurrencyCode
            );

            // Set the transaction volume, filing frequency, and other properties from the request
            calculation.UpdateTransactionVolume(request.TransactionVolume);
            calculation.UpdateFilingFrequency(request.Frequency);

            // For each country breakdown in the request, add a country to the calculation
            foreach (var countryBreakdown in request.CountryBreakdowns)
            {
                var countryCost = Money.Create(countryBreakdown.TotalCost, request.CurrencyCode);
                calculation.AddCountry(countryBreakdown.CountryCode, countryCost);
            }

            // Save the calculation to the repository using _calculationRepository.AddAsync
            await _calculationRepository.AddAsync(calculation);

            // Create a SaveCalculationResponse with the calculation ID and date
            var saveCalculationResponse = new SaveCalculationResponse
            {
                CalculationId = calculation.CalculationId,
                CalculationDate = calculation.CalculationDate
            };

            // Return a successful Result with the SaveCalculationResponse
            return Result<SaveCalculationResponse>.Success(saveCalculationResponse);
        }

        /// <summary>
        ///     Retrieves calculation history for a specific user with optional filtering
        /// </summary>
        /// <param name="request">The request containing filtering parameters and pagination settings</param>
        /// <param name="userId">The ID of the user whose calculation history to retrieve</param>
        /// <returns>Result containing the calculation history or error</returns>
        public async Task<Result<CalculationHistoryResponse>> GetCalculationHistoryAsync(
            GetCalculationHistoryRequest request, string userId)
        {
            // Validate that request is not null
            if (request == null)
                return Result<CalculationHistoryResponse>.Failure(
                    "GetCalculationHistoryRequest cannot be null.",
                    ErrorCodes.Pricing.InvalidParameters
                );

            // Validate that userId is not null or empty
            if (string.IsNullOrEmpty(userId))
                return Result<CalculationHistoryResponse>.Failure(
                    "UserId cannot be null or empty.",
                    ErrorCodes.General.Unauthorized
                );

            // Set default page size if not specified
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;

            // Retrieve paginated calculation history from _calculationRepository.GetPagedByUserIdAsync
            var pagedCalculations =
                await _calculationRepository.GetPagedByUserIdAsync(userId, request.Page, pageSize);

            // Create a CalculationHistoryResponse with the items, page number, and total count
            var calculationHistoryResponse = new CalculationHistoryResponse
            {
                Items = pagedCalculations.Items.Select(c => new CalculationSummaryResponse
                {
                    CalculationId = c.CalculationId,
                    ServiceType = c.ServiceType,
                    TransactionVolume = c.TransactionVolume,
                    Frequency = c.FilingFrequency,
                    TotalCost = c.TotalCost.Amount,
                    CurrencyCode = c.CurrencyCode,
                    CalculationDate = c.CalculationDate,
                    Countries = c.CalculationCountries.Select(cc => cc.CountryCode.Value).ToList()
                }).ToList(),
                PageNumber = pagedCalculations.PageNumber,
                PageSize = pagedCalculations.PageSize,
                TotalCount = pagedCalculations.TotalCount
            };

            // Return a successful Result with the CalculationHistoryResponse
            return Result<CalculationHistoryResponse>.Success(calculationHistoryResponse);
        }

        /// <summary>
        ///     Compares multiple calculation scenarios to help users identify the most cost-effective options
        /// </summary>
        /// <param name="request">The request containing multiple calculation scenarios to compare</param>
        /// <returns>Result containing the calculation comparison or error</returns>
        public async Task<Result<CalculationComparisonResponse>> CompareCalculationsAsync(
            CompareCalculationsRequest request)
        {
            // Validate that request is not null
            if (request == null)
                return Result<CalculationComparisonResponse>.Failure(
                    "CompareCalculationsRequest cannot be null.",
                    ErrorCodes.Pricing.InvalidParameters
                );

            // Validate that request.Scenarios contains at least two scenarios
            if (request.Scenarios == null || request.Scenarios.Count < 2)
                return Result<CalculationComparisonResponse>.Failure(
                    "At least two scenarios are required for comparison.",
                    ErrorCodes.Pricing.InvalidParameters
                );

            // Create a list to store calculation results
            var calculationResults = new List<CalculationResponse>();

            // For each scenario, calculate pricing using CalculatePricingAsync
            foreach (var scenario in request.Scenarios)
            {
                var calculateRequest = new CalculateRequest
                {
                    ServiceType = scenario.ServiceType,
                    TransactionVolume = scenario.TransactionVolume,
                    Frequency = scenario.Frequency,
                    CountryCodes = scenario.CountryCodes,
                    AdditionalServices = scenario.AdditionalServices
                };

                var calculationResult = await CalculatePricingAsync(calculateRequest);

                // If any calculation fails, return failure result with appropriate error code
                if (!calculationResult.IsSuccess)
                    return Result<CalculationComparisonResponse>.Failure(
                        $"Calculation failed for scenario '{scenario.ScenarioName}': {calculationResult.ErrorMessage}",
                        calculationResult.ErrorCode
                    );

                calculationResults.Add(calculationResult.Value);
            }

            // Generate comparison metrics between scenarios (differences, potential savings)
            // TODO: Implement comparison logic

            // Create a CalculationComparisonResponse with scenarios and comparisons
            var calculationComparisonResponse = new CalculationComparisonResponse();

            // Return a successful Result with the CalculationComparisonResponse
            return Result<CalculationComparisonResponse>.Success(calculationComparisonResponse);
        }

        /// <summary>
        ///     Deletes a specific calculation by its ID
        /// </summary>
        /// <param name="calculationId">The unique identifier of the calculation to delete</param>
        /// <param name="userId">The ID of the user requesting the deletion</param>
        /// <returns>Result indicating success or failure of the delete operation</returns>
        public async Task<Result> DeleteCalculationAsync(string calculationId, string userId)
        {
            // Validate that calculationId is not null or empty
            if (string.IsNullOrEmpty(calculationId))
                return Result.Failure(
                    "CalculationId cannot be null or empty.",
                    ErrorCodes.Pricing.InvalidParameters
                );

            // Validate that userId is not null or empty
            if (string.IsNullOrEmpty(userId))
                return Result.Failure(
                    "UserId cannot be null or empty.",
                    ErrorCodes.General.Unauthorized
                );

            // Retrieve the calculation by ID from _calculationRepository.GetByIdWithDetailsAsync
            var calculation = await _calculationRepository.GetByIdWithDetailsAsync(calculationId);

            // If calculation not found, return failure result with appropriate error code
            if (calculation == null)
                return Result.Failure(
                    $"Calculation with ID '{calculationId}' not found.",
                    ErrorCodes.Pricing.CalculationNotFound
                );

            // Verify that the user has permission to delete the calculation (matching userId)
            if (calculation.UserId != userId)
                return Result.Failure(
                    "You do not have permission to delete this calculation.",
                    ErrorCodes.General.Forbidden
                );

            // Delete the calculation from the repository using _calculationRepository.DeleteAsync
            var deletionResult = await _calculationRepository.DeleteAsync(calculationId);

            // Return a successful Result
            return Result.Success();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Validates that all provided country codes exist in the system
        /// </summary>
        /// <param name="countryCodes">The country codes to validate</param>
        /// <returns>Result indicating success or failure of the validation</returns>
        private async Task<Result> ValidateCountryCodesAsync(IEnumerable<string> countryCodes)
        {
            // Validate that countryCodes is not null
            if (countryCodes == null)
                return Result.Failure(
                    "Country codes cannot be null.",
                    ErrorCodes.Pricing.InvalidParameters
                );

            // For each country code, check if it exists using _countryRepository.ExistsByCodeAsync
            foreach (var countryCode in countryCodes)
            {
                var countryExists = await _countryRepository.ExistsByCodeAsync(countryCode);
                if (!countryExists)
                    return Result.Failure(
                        $"Country with code '{countryCode}' not found.",
                        ErrorCodes.Pricing.CountryNotSupported
                    );
            }

            // Return a successful Result if all country codes are valid
            return Result.Success();
        }

        /// <summary>
        ///     Gets the service ID corresponding to a service type
        /// </summary>
        /// <param name="serviceType">The service type</param>
        /// <returns>The service ID for the specified service type</returns>
        private string GetServiceId(string serviceType)
        {
            // Check if _serviceTypeMapping contains the serviceType key
            if (_serviceTypeMapping.ContainsKey(serviceType))
                // If found, return the corresponding service ID
                return _serviceTypeMapping[serviceType];

            // If not found, return a default service ID
            return "StandardFiling";
        }

        #endregion
    }
}