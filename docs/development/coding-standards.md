## Introduction

This document defines the coding standards and best practices for the VAT Filing Pricing Tool project. Following these standards ensures code consistency, maintainability, and quality across the development team. All team members are expected to adhere to these standards when contributing to the project.

## General Principles

These general principles apply to all code in the project, regardless of language or technology:

### Readability
- Write code that is easy to read and understand
- Choose clear, descriptive names for variables, methods, and classes
- Use consistent formatting and indentation
- Include appropriate comments for complex logic
- Prioritize readability over cleverness or brevity

### Maintainability
- Follow the Single Responsibility Principle (SRP)
- Keep methods and classes focused and cohesive
- Limit method length (aim for < 30 lines)
- Limit class size (aim for < 300 lines)
- Use meaningful abstractions
- Avoid deep nesting (maximum 3-4 levels)
- Minimize dependencies between components

### Consistency
- Follow established patterns and conventions
- Use consistent naming, formatting, and organization
- Apply the same solutions to similar problems
- Maintain consistency with existing code
- Use automated tools to enforce consistency

### Documentation
- Document public APIs with XML comments
- Include summaries for classes and methods
- Document parameters, return values, and exceptions
- Keep documentation up-to-date with code changes
- Document complex algorithms and business rules
- Include examples for non-obvious usage

### Error Handling
- Use exceptions for exceptional conditions, not for control flow
- Catch exceptions at appropriate levels
- Include meaningful error messages
- Log exceptions with appropriate context
- Use specific exception types
- Validate inputs at system boundaries

## C# Coding Standards

These standards apply to all C# code in the backend services and Blazor WebAssembly components:

### Naming Conventions
- **PascalCase**: Classes, interfaces, methods, properties, public fields, namespaces, enums
  - Example: `CalculationService`, `IUserRepository`, `CalculateTotalCost()`
- **camelCase**: Local variables, parameters, private fields
  - Example: `userId`, `transactionVolume`, `private int _counter;`
- **UPPER_CASE**: Constants
  - Example: `const string API_VERSION = "v1";`
- **Prefixes and Suffixes**:
  - Interfaces: Prefix with 'I' (e.g., `IUserService`)
  - Abstract classes: Consider 'Base' suffix (e.g., `RepositoryBase`)
  - Generic type parameters: Use 'T' or descriptive name (e.g., `TEntity`)
- **Descriptive Names**:
  - Use meaningful, descriptive names
  - Avoid abbreviations unless widely understood
  - Include units in names when applicable (e.g., `timeoutInSeconds`)
  - Boolean variables should be phrased as questions (e.g., `isValid`, `hasPermission`)

### File Organization
- One class per file (except for small related classes)
- Filename should match the primary class name
- Organize files in a directory structure that reflects the namespace structure
- Group related files in appropriate folders
- Follow this order within files:
  1. File header comment
  2. Namespace declaration
  3. Using directives (sorted: System namespaces first, then other namespaces alphabetically)
  4. Class/interface declaration
  5. Fields
  6. Properties
  7. Constructors
  8. Methods (grouped by functionality)
  9. Nested types

### Code Style
- Use 4 spaces for indentation (not tabs)
- Use braces for all control structures, even single-line statements
- Place opening braces on the same line as the declaration
- Place closing braces on a new line
- Add a space before opening braces
- Add a space after commas
- Add spaces around operators
- Limit line length to 120 characters
- Use explicit access modifiers (public, private, etc.)
- Prefer expression-bodied members for simple methods and properties
- Use pattern matching where appropriate
- Use `var` only when the type is obvious from the right side of the assignment

Example:
```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }
    
    public async Task<User> GetUserByIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        }
        
        return await _userRepository.GetByIdAsync(userId);
    }
}
```

### Language Features
- Use C# 10.0 features appropriately
- Use nullable reference types with appropriate annotations
- Use async/await for asynchronous operations
- Use LINQ for collection operations (but avoid excessive chaining)
- Use expression-bodied members for simple methods and properties
- Use string interpolation instead of string.Format
- Use object initializers and collection initializers
- Use pattern matching for type checking and casting
- Use tuple returns for simple multiple return values
- Use record types for immutable data

