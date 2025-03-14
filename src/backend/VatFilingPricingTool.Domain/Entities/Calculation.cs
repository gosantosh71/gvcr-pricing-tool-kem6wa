using System;
using System.Collections.Generic;
using System.Linq;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.ValueObjects;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Common.Constants;

namespace VatFilingPricingTool.Domain.Entities
{
    /// <summary>
    /// Represents a VAT filing cost calculation with detailed breakdown by country and service
    /// </summary>
    public class Calculation
    {
        /// <summary>
        /// Unique identifier for the calculation
        /// </summary>
        public string CalculationId { get; private set; }
        
        /// <summary>
        /// User who created the calculation
        /// </summary>
        public string UserId { get; private set; }
        
        /// <summary>
        /// The VAT filing service type selected for this calculation
        /// </summary>
        public string ServiceId { get; private set; }
        
        /// <summary>
        /// Number of transactions/invoices per period
        /// </summary>
        public int TransactionVolume { get; private set; }
        
        /// <summary>
        /// How frequently VAT filing is performed
        /// </summary>
        public FilingFrequency FilingFrequency { get; private set; }
        
        /// <summary>
        /// Total cost of VAT filing across all countries and services
        /// </summary>
        public Money TotalCost { get; private set; }
        
        /// <summary>
        /// When the calculation was created
        /// </summary>
        public DateTime CalculationDate { get; private set; }
        
        /// <summary>
        /// Currency code used for the calculation (ISO 4217)
        /// </summary>
        public string CurrencyCode { get; private set; }
        
        /// <summary>
        /// Indicates if the calculation has been archived
        /// </summary>
        public bool IsArchived { get; private set; }
        
        // Navigation properties
        /// <summary>
        /// Navigation property to the user who created the calculation
        /// </summary>
        public User User { get; private set; }
        
        /// <summary>
        /// Navigation property to the selected service
        /// </summary>
        public Service Service { get; private set; }
        
        /// <summary>
        /// Countries included in this calculation with their specific costs
        /// </summary>
        public ICollection<CalculationCountry> CalculationCountries { get; private set; }
        
        /// <summary>
        /// Additional services included in this calculation
        /// </summary>
        public ICollection<CalculationAdditionalService> AdditionalServices { get; private set; }
        
        /// <summary>
        /// Reports generated for this calculation
        /// </summary>
        public ICollection<Report> Reports { get; private set; }
        
        /// <summary>
        /// Default constructor for the Calculation entity
        /// </summary>
        protected Calculation()
        {
            CalculationCountries = new HashSet<CalculationCountry>();
            AdditionalServices = new HashSet<CalculationAdditionalService>();
            Reports = new HashSet<Report>();
            CalculationDate = DateTime.UtcNow;
            IsArchived = false;
        }
        
        /// <summary>
        /// Factory method to create a new Calculation instance
        /// </summary>
        /// <param name="userId">The ID of the user creating the calculation</param>
        /// <param name="serviceId">The selected service type ID</param>
        /// <param name="transactionVolume">Number of transactions/invoices</param>
        /// <param name="filingFrequency">How often VAT is filed</param>
        /// <param name="currencyCode">The currency code for the calculation (ISO 4217)</param>
        /// <returns>A new Calculation instance</returns>
        public static Calculation Create(string userId, string serviceId, int transactionVolume, FilingFrequency filingFrequency, string currencyCode)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ValidationException("User ID cannot be null or empty", 
                    new List<string> { "UserId is required" });
                
            if (string.IsNullOrEmpty(serviceId))
                throw new ValidationException("Service ID cannot be null or empty", 
                    new List<string> { "ServiceId is required" });
                
            if (transactionVolume <= 0)
                throw new ValidationException("Transaction volume must be greater than zero", 
                    new List<string> { $"Invalid transaction volume: {transactionVolume}" });
                
            if (string.IsNullOrEmpty(currencyCode))
                throw new ValidationException("Currency code cannot be null or empty", 
                    new List<string> { "CurrencyCode is required" });
            
            var calculation = new Calculation
            {
                CalculationId = Guid.NewGuid().ToString(),
                UserId = userId,
                ServiceId = serviceId,
                TransactionVolume = transactionVolume,
                FilingFrequency = filingFrequency,
                CurrencyCode = currencyCode,
                TotalCost = Money.CreateZero(currencyCode),
                CalculationDate = DateTime.UtcNow,
                IsArchived = false
            };
            
