using System; // System v6.0.0
using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations v6.0.0
using System.Linq; // System.Linq v6.0.0
using System.Text.Json.Serialization; // System.Text.Json v6.0.0
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Web.Models
{
    /// <summary>
    /// Represents a country with VAT filing requirements for pricing calculations
    /// </summary>
    public class CountryModel
    {
        /// <summary>
        /// The ISO country code (e.g., "GB" for United Kingdom)
        /// </summary>
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string CountryCode { get; set; }

        /// <summary>
        /// The full name of the country
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// The standard VAT rate for the country (e.g., 20.00 for 20%)
        /// </summary>
        [Range(0, 100)]
        public decimal StandardVatRate { get; set; }

        /// <summary>
        /// The ISO currency code for the country (e.g., "EUR" for Euro)
        /// </summary>
        [StringLength(3, MinimumLength = 3)]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// The available filing frequencies for this country
        /// </summary>
        public List<FilingFrequency> AvailableFilingFrequencies { get; set; }

        /// <summary>
        /// Indicates if the country is active in the system
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The date and time when this country information was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountryModel"/> class
        /// </summary>
        public CountryModel()
        {
            AvailableFilingFrequencies = new List<FilingFrequency>();
            IsActive = true;
            LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Simplified representation of a country for dropdown lists and summary displays
    /// </summary>
    public class CountrySummaryModel
    {
        /// <summary>
        /// The ISO country code (e.g., "GB" for United Kingdom)
        /// </summary>
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string CountryCode { get; set; }

        /// <summary>
        /// The full name of the country
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// The standard VAT rate for the country (e.g., 20.00 for 20%)
        /// </summary>
        [Range(0, 100)]
        public decimal StandardVatRate { get; set; }

        /// <summary>
        /// Indicates if the country is active in the system
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountrySummaryModel"/> class
        /// </summary>
        public CountrySummaryModel()
        {
            IsActive = true;
        }
    }

    /// <summary>
    /// Represents a country option in a selection dropdown
    /// </summary>
    public class CountryOption
    {
        /// <summary>
        /// The value used for the country option (typically the ISO country code)
        /// </summary>
        [Required]
        public string Value { get; set; }

        /// <summary>
        /// The display text for the country option
        /// </summary>
        [Required]
        public string Text { get; set; }

        /// <summary>
        /// The country flag code used for displaying flag icons
        /// </summary>
        public string FlagCode { get; set; }

        /// <summary>
        /// Indicates if this country option is currently selected
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountryOption"/> class
        /// </summary>
        public CountryOption()
        {
            IsSelected = false;
        }
    }

    /// <summary>
    /// Model for managing country selection in the UI
    /// </summary>
    public class CountrySelectionModel
    {
        /// <summary>
        /// The list of available countries for selection
        /// </summary>
        public List<CountryOption> AvailableCountries { get; set; }

        /// <summary>
        /// The list of currently selected country codes
        /// </summary>
        public List<string> SelectedCountryCodes { get; set; }

        /// <summary>
        /// The current search term for filtering countries
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountrySelectionModel"/> class
        /// </summary>
        public CountrySelectionModel()
        {
            AvailableCountries = new List<CountryOption>();
            SelectedCountryCodes = new List<string>();
            SearchTerm = string.Empty;
        }

        /// <summary>
        /// Filters available countries based on the search term
        /// </summary>
        /// <returns>A filtered list of country options</returns>
        public List<CountryOption> GetFilteredCountries()
        {
            if (string.IsNullOrEmpty(SearchTerm))
                return AvailableCountries;

            return AvailableCountries
                .Where(c => c.Text.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Toggles the selection state of a country
        /// </summary>
        /// <param name="countryCode">The country code to toggle</param>
        public void ToggleCountrySelection(string countryCode)
        {
            var country = AvailableCountries.FirstOrDefault(c => c.Value == countryCode);
            if (country != null)
            {
                country.IsSelected = !country.IsSelected;

                if (country.IsSelected)
                {
                    if (!SelectedCountryCodes.Contains(countryCode))
                        SelectedCountryCodes.Add(countryCode);
                }
                else
                {
                    SelectedCountryCodes.Remove(countryCode);
                }
            }
        }

        /// <summary>
        /// Checks if a country is currently selected
        /// </summary>
        /// <param name="countryCode">The country code to check</param>
        /// <returns>True if the country is selected, false otherwise</returns>
        public bool IsCountrySelected(string countryCode)
        {
            return SelectedCountryCodes.Contains(countryCode);
        }

        /// <summary>
        /// Clears all selected countries
        /// </summary>
        public void ClearSelection()
        {
            SelectedCountryCodes.Clear();
            foreach (var country in AvailableCountries)
            {
                country.IsSelected = false;
            }
        }
    }
}