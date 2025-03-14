using System.Collections.Generic;  // System.Collections.Generic v6.0.0
using System.Threading.Tasks;  // System.Threading.Tasks v6.0.0
using VatFilingPricingTool.Web.Models;

namespace VatFilingPricingTool.Web.Services.Interfaces
{
    /// <summary>
    /// Interface that defines the contract for pricing calculation services in the web application.
    /// Provides methods for VAT filing cost calculations, retrieval, saving, and comparison.
    /// </summary>
    public interface IPricingService
    {
        /// <summary>
        /// Calculates VAT filing costs based on the provided input parameters.
        /// </summary>
        /// <param name="input">The calculation input parameters including countries, service type, and transaction volume.</param>
        /// <returns>Calculation result with cost breakdown.</returns>
        Task<CalculationResultModel> CalculatePricingAsync(CalculationInputModel input);

        /// <summary>
        /// Retrieves a specific calculation by its ID.
        /// </summary>
        /// <param name="calculationId">The unique identifier of the calculation to retrieve.</param>
        /// <returns>The requested calculation result.</returns>
        Task<CalculationResultModel> GetCalculationAsync(string calculationId);

        /// <summary>
        /// Saves a calculation result for future reference.
        /// </summary>
        /// <param name="model">The model containing the calculation ID and metadata.</param>
        /// <returns>True if the calculation was saved successfully, otherwise false.</returns>
        Task<bool> SaveCalculationAsync(SaveCalculationModel model);

        /// <summary>
        /// Retrieves calculation history with optional filtering.
        /// </summary>
        /// <param name="filter">Optional filter parameters for the calculation history.</param>
        /// <returns>Calculation history with pagination.</returns>
        Task<CalculationHistoryModel> GetCalculationHistoryAsync(CalculationFilterModel filter);

        /// <summary>
        /// Deletes a specific calculation by its ID.
        /// </summary>
        /// <param name="calculationId">The unique identifier of the calculation to delete.</param>
        /// <returns>True if the calculation was deleted successfully, otherwise false.</returns>
        Task<bool> DeleteCalculationAsync(string calculationId);

        /// <summary>
        /// Retrieves available service type options for the pricing calculator.
        /// </summary>
        /// <returns>List of available service type options.</returns>
        Task<List<ServiceTypeOption>> GetServiceTypeOptionsAsync();

        /// <summary>
        /// Retrieves available filing frequency options for the pricing calculator.
        /// </summary>
        /// <returns>List of available filing frequency options.</returns>
        Task<List<FilingFrequencyOption>> GetFilingFrequencyOptionsAsync();

        /// <summary>
        /// Retrieves available additional service options for the pricing calculator.
        /// </summary>
        /// <returns>List of available additional service options.</returns>
        Task<List<AdditionalServiceOption>> GetAdditionalServiceOptionsAsync();

        /// <summary>
        /// Compares multiple calculation scenarios to help users identify the most cost-effective options.
        /// </summary>
        /// <param name="inputs">List of calculation input models to compare.</param>
        /// <returns>Comparison of calculation results.</returns>
        Task<CalculationComparisonModel> CompareCalculationsAsync(List<CalculationInputModel> inputs);
    }
}