            return calculation;
        }
        
        /// <summary>
        /// Adds a country to the calculation with its specific cost
        /// </summary>
        /// <param name="countryCode">The country code to add</param>
        /// <param name="countryCost">The cost for VAT filing in this country</param>
        /// <returns>The created calculation country relationship</returns>
        public CalculationCountry AddCountry(string countryCode, Money countryCost)
        {
            if (string.IsNullOrEmpty(countryCode))
                throw new ValidationException("Country code cannot be null or empty", 
                    new List<string> { "CountryCode is required" });
                
            if (countryCost == null)
                throw new ValidationException("Country cost cannot be null", 
                    new List<string> { "CountryCost is required" });
                
            if (countryCost.Currency != CurrencyCode)
                throw new ValidationException("Country cost currency must match calculation currency", 
                    new List<string> { $"Currency mismatch: {countryCost.Currency} vs {CurrencyCode}" });
            
            // Check if country already exists
            if (CalculationCountries.Any(c => c.CountryCode == countryCode))
                throw new ValidationException("Country already added to calculation", 
                    new List<string> { $"Country {countryCode} is already included in this calculation" });
            
            // Add country
            var calculationCountry = CalculationCountry.Create(CalculationId, countryCode, countryCost);
            CalculationCountries.Add(calculationCountry);
            
            // Update total cost
            TotalCost = TotalCost.Add(countryCost);
            
            return calculationCountry;
        }
        
        /// <summary>
        /// Removes a country from the calculation
        /// </summary>
        /// <param name="countryCode">The country code to remove</param>
        /// <returns>True if the country was removed, false if it wasn't found</returns>
        public bool RemoveCountry(string countryCode)
        {
            var calculationCountry = CalculationCountries.FirstOrDefault(c => c.CountryCode == countryCode);
            if (calculationCountry == null)
                return false;
            
            CalculationCountries.Remove(calculationCountry);
            
            // Update total cost
            TotalCost = TotalCost.Subtract(calculationCountry.CountryCost);
            
            return true;
        }
        
        /// <summary>
        /// Adds an additional service to the calculation
        /// </summary>
        /// <param name="additionalServiceId">The additional service ID to add</param>
        /// <param name="serviceCost">The cost for this additional service</param>
        /// <returns>The created calculation additional service relationship</returns>
        public CalculationAdditionalService AddAdditionalService(string additionalServiceId, Money serviceCost)
        {
            if (string.IsNullOrEmpty(additionalServiceId))
                throw new ValidationException("Additional service ID cannot be null or empty", 
                    new List<string> { "AdditionalServiceId is required" });
                
            if (serviceCost == null)
                throw new ValidationException("Service cost cannot be null", 
                    new List<string> { "ServiceCost is required" });
                
            if (serviceCost.Currency != CurrencyCode)
                throw new ValidationException("Service cost currency must match calculation currency", 
                    new List<string> { $"Currency mismatch: {serviceCost.Currency} vs {CurrencyCode}" });
            
            // Check if service already exists
            if (AdditionalServices.Any(s => s.AdditionalServiceId == additionalServiceId))
                throw new ValidationException("Additional service already added to calculation", 
                    new List<string> { $"Service {additionalServiceId} is already included in this calculation" });
            
            // Add service
            var calculationService = CalculationAdditionalService.Create(CalculationId, additionalServiceId, serviceCost);
            AdditionalServices.Add(calculationService);
            
            // Update total cost
            TotalCost = TotalCost.Add(serviceCost);
            
            return calculationService;
        }
        
        /// <summary>
        /// Removes an additional service from the calculation
        /// </summary>
        /// <param name="additionalServiceId">The additional service ID to remove</param>
        /// <returns>True if the service was removed, false if it wasn't found</returns>
        public bool RemoveAdditionalService(string additionalServiceId)
        {
            var calculationService = AdditionalServices.FirstOrDefault(s => s.AdditionalServiceId == additionalServiceId);
            if (calculationService == null)
                return false;
            
            AdditionalServices.Remove(calculationService);
            
            // Update total cost
            TotalCost = TotalCost.Subtract(calculationService.Cost);
            
            return true;
        }
        
