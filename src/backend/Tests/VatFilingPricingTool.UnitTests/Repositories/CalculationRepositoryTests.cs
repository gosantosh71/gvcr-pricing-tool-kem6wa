using FluentAssertions; // Version 6.7.0
using Microsoft.EntityFrameworkCore; // Version 6.0.0
using Microsoft.Extensions.Logging; // Version 6.0.0
using Moq; // Version 4.18.2
using System; // Version 6.0.0
using System.Collections.Generic; // Version 6.0.0
using System.Linq; // Version 6.0.0
using System.Threading.Tasks; // Version 6.0.0
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Data.Context;
using VatFilingPricingTool.Data.Repositories.Implementations;
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.UnitTests.Helpers;
using Xunit; // Version 2.4.2

namespace VatFilingPricingTool.UnitTests.Repositories
{
    /// <summary>
    /// Test class for the CalculationRepository implementation
    /// </summary>
    public class CalculationRepositoryTests
    {
        private readonly Mock<IVatFilingDbContext> _mockContext;
        private readonly Mock<DbSet<Calculation>> _mockCalculationDbSet;
        private readonly Mock<DbSet<CalculationCountry>> _mockCalculationCountryDbSet;
        private readonly List<Calculation> _calculations;
        private readonly Mock<ILogger<CalculationRepository>> _mockLogger;
        private readonly ICalculationRepository _repository;

