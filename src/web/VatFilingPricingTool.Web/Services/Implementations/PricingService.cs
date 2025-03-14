using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VatFilingPricingTool.Web.Clients;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Services.Interfaces;

namespace VatFilingPricingTool.Web.Services.Implementations
{
    /// <summary>
    /// Implementation of the IPricingService interface that handles VAT filing cost calculations
    /// and related operations by communicating with the backend API.
    /// </summary>
    public class PricingService : IPricingService
    {
        private readonly ApiClient apiClient;
        private readonly ILogger<PricingService> logger;

        /// <summary>
        /// Initializes a new instance of the PricingService class with required dependencies.
        /// </summary>
        /// <param name="apiClient">Client for making HTTP requests to the backend API.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        public PricingService(ApiClient apiClient, ILogger<PricingService> logger)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Calculates VAT filing costs based on the provided input parameters.
        /// </summary>
        /// <param name="input">The calculation input parameters including countries, service type, and transaction volume.</param>
        /// <returns>Calculation result with cost breakdown.</returns>
        public async Task<CalculationResultModel> CalculatePricingAsync(CalculationInputModel input)
        {
            logger.LogInformation("Calculating VAT filing costs with {CountryCount} countries and {TransactionVolume} transactions", 
                input?.CountryCodes?.Count ?? 0, input?.TransactionVolume ?? 0);

            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), "Calculation input cannot be null");
            }

            if (input.CountryCodes == null || input.CountryCodes.Count == 0)
            {
                throw new ArgumentException("At least one country must be selected", nameof(input.CountryCodes));
            }

            if (input.TransactionVolume <= 0)
            {
                throw new ArgumentException("Transaction volume must be greater than zero", nameof(input.TransactionVolume));
            }

            var result = await apiClient.PostAsync<CalculationInputModel, CalculationResultModel>(
                ApiEndpoints.Pricing.Calculate, input);

            logger.LogInformation("Calculation completed with ID: {CalculationId}, Total Cost: {TotalCost}", 
                result.CalculationId, result.TotalCost);

            return result;
        }

        /// <summary>
        /// Retrieves a specific calculation by its ID.
        /// </summary>
        /// <param name="calculationId">The unique identifier of the calculation to retrieve.</param>
        /// <returns>The requested calculation result.</returns>
        public async Task<CalculationResultModel> GetCalculationAsync(string calculationId)
        {
            logger.LogInformation("Retrieving calculation with ID: {CalculationId}", calculationId);

            if (string.IsNullOrEmpty(calculationId))
            {
                throw new ArgumentException("Calculation ID cannot be null or empty", nameof(calculationId));
            }

            var endpoint = ApiEndpoints.Pricing.GetById.Replace("{id}", calculationId);
            var result = await apiClient.GetAsync<CalculationResultModel>(endpoint);

            logger.LogInformation("Retrieved calculation with ID: {CalculationId}", calculationId);

            return result;
        }

        /// <summary>
        /// Saves a calculation result for future reference.
        /// </summary>
        /// <param name="model">The model containing the calculation ID and metadata.</param>
        /// <returns>True if the calculation was saved successfully, otherwise false.</returns>
        public async Task<bool> SaveCalculationAsync(SaveCalculationModel model)
        {
            logger.LogInformation("Saving calculation with ID: {CalculationId}, Name: {Name}", 
                model?.CalculationId, model?.Name);

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Save calculation model cannot be null");
            }

            if (string.IsNullOrEmpty(model.CalculationId))
            {
                throw new ArgumentException("Calculation ID cannot be null or empty", nameof(model.CalculationId));
            }

            if (string.IsNullOrEmpty(model.Name))
            {
                throw new ArgumentException("Calculation name cannot be null or empty", nameof(model.Name));
            }

            var response = await apiClient.PostAsync<SaveCalculationModel, bool>(
                ApiEndpoints.Pricing.Save, model);

            logger.LogInformation("Calculation saved successfully: {Result}", response);

            return response;
        }

        /// <summary>
        /// Retrieves calculation history with optional filtering.
        /// </summary>
        /// <param name="filter">Optional filter parameters for the calculation history.</param>
        /// <returns>Calculation history with pagination.</returns>
        public async Task<CalculationHistoryModel> GetCalculationHistoryAsync(CalculationFilterModel filter)
        {
            logger.LogInformation("Retrieving calculation history with page {PageNumber}, page size {PageSize}", 
                filter?.PageNumber ?? 1, filter?.PageSize ?? 10);

            // Initialize filter with default values if null
            filter ??= new CalculationFilterModel();

            var result = await apiClient.PostAsync<CalculationFilterModel, CalculationHistoryModel>(
                ApiEndpoints.Pricing.History, filter);

            logger.LogInformation("Retrieved {ItemCount} calculation history items", result.Items.Count);

            return result;
        }

        /// <summary>
        /// Deletes a specific calculation by its ID.
        /// </summary>
        /// <param name="calculationId">The unique identifier of the calculation to delete.</param>
        /// <returns>True if the calculation was deleted successfully, otherwise false.</returns>
        public async Task<bool> DeleteCalculationAsync(string calculationId)
        {
            logger.LogInformation("Deleting calculation with ID: {CalculationId}", calculationId);

            if (string.IsNullOrEmpty(calculationId))
            {
                throw new ArgumentException("Calculation ID cannot be null or empty", nameof(calculationId));
            }

            var endpoint = ApiEndpoints.Pricing.Delete.Replace("{id}", calculationId);
            var result = await apiClient.DeleteAsync<bool>(endpoint);

            logger.LogInformation("Calculation deletion result: {Result}", result);

            return result;
        }

        /// <summary>
        /// Retrieves available service type options for the pricing calculator.
        /// </summary>
        /// <returns>List of available service type options.</returns>
        public async Task<List<ServiceTypeOption>> GetServiceTypeOptionsAsync()
        {
            logger.LogInformation("Retrieving service type options");

            var options = await apiClient.GetAsync<List<ServiceTypeOption>>(
                ApiEndpoints.Pricing.ServiceTypes);

            logger.LogInformation("Retrieved {OptionCount} service type options", options.Count);

            return options;
        }

        /// <summary>
        /// Retrieves available filing frequency options for the pricing calculator.
        /// </summary>
        /// <returns>List of available filing frequency options.</returns>
        public async Task<List<FilingFrequencyOption>> GetFilingFrequencyOptionsAsync()
        {
            logger.LogInformation("Retrieving filing frequency options");

            var options = await apiClient.GetAsync<List<FilingFrequencyOption>>(
                ApiEndpoints.Pricing.FilingFrequencies);

            logger.LogInformation("Retrieved {OptionCount} filing frequency options", options.Count);

            return options;
        }

        /// <summary>
        /// Retrieves available additional service options for the pricing calculator.
        /// </summary>
        /// <returns>List of available additional service options.</returns>
        public async Task<List<AdditionalServiceOption>> GetAdditionalServiceOptionsAsync()
        {
            logger.LogInformation("Retrieving additional service options");

            var options = await apiClient.GetAsync<List<AdditionalServiceOption>>(
                ApiEndpoints.Pricing.AdditionalServices);

            logger.LogInformation("Retrieved {OptionCount} additional service options", options.Count);

            return options;
        }

        /// <summary>
        /// Compares multiple calculation scenarios to help users identify the most cost-effective options.
        /// </summary>
        /// <param name="inputs">List of calculation input models to compare.</param>
        /// <returns>Comparison of calculation results.</returns>
        public async Task<CalculationComparisonModel> CompareCalculationsAsync(List<CalculationInputModel> inputs)
        {
            logger.LogInformation("Comparing {ScenarioCount} calculation scenarios", inputs?.Count ?? 0);

            if (inputs == null || inputs.Count < 2)
            {
                throw new ArgumentException("At least two calculation scenarios are required for comparison", nameof(inputs));
            }

            var results = await apiClient.PostAsync<List<CalculationInputModel>, List<CalculationResultModel>>(
                ApiEndpoints.Pricing.Compare, inputs);

            // Create comparison model
            var comparison = new CalculationComparisonModel
            {
                Calculations = results
            };

            // Find the lowest cost calculation and build country comparisons
            comparison.FindLowestCostCalculation();
            comparison.BuildCountryComparisons();

            logger.LogInformation("Comparison completed with {ScenarioCount} scenarios", results.Count);

            return comparison;
        }
    }
}