        /// <summary>
        /// Updates the transaction volume for the calculation
        /// </summary>
        /// <param name="newTransactionVolume">The new transaction volume</param>
        public void UpdateTransactionVolume(int newTransactionVolume)
        {
            if (newTransactionVolume <= 0)
                throw new ValidationException("Transaction volume must be greater than zero", 
                    new List<string> { $"Invalid transaction volume: {newTransactionVolume}" });
            
            TransactionVolume = newTransactionVolume;
        }
        
        /// <summary>
        /// Updates the filing frequency for the calculation
        /// </summary>
        /// <param name="newFilingFrequency">The new filing frequency</param>
        public void UpdateFilingFrequency(FilingFrequency newFilingFrequency)
        {
            FilingFrequency = newFilingFrequency;
        }
        
        /// <summary>
        /// Recalculates the total cost based on all countries and additional services
        /// </summary>
        /// <returns>The updated total cost</returns>
        public Money RecalculateTotalCost()
        {
            var totalCost = Money.CreateZero(CurrencyCode);
            
            // Add country costs
            foreach (var country in CalculationCountries)
            {
                totalCost = totalCost.Add(country.CountryCost);
            }
            
            // Add additional service costs
            foreach (var service in AdditionalServices)
            {
                totalCost = totalCost.Add(service.Cost);
            }
            
            TotalCost = totalCost;
            return TotalCost;
        }
        
        /// <summary>
        /// Applies a discount to the total cost
        /// </summary>
        /// <param name="discountPercentage">The discount percentage (0-100)</param>
        /// <param name="discountReason">The reason for the discount</param>
        /// <returns>The discounted amount</returns>
        public Money ApplyDiscount(decimal discountPercentage, string discountReason)
        {
            if (discountPercentage < 0 || discountPercentage > 100)
                throw new ValidationException("Discount percentage must be between 0 and 100", 
                    new List<string> { $"Invalid discount percentage: {discountPercentage}. Valid range is 0-100%" });
                
            if (string.IsNullOrEmpty(discountReason))
                throw new ValidationException("Discount reason cannot be null or empty", 
                    new List<string> { "DiscountReason is required" });
            
            var originalTotal = TotalCost;
            TotalCost = TotalCost.ApplyDiscount(discountPercentage);
            
            return originalTotal.Subtract(TotalCost); // Return the discount amount
        }
        
        /// <summary>
        /// Archives the calculation
        /// </summary>
        public void Archive()
        {
            IsArchived = true;
        }
        
        /// <summary>
        /// Unarchives the calculation
        /// </summary>
        public void Unarchive()
        {
            IsArchived = false;
        }
        
        /// <summary>
        /// Gets the cost for a specific country
        /// </summary>
        /// <param name="countryCode">The country code to get the cost for</param>
        /// <returns>The cost for the specified country, or null if not found</returns>
        public Money GetCountryCost(string countryCode)
        {
            var calculationCountry = CalculationCountries.FirstOrDefault(c => c.CountryCode == countryCode);
            return calculationCountry?.CountryCost;
        }
        
        /// <summary>
        /// Gets the number of countries included in the calculation
        /// </summary>
        /// <returns>The number of countries</returns>
        public int GetCountriesCount()
        {
            return CalculationCountries.Count;
        }
        
        /// <summary>
        /// Gets the number of additional services included in the calculation
        /// </summary>
        /// <returns>The number of additional services</returns>
        public int GetAdditionalServicesCount()
        {
            return AdditionalServices.Count;
        }
        
        /// <summary>
        /// Validates the calculation data
        /// </summary>
        private void Validate()
        {
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(UserId))
                errors.Add("UserId is required");
                
            if (string.IsNullOrEmpty(ServiceId))
                errors.Add("ServiceId is required");
                
            if (TransactionVolume <= 0)
                errors.Add($"Invalid transaction volume: {TransactionVolume}");
                
            if (string.IsNullOrEmpty(CurrencyCode))
                errors.Add("CurrencyCode is required");
            
            if (errors.Count > 0)
                throw new ValidationException("Calculation validation failed", errors);
        }
    }
}