#nullable disable
using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq; // System.Linq package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using Microsoft.EntityFrameworkCore; // Microsoft.EntityFrameworkCore package version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using Moq; // For creating and configuring mock objects - Moq package version 4.18.2
using VatFilingPricingTool.Data.Context; // Provides access to the database context
using VatFilingPricingTool.Data.Repositories.Implementations; // The repository class being tested
using VatFilingPricingTool.Data.Repositories.Interfaces; // Interface that this repository implements
using VatFilingPricingTool.Domain.Entities; // The Country entity that this repository manages
using VatFilingPricingTool.Domain.Enums; // Enum for testing country filing frequency operations
using VatFilingPricingTool.UnitTests.Helpers; // Provides mock country data for testing
using Xunit; // Testing framework - Xunit package version 2.4.2
using FluentAssertions; // For more readable assertions - FluentAssertions package version 6.7.0

namespace VatFilingPricingTool.UnitTests.Repositories
{
    /// <summary>
    /// Test class for the CountryRepository implementation
    /// </summary>
    public class CountryRepositoryTests
    {
        private readonly Mock<IVatFilingDbContext> _mockContext;
        private readonly Mock<DbSet<Country>> _mockCountryDbSet;
        private readonly List<Country> _countries;
        private readonly ICountryRepository _repository;
        private readonly Mock<ILogger<CountryRepository>> _mockLogger;

        /// <summary>
        /// Initializes a new instance of the CountryRepositoryTests class
        /// </summary>
        public CountryRepositoryTests()
        {
            // Initialize mock objects and test data
            _mockContext = new Mock<IVatFilingDbContext>();
            _mockCountryDbSet = new Mock<DbSet<Country>>();
            _countries = MockData.GetMockCountries();

            // Setup mock DbSet with _countries data
            SetupMockDbSet();

            // Setup _mockContext.Countries to return _mockCountryDbSet.Object
            _mockContext.Setup(c => c.Countries).Returns(_mockCountryDbSet.Object);

            // Initialize mock logger
            _mockLogger = new Mock<ILogger<CountryRepository>>();

            // Initialize the repository with the mock context and logger
            _repository = new CountryRepository(_mockContext.Object, _mockLogger.Object);
        }