Example:
```csharp
// Use string interpolation
var message = $"User {userName} created at {creationTime:yyyy-MM-dd}";

// Use object initializers
var user = new User
{
    Id = Guid.NewGuid().ToString(),
    Name = userName,
    Email = email,
    CreatedAt = DateTime.UtcNow
};

// Use pattern matching
if (entity is User user && user.IsActive)
{
    // Use user directly
}
```

### Clean Code Practices
- Follow SOLID principles
- Use dependency injection for dependencies
- Avoid static methods and properties (except for pure utility functions)
- Prefer composition over inheritance
- Keep methods focused on a single responsibility
- Limit method parameters (aim for <= 3, use parameter objects for more)
- Avoid primitive obsession (use value objects for domain concepts)
- Use guard clauses for parameter validation
- Return early to avoid deep nesting
- Avoid using regions
- Minimize comments by writing self-documenting code

Example:
```csharp
// Good: Using guard clauses and returning early
public decimal CalculateVatAmount(decimal amount, decimal vatRate)
{
    if (amount < 0)
    {
        throw new ArgumentException("Amount cannot be negative", nameof(amount));
    }
    
    if (vatRate < 0 || vatRate > 100)
    {
        throw new ArgumentException("VAT rate must be between 0 and 100", nameof(vatRate));
    }
    
    return amount * (vatRate / 100);
}
```

### Documentation
- Use XML documentation comments for public APIs
- Document all public classes, interfaces, methods, properties, and fields
- Include summaries that describe the purpose and behavior
- Document parameters, return values, and exceptions
- Include examples for complex or non-obvious usage
- Keep documentation up-to-date with code changes

Example:
```csharp
/// <summary>
/// Calculates the VAT amount based on the given amount and VAT rate.
/// </summary>
/// <param name="amount">The base amount to calculate VAT on. Must be non-negative.</param>
/// <param name="vatRate">The VAT rate as a percentage (0-100). Must be between 0 and 100.</param>
/// <returns>The calculated VAT amount.</returns>
/// <exception cref="ArgumentException">Thrown when amount is negative or vatRate is outside the valid range.</exception>
public decimal CalculateVatAmount(decimal amount, decimal vatRate)
{
    // Implementation
}
```

## Domain-Driven Design Practices

The VAT Filing Pricing Tool follows Domain-Driven Design (DDD) principles to model complex business domains effectively:

### Domain Layer
- Place domain entities, value objects, and domain services in the Domain project
- Make domain entities rich with behavior, not just data containers
- Use factory methods for complex object creation
- Implement domain validation within entities
- Use value objects for concepts with no identity (Money, CountryCode, etc.)
- Implement domain events for cross-aggregate communication

Example:
```csharp
// Rich domain entity with behavior
public class Calculation
{
    // Properties
    public string CalculationId { get; private set; }
    public string UserId { get; private set; }
    public int TransactionVolume { get; private set; }
    public Money TotalCost { get; private set; }
    public ICollection<CalculationCountry> CalculationCountries { get; private set; }
    
    // Factory method
    public static Calculation Create(string userId, int transactionVolume, string currencyCode)
    {
        // Validation
        if (string.IsNullOrEmpty(userId))
            throw new ValidationException(ErrorCodes.Calculation.InvalidUserId);
            
        if (transactionVolume <= 0)
            throw new ValidationException(ErrorCodes.Calculation.InvalidTransactionVolume);
        
        // Create new instance
        return new Calculation
        {
            CalculationId = Guid.NewGuid().ToString(),
            UserId = userId,
            TransactionVolume = transactionVolume,
            TotalCost = Money.CreateZero(currencyCode),
            CalculationCountries = new HashSet<CalculationCountry>()
        };
    }
    
    // Domain behavior
    public CalculationCountry AddCountry(string countryCode, Money countryCost)
    {
        // Implementation with domain logic
    }
    
    public void RecalculateTotalCost()
    {
        // Implementation with domain logic
    }
}
```

### Value Objects
- Implement value objects as immutable
- Override equality methods (Equals, GetHashCode)
- Provide factory methods for creation with validation
- Use value objects for concepts like Money, CountryCode, VatRate

