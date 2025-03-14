using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging v6.0.0
using VatFilingPricingTool.Service.Interfaces;
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Service.Models;
using VatFilingPricingTool.Common.Models.Result;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Contracts.V1.Responses;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.Entities;

namespace VatFilingPricingTool.Service.Implementations
{
    /// <summary>
    /// Service implementation for country-related operations in the VAT Filing Pricing Tool
    /// </summary>
    public class CountryService : ICountryService
    {
        private readonly ICountryRepository _countryRepository;
        private readonly ILogger<CountryService> _logger;

        /// <summary>
        /// Initializes a new instance of the CountryService class
        /// </summary>
        /// <param name="countryRepository">Repository for country data access</param>
        /// <param name="logger">Logger for diagnostic information</param>
        /// <exception cref="ArgumentNullException">Thrown when repository or logger is null</exception>
        public CountryService(ICountryRepository countryRepository, ILogger<CountryService> logger)
        {
            _countryRepository = countryRepository ?? throw new ArgumentNullException(nameof(countryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a country by its country code
        /// </summary>
        /// <param name="request">The request containing the country code to retrieve</param>
        /// <returns>A Result containing the country response or error details</returns>
        public async Task<Result<CountryResponse>> GetCountryAsync(GetCountryRequest request)
        {
            try
            {
                _logger.LogInformation("Getting country with code: {CountryCode}", request?.CountryCode);
                
                if (request == null)
                {
                    return Result<CountryResponse>.Failure("Request cannot be null", ErrorCodes.General.BadRequest);
                }
                
                if (string.IsNullOrEmpty(request.CountryCode))
                {
                    return Result<CountryResponse>.Failure("Country code cannot be null or empty", ErrorCodes.General.BadRequest);
                }
                
                var country = await _countryRepository.GetByCodeAsync(request.CountryCode);
                
                if (country == null)
                {
                    return Result<CountryResponse>.Failure($"Country with code {request.CountryCode} not found", ErrorCodes.Country.CountryNotFound);
                }
                
                var countryModel = CountryModel.FromDomain(country);
                var response = new CountryResponse
                {
                    CountryCode = countryModel.CountryCode,
                    Name = countryModel.Name,
                    StandardVatRate = countryModel.StandardVatRate,
                    CurrencyCode = countryModel.CurrencyCode,
                    AvailableFilingFrequencies = countryModel.AvailableFilingFrequencies,
                    IsActive = countryModel.IsActive,
                    LastUpdated = countryModel.LastUpdated
                };
                
                return Result<CountryResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving country with code: {CountryCode}", request?.CountryCode);
                return Result<CountryResponse>.Failure("An error occurred while retrieving the country", ErrorCodes.General.ServerError);
            }
        }

        /// <summary>
        /// Retrieves a paginated list of countries with optional filtering
        /// </summary>
        /// <param name="request">The request containing pagination and filtering parameters</param>
        /// <returns>A Result containing the paginated countries response or error details</returns>
        public async Task<Result<CountriesResponse>> GetCountriesAsync(GetCountriesRequest request)
        {
            try
            {
                _logger.LogInformation("Getting countries with pagination: Page {Page}, PageSize {PageSize}, ActiveOnly {ActiveOnly}", 
                    request?.Page, request?.PageSize, request?.ActiveOnly);
                
                if (request == null)
                {
                    return Result<CountriesResponse>.Failure("Request cannot be null", ErrorCodes.General.BadRequest);
                }
                
                // Ensure valid pagination values
                int pageNumber = request.Page <= 0 ? 1 : request.Page;
                int pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
                
                // Get countries from repository with pagination
                var (countries, totalCount) = await _countryRepository.GetPagedCountriesAsync(
                    pageNumber, pageSize, request.ActiveOnly);
                
                // Create response with pagination metadata
                var response = new CountriesResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    Items = new List<CountryResponse>()
                };
                
                response.HasPreviousPage = pageNumber > 1;
                response.HasNextPage = pageNumber < response.TotalPages;
                
                // Transform domain entities to response models
                foreach (var country in countries)
                {
                    var countryModel = CountryModel.FromDomain(country);
                    response.Items.Add(new CountryResponse
                    {
                        CountryCode = countryModel.CountryCode,
                        Name = countryModel.Name,
                        StandardVatRate = countryModel.StandardVatRate,
                        CurrencyCode = countryModel.CurrencyCode,
                        AvailableFilingFrequencies = countryModel.AvailableFilingFrequencies,
                        IsActive = countryModel.IsActive,
                        LastUpdated = countryModel.LastUpdated
                    });
                }
                
                return Result<CountriesResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving countries with pagination: Page {Page}, PageSize {PageSize}", 
                    request?.Page, request?.PageSize);
                return Result<CountriesResponse>.Failure("An error occurred while retrieving countries", ErrorCodes.General.ServerError);
            }
        }

        /// <summary>
        /// Retrieves all active countries
        /// </summary>
        /// <returns>A Result containing a list of active countries or error details</returns>
        public async Task<Result<List<CountryResponse>>> GetActiveCountriesAsync()
        {
            try
            {
                _logger.LogInformation("Getting all active countries");
                
                var countries = await _countryRepository.GetActiveCountriesAsync();
                var response = new List<CountryResponse>();
                
                foreach (var country in countries)
                {
                    var countryModel = CountryModel.FromDomain(country);
                    response.Add(new CountryResponse
                    {
                        CountryCode = countryModel.CountryCode,
                        Name = countryModel.Name,
                        StandardVatRate = countryModel.StandardVatRate,
                        CurrencyCode = countryModel.CurrencyCode,
                        AvailableFilingFrequencies = countryModel.AvailableFilingFrequencies,
                        IsActive = countryModel.IsActive,
                        LastUpdated = countryModel.LastUpdated
                    });
                }
                
                return Result<List<CountryResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active countries");
                return Result<List<CountryResponse>>.Failure("An error occurred while retrieving active countries", ErrorCodes.General.ServerError);
            }
        }

        /// <summary>
        /// Retrieves countries that support a specific filing frequency
        /// </summary>
        /// <param name="filingFrequency">The filing frequency to filter by</param>
        /// <returns>A Result containing a list of countries supporting the specified filing frequency or error details</returns>
        public async Task<Result<List<CountryResponse>>> GetCountriesByFilingFrequencyAsync(FilingFrequency filingFrequency)
        {
            try
            {
                _logger.LogInformation("Getting countries by filing frequency: {FilingFrequency}", filingFrequency);
                
                var countries = await _countryRepository.GetCountriesByFilingFrequencyAsync(filingFrequency);
                var response = new List<CountryResponse>();
                
                foreach (var country in countries)
                {
                    var countryModel = CountryModel.FromDomain(country);
                    response.Add(new CountryResponse
                    {
                        CountryCode = countryModel.CountryCode,
                        Name = countryModel.Name,
                        StandardVatRate = countryModel.StandardVatRate,
                        CurrencyCode = countryModel.CurrencyCode,
                        AvailableFilingFrequencies = countryModel.AvailableFilingFrequencies,
                        IsActive = countryModel.IsActive,
                        LastUpdated = countryModel.LastUpdated
                    });
                }
                
                return Result<List<CountryResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving countries by filing frequency: {FilingFrequency}", filingFrequency);
                return Result<List<CountryResponse>>.Failure("An error occurred while retrieving countries by filing frequency", ErrorCodes.General.ServerError);
            }
        }

        /// <summary>
        /// Retrieves a simplified list of countries for dropdown menus and selection components
        /// </summary>
        /// <returns>A Result containing a list of country summaries or error details</returns>
        public async Task<Result<List<CountrySummaryResponse>>> GetCountrySummariesAsync()
        {
            try
            {
                _logger.LogInformation("Getting country summaries");
                
                var countries = await _countryRepository.GetActiveCountriesAsync();
                var response = new List<CountrySummaryResponse>();
                
                foreach (var country in countries)
                {
                    response.Add(new CountrySummaryResponse
                    {
                        CountryCode = country.Code.Value,
                        Name = country.Name,
                        StandardVatRate = country.StandardVatRate.Value,
                        IsActive = country.IsActive
                    });
                }
                
                return Result<List<CountrySummaryResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving country summaries");
                return Result<List<CountrySummaryResponse>>.Failure("An error occurred while retrieving country summaries", ErrorCodes.General.ServerError);
            }
        }

        /// <summary>
        /// Creates a new country with the specified details
        /// </summary>
        /// <param name="request">The request containing the country details to create</param>
        /// <returns>A Result containing the creation response or error details</returns>
        public async Task<Result<CreateCountryResponse>> CreateCountryAsync(CreateCountryRequest request)
        {
            try
            {
                _logger.LogInformation("Creating new country with code: {CountryCode}", request?.CountryCode);
                
                if (request == null)
                {
                    return Result<CreateCountryResponse>.Failure("Request cannot be null", ErrorCodes.General.BadRequest);
                }
                
                if (string.IsNullOrEmpty(request.CountryCode))
                {
                    return Result<CreateCountryResponse>.Failure("Country code cannot be null or empty", ErrorCodes.General.BadRequest);
                }
                
                if (string.IsNullOrEmpty(request.Name))
                {
                    return Result<CreateCountryResponse>.Failure("Country name cannot be null or empty", ErrorCodes.General.BadRequest);
                }
                
                if (string.IsNullOrEmpty(request.CurrencyCode))
                {
                    return Result<CreateCountryResponse>.Failure("Currency code cannot be null or empty", ErrorCodes.General.BadRequest);
                }
                
                if (request.AvailableFilingFrequencies == null || !request.AvailableFilingFrequencies.Any())
                {
                    return Result<CreateCountryResponse>.Failure("At least one filing frequency must be specified", ErrorCodes.General.BadRequest);
                }
                
                // Check if country already exists
                bool exists = await CountryExistsAsync(request.CountryCode);
                if (exists)
                {
                    return Result<CreateCountryResponse>.Failure($"Country with code {request.CountryCode} already exists", ErrorCodes.Country.DuplicateCountryCode);
                }
                
                try
                {
                    // Create domain entity
                    var country = Country.Create(
                        request.CountryCode,
                        request.Name,
                        request.StandardVatRate,
                        request.CurrencyCode
                    );
                    
                    // Add filing frequencies
                    foreach (var frequency in request.AvailableFilingFrequencies)
                    {
                        country.AddFilingFrequency(frequency);
                    }
                    
                    // Save to repository
                    var createdCountry = await _countryRepository.CreateAsync(country);
                    
                    // Create response
                    var response = new CreateCountryResponse
                    {
                        CountryCode = createdCountry.Code.Value,
                        Name = createdCountry.Name,
                        Success = true,
                        Message = $"Country {createdCountry.Name} created successfully"
                    };
                    
                    return Result<CreateCountryResponse>.Success(response);
                }
                catch (Domain.Exceptions.ValidationException vex)
                {
                    _logger.LogWarning(vex, "Validation error when creating country: {CountryCode}", request.CountryCode);
                    return Result<CreateCountryResponse>.Failure(vex.Message, ErrorCodes.Country.CountryCreationFailed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating country: {CountryCode}", request?.CountryCode);
                return Result<CreateCountryResponse>.Failure("An error occurred while creating the country", ErrorCodes.General.ServerError);
            }
        }

        /// <summary>
        /// Updates an existing country with the specified details
        /// </summary>
        /// <param name="request">The request containing the updated country details</param>
        /// <returns>A Result containing the update response or error details</returns>
        public async Task<Result<UpdateCountryResponse>> UpdateCountryAsync(UpdateCountryRequest request)
        {
            try
            {
                _logger.LogInformation("Updating country with code: {CountryCode}", request?.CountryCode);
                
                if (request == null)
                {
                    return Result<UpdateCountryResponse>.Failure("Request cannot be null", ErrorCodes.General.BadRequest);
                }
                
                if (string.IsNullOrEmpty(request.CountryCode))
                {
                    return Result<UpdateCountryResponse>.Failure("Country code cannot be null or empty", ErrorCodes.General.BadRequest);
                }
                
                if (string.IsNullOrEmpty(request.Name))
                {
                    return Result<UpdateCountryResponse>.Failure("Country name cannot be null or empty", ErrorCodes.General.BadRequest);
                }
                
                if (string.IsNullOrEmpty(request.CurrencyCode))
                {
                    return Result<UpdateCountryResponse>.Failure("Currency code cannot be null or empty", ErrorCodes.General.BadRequest);
                }
                
                if (request.AvailableFilingFrequencies == null || !request.AvailableFilingFrequencies.Any())
                {
                    return Result<UpdateCountryResponse>.Failure("At least one filing frequency must be specified", ErrorCodes.General.BadRequest);
                }
                
                try
                {
                    // Get existing country
                    var existingCountry = await _countryRepository.GetByCodeAsync(request.CountryCode);
                    
                    if (existingCountry == null)
                    {
                        return Result<UpdateCountryResponse>.Failure($"Country with code {request.CountryCode} not found", ErrorCodes.Country.CountryNotFound);
                    }
                    
                    // Update properties
                    existingCountry.UpdateName(request.Name);
                    existingCountry.UpdateStandardVatRate(request.StandardVatRate);
                    existingCountry.UpdateCurrencyCode(request.CurrencyCode);
                    existingCountry.SetActive(request.IsActive);
                    
                    // Update filing frequencies
                    // First, remove all existing frequencies
                    var currentFrequencies = existingCountry.AvailableFilingFrequencies.ToList();
                    foreach (var frequency in currentFrequencies)
                    {
                        existingCountry.RemoveFilingFrequency(frequency);
                    }
                    
                    // Then add the new ones
                    foreach (var frequency in request.AvailableFilingFrequencies)
                    {
                        existingCountry.AddFilingFrequency(frequency);
                    }
                    
                    // Save to repository
                    var updatedCountry = await _countryRepository.UpdateAsync(existingCountry);
                    
                    // Create response
                    var response = new UpdateCountryResponse
                    {
                        CountryCode = updatedCountry.Code.Value,
                        Name = updatedCountry.Name,
                        LastUpdated = updatedCountry.LastUpdated,
                        Success = true,
                        Message = $"Country {updatedCountry.Name} updated successfully"
                    };
                    
                    return Result<UpdateCountryResponse>.Success(response);
                }
                catch (Domain.Exceptions.ValidationException vex)
                {
                    _logger.LogWarning(vex, "Validation error when updating country: {CountryCode}", request.CountryCode);
                    return Result<UpdateCountryResponse>.Failure(vex.Message, ErrorCodes.Country.CountryUpdateFailed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating country: {CountryCode}", request?.CountryCode);
                return Result<UpdateCountryResponse>.Failure("An error occurred while updating the country", ErrorCodes.General.ServerError);
            }
        }

        /// <summary>
        /// Deletes a country with the specified country code
        /// </summary>
        /// <param name="request">The request containing the country code to delete</param>
        /// <returns>A Result containing the deletion response or error details</returns>
        public async Task<Result<DeleteCountryResponse>> DeleteCountryAsync(DeleteCountryRequest request)
        {
            try
            {
                _logger.LogInformation("Deleting country with code: {CountryCode}", request?.CountryCode);
                
                if (request == null)
                {
                    return Result<DeleteCountryResponse>.Failure("Request cannot be null", ErrorCodes.General.BadRequest);
                }
                
                if (string.IsNullOrEmpty(request.CountryCode))
                {
                    return Result<DeleteCountryResponse>.Failure("Country code cannot be null or empty", ErrorCodes.General.BadRequest);
                }
                
                // Check if country exists
                bool exists = await CountryExistsAsync(request.CountryCode);
                if (!exists)
                {
                    return Result<DeleteCountryResponse>.Failure($"Country with code {request.CountryCode} not found", ErrorCodes.Country.CountryNotFound);
                }
                
                // Delete from repository
                bool deleted = await _countryRepository.DeleteByCodeAsync(request.CountryCode);
                
                if (!deleted)
                {
                    return Result<DeleteCountryResponse>.Failure($"Failed to delete country with code {request.CountryCode}", ErrorCodes.Country.CountryDeletionFailed);
                }
                
                // Create response
                var response = new DeleteCountryResponse
                {
                    CountryCode = request.CountryCode,
                    Success = true,
                    Message = $"Country with code {request.CountryCode} deleted successfully"
                };
                
                return Result<DeleteCountryResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting country: {CountryCode}", request?.CountryCode);
                return Result<DeleteCountryResponse>.Failure("An error occurred while deleting the country", ErrorCodes.General.ServerError);
            }
        }

        /// <summary>
        /// Checks if a country with the specified country code exists
        /// </summary>
        /// <param name="countryCode">The country code to check</param>
        /// <returns>True if the country exists, false otherwise</returns>
        public async Task<bool> CountryExistsAsync(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                return false;
            }
            
            return await _countryRepository.ExistsByCodeAsync(countryCode);
        }
    }
}