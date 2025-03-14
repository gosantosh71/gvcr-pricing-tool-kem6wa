using FluentAssertions; // FluentAssertions package version 6.2.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using System; // System
using System.Collections.Generic; // System.Collections.Generic
using System.Linq; // System.Linq
using System.Threading.Tasks; // System.Threading.Tasks
using VatFilingPricingTool.Data.Repositories.Implementations; // Import for Repository<T>
using VatFilingPricingTool.Data.Repositories.Interfaces; // Import for IRepository<T>
using VatFilingPricingTool.Domain.Entities; // Import for Country
using VatFilingPricingTool.Domain.Enums; // Import for FilingFrequency enum
using VatFilingPricingTool.IntegrationTests.Database; // Import for DatabaseFixture
using VatFilingPricingTool.IntegrationTests.Utilities; // Import for IntegrationTestHelpers

using Xunit; // Xunit

namespace VatFilingPricingTool.IntegrationTests.Database
{
    /// <summary>
    /// Integration tests for repository implementations using an in-memory database
    /// </summary>
    public class RepositoryIntegrationTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of the RepositoryIntegrationTests class
        /// </summary>
        /// <param name="fixture">The database fixture</param>
        public RepositoryIntegrationTests(DatabaseFixture fixture)
        {
            // Store the provided fixture in _fixture field
            _fixture = fixture;

            // Create a new ILoggerFactory instance for logging
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddDebug();
            });

            // Reset the database to ensure a clean state for each test
            _fixture.ResetDatabase().Wait();
        }

        /// <summary>
        /// Tests that the generic repository can retrieve an entity by ID
        /// </summary>
        [Fact]
        public async Task GenericRepository_GetById_ReturnsCorrectEntity()
        {
            // Create a test country entity
            var testCountry = CreateTestCountry();

            // Create a generic repository for Country entities
            var repository = CreateGenericRepository<Country>();

            // Add the test country to the repository
            await repository.AddAsync(testCountry);

            // Retrieve the country by its ID
            var retrievedCountry = await repository.GetByIdAsync(testCountry.Code);

            // Assert that the retrieved country is not null
            Assert.NotNull(retrievedCountry);

            // Assert that the retrieved country has the expected properties
            Assert.Equal(testCountry.Code, retrievedCountry.Code);
            Assert.Equal(testCountry.Name, retrievedCountry.Name);
        }

        /// <summary>
        /// Tests that the generic repository can retrieve all entities
        /// </summary>
        [Fact]
        public async Task GenericRepository_GetAll_ReturnsAllEntities()
        {
            // Create multiple test country entities
            var testCountry1 = CreateTestCountry("GB", "United Kingdom", 20.0m);
            var testCountry2 = CreateTestCountry("DE", "Germany", 19.0m);

            // Create a generic repository for Country entities
            var repository = CreateGenericRepository<Country>();

            // Add the test countries to the repository
            await repository.AddAsync(testCountry1);
            await repository.AddAsync(testCountry2);

            // Retrieve all countries
            var allCountries = await repository.GetAllAsync();

            // Assert that the retrieved collection is not null
            Assert.NotNull(allCountries);

            // Assert that the retrieved collection contains the expected number of countries
            Assert.Equal(2, allCountries.Count());

            // Assert that all expected countries are present in the collection
            Assert.Contains(testCountry1, allCountries);
            Assert.Contains(testCountry2, allCountries);
        }

        /// <summary>
        /// Tests that the generic repository can find entities matching a predicate
        /// </summary>
        [Fact]
        public async Task GenericRepository_Find_ReturnsMatchingEntities()
        {
            // Create multiple test country entities with different properties
            var testCountry1 = CreateTestCountry("GB", "United Kingdom", 20.0m, true);
            var testCountry2 = CreateTestCountry("DE", "Germany", 19.0m, false);
            var testCountry3 = CreateTestCountry("FR", "France", 20.0m, true);

            // Create a generic repository for Country entities
            var repository = CreateGenericRepository<Country>();

            // Add the test countries to the repository
            await repository.AddAsync(testCountry1);
            await repository.AddAsync(testCountry2);
            await repository.AddAsync(testCountry3);

            // Find countries matching a specific predicate (e.g., IsActive == true)
            var activeCountries = await repository.FindAsync(c => c.IsActive == true);

            // Assert that the retrieved collection is not null
            Assert.NotNull(activeCountries);

            // Assert that the retrieved collection contains only countries matching the predicate
            Assert.True(activeCountries.All(c => c.IsActive == true));

            // Assert that the count of retrieved countries matches the expected count
            Assert.Equal(2, activeCountries.Count());
        }

        /// <summary>
        /// Tests that the generic repository can add a new entity to the database
        /// </summary>
        [Fact]
        public async Task GenericRepository_Add_AddsEntityToDatabase()
        {
            // Create a test country entity
            var testCountry = CreateTestCountry();

            // Create a generic repository for Country entities
            var repository = CreateGenericRepository<Country>();

            // Add the test country to the repository
            await repository.AddAsync(testCountry);

            // Retrieve all countries from the repository
            var allCountries = await repository.GetAllAsync();

            // Assert that the retrieved collection contains the added country
            Assert.Contains(testCountry, allCountries);

            // Assert that the added country has the expected properties
            var addedCountry = allCountries.FirstOrDefault(c => c.Code == testCountry.Code);
            Assert.NotNull(addedCountry);
            Assert.Equal(testCountry.Name, addedCountry.Name);
        }

        /// <summary>
        /// Tests that the generic repository can update an existing entity in the database
        /// </summary>
        [Fact]
        public async Task GenericRepository_Update_UpdatesEntityInDatabase()
        {
            // Create a test country entity
            var testCountry = CreateTestCountry();

            // Create a generic repository for Country entities
            var repository = CreateGenericRepository<Country>();

            // Add the test country to the repository
            await repository.AddAsync(testCountry);

            // Modify the country's properties (e.g., change the name)
            testCountry.UpdateName("New Country Name");

            // Update the country in the repository
            await repository.UpdateAsync(testCountry);

            // Retrieve the country by ID
            var updatedCountry = await repository.GetByIdAsync(testCountry.Code);

            // Assert that the retrieved country has the updated properties
            Assert.NotNull(updatedCountry);
            Assert.Equal("New Country Name", updatedCountry.Name);
        }

        /// <summary>
        /// Tests that the generic repository can delete an entity from the database
        /// </summary>
        [Fact]
        public async Task GenericRepository_Delete_RemovesEntityFromDatabase()
        {
            // Create a test country entity
            var testCountry = CreateTestCountry();

            // Create a generic repository for Country entities
            var repository = CreateGenericRepository<Country>();

            // Add the test country to the repository
            await repository.AddAsync(testCountry);

            // Delete the country from the repository
            await repository.DeleteAsync(testCountry.Code);

            // Attempt to retrieve the deleted country by ID
            var deletedCountry = await repository.GetByIdAsync(testCountry.Code);

            // Assert that the retrieved country is null
            Assert.Null(deletedCountry);

            // Retrieve all countries
            var allCountries = await repository.GetAllAsync();

            // Assert that the deleted country is not in the collection
            Assert.DoesNotContain(testCountry, allCountries);
        }

        /// <summary>
        /// Tests that the country repository can retrieve a country by its code
        /// </summary>
        [Fact]
        public async Task CountryRepository_GetByCode_ReturnsCorrectCountry()
        {
            // Create a test country entity with a specific country code
            var testCountry = CreateTestCountry("XX", "Test Country", 20.0m);

            // Create a country repository
            var repository = CreateCountryRepository();

            // Add the test country to the repository
            await repository.AddAsync(testCountry);

            // Retrieve the country by its code
            var retrievedCountry = await repository.GetByCodeAsync("XX");

            // Assert that the retrieved country is not null
            Assert.NotNull(retrievedCountry);

            // Assert that the retrieved country has the expected country code
            Assert.Equal("XX", retrievedCountry.Code);

            // Assert that the retrieved country has the expected properties
            Assert.Equal("Test Country", retrievedCountry.Name);
        }

        /// <summary>
        /// Tests that the country repository can retrieve only active countries
        /// </summary>
        [Fact]
        public async Task CountryRepository_GetActiveCountries_ReturnsOnlyActiveCountries()
        {
            // Create multiple test country entities with different active states
            var testCountry1 = CreateTestCountry("GB", "United Kingdom", 20.0m, true);
            var testCountry2 = CreateTestCountry("DE", "Germany", 19.0m, false);
            var testCountry3 = CreateTestCountry("FR", "France", 20.0m, true);

            // Create a country repository
            var repository = CreateCountryRepository();

            // Add the test countries to the repository
            await repository.AddAsync(testCountry1);
            await repository.AddAsync(testCountry2);
            await repository.AddAsync(testCountry3);

            // Retrieve active countries
            var activeCountries = await repository.GetActiveCountriesAsync();

            // Assert that the retrieved collection is not null
            Assert.NotNull(activeCountries);

            // Assert that the retrieved collection contains only active countries
            Assert.True(activeCountries.All(c => c.IsActive == true));

            // Assert that the count of retrieved countries matches the expected count of active countries
            Assert.Equal(2, activeCountries.Count());
        }

        /// <summary>
        /// Tests that the country repository can retrieve countries by their codes
        /// </summary>
        [Fact]
        public async Task CountryRepository_GetCountriesByCodes_ReturnsMatchingCountries()
        {
            // Create multiple test country entities with different country codes
            var testCountry1 = CreateTestCountry("GB", "United Kingdom", 20.0m);
            var testCountry2 = CreateTestCountry("DE", "Germany", 19.0m);
            var testCountry3 = CreateTestCountry("FR", "France", 20.0m);

            // Create a country repository
            var repository = CreateCountryRepository();

            // Add the test countries to the repository
            await repository.AddAsync(testCountry1);
            await repository.AddAsync(testCountry2);
            await repository.AddAsync(testCountry3);

            // Retrieve countries by a subset of country codes
            var selectedCountries = await repository.GetCountriesByCodesAsync(new List<string> { "GB", "FR" });

            // Assert that the retrieved collection is not null
            Assert.NotNull(selectedCountries);

            // Assert that the retrieved collection contains only countries with the specified codes
            Assert.True(selectedCountries.All(c => c.Code == "GB" || c.Code == "FR"));

            // Assert that the count of retrieved countries matches the expected count
            Assert.Equal(2, selectedCountries.Count());
        }
        
        /// <summary>
        /// Tests that the country repository can retrieve countries by filing frequency
        /// </summary>
        [Fact]
        public async Task CountryRepository_GetCountriesByFilingFrequency_ReturnsMatchingCountries()
        {
            // Create multiple test country entities with different filing frequencies
            var testCountry1 = CreateTestCountry("GB", "United Kingdom", 20.0m);
            testCountry1.AddFilingFrequency(FilingFrequency.Monthly);
            var testCountry2 = CreateTestCountry("DE", "Germany", 19.0m);
            testCountry2.AddFilingFrequency(FilingFrequency.Quarterly);
            var testCountry3 = CreateTestCountry("FR", "France", 20.0m);
            testCountry3.AddFilingFrequency(FilingFrequency.Monthly);

            // Create a country repository
            var repository = CreateCountryRepository();

            // Add the test countries to the repository
            await repository.AddAsync(testCountry1);
            await repository.AddAsync(testCountry2);
            await repository.AddAsync(testCountry3);

            // Retrieve countries by a specific filing frequency
            var monthlyCountries = await repository.GetCountriesByFilingFrequencyAsync(FilingFrequency.Monthly);

            // Assert that the retrieved collection is not null
            Assert.NotNull(monthlyCountries);

            // Assert that the retrieved collection contains only countries supporting the specified filing frequency
            Assert.True(monthlyCountries.All(c => c.SupportsFilingFrequency(FilingFrequency.Monthly)));

            // Assert that the count of retrieved countries matches the expected count
            Assert.Equal(2, monthlyCountries.Count());
        }

        /// <summary>
        /// Creates a test country entity for use in tests
        /// </summary>
        /// <param name="countryCode">The country code</param>
        /// <param name="name">The country name</param>
        /// <param name="vatRate">The VAT rate</param>
        /// <param name="isActive">Whether the country is active</param>
        /// <returns>A test country entity</returns>
        private Country CreateTestCountry(string countryCode = "GB", string name = "United Kingdom", decimal vatRate = 20.0m, bool isActive = true)
        {
            // Create a new Country entity using the Country.Create factory method
            var country = Country.Create(countryCode, name, vatRate, "EUR");

            // Set the country as active or inactive based on the isActive parameter
            country.SetActive(isActive);
            
            // Add Monthly and Quarterly filing frequencies to the country
            country.AddFilingFrequency(FilingFrequency.Monthly);
            country.AddFilingFrequency(FilingFrequency.Quarterly);

            // Return the created country entity
            return country;
        }

        /// <summary>
        /// Creates a generic repository for the specified entity type
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <returns>A generic repository instance</returns>
        private IRepository<T> CreateGenericRepository<T>() where T : class
        {
            // Create a new Repository<T> instance with the database context from the fixture
            var repository = new Repository<T>(_fixture.DbContext, _loggerFactory.CreateLogger<Repository<T>>());

            // Return the created repository
            return repository;
        }

        /// <summary>
        /// Creates a country repository for testing
        /// </summary>
        /// <returns>A country repository instance</returns>
        private ICountryRepository CreateCountryRepository()
        {
            // Create a new CountryRepository instance with the database context from the fixture
            var repository = new CountryRepository(_fixture.DbContext, _loggerFactory.CreateLogger<CountryRepository>());

            // Return the created repository
            return repository;
        }
    }
}