Example:
```csharp
public class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string CurrencyCode { get; }
    
    private Money(decimal amount, string currencyCode)
    {
        Amount = amount;
        CurrencyCode = currencyCode;
    }
    
    public static Money Create(decimal amount, string currencyCode)
    {
        if (string.IsNullOrEmpty(currencyCode))
            throw new ValidationException("Currency code cannot be null or empty");
            
        if (currencyCode.Length != 3)
            throw new ValidationException("Currency code must be 3 characters");
            
        return new Money(amount, currencyCode.ToUpperInvariant());
    }
    
    public static Money CreateZero(string currencyCode) => Create(0, currencyCode);
    
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount + other.Amount, CurrencyCode);
    }
    
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount - other.Amount, CurrencyCode);
    }
    
    private void EnsureSameCurrency(Money other)
    {
        if (CurrencyCode != other.CurrencyCode)
            throw new InvalidOperationException($"Cannot operate on money with different currencies: {CurrencyCode} and {other.CurrencyCode}");
    }
    
    // Equality implementation
    public bool Equals(Money other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Amount == other.Amount && CurrencyCode == other.CurrencyCode;
    }
    
    public override bool Equals(object obj) => Equals(obj as Money);
    
    public override int GetHashCode() => HashCode.Combine(Amount, CurrencyCode);
    
    public static bool operator ==(Money left, Money right) => Equals(left, right);
    
    public static bool operator !=(Money left, Money right) => !Equals(left, right);
}
```

### Repository Pattern
- Define repository interfaces in the Domain layer
- Implement repositories in the Data layer
- Use the Unit of Work pattern for transaction management
- Return domain entities from repositories, not data models
- Keep repositories focused on a single aggregate

Example:
```csharp
// Domain layer interface
public interface ICalculationRepository
{
    Task<Calculation> GetByIdAsync(string calculationId);
    Task<IEnumerable<Calculation>> GetByUserIdAsync(string userId);
    Task<Calculation> AddAsync(Calculation calculation);
    Task UpdateAsync(Calculation calculation);
    Task DeleteAsync(string calculationId);
}

// Data layer implementation
public class CalculationRepository : ICalculationRepository
{
    private readonly VatFilingDbContext _dbContext;
    
    public CalculationRepository(VatFilingDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Calculation> GetByIdAsync(string calculationId)
    {
        return await _dbContext.Calculations
            .Include(c => c.CalculationCountries)
            .Include(c => c.AdditionalServices)
            .FirstOrDefaultAsync(c => c.CalculationId == calculationId);
    }
    
    // Other methods implementation
}
```

### Service Layer
- Implement application services in the Service layer
- Use services to orchestrate domain operations
- Keep business logic in the domain layer, not in services
- Use services for cross-aggregate operations
- Implement CQRS pattern with separate command and query services when appropriate

Example:
```csharp
public class PricingService : IPricingService
{
    private readonly ICalculationRepository _calculationRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly IRuleEngine _ruleEngine;
    
    public PricingService(
        ICalculationRepository calculationRepository,
        ICountryRepository countryRepository,
        IRuleEngine ruleEngine)
    {
        _calculationRepository = calculationRepository;
        _countryRepository = countryRepository;
        _ruleEngine = ruleEngine;
    }
    
    public async Task<CalculationModel> CalculatePricingAsync(CalculationRequest request)
    {
        // Create domain entity using factory method
        var calculation = Calculation.Create(
            request.UserId,
            request.TransactionVolume,
            request.CurrencyCode);
            
        // Apply domain logic
        foreach (var countryCode in request.CountryCodes)
        {
            var country = await _countryRepository.GetByCodeAsync(countryCode);
            var countryCost = _ruleEngine.CalculateCountryCost(
                country,
                request.ServiceType,
                request.TransactionVolume,
                request.FilingFrequency);
                
            calculation.AddCountry(countryCode, countryCost);
        }
        
        // Persist the entity
        await _calculationRepository.AddAsync(calculation);
        
        // Return DTO
        return MapToModel(calculation);
    }
    
    private CalculationModel MapToModel(Calculation calculation)
    {
        // Mapping implementation
    }
}
```