        /// <summary>
        /// Sets up the mock DbSet with the test data
        /// </summary>
        private void SetupMockDbSet()
        {
            // Create a queryable source from _countries
            var queryable = _countries.AsQueryable();

            // Setup _mockCountryDbSet.As<IQueryable<Country>>().Provider to return queryable.Provider
            _mockCountryDbSet.As<IQueryable<Country>>().Setup(m => m.Provider).Returns(queryable.Provider);

            // Setup _mockCountryDbSet.As<IQueryable<Country>>().Expression to return queryable.Expression
            _mockCountryDbSet.As<IQueryable<Country>>().Setup(m => m.Expression).Returns(queryable.Expression);

            // Setup _mockCountryDbSet.As<IQueryable<Country>>().ElementType to return queryable.ElementType
            _mockCountryDbSet.As<IQueryable<Country>>().Setup(m => m.ElementType).Returns(queryable.ElementType);

            // Setup _mockCountryDbSet.As<IQueryable<Country>>().GetEnumerator() to return queryable.GetEnumerator()
            _mockCountryDbSet.As<IQueryable<Country>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            // Setup _mockCountryDbSet.As<IAsyncEnumerable<Country>>().GetAsyncEnumerator() to return TestHelpers.CreateAsyncEnumerable(_countries).GetAsyncEnumerator()
            _mockCountryDbSet.As<IAsyncEnumerable<Country>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(TestHelpers.CreateAsyncEnumerable(_countries).GetAsyncEnumerator());

            // Setup _mockCountryDbSet.FindAsync() to return ValueTask.FromResult(_countries.FirstOrDefault())
            _mockCountryDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] keyValues) => _countries.FirstOrDefault(c => c.Code.Value == (string)keyValues[0]));
        }

        /// <summary>
        /// Tests that GetByCodeAsync returns the correct country when a valid country code is provided
        /// </summary>
        [Fact]
        public async Task GetByCodeAsync_WithValidCode_ReturnsCountry()
        {
            // Arrange: Get a test country code from the mock data
            string testCountryCode = _countries.First().Code.Value;

            // Setup _mockCountryDbSet.FindAsync() to return the country with the matching code
            _mockCountryDbSet.Setup(m => m.FindAsync(testCountryCode)).ReturnsAsync(_countries.First(c => c.Code.Value == testCountryCode));

            // Act: Call _repository.GetByCodeAsync with the test country code
            var country = await _repository.GetByCodeAsync(testCountryCode);

            // Assert: Verify the returned country is not null
            Assert.NotNull(country);

            // Assert: Verify the returned country has the correct country code
            Assert.Equal(testCountryCode, country.Code.Value);
        }

        /// <summary>
        /// Tests that GetByCodeAsync returns null when an invalid country code is provided
        /// </summary>
        [Fact]
        public async Task GetByCodeAsync_WithInvalidCode_ReturnsNull()
        {
            // Arrange: Create an invalid country code
            string invalidCountryCode = "XX";

            // Setup _mockCountryDbSet.FindAsync() to return null
            _mockCountryDbSet.Setup(m => m.FindAsync(invalidCountryCode)).ReturnsAsync((Country)null);

            // Act: Call _repository.GetByCodeAsync with the invalid country code
            var country = await _repository.GetByCodeAsync(invalidCountryCode);

            // Assert: Verify the returned country is null
            Assert.Null(country);
        }

        /// <summary>
        /// Tests that GetActiveCountriesAsync returns only countries where IsActive is true
        /// </summary>
        [Fact]
        public async Task GetActiveCountriesAsync_ReturnsOnlyActiveCountries()
        {
            // Arrange: Set some countries in the mock data to inactive
            _countries[0].SetActive(false);
            _countries[2].SetActive(false);

            // Act: Call _repository.GetActiveCountriesAsync()
            var activeCountries = await _repository.GetActiveCountriesAsync();

            // Assert: Verify the returned collection only contains active countries
            Assert.All(activeCountries, c => Assert.True(c.IsActive));

            // Assert: Verify the count of returned countries matches the expected count
            Assert.Equal(_countries.Count(c => c.IsActive), activeCountries.Count());
        }

        /// <summary>
        /// Tests that GetCountriesByCodesAsync returns countries matching the provided codes
        /// </summary>
        [Fact]
        public async Task GetCountriesByCodesAsync_WithValidCodes_ReturnsMatchingCountries()
        {
            // Arrange: Create a list of valid country codes from the mock data
            var testCountryCodes = new List<string> { _countries[0].Code.Value, _countries[1].Code.Value };

            // Act: Call _repository.GetCountriesByCodesAsync with the list of codes
            var countries = await _repository.GetCountriesByCodesAsync(testCountryCodes);

            // Assert: Verify the returned collection contains countries with the requested codes
            Assert.All(countries, c => Assert.Contains(c.Code.Value, testCountryCodes));

            // Assert: Verify the count of returned countries matches the expected count
            Assert.Equal(testCountryCodes.Count, countries.Count());
        }

        /// <summary>
        /// Tests that GetCountriesByCodesAsync returns an empty collection when invalid codes are provided
        /// </summary>
        [Fact]
        public async Task GetCountriesByCodesAsync_WithInvalidCodes_ReturnsEmptyCollection()
        {
            // Arrange: Create a list of invalid country codes
            var invalidCountryCodes = new List<string> { "XX", "YY", "ZZ" };

            // Act: Call _repository.GetCountriesByCodesAsync with the invalid codes
            var countries = await _repository.GetCountriesByCodesAsync(invalidCountryCodes);

            // Assert: Verify the returned collection is empty
            Assert.Empty(countries);
        }

        /// <summary>
        /// Tests that GetCountriesByFilingFrequencyAsync returns countries that support the specified filing frequency
        /// </summary>
        [Fact]
        public async Task GetCountriesByFilingFrequencyAsync_ReturnsCountriesWithMatchingFrequency()
        {
            // Arrange: Select a filing frequency to test
            FilingFrequency testFilingFrequency = FilingFrequency.Quarterly;

            // Act: Call _repository.GetCountriesByFilingFrequencyAsync with the selected frequency
            var countries = await _repository.GetCountriesByFilingFrequencyAsync(testFilingFrequency);

            // Assert: Verify all returned countries support the specified filing frequency
            Assert.All(countries, c => Assert.Contains(testFilingFrequency, c.AvailableFilingFrequencies));

            // Assert: Verify the count of returned countries matches the expected count
            Assert.Equal(_countries.Count(c => c.SupportsFilingFrequency(testFilingFrequency)), countries.Count());
        }

        /// <summary>
        /// Tests that GetPagedCountriesAsync returns the correct page of countries
        /// </summary>
        [Fact]
        public async Task GetPagedCountriesAsync_ReturnsCorrectPageOfCountries()
        {
            // Arrange: Define page number and page size
            int pageNumber = 2;
            int pageSize = 2;

            // Act: Call _repository.GetPagedCountriesAsync with the page parameters
            var (countries, totalCount) = await _repository.GetPagedCountriesAsync(pageNumber, pageSize, false);

            // Assert: Verify the returned countries match the expected page
            Assert.Equal(_countries.Skip((pageNumber - 1) * pageSize).Take(pageSize), countries);

            // Assert: Verify the total count matches the expected total
            Assert.Equal(_countries.Count, totalCount);
        }

        /// <summary>
        /// Tests that GetPagedCountriesAsync with activeOnly=true returns only active countries
        /// </summary>
        [Fact]
        public async Task GetPagedCountriesAsync_WithActiveOnly_ReturnsOnlyActiveCountries()
        {
            // Arrange: Set some countries in the mock data to inactive
            _countries[0].SetActive(false);
            _countries[2].SetActive(false);

            // Define page number and page size
            int pageNumber = 1;
            int pageSize = 10;

            // Act: Call _repository.GetPagedCountriesAsync with activeOnly=true
            var (countries, totalCount) = await _repository.GetPagedCountriesAsync(pageNumber, pageSize, true);

            // Assert: Verify all returned countries have IsActive=true
            Assert.All(countries, c => Assert.True(c.IsActive));

            // Assert: Verify the total count matches the expected count of active countries
            Assert.Equal(_countries.Count(c => c.IsActive), totalCount);
        }

        /// <summary>
        /// Tests that ExistsByCodeAsync returns true when a country with the specified code exists
        /// </summary>
        [Fact]
        public async Task ExistsByCodeAsync_WithExistingCode_ReturnsTrue()
        {
            // Arrange: Get a test country code from the mock data
            string testCountryCode = _countries[0].Code.Value;

            // Act: Call _repository.ExistsByCodeAsync with the test country code
            var exists = await _repository.ExistsByCodeAsync(testCountryCode);

            // Assert: Verify the result is true
            Assert.True(exists);
        }

        /// <summary>
        /// Tests that ExistsByCodeAsync returns false when no country with the specified code exists
        /// </summary>
        [Fact]
        public async Task ExistsByCodeAsync_WithNonExistingCode_ReturnsFalse()
        {
            // Arrange: Create a non-existing country code
            string nonExistingCountryCode = "XX";

            // Act: Call _repository.ExistsByCodeAsync with the non-existing code
            var exists = await _repository.ExistsByCodeAsync(nonExistingCountryCode);

            // Assert: Verify the result is false
            Assert.False(exists);
        }

        /// <summary>
        /// Tests that CreateAsync adds a valid country to the database
        /// </summary>
        [Fact]
        public async Task CreateAsync_WithValidCountry_AddsCountryToDbSet()
        {
            // Arrange: Create a new valid country that doesn't exist in the mock data
            var newCountry = Country.Create("ZZ", "New Country", 10.0m, "USD");

            // Setup _mockCountryDbSet.Add() to capture the added country
            Country capturedCountry = null;
            _mockCountryDbSet.Setup(m => m.AddAsync(It.IsAny<Country>(), default))
                .Callback<Country, System.Threading.CancellationToken>((c, token) => capturedCountry = c)
                .Returns(Task.CompletedTask);

            // Setup _mockContext.SaveChangesAsync() to return 1
            _mockContext.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act: Call _repository.CreateAsync with the new country
            await _repository.CreateAsync(newCountry);

            // Assert: Verify _mockCountryDbSet.Add was called once with the new country
            _mockCountryDbSet.Verify(m => m.AddAsync(It.IsAny<Country>(), default), Times.Once);
            Assert.Equal(newCountry, capturedCountry);

            // Assert: Verify _mockContext.SaveChangesAsync was called once
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// Tests that CreateAsync throws an exception when trying to create a country with an existing code
        /// </summary>
        [Fact]
        public async Task CreateAsync_WithExistingCountryCode_ThrowsException()
        {
            // Arrange: Get an existing country from the mock data
            var existingCountry = _countries[0];

            // Act & Assert: Verify that calling _repository.CreateAsync with the existing country throws an exception
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.CreateAsync(existingCountry));
        }

        /// <summary>
        /// Tests that UpdateAsync updates an existing country in the database
        /// </summary>
        [Fact]
        public async Task UpdateAsync_WithValidCountry_UpdatesCountryInDbSet()
        {
            // Arrange: Get an existing country from the mock data and modify it
            var existingCountry = _countries[0];
            existingCountry.UpdateName("Updated Country Name");

            // Setup _mockContext.SaveChangesAsync() to return 1
            _mockContext.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act: Call _repository.UpdateAsync with the modified country
            await _repository.UpdateAsync(existingCountry);

            // Assert: Verify _mockContext.SaveChangesAsync was called once
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateAsync throws an exception when trying to update a non-existing country
        /// </summary>
        [Fact]
        public async Task UpdateAsync_WithNonExistingCountry_ThrowsException()
        {
            // Arrange: Create a country with a non-existing code
            var nonExistingCountry = Country.Create("XX", "Non Existing Country", 10.0m, "USD");

            // Act & Assert: Verify that calling _repository.UpdateAsync with the non-existing country throws an exception
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.UpdateAsync(nonExistingCountry));
        }

        /// <summary>
        /// Tests that DeleteByCodeAsync removes a country with the specified code from the database
        /// </summary>
        [Fact]
        public async Task DeleteByCodeAsync_WithExistingCode_RemovesCountryFromDbSet()
        {
            // Arrange: Get a test country from the mock data
            var testCountry = _countries[0];

            // Setup _mockCountryDbSet.Remove() to capture the removed country
            Country removedCountry = null;
            _mockCountryDbSet.Setup(m => m.Remove(It.IsAny<Country>()))
                .Callback<Country>(c => removedCountry = c);

            // Setup _mockContext.SaveChangesAsync() to return 1
            _mockContext.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act: Call _repository.DeleteByCodeAsync with the test country code
            var result = await _repository.DeleteByCodeAsync(testCountry.Code.Value);

            // Assert: Verify _mockCountryDbSet.Remove was called once with the test country
            _mockCountryDbSet.Verify(m => m.Remove(It.IsAny<Country>()), Times.Once);
            Assert.Equal(testCountry, removedCountry);

            // Assert: Verify _mockContext.SaveChangesAsync was called once
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);

            // Assert: Verify the result is true
            Assert.True(result);
        }

        /// <summary>
        /// Tests that DeleteByCodeAsync returns false when trying to delete a non-existing country
        /// </summary>
        [Fact]
        public async Task DeleteByCodeAsync_WithNonExistingCode_ReturnsFalse()
        {
            // Arrange: Create a non-existing country code
            string nonExistingCountryCode = "XX";

            // Act: Call _repository.DeleteByCodeAsync with the non-existing code
            var result = await _repository.DeleteByCodeAsync(nonExistingCountryCode);

            // Assert: Verify the result is false
            Assert.False(result);

            // Assert: Verify _mockCountryDbSet.Remove was not called
            _mockCountryDbSet.Verify(m => m.Remove(It.IsAny<Country>()), Times.Never);

            // Assert: Verify _mockContext.SaveChangesAsync was not called
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Never);
        }
    }
}