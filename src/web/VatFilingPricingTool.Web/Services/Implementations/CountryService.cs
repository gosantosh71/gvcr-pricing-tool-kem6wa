using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging v6.0.0
using VatFilingPricingTool.Web.Clients;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Services.Interfaces;

namespace VatFilingPricingTool.Web.Services.Implementations
{
    /// <summary>
    /// Implementation of the ICountryService interface that provides country-related functionality
    /// for the VAT Filing Pricing Tool web application.
    /// </summary>
    public class CountryService : ICountryService
    {
        private readonly ApiClient apiClient;
        private readonly ILogger<CountryService> logger;

        /// <summary>
        /// Initializes a new instance of the CountryService class with required dependencies.
        /// </summary>
        /// <param name="apiClient">Client for making HTTP requests to the backend API.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        public CountryService(ApiClient apiClient, ILogger<CountryService> logger)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a specific country by its country code.
        /// </summary>
        /// <param name="countryCode">The ISO country code (e.g., "GB" for United Kingdom).</param>
        /// <returns>The country model for the specified country code, or null if not found.</returns>
        public async Task<CountryModel> GetCountryAsync(string countryCode)
        {
            logger.LogInformation("Retrieving country with code: {CountryCode}", countryCode);
            
            if (string.IsNullOrEmpty(countryCode))
            {
                logger.LogWarning("Country code is null or empty");
                return null;
            }
            
            string endpoint = ApiEndpoints.Country.GetById.Replace("{id}", countryCode);
            return await apiClient.GetAsync<CountryModel>(endpoint);
        }

        /// <summary>
        /// Retrieves a list of countries with optional filtering for active countries only.
        /// </summary>
        /// <param name="activeOnly">When true, returns only active countries; otherwise, returns all countries.</param>
        /// <returns>List of countries matching the filter criteria.</returns>
        public async Task<List<CountryModel>> GetCountriesAsync(bool activeOnly = false)
        {
            logger.LogInformation("Retrieving countries with activeOnly: {ActiveOnly}", activeOnly);
            
            var countries = await apiClient.GetAsync<List<CountryModel>>(ApiEndpoints.Country.Get);
            
            if (activeOnly && countries != null)
            {
                countries = countries.Where(c => c.IsActive).ToList();
            }
            
            return countries ?? new List<CountryModel>();
        }

        /// <summary>
        /// Retrieves all active countries.
        /// </summary>
        /// <returns>List of active countries.</returns>
        public async Task<List<CountryModel>> GetActiveCountriesAsync()
        {
            logger.LogInformation("Retrieving active countries");
            return await GetCountriesAsync(true);
        }

        /// <summary>
        /// Retrieves countries that support a specific filing frequency.
        /// </summary>
        /// <param name="filingFrequency">The filing frequency to filter by.</param>
        /// <returns>List of countries supporting the specified filing frequency.</returns>
        public async Task<List<CountryModel>> GetCountriesByFilingFrequencyAsync(int filingFrequency)
        {
            logger.LogInformation("Retrieving countries by filing frequency: {FilingFrequency}", filingFrequency);
            
            string endpoint = ApiEndpoints.Country.GetByFrequency.Replace("{frequency}", filingFrequency.ToString());
            var countries = await apiClient.GetAsync<List<CountryModel>>(endpoint);
            
            return countries ?? new List<CountryModel>();
        }

        /// <summary>
        /// Retrieves a simplified list of countries for dropdown menus and selection components.
        /// </summary>
        /// <returns>List of country summaries.</returns>
        public async Task<List<CountrySummaryModel>> GetCountrySummariesAsync()
        {
            logger.LogInformation("Retrieving country summaries");
            
            string endpoint = $"{ApiEndpoints.Country.Get}?summaries=true";
            var summaries = await apiClient.GetAsync<List<CountrySummaryModel>>(endpoint);
            
            return summaries ?? new List<CountrySummaryModel>();
        }

        /// <summary>
        /// Initializes a country selection model with available countries for the selection UI.
        /// </summary>
        /// <returns>Initialized country selection model.</returns>
        public async Task<CountrySelectionModel> InitializeCountrySelectionAsync()
        {
            logger.LogInformation("Initializing country selection");
            
            var model = new CountrySelectionModel();
            var countries = await GetActiveCountriesAsync();
            
            model.AvailableCountries = countries.Select(c => new CountryOption
            {
                Value = c.CountryCode,
                Text = c.Name,
                FlagCode = c.CountryCode.ToLower(),
                IsSelected = false
            }).ToList();
            
            // SelectedCountryCodes is already initialized in the CountrySelectionModel constructor
            
            return model;
        }

        /// <summary>
        /// Searches for countries matching the provided search term.
        /// </summary>
        /// <param name="searchTerm">The search term to match against country names.</param>
        /// <returns>List of country options matching the search term.</returns>
        public async Task<List<CountryOption>> SearchCountriesAsync(string searchTerm)
        {
            logger.LogInformation("Searching countries with term: {SearchTerm}", searchTerm);
            
            var countries = await GetActiveCountriesAsync();
            
            if (string.IsNullOrEmpty(searchTerm))
            {
                return countries.Select(c => new CountryOption
                {
                    Value = c.CountryCode,
                    Text = c.Name,
                    FlagCode = c.CountryCode.ToLower(),
                    IsSelected = false
                }).ToList();
            }
            
            var filteredCountries = countries
                .Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Select(c => new CountryOption
                {
                    Value = c.CountryCode,
                    Text = c.Name,
                    FlagCode = c.CountryCode.ToLower(),
                    IsSelected = false
                })
                .ToList();
            
            return filteredCountries;
        }

        /// <summary>
        /// Retrieves detailed country models for the selected country codes.
        /// </summary>
        /// <param name="countryCodes">List of country codes to retrieve details for.</param>
        /// <returns>List of country models for the selected country codes.</returns>
        public async Task<List<CountryModel>> GetSelectedCountriesAsync(List<string> countryCodes)
        {
            logger.LogInformation("Retrieving selected countries: {CountryCodes}", 
                countryCodes != null ? string.Join(", ", countryCodes) : "null");
            
            if (countryCodes == null)
            {
                logger.LogWarning("Country codes list is null");
                return new List<CountryModel>();
            }
            
            if (countryCodes.Count == 0)
            {
                return new List<CountryModel>();
            }
            
            var allCountries = await GetCountriesAsync(false);
            var selectedCountries = allCountries
                .Where(c => countryCodes.Contains(c.CountryCode))
                .ToList();
            
            return selectedCountries;
        }
    }
}