## Blazor WebAssembly Standards

These standards apply to the Blazor WebAssembly frontend application:

### Component Organization
- Use a consistent folder structure for components
- Group related components in folders
- Use partial classes for complex components
- Separate component logic from presentation
- Keep components focused and reusable
- Follow a consistent naming convention for components

Folder structure:
```
Components/
  Common/           # Shared utility components
  Forms/            # Form-related components
  Layout/           # Layout components
  Pricing/          # Pricing-specific components
  Reports/          # Reporting components
Pages/              # Application pages
Shared/             # Shared layouts and components
```

### Component Design
- Design components to be reusable and composable
- Use parameters for component configuration
- Implement cascading parameters for shared state
- Use EventCallback for component events
- Implement IDisposable for cleanup when necessary
- Keep components small and focused
- Use CSS isolation for component styling

Example:
```csharp
@inherits ComponentBase
@implements IDisposable

<div class="country-selector @(Disabled ? "disabled" : "")">
    <div class="dropdown @(IsOpen ? "show" : "")">
        <button class="btn btn-outline-primary dropdown-toggle" 
                @onclick="ToggleDropdown" 
                disabled="@Disabled">
            @GetDropdownText()
        </button>
        
        @if (IsOpen)
        {
            <div class="dropdown-menu show">
                <div class="dropdown-header">
                    <input type="text" 
                           class="form-control" 
                           placeholder="Search countries..." 
                           @oninput="OnSearchInput" 
                           @ref="searchInput" />
                </div>
                
                @if (CountrySelection?.AvailableCountries?.Any() == true)
                {
                    @foreach (var country in CountrySelection.GetFilteredCountries())
                    {
                        <div class="dropdown-item @(country.IsSelected ? "active" : "")" 
                             @onclick="() => ToggleCountrySelection(country.Value)">
                            <span class="flag-icon flag-icon-@country.FlagCode.ToLower()"></span>
                            @country.Text
                        </div>
                    }
                }
                else
                {
                    <div class="dropdown-item disabled">No countries found</div>
                }
                
                @if (CountrySelection?.SelectedCountryCodes?.Any() == true)
                {
                    <div class="dropdown-divider"></div>
                    <div class="dropdown-item" @onclick="ClearSelection">
                        <i class="fas fa-times-circle"></i> Clear selection
                    </div>
                }
            </div>
        }
    </div>
</div>

@code {
    [Parameter] public IEnumerable<string> SelectedCountries { get; set; }
    [Parameter] public EventCallback<IEnumerable<string>> OnSelectionChanged { get; set; }
    [Parameter] public bool Disabled { get; set; }
    
    private bool IsOpen { get; set; }
    private bool IsLoading { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; }
    private ElementReference searchInput;
    private CountrySelectionModel CountrySelection { get; set; }
    
    // Component methods
}
```

### State Management
- Use cascading parameters for shared state
- Implement state containers for complex state
- Use services for global state
- Avoid excessive component parameters
- Consider using Flux/Redux patterns for complex applications
- Use EventCallback for component events

Example:
```csharp
// State container service
public class CalculationState
{
    private CalculationModel _currentCalculation;
    
    public CalculationModel CurrentCalculation
    {
        get => _currentCalculation;
        set
        {
            _currentCalculation = value;
            NotifyStateChanged();
        }
    }
    
    public event Action OnChange;
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}

// Component usage
@inject CalculationState CalculationState
@implements IDisposable

<div>
    @if (CalculationState.CurrentCalculation != null)
    {
        <h3>Total Cost: @CalculationState.CurrentCalculation.TotalCost.FormatAsCurrency()</h3>
        // Display calculation details
    }
</div>

@code {
    protected override void OnInitialized()
    {
        CalculationState.OnChange += StateHasChanged;
    }
    
    public void Dispose()
    {
        CalculationState.OnChange -= StateHasChanged;
    }
}
```

### Performance Optimization
- Use `@key` directive for optimizing list rendering
- Implement `ShouldRender()` for conditional rendering
- Use lazy loading for complex components
- Minimize component re-rendering
- Use virtualization for long lists
- Implement component memoization when appropriate
- Avoid excessive JavaScript interop

