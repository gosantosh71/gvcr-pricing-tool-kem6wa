using System; // Version 6.0.0
using System.Collections.Generic; // Version 6.0.0
using System.Linq; // Version 6.0.0

namespace VatFilingPricingTool.Service.Models
{
    /// <summary>
    /// Service model representing a country with VAT filing requirements for pricing calculations
    /// </summary>
    public class CountryModel
    {
        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        public string CountryCode { get; set; }
        
        /// <summary>
        /// Gets or sets the full name of the country.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the standard VAT rate applied in this country.
        /// </summary>
        public decimal StandardVatRate { get; set; }
        
        /// <summary>
        /// Gets or sets the three-letter ISO currency code used in this country.
        /// </summary>
        public string CurrencyCode { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of filing frequencies available in this country.
        /// </summary>
        public List<Domain.Enums.FilingFrequency> AvailableFilingFrequencies { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this country is active for VAT filing calculations.
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when this country information was last updated.
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CountryModel"/> class.
        /// </summary>
        public CountryModel()
        {
            AvailableFilingFrequencies = new List<Domain.Enums.FilingFrequency>();
            IsActive = true;
            LastUpdated = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Creates a service model from a domain entity.
        /// </summary>
        /// <param name="country">The domain entity.</param>
        /// <returns>A new CountryModel populated with data from the domain entity.</returns>
        public static CountryModel FromDomain(Domain.Entities.Country country)
        {
            if (country == null)
            {
                throw new ArgumentNullException(nameof(country));
            }
            
            return new CountryModel
            {
                CountryCode = country.Code.Value,
                Name = country.Name,
                StandardVatRate = country.StandardVatRate.Value,
                CurrencyCode = country.CurrencyCode,
                AvailableFilingFrequencies = new List<Domain.Enums.FilingFrequency>(country.AvailableFilingFrequencies),
                IsActive = country.IsActive,
                LastUpdated = country.LastUpdated
            };
        }
        
        /// <summary>
        /// Creates a domain entity from a service model.
        /// </summary>
        /// <returns>A new Country domain entity populated with data from the service model.</returns>
        public Domain.Entities.Country ToDomain()
        {
            var country = Domain.Entities.Country.Create(
                CountryCode,
                Name,
                StandardVatRate,
                CurrencyCode);
            
            foreach (var frequency in AvailableFilingFrequencies)
            {
                country.AddFilingFrequency(frequency);
            }
            
            if (!IsActive)
            {
                country.SetActive(false);
            }
            
            return country;
        }
        
        /// <summary>
        /// Converts the service model to a contract model for API responses.
        /// </summary>
        /// <returns>A contract model populated with data from the service model.</returns>
        public Contracts.V1.Models.CountryModel ToContract()
        {
            return new Contracts.V1.Models.CountryModel
            {
                CountryCode = this.CountryCode,
                Name = this.Name,
                StandardVatRate = this.StandardVatRate,
                CurrencyCode = this.CurrencyCode,
                AvailableFilingFrequencies = new List<Domain.Enums.FilingFrequency>(this.AvailableFilingFrequencies),
                IsActive = this.IsActive,
                LastUpdated = this.LastUpdated
            };
        }
        
        /// <summary>
        /// Creates a service model from a contract model.
        /// </summary>
        /// <param name="contractModel">The contract model.</param>
        /// <returns>A new CountryModel populated with data from the contract model.</returns>
        public static CountryModel FromContract(Contracts.V1.Models.CountryModel contractModel)
        {
            if (contractModel == null)
            {
                throw new ArgumentNullException(nameof(contractModel));
            }
            
            return new CountryModel
            {
                CountryCode = contractModel.CountryCode,
                Name = contractModel.Name,
                StandardVatRate = contractModel.StandardVatRate,
                CurrencyCode = contractModel.CurrencyCode,
                AvailableFilingFrequencies = new List<Domain.Enums.FilingFrequency>(contractModel.AvailableFilingFrequencies),
                IsActive = contractModel.IsActive,
                LastUpdated = contractModel.LastUpdated
            };
        }
    }
}