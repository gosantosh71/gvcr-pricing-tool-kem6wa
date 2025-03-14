using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.ValueObjects;

namespace VatFilingPricingTool.Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Rule entities with specialized query methods for accessing and managing
    /// country-specific VAT rules in the VAT Filing Pricing Tool.
    /// </summary>
    public interface IRuleRepository : IRepository<Rule>
    {
        /// <summary>
        /// Retrieves all rules for a specific country
        /// </summary>
        /// <param name="countryCode">The country code to filter rules by</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of rules for the specified country</returns>
        Task<IEnumerable<Rule>> GetRulesByCountryAsync(CountryCode countryCode);

        /// <summary>
        /// Retrieves all rules of a specific type
        /// </summary>
        /// <param name="ruleType">The rule type to filter rules by</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of rules of the specified type</returns>
        Task<IEnumerable<Rule>> GetRulesByTypeAsync(RuleType ruleType);

        /// <summary>
        /// Retrieves all active rules
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of active rules</returns>
        Task<IEnumerable<Rule>> GetActiveRulesAsync();

        /// <summary>
        /// Retrieves all rules effective at a specific date
        /// </summary>
        /// <param name="effectiveDate">The date for which to retrieve effective rules</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of rules effective at the specified date</returns>
        Task<IEnumerable<Rule>> GetEffectiveRulesAsync(DateTime effectiveDate);

        /// <summary>
        /// Retrieves a paginated list of rules with optional filtering
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="countryCode">Optional country code filter</param>
        /// <param name="ruleType">Optional rule type filter</param>
        /// <param name="activeOnly">When true, only retrieves active rules</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the paginated rules and total count</returns>
        Task<(IEnumerable<Rule> Rules, int TotalCount)> GetPagedRulesAsync(int pageNumber, int pageSize, CountryCode countryCode = null, RuleType? ruleType = null, bool activeOnly = false);

        /// <summary>
        /// Creates a new rule in the repository
        /// </summary>
        /// <param name="rule">The rule to create</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created rule</returns>
        Task<Rule> CreateAsync(Rule rule);

        /// <summary>
        /// Updates an existing rule in the repository
        /// </summary>
        /// <param name="rule">The rule to update</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated rule</returns>
        Task<Rule> UpdateAsync(Rule rule);

        /// <summary>
        /// Checks if a rule with the specified identifier exists
        /// </summary>
        /// <param name="ruleId">The rule identifier to check</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the rule exists</returns>
        Task<bool> ExistsByIdAsync(string ruleId);
    }
}