Example:
```csharp
<div>
    @if (Countries != null)
    {
        <Virtualize Items="Countries" Context="country" ItemSize="50">
            <div @key="country.Code" class="country-item">
                <span class="flag-icon flag-icon-@country.Code.ToLower()"></span>
                @country.Name
            </div>
        </Virtualize>
    }
</div>

@code {
    [Parameter] public List<CountryModel> Countries { get; set; }
    private HashSet<string> _previousCountryCodes = new();
    
    protected override bool ShouldRender()
    {
        if (Countries == null) return true;
        
        var currentCountryCodes = new HashSet<string>(Countries.Select(c => c.Code));
        bool shouldRender = !_previousCountryCodes.SetEquals(currentCountryCodes);
        
        if (shouldRender)
        {
            _previousCountryCodes = currentCountryCodes;
        }
        
        return shouldRender;
    }
}
```

### Form Handling
- Use EditForm for form handling
- Implement data annotations for validation
- Use custom validators for complex validation
- Implement proper error handling and display
- Use form submission events appropriately
- Consider using Fluent Validation for complex validation

Example:
```csharp
<EditForm Model="@calculationRequest" OnValidSubmit="HandleValidSubmit">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    <div class="form-group">
        <label for="transactionVolume">Transaction Volume:</label>
        <InputNumber id="transactionVolume" class="form-control" 
                     @bind-Value="calculationRequest.TransactionVolume" />
        <ValidationMessage For="@(() => calculationRequest.TransactionVolume)" />
    </div>
    
    <div class="form-group">
        <label>Filing Frequency:</label>
        <InputSelect class="form-control" @bind-Value="calculationRequest.FilingFrequency">
            <option value="@FilingFrequency.Monthly">Monthly</option>
            <option value="@FilingFrequency.Quarterly">Quarterly</option>
            <option value="@FilingFrequency.Annually">Annually</option>
        </InputSelect>
        <ValidationMessage For="@(() => calculationRequest.FilingFrequency)" />
    </div>
    
    <button type="submit" class="btn btn-primary">Calculate</button>
</EditForm>

@code {
    private CalculationRequest calculationRequest = new();
    
    private async Task HandleValidSubmit()
    {
        try
        {
            var result = await PricingService.CalculatePricingAsync(calculationRequest);
            // Handle successful calculation
        }
        catch (Exception ex)
        {
            // Handle error
        }
    }
}
```

### CSS and Styling
- Use CSS isolation for component-specific styles
- Follow a consistent naming convention for CSS classes
- Use Bootstrap for layout and common components
- Implement responsive design principles
- Use CSS variables for theming
- Minimize inline styles

Example:
```css
/* CountrySelector.razor.css */
.country-selector {
    position: relative;
    width: 100%;
}

.country-selector .dropdown-toggle {
    width: 100%;
    text-align: left;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.country-selector .dropdown-menu {
    width: 100%;
    max-height: 300px;
    overflow-y: auto;
}

.country-selector .dropdown-item {
    cursor: pointer;
    padding: 0.5rem 1rem;
}

.country-selector .dropdown-item.active {
    background-color: var(--primary-color-light);
    color: var(--text-color);
}

.country-selector .flag-icon {
    margin-right: 0.5rem;
}

.country-selector.disabled {
    opacity: 0.6;
    cursor: not-allowed;
}
```

## TypeScript/JavaScript Standards

These standards apply to TypeScript and JavaScript code in the frontend application:

### TypeScript Usage
- Use TypeScript for all new JavaScript code
- Enable strict type checking
- Define interfaces for data structures
- Use type annotations for function parameters and return types
- Use enums for fixed sets of values
- Leverage TypeScript's advanced features appropriately

Example:
```typescript
// Define interfaces for data structures
interface CalculationRequest {
    userId: string;
    serviceType: ServiceType;
    transactionVolume: number;
    filingFrequency: FilingFrequency;
    countryCodes: string[];
    currencyCode: string;
}

// Use enums for fixed sets of values
enum ServiceType {
    Standard = 0,
    Complex = 1,
    Priority = 2
}

enum FilingFrequency {
    Monthly = 0,
    Quarterly = 1,
    Annually = 2
}

// Type annotations for functions
function calculateTotalCost(request: CalculationRequest): Promise<number> {
    // Implementation
    return fetch('/api/pricing/calculate', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(request)
    })
    .then(response => response.json())
    .then(data => data.totalCost);
}
```

