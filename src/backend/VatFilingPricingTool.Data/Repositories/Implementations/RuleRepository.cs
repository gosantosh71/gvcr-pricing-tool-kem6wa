using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VatFilingPricingTool.Data.Context;
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.ValueObjects;

namespace VatFilingPricingTool.Data.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for Rule entities with specialized query methods
    /// for accessing and managing country-specific VAT rules in the VAT Filing Pricing Tool.
    /// </summary>
    public class RuleRepository : Repository<Rule>, IRuleRepository
    {
        /// <summary>
        /// Initializes a new instance of the RuleRepository class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="logger">Optional logger instance</param>
        public RuleRepository(IVatFilingDbContext context, ILogger<RuleRepository> logger = null) 
            : base(context, logger)
        {
        }

        /// <summary>
        /// Retrieves all rules for a specific country
        /// </summary>
        /// <param name="countryCode">The country code to filter rules by</param>
        /// <returns>A collection of rules for the specified country</returns>
        public async Task<IEnumerable<Rule>> GetRulesByCountryAsync(CountryCode countryCode)
        {
            _logger?.LogInformation("Retrieving rules for country {CountryCode}", countryCode?.Value);
            
            if (countryCode == null)
            {
                throw new ArgumentNullException(nameof(countryCode), "Country code cannot be null");
            }

            var rules = await _dbSet
                .Where(r => r.CountryCode.Value == countryCode.Value)
                .Include(r => r.Parameters)
                .ToListAsync();
            
            _logger?.LogInformation("Retrieved {Count} rules for country {CountryCode}", rules.Count, countryCode.Value);
            
            return rules;
        }

        /// <summary>
        /// Retrieves all rules of a specific type
        /// </summary>
        /// <param name="ruleType">The rule type to filter rules by</param>
        /// <returns>A collection of rules of the specified type</returns>
        public async Task<IEnumerable<Rule>> GetRulesByTypeAsync(RuleType ruleType)
        {
            _logger?.LogInformation("Retrieving rules for type {RuleType}", ruleType);
            
            var rules = await _dbSet
                .Where(r => r.Type == ruleType)
                .Include(r => r.Parameters)
                .ToListAsync();
            
            _logger?.LogInformation("Retrieved {Count} rules of type {RuleType}", rules.Count, ruleType);
            
            return rules;
        }

        /// <summary>
        /// Retrieves all active rules
        /// </summary>
        /// <returns>A collection of active rules</returns>
        public async Task<IEnumerable<Rule>> GetActiveRulesAsync()
        {
            _logger?.LogInformation("Retrieving active rules");
            
            var rules = await _dbSet
                .Where(r => r.IsActive)
                .Include(r => r.Parameters)
                .ToListAsync();
            
            _logger?.LogInformation("Retrieved {Count} active rules", rules.Count);
            
            return rules;
        }

        /// <summary>
        /// Retrieves all rules effective at a specific date
        /// </summary>
        /// <param name="effectiveDate">The date for which to retrieve effective rules</param>
        /// <returns>A collection of rules effective at the specified date</returns>
        public async Task<IEnumerable<Rule>> GetEffectiveRulesAsync(DateTime effectiveDate)
        {
            _logger?.LogInformation("Retrieving rules effective at {EffectiveDate}", effectiveDate);
            
            var rules = await _dbSet
                .Where(r => r.EffectiveFrom <= effectiveDate && 
                           (!r.EffectiveTo.HasValue || r.EffectiveTo.Value > effectiveDate))
                .Include(r => r.Parameters)
                .ToListAsync();
            
            _logger?.LogInformation("Retrieved {Count} rules effective at {EffectiveDate}", rules.Count, effectiveDate);
            
            return rules;
        }

        /// <summary>
        /// Retrieves a paginated list of rules with optional filtering
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="countryCode">Optional country code filter</param>
        /// <param name="ruleType">Optional rule type filter</param>
        /// <param name="activeOnly">When true, only retrieves active rules</param>
        /// <returns>A tuple containing the paginated rules and the total count</returns>
        public async Task<(IEnumerable<Rule> Rules, int TotalCount)> GetPagedRulesAsync(
            int pageNumber, 
            int pageSize, 
            CountryCode countryCode = null, 
            RuleType? ruleType = null, 
            bool activeOnly = false)
        {
            _logger?.LogInformation(
                "Retrieving paged rules - Page: {PageNumber}, Size: {PageSize}, CountryCode: {CountryCode}, RuleType: {RuleType}, ActiveOnly: {ActiveOnly}", 
                pageNumber, pageSize, countryCode?.Value, ruleType, activeOnly);
            
            if (pageNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero");
            }
            
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero");
            }

            // Build query with filters
            var query = _dbSet.AsQueryable();
            
            if (countryCode != null)
            {
                query = query.Where(r => r.CountryCode.Value == countryCode.Value);
            }
            
            if (ruleType.HasValue)
            {
                query = query.Where(r => r.Type == ruleType.Value);
            }
            
            if (activeOnly)
            {
                query = query.Where(r => r.IsActive);
            }
            
            // Get total count
            var totalCount = await query.CountAsync();
            
            // Apply pagination
            var rules = await query
                .OrderBy(r => r.Priority)
                .ThenBy(r => r.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(r => r.Parameters)
                .ToListAsync();
            
            _logger?.LogInformation(
                "Retrieved {Count} rules (page {PageNumber} of {TotalPages}, {TotalCount} total records)", 
                rules.Count, pageNumber, (int)Math.Ceiling(totalCount / (double)pageSize), totalCount);
            
            return (rules, totalCount);
        }

        /// <summary>
        /// Creates a new rule in the repository
        /// </summary>
        /// <param name="rule">The rule to create</param>
        /// <returns>The created rule</returns>
        public async Task<Rule> CreateAsync(Rule rule)
        {
            _logger?.LogInformation("Creating rule with ID {RuleId} for country {CountryCode}", rule?.RuleId, rule?.CountryCode?.Value);
            
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule), "Rule cannot be null");
            }
            
            await _dbSet.AddAsync(rule);
            await _context.SaveChangesAsync();
            
            _logger?.LogInformation("Successfully created rule with ID {RuleId}", rule.RuleId);
            
            return rule;
        }

        /// <summary>
        /// Updates an existing rule in the repository
        /// </summary>
        /// <param name="rule">The rule to update</param>
        /// <returns>The updated rule</returns>
        public async Task<Rule> UpdateAsync(Rule rule)
        {
            _logger?.LogInformation("Updating rule with ID {RuleId}", rule?.RuleId);
            
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule), "Rule cannot be null");
            }
            
            _dbSet.Update(rule);
            await _context.SaveChangesAsync();
            
            _logger?.LogInformation("Successfully updated rule with ID {RuleId}", rule.RuleId);
            
            return rule;
        }

        /// <summary>
        /// Checks if a rule with the specified identifier exists
        /// </summary>
        /// <param name="ruleId">The rule identifier to check</param>
        /// <returns>True if the rule exists, otherwise false</returns>
        public async Task<bool> ExistsByIdAsync(string ruleId)
        {
            _logger?.LogInformation("Checking if rule with ID {RuleId} exists", ruleId);
            
            if (string.IsNullOrEmpty(ruleId))
            {
                throw new ArgumentException("Rule ID cannot be null or empty", nameof(ruleId));
            }
            
            var exists = await _dbSet.AnyAsync(r => r.RuleId == ruleId);
            
            _logger?.LogInformation("Rule with ID {RuleId} {Exists}", ruleId, exists ? "exists" : "does not exist");
            
            return exists;
        }
    }
}