        /// <summary>
        /// Initializes a new instance of the CalculationRepositoryTests class
        /// </summary>
        public CalculationRepositoryTests()
        {
            // Initialize mock objects
            _mockContext = new Mock<IVatFilingDbContext>();
            _mockCalculationDbSet = new Mock<DbSet<Calculation>>();
            _mockCalculationCountryDbSet = new Mock<DbSet<CalculationCountry>>();
            _calculations = MockData.GetMockCalculations().ToList();
            _mockLogger = new Mock<ILogger<CalculationRepository>>();

            // Setup mock DbSets with test data using Entity Framework Core testing patterns
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.Provider).Returns(_calculations.AsQueryable().Provider);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.Expression).Returns(_calculations.AsQueryable().Expression);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.ElementType).Returns(_calculations.AsQueryable().ElementType);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.GetEnumerator()).Returns(_calculations.AsQueryable().GetEnumerator());

            // Setup _mockContext.Calculations to return _mockCalculationDbSet.Object
            _mockContext.Setup(c => c.Calculations).Returns(_mockCalculationDbSet.Object);
            _mockContext.Setup(c => c.CalculationCountries).Returns(_mockCalculationCountryDbSet.Object);

            // Initialize the repository with the mock context and logger
            _repository = new CalculationRepository(_mockContext.Object, _mockLogger.Object);
        }

        /// <summary>
        /// Tests that GetByIdWithDetailsAsync returns the correct calculation with all details when it exists
        /// </summary>
        [Fact]
        public async Task GetByIdWithDetailsAsync_ShouldReturnCalculation_WhenCalculationExists()
        {
            // Arrange: Get a test calculation ID from the mock data
            var testId = _calculations.First().CalculationId;

            // Arrange: Setup the mock DbSet to return the calculation when FindAsync is called
            _mockCalculationDbSet.Setup(x => x.FindAsync(testId)).ReturnsAsync(_calculations.First(c => c.CalculationId == testId));

            // Act: Call _repository.GetByIdWithDetailsAsync with the test ID
            var result = await _repository.GetByIdWithDetailsAsync(testId);

            // Assert: Verify the result is not null
            result.Should().NotBeNull();

            // Assert: Verify the result has the correct ID
            result.CalculationId.Should().Be(testId);

            // Assert: Verify that Include methods were called for related entities
            _mockCalculationDbSet.Verify(x => x.FindAsync(testId), Times.Once);
        }

        /// <summary>
        /// Tests that GetByIdWithDetailsAsync returns null when the calculation does not exist
        /// </summary>
        [Fact]
        public async Task GetByIdWithDetailsAsync_ShouldReturnNull_WhenCalculationDoesNotExist()
        {
            // Arrange: Generate a non-existent calculation ID
            var nonExistentId = "non-existent-id";

            // Arrange: Setup the mock DbSet to return null when FindAsync is called
            _mockCalculationDbSet.Setup(x => x.FindAsync(nonExistentId)).ReturnsAsync((Calculation)null);

            // Act: Call _repository.GetByIdWithDetailsAsync with the non-existent ID
            var result = await _repository.GetByIdWithDetailsAsync(nonExistentId);

            // Assert: Verify the result is null
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests that GetByUserIdAsync returns all calculations for a specific user
        /// </summary>
        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnCalculations_WhenUserHasCalculations()
        {
            // Arrange: Get a test user ID from the mock data
            var testUserId = _calculations.First().UserId;

            // Arrange: Setup the mock DbSet to return filtered calculations for the user
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.Provider).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().Provider);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.Expression).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().Expression);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.ElementType).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().ElementType);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.GetEnumerator()).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().GetEnumerator());

            // Act: Call _repository.GetByUserIdAsync with the test user ID
            var result = await _repository.GetByUserIdAsync(testUserId);

            // Assert: Verify the result is not null
            result.Should().NotBeNull();

            // Assert: Verify the result contains the expected number of calculations
            result.Count().Should().Be(_calculations.Count(c => c.UserId == testUserId));

            // Assert: Verify all returned calculations belong to the specified user
            result.All(c => c.UserId == testUserId).Should().BeTrue();
        }

        /// <summary>
        /// Tests that GetByUserIdAsync returns an empty list when the user has no calculations
        /// </summary>
        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserHasNoCalculations()
        {
            // Arrange: Generate a user ID with no calculations
            var nonExistentUserId = "non-existent-user-id";

            // Arrange: Setup the mock DbSet to return an empty list for the user
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.Provider).Returns(_calculations.Where(c => c.UserId == nonExistentUserId).AsQueryable().Provider);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.Expression).Returns(_calculations.Where(c => c.UserId == nonExistentUserId).AsQueryable().Expression);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.ElementType).Returns(_calculations.Where(c => c.UserId == nonExistentUserId).AsQueryable().ElementType);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.GetEnumerator()).Returns(_calculations.Where(c => c.UserId == nonExistentUserId).AsQueryable().GetEnumerator());

            // Act: Call _repository.GetByUserIdAsync with the user ID
            var result = await _repository.GetByUserIdAsync(nonExistentUserId);

            // Assert: Verify the result is not null
            result.Should().NotBeNull();

            // Assert: Verify the result is an empty collection
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that GetPagedByUserIdAsync returns a paginated list of calculations for a specific user
        /// </summary>
        [Fact]
        public async Task GetPagedByUserIdAsync_ShouldReturnPagedList_WhenUserHasCalculations()
        {
            // Arrange: Get a test user ID from the mock data
            var testUserId = _calculations.First().UserId;

            // Arrange: Setup the mock DbSet to return filtered calculations for the user
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.Provider).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().Provider);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.Expression).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().Expression);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.ElementType).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().ElementType);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.GetEnumerator()).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().GetEnumerator());

            // Arrange: Setup pagination parameters (page number, page size)
            int pageNumber = 1;
            int pageSize = 2;

            // Act: Call _repository.GetPagedByUserIdAsync with the test user ID and pagination parameters
            var result = await _repository.GetPagedByUserIdAsync(testUserId, pageNumber, pageSize);

            // Assert: Verify the result is not null
            result.Should().NotBeNull();

            // Assert: Verify the result contains the expected number of calculations per page
            result.Items.Count.Should().BeLessThanOrEqualTo(pageSize);

            // Assert: Verify the pagination metadata (total count, pages, etc.) is correct
            result.TotalCount.Should().Be(_calculations.Count(c => c.UserId == testUserId));
            result.PageNumber.Should().Be(pageNumber);
            result.PageSize.Should().Be(pageSize);
        }

        /// <summary>
        /// Tests that GetByCountryCodeAsync returns all calculations that include a specific country
        /// </summary>
        [Fact]
        public async Task GetByCountryCodeAsync_ShouldReturnCalculations_WhenCountryExists()
        {
            // Arrange: Get a test country code from the mock data
            var testCountryCode = "GB";

            // Arrange: Setup the mock DbSets to return calculations with the specified country
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.Provider).Returns(_calculations.Where(c => c.CalculationCountries.Any(cc => cc.CountryCode == testCountryCode)).AsQueryable().Provider);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.Expression).Returns(_calculations.Where(c => c.CalculationCountries.Any(cc => cc.CountryCode == testCountryCode)).AsQueryable().Expression);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.ElementType).Returns(_calculations.Where(c => c.CalculationCountries.Any(cc => cc.CountryCode == testCountryCode)).AsQueryable().ElementType);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.GetEnumerator()).Returns(_calculations.Where(c => c.CalculationCountries.Any(cc => cc.CountryCode == testCountryCode)).AsQueryable().GetEnumerator());

            // Act: Call _repository.GetByCountryCodeAsync with the test country code
            var result = await _repository.GetByCountryCodeAsync(testCountryCode);

            // Assert: Verify the result is not null
            result.Should().NotBeNull();

            // Assert: Verify the result contains the expected number of calculations
            result.Count().Should().Be(_calculations.Count(c => c.CalculationCountries.Any(cc => cc.CountryCode == testCountryCode)));

            // Assert: Verify all returned calculations include the specified country
            result.All(c => c.CalculationCountries.Any(cc => cc.CountryCode == testCountryCode)).Should().BeTrue();
        }

        /// <summary>
        /// Tests that GetRecentCalculationsAsync returns the most recent calculations for a specific user, limited by count
        /// </summary>
        [Fact]
        public async Task GetRecentCalculationsAsync_ShouldReturnLimitedCalculations_WhenUserHasCalculations()
        {
            // Arrange: Get a test user ID from the mock data
            var testUserId = _calculations.First().UserId;

            // Arrange: Setup the mock DbSet to return filtered calculations for the user
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.Provider).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().Provider);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.Expression).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().Expression);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.ElementType).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().ElementType);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.GetEnumerator()).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().GetEnumerator());

            // Arrange: Set a limit count for recent calculations
            int limitCount = 2;

            // Act: Call _repository.GetRecentCalculationsAsync with the test user ID and limit count
            var result = await _repository.GetRecentCalculationsAsync(testUserId, limitCount);

            // Assert: Verify the result is not null
            result.Should().NotBeNull();

            // Assert: Verify the result contains the expected number of calculations (limited by count)
            result.Count().Should().BeLessThanOrEqualTo(limitCount);

            // Assert: Verify the calculations are ordered by date descending (most recent first)
            result.Should().BeInDescendingOrder(c => c.CalculationDate);
        }

        /// <summary>
        /// Tests that GetCalculationCountByUserIdAsync returns the correct count of calculations for a specific user
        /// </summary>
        [Fact]
        public async Task GetCalculationCountByUserIdAsync_ShouldReturnCount_WhenUserHasCalculations()
        {
            // Arrange: Get a test user ID from the mock data
            var testUserId = _calculations.First().UserId;

            // Arrange: Setup the mock DbSet to return filtered calculations for the user
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.Provider).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().Provider);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.Expression).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().Expression);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.ElementType).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().ElementType);
            _mockCalculationDbSet.As<IQueryable<Calculation>>().Setup(m => m.GetEnumerator()).Returns(_calculations.Where(c => c.UserId == testUserId).AsQueryable().GetEnumerator());

            // Act: Call _repository.GetCalculationCountByUserIdAsync with the test user ID
            var result = await _repository.GetCalculationCountByUserIdAsync(testUserId);

            // Assert: Verify the result equals the expected count of calculations for the user
            result.Should().Be(_calculations.Count(c => c.UserId == testUserId));
        }

        /// <summary>
        /// Tests that ArchiveCalculationAsync correctly archives a calculation and returns true
        /// </summary>
        [Fact]
        public async Task ArchiveCalculationAsync_ShouldReturnTrue_WhenCalculationExists()
        {
            // Arrange: Get a test calculation ID from the mock data
            var testId = _calculations.First().CalculationId;

            // Arrange: Setup the mock DbSet to return the calculation when FindAsync is called
            _mockCalculationDbSet.Setup(x => x.FindAsync(testId)).ReturnsAsync(_calculations.First(c => c.CalculationId == testId));

            // Arrange: Setup _mockContext.SaveChangesAsync to return 1 (success)
            _mockContext.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act: Call _repository.ArchiveCalculationAsync with the test ID
            var result = await _repository.ArchiveCalculationAsync(testId);

            // Assert: Verify the result is true
            result.Should().BeTrue();

            // Assert: Verify the calculation's IsArchived property was set to true
            _calculations.First(c => c.CalculationId == testId).IsArchived.Should().BeTrue();

            // Assert: Verify SaveChangesAsync was called once
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// Tests that ArchiveCalculationAsync returns false when the calculation does not exist
        /// </summary>
        [Fact]
        public async Task ArchiveCalculationAsync_ShouldReturnFalse_WhenCalculationDoesNotExist()
        {
            // Arrange: Generate a non-existent calculation ID
            var nonExistentId = "non-existent-id";

            // Arrange: Setup the mock DbSet to return null when FindAsync is called
            _mockCalculationDbSet.Setup(x => x.FindAsync(nonExistentId)).ReturnsAsync((Calculation)null);

            // Act: Call _repository.ArchiveCalculationAsync with the non-existent ID
            var result = await _repository.ArchiveCalculationAsync(nonExistentId);

            // Assert: Verify the result is false
            result.Should().BeFalse();

            // Assert: Verify SaveChangesAsync was not called
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
        }

        /// <summary>
        /// Tests that UnarchiveCalculationAsync correctly unarchives a calculation and returns true
        /// </summary>
        [Fact]
        public async Task UnarchiveCalculationAsync_ShouldReturnTrue_WhenCalculationExists()
        {
            // Arrange: Get a test calculation ID from the mock data
            var testId = _calculations.First().CalculationId;

            // Arrange: Setup the calculation to be archived initially
            _calculations.First(c => c.CalculationId == testId).Archive();

            // Arrange: Setup the mock DbSet to return the calculation when FindAsync is called
            _mockCalculationDbSet.Setup(x => x.FindAsync(testId)).ReturnsAsync(_calculations.First(c => c.CalculationId == testId));

            // Arrange: Setup _mockContext.SaveChangesAsync to return 1 (success)
            _mockContext.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act: Call _repository.UnarchiveCalculationAsync with the test ID
            var result = await _repository.UnarchiveCalculationAsync(testId);

            // Assert: Verify the result is true
            result.Should().BeTrue();

            // Assert: Verify the calculation's IsArchived property was set to false
            _calculations.First(c => c.CalculationId == testId).IsArchived.Should().BeFalse();

            // Assert: Verify SaveChangesAsync was called once
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// Tests that UnarchiveCalculationAsync returns false when the calculation does not exist
        /// </summary>
        [Fact]
        public async Task UnarchiveCalculationAsync_ShouldReturnFalse_WhenCalculationDoesNotExist()
        {
            // Arrange: Generate a non-existent calculation ID
            var nonExistentId = "non-existent-id";

            // Arrange: Setup the mock DbSet to return null when FindAsync is called
            _mockCalculationDbSet.Setup(x => x.FindAsync(nonExistentId)).ReturnsAsync((Calculation)null);

            // Act: Call _repository.UnarchiveCalculationAsync with the non-existent ID
            var result = await _repository.UnarchiveCalculationAsync(nonExistentId);

            // Assert: Verify the result is false
            result.Should().BeFalse();

            // Assert: Verify SaveChangesAsync was not called
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
        }
    }
}