### Naming Conventions
- **camelCase**: Variables, functions, methods, properties, parameters
  - Example: `calculateTotalCost`, `userId`
- **PascalCase**: Classes, interfaces, types, enums, namespaces
  - Example: `CalculationService`, `UserModel`
- **UPPER_CASE**: Constants
  - Example: `MAX_RETRY_COUNT`
- Use descriptive, meaningful names
- Boolean variables should be phrased as questions
  - Example: `isValid`, `hasPermission`
- Avoid abbreviations unless widely understood

### Code Style
- Use 2 spaces for indentation
- Use semicolons at the end of statements
- Use single quotes for string literals
- Add trailing commas for multi-line arrays and objects
- Limit line length to 100 characters
- Use explicit type annotations when not obvious
- Use arrow functions for callbacks
- Use template literals for string interpolation
- Use destructuring assignment when appropriate
- Use spread operator for shallow copies

Example:
```typescript
// Good code style example
const calculateDiscount = (price: number, discountPercent: number): number => {
  if (price < 0) {
    throw new Error('Price cannot be negative');
  }
  
  if (discountPercent < 0 || discountPercent > 100) {
    throw new Error('Discount percentage must be between 0 and 100');
  }
  
  return price * (discountPercent / 100);
};

// Using destructuring and template literals
const formatUserInfo = (user: User): string => {
  const { firstName, lastName, email } = user;
  return `${firstName} ${lastName} (${email})`;
};

// Using spread operator
const updateUserPreferences = (user: User, preferences: Partial<Preferences>): User => {
  return {
    ...user,
    preferences: {
      ...user.preferences,
      ...preferences,
    },
  };
};
```

### JavaScript Interop
- Minimize JavaScript interop in Blazor
- Use IJSRuntime for JavaScript interop
- Create TypeScript wrapper classes for complex JavaScript functionality
- Use JavaScript isolation in Blazor
- Document all JavaScript interop functions
- Handle errors in JavaScript interop

Example:
```csharp
// C# code for JavaScript interop
@inject IJSRuntime JSRuntime

<button @onclick="ExportToPdf">Export to PDF</button>

@code {
    private async Task ExportToPdf()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("pdfExport.exportToPdf", "report-container");
        }
        catch (Exception ex)
        {
            // Handle error
        }
    }
}

// JavaScript code (wwwroot/js/pdfExport.js)
window.pdfExport = {
  exportToPdf: function(elementId) {
    const element = document.getElementById(elementId);
    if (!element) {
      console.error(`Element with id '${elementId}' not found`);
      return;
    }
    
    // PDF export implementation using a library like html2pdf.js
    html2pdf()
      .from(element)
      .save('report.pdf');
  }
};
```

### Error Handling
- Use try-catch blocks for error handling
- Create custom error types for specific errors
- Log errors with appropriate context
- Provide user-friendly error messages
- Handle async errors with try-catch or .catch()
- Use finally blocks for cleanup

Example:
```typescript
// Custom error type
class ApiError extends Error {
  constructor(public statusCode: number, message: string) {
    super(message);
    this.name = 'ApiError';
  }
}

// Error handling in async function
async function fetchUserData(userId: string): Promise<User> {
  try {
    const response = await fetch(`/api/users/${userId}`);
    
    if (!response.ok) {
      throw new ApiError(response.status, `Failed to fetch user data: ${response.statusText}`);
    }
    
    return await response.json();
  } catch (error) {
    console.error('Error fetching user data:', error);
    
    if (error instanceof ApiError) {
      // Handle API-specific error
      notifyUser(`API Error: ${error.message}`);
    } else {
      // Handle generic error
      notifyUser('An unexpected error occurred. Please try again later.');
    }
    
    throw error; // Re-throw for caller to handle if needed
  }
}
```

## Database and Data Access Standards

These standards apply to database design and data access code:

