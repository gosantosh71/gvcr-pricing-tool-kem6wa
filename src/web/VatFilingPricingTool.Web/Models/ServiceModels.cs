using System; // Version 6.0.0
using System.Collections.Generic; // Version 6.0.0
using System.ComponentModel.DataAnnotations; // Version 6.0.0
using System.Text.Json.Serialization; // Version 6.0.0
using VatFilingPricingTool.Domain.Enums; // Internal import for ServiceType enum

namespace VatFilingPricingTool.Web.Models
{
    /// <summary>
    /// Represents a VAT filing service type with pricing and complexity information for the web application.
    /// This model is used for displaying detailed service information in the UI.
    /// </summary>
    public class ServiceModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the service.
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        [Required(ErrorMessage = "Service name is required")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the service.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the base price of the service.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Base price must be a positive value")]
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Gets or sets the currency code for the service price (e.g., EUR, USD).
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the formatted base price including the currency symbol.
        /// </summary>
        public string FormattedBasePrice { get; set; }

        /// <summary>
        /// Gets or sets the type of service (StandardFiling, ComplexFiling, PriorityService).
        /// </summary>
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the name of the service type for display purposes.
        /// </summary>
        public string ServiceTypeName { get; set; }

        /// <summary>
        /// Gets or sets the complexity level of the service (1-5, where 5 is most complex).
        /// </summary>
        [Range(1, 5, ErrorMessage = "Complexity level must be between 1 and 5")]
        public int ComplexityLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the service is active and available for selection.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceModel"/> class.
        /// </summary>
        public ServiceModel()
        {
            // Initialize properties with default values
            ServiceId = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            CurrencyCode = "EUR";
            FormattedBasePrice = string.Empty;
            ServiceTypeName = string.Empty;
            IsActive = true;
        }
    }

    /// <summary>
    /// Simplified representation of a service for dropdown lists and summary displays.
    /// Used for presenting service information in a condensed format.
    /// </summary>
    public class ServiceSummaryModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the service.
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of service.
        /// </summary>
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the name of the service type for display purposes.
        /// </summary>
        public string ServiceTypeName { get; set; }

        /// <summary>
        /// Gets or sets the base price of the service.
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Gets or sets the formatted base price including the currency symbol.
        /// </summary>
        public string FormattedBasePrice { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the service is active and available for selection.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceSummaryModel"/> class.
        /// </summary>
        public ServiceSummaryModel()
        {
            ServiceId = string.Empty;
            Name = string.Empty;
            ServiceTypeName = string.Empty;
            FormattedBasePrice = string.Empty;
            IsActive = true;
        }
    }

    /// <summary>
    /// Represents a service option in a selection dropdown.
    /// Used for creating selectable service options in UI components.
    /// </summary>
    public class ServiceOption
    {
        /// <summary>
        /// Gets or sets the value of the service option.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the display text of the service option.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the description of the service option.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the base price of the service option.
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Gets or sets the formatted base price including the currency symbol.
        /// </summary>
        public string FormattedBasePrice { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this option is currently selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceOption"/> class.
        /// </summary>
        /// <param name="value">The value of the service option.</param>
        /// <param name="text">The display text of the service option.</param>
        /// <param name="description">The description of the service option.</param>
        /// <param name="basePrice">The base price of the service option.</param>
        /// <param name="currencyCode">The currency code for formatting the price.</param>
        public ServiceOption(string value, string text, string description, decimal basePrice, string currencyCode)
        {
            Value = value;
            Text = text;
            Description = description;
            BasePrice = basePrice;
            FormattedBasePrice = string.Format("{0} {1:N2}", currencyCode, basePrice);
            IsSelected = false;
        }
    }

    /// <summary>
    /// View model for managing service type selection in the UI.
    /// Provides functionality for service type selection and display.
    /// </summary>
    public class ServiceTypeViewModel
    {
        /// <summary>
        /// Gets or sets the list of available service types.
        /// </summary>
        public List<ServiceOption> ServiceTypes { get; set; }

        /// <summary>
        /// Gets or sets the currently selected service type.
        /// </summary>
        public ServiceType SelectedServiceType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceTypeViewModel"/> class.
        /// </summary>
        public ServiceTypeViewModel()
        {
            ServiceTypes = new List<ServiceOption>();
            SelectedServiceType = ServiceType.StandardFiling;
        }

        /// <summary>
        /// Gets the description for the currently selected service type.
        /// </summary>
        /// <returns>The description of the selected service type.</returns>
        public string GetServiceTypeDescription()
        {
            var selectedOption = ServiceTypes.Find(st => st.Value == SelectedServiceType.ToString());
            return selectedOption?.Description ?? string.Empty;
        }

        /// <summary>
        /// Gets the display name for the currently selected service type.
        /// </summary>
        /// <returns>The display name of the selected service type.</returns>
        public string GetServiceTypeName()
        {
            var selectedOption = ServiceTypes.Find(st => st.Value == SelectedServiceType.ToString());
            return selectedOption?.Text ?? SelectedServiceType.ToString();
        }

        /// <summary>
        /// Selects a specific service type.
        /// </summary>
        /// <param name="serviceType">The service type to select.</param>
        public void SelectServiceType(ServiceType serviceType)
        {
            SelectedServiceType = serviceType;
            
            // Update the IsSelected property for all service types
            foreach (var option in ServiceTypes)
            {
                option.IsSelected = option.Value == serviceType.ToString();
            }
        }

        /// <summary>
        /// Checks if a specific service type is currently selected.
        /// </summary>
        /// <param name="serviceType">The service type to check.</param>
        /// <returns>True if the service type is selected, false otherwise.</returns>
        public bool IsServiceTypeSelected(ServiceType serviceType)
        {
            return SelectedServiceType == serviceType;
        }
    }

    /// <summary>
    /// Represents an additional service that can be added to the VAT filing service.
    /// These are optional services that can be selected alongside the main service type.
    /// </summary>
    public class AdditionalServiceModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the additional service.
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// Gets or sets the name of the additional service.
        /// </summary>
        [Required(ErrorMessage = "Service name is required")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the additional service.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the cost of the additional service.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Cost must be a positive value")]
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the currency code for the service cost (e.g., EUR, USD).
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the formatted cost including the currency symbol.
        /// </summary>
        public string FormattedCost { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the additional service is active and available.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalServiceModel"/> class.
        /// </summary>
        public AdditionalServiceModel()
        {
            ServiceId = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            CurrencyCode = "EUR";
            FormattedCost = string.Empty;
            IsActive = true;
        }
    }
}