### Database Design
- Use meaningful table and column names
- Use singular nouns for table names
- Use appropriate data types for columns
- Define primary keys for all tables
- Use foreign keys for relationships
- Implement proper indexing for performance
- Use constraints to enforce data integrity
- Normalize database design appropriately
- Document database schema

Example:
```sql
-- Good table design
CREATE TABLE Calculation (
    CalculationId NVARCHAR(36) NOT NULL PRIMARY KEY,
    UserId NVARCHAR(36) NOT NULL,
    ServiceId NVARCHAR(36) NOT NULL,
    TransactionVolume INT NOT NULL,
    FilingFrequency INT NOT NULL,
    TotalCost DECIMAL(18, 2) NOT NULL,
    CurrencyCode NVARCHAR(3) NOT NULL,
    CalculationDate DATETIME2 NOT NULL,
    IsArchived BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Calculation_User FOREIGN KEY (UserId) REFERENCES [User](UserId),
    CONSTRAINT FK_Calculation_Service FOREIGN KEY (ServiceId) REFERENCES Service(ServiceId),
    CONSTRAINT CK_Calculation_TransactionVolume CHECK (TransactionVolume > 0)
);

-- Create appropriate indexes
CREATE INDEX IX_Calculation_UserId ON Calculation(UserId);
CREATE INDEX IX_Calculation_CalculationDate ON Calculation(CalculationDate);
```

### Entity Framework Core
- Use the repository pattern with Entity Framework
- Define entity configurations in separate classes
- Use fluent API for complex configurations
- Implement proper lazy loading or eager loading
- Use async methods for database operations
- Implement database migrations
- Use value conversions for value objects
- Document entity configurations

Example:
```csharp
// Entity configuration
public class CalculationConfiguration : IEntityTypeConfiguration<Calculation>
{
    public void Configure(EntityTypeBuilder<Calculation> builder)
    {
        builder.ToTable("Calculation");
        
        builder.HasKey(c => c.CalculationId);
        
        builder.Property(c => c.CalculationId)
            .HasMaxLength(36)
            .IsRequired();
            
        builder.Property(c => c.UserId)
            .HasMaxLength(36)
            .IsRequired();
            
        builder.Property(c => c.ServiceId)
            .HasMaxLength(36)
            .IsRequired();
            
        builder.Property(c => c.TransactionVolume)
            .IsRequired();
            
        builder.Property(c => c.FilingFrequency)
            .IsRequired()
            .HasConversion<int>();
            
        builder.OwnsOne(c => c.TotalCost, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalCost")
                .IsRequired();
                
            money.Property(m => m.CurrencyCode)
                .HasColumnName("CurrencyCode")
                .HasMaxLength(3)
                .IsRequired();
        });
        
        builder.Property(c => c.CalculationDate)
            .IsRequired();
            
        builder.Property(c => c.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.HasOne(c => c.User)
            .WithMany(u => u.Calculations)
            .HasForeignKey(c => c.UserId);
            
        builder.HasOne(c => c.Service)
            .WithMany(s => s.Calculations)
            .HasForeignKey(c => c.ServiceId);
            
        builder.HasMany(c => c.CalculationCountries)
            .WithOne(cc => cc.Calculation)
            .HasForeignKey(cc => cc.CalculationId);
            
        builder.HasMany(c => c.Reports)
            .WithOne(r => r.Calculation)
            .HasForeignKey(r => r.CalculationId);
    }
}

// DbContext configuration
public class VatFilingDbContext : DbContext, IVatFilingDbContext
{
    public VatFilingDbContext(DbContextOptions<VatFilingDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Calculation> Calculations { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Rule> Rules { get; set; }
    public DbSet<Report> Reports { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new CalculationConfiguration());
        modelBuilder.ApplyConfiguration(new CountryConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceConfiguration());
        modelBuilder.ApplyConfiguration(new RuleConfiguration());
        modelBuilder.ApplyConfiguration(new ReportConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}
```

### Data Access Patterns
- Use the repository pattern for data access
- Implement the Unit of Work pattern for transactions
- Use async/await for database operations
- Implement proper exception handling
- Use pagination for large result sets
- Optimize queries for performance
- Use stored procedures for complex queries when appropriate
- Implement proper connection management

Example: