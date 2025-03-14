#nullable enable
using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq; // System.Linq package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using Microsoft.EntityFrameworkCore; // Microsoft.EntityFrameworkCore package version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using Moq; // Moq package version 4.18.2
using Xunit; // Xunit package version 2.4.2
using FluentAssertions; // FluentAssertions package version 6.7.0
using VatFilingPricingTool.Data.Context; // Import for IVatFilingDbContext interface
using VatFilingPricingTool.Data.Repositories.Implementations; // Import for RuleRepository class
using VatFilingPricingTool.Data.Repositories.Interfaces; // Import for IRuleRepository interface
using VatFilingPricingTool.Domain.Entities; // Import for Rule entity class
using VatFilingPricingTool.Domain.Enums; // Import for RuleType enum
using VatFilingPricingTool.Domain.ValueObjects; // Import for CountryCode value object
using VatFilingPricingTool.UnitTests.Helpers; // Import for MockData class
using VatFilingPricingTool.UnitTests.Helpers; // Import for TestHelpers class

namespace VatFilingPricingTool.UnitTests.Repositories
{
    /// <summary>
    /// Test class for the RuleRepository implementation
    /// </summary>
    public class RuleRepositoryTests
    {
        private readonly Mock<IVatFilingDbContext> _mockContext;
        private readonly Mock<DbSet<Rule>> _mockRuleDbSet;
        private readonly Mock<DbSet<RuleParameter>> _mockRuleParameterDbSet;
        private readonly List<Rule> _mockRules;
        private readonly Mock<ILogger<RuleRepository>> _mockLogger;
        private readonly IRuleRepository _repository;

        /// <summary>
        /// Initializes a new instance of the RuleRepositoryTests class
        /// </summary>
        public RuleRepositoryTests()
        {
            // Initialize mock objects
            _mockContext = new Mock<IVatFilingDbContext>();
            _mockRuleDbSet = new Mock<DbSet<Rule>>();
            _mockRuleParameterDbSet = new Mock<DbSet<RuleParameter>>();
            _mockRules = MockData.GetMockRules();
            _mockLogger = new Mock<ILogger<RuleRepository>>();

            // Setup mock DbSet with _mockRules data
            SetupMockDbSet();

            // Setup _mockContext.Rules to return _mockRuleDbSet.Object
            _mockContext.Setup(c => c.Rules).Returns(_mockRuleDbSet.Object);

            // Setup _mockContext.RuleParameters to return _mockRuleParameterDbSet.Object
            _mockContext.Setup(c => c.RuleParameters).Returns(_mockRuleParameterDbSet.Object);

            // Initialize _repository with mock dependencies
            _repository = new RuleRepository(_mockContext.Object, _mockLogger.Object);
        }

        /// <summary>
        /// Sets up the mock DbSet with the provided data
        /// </summary>
        private void SetupMockDbSet()
        {
            // Create IQueryable from _mockRules
            var queryable = _mockRules.AsQueryable();

            // Setup _mockRuleDbSet.As<IQueryable<Rule>>().Provider to return queryable.Provider
            _mockRuleDbSet.As<IQueryable<Rule>>().Setup(m => m.Provider).Returns(queryable.Provider);

            // Setup _mockRuleDbSet.As<IQueryable<Rule>>().Expression to return queryable.Expression
            _mockRuleDbSet.As<IQueryable<Rule>>().Setup(m => m.Expression).Returns(queryable.Expression);

            // Setup _mockRuleDbSet.As<IQueryable<Rule>>().ElementType to return queryable.ElementType
            _mockRuleDbSet.As<IQueryable<Rule>>().Setup(m => m.ElementType).Returns(queryable.ElementType);

            // Setup _mockRuleDbSet.As<IQueryable<Rule>>().GetEnumerator() to return queryable.GetEnumerator()
            _mockRuleDbSet.As<IQueryable<Rule>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            // Setup _mockRuleDbSet.As<IAsyncEnumerable<Rule>>().GetAsyncEnumerator() to return TestHelpers.CreateAsyncEnumerable(_mockRules).GetAsyncEnumerator()
            _mockRuleDbSet.As<IAsyncEnumerable<Rule>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(TestHelpers.CreateAsyncEnumerable(_mockRules).GetAsyncEnumerator());

            // Setup _mockRuleDbSet.FindAsync() to return ValueTask.FromResult(_mockRules.FirstOrDefault())
            _mockRuleDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((object[] keyValues) => _mockRules.FirstOrDefault(r => r.RuleId == (string)keyValues[0]));
        }

        /// <summary>
        /// Tests that GetRulesByCountryAsync returns rules for a specific country
        /// </summary>
        [Fact]
        public async Task GetRulesByCountryAsync_ShouldReturnRulesForSpecificCountry()
        {
            // Arrange
            var countryCode = new CountryCode("GB");
            var expectedRules = _mockRules.Where(r => r.CountryCode.Value == countryCode.Value).ToList();

            // Act
            var result = await _repository.GetRulesByCountryAsync(countryCode);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedRules.Count);
            result.Should().BeEquivalentTo(expectedRules);
            _mockContext.Verify(c => c.Rules, Times.Once);
        }

        /// <summary>
        /// Tests that GetRulesByTypeAsync returns rules of a specific type
        /// </summary>
        [Fact]
        public async Task GetRulesByTypeAsync_ShouldReturnRulesOfSpecificType()
        {
            // Arrange
            var ruleType = RuleType.VatRate;
            var expectedRules = _mockRules.Where(r => r.Type == ruleType).ToList();

            // Act
            var result = await _repository.GetRulesByTypeAsync(ruleType);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedRules.Count);
            result.Should().BeEquivalentTo(expectedRules);
            _mockContext.Verify(c => c.Rules, Times.Once);
        }

        /// <summary>
        /// Tests that GetActiveRulesAsync returns only active rules
        /// </summary>
        [Fact]
        public async Task GetActiveRulesAsync_ShouldReturnOnlyActiveRules()
        {
            // Arrange
            var expectedRules = _mockRules.Where(r => r.IsActive).ToList();

            // Act
            var result = await _repository.GetActiveRulesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedRules.Count);
            result.Should().BeEquivalentTo(expectedRules);
            _mockContext.Verify(c => c.Rules, Times.Once);
        }

        /// <summary>
        /// Tests that GetEffectiveRulesAsync returns rules effective at a specific date
        /// </summary>
        [Fact]
        public async Task GetEffectiveRulesAsync_ShouldReturnRulesEffectiveAtSpecificDate()
        {
            // Arrange
            var effectiveDate = DateTime.UtcNow.AddDays(1);
            var expectedRules = _mockRules.Where(r => r.EffectiveFrom <= effectiveDate && (r.EffectiveTo == null || r.EffectiveTo > effectiveDate)).ToList();

            // Act
            var result = await _repository.GetEffectiveRulesAsync(effectiveDate);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedRules.Count);
            result.Should().BeEquivalentTo(expectedRules);
            _mockContext.Verify(c => c.Rules, Times.Once);
        }

        /// <summary>
        /// Tests that GetPagedRulesAsync returns paginated rules
        /// </summary>
        [Fact]
        public async Task GetPagedRulesAsync_ShouldReturnPagedRules()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 2;
            var countryCode = new CountryCode("GB");
            var ruleType = RuleType.VatRate;
            bool activeOnly = true;

            var filteredRules = _mockRules.Where(r => r.CountryCode.Value == countryCode.Value && r.Type == ruleType && r.IsActive).ToList();
            var expectedRules = filteredRules.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var expectedTotalCount = filteredRules.Count;

            // Act
            var result = await _repository.GetPagedRulesAsync(pageNumber, pageSize, countryCode, ruleType, activeOnly);

            // Assert
            result.Rules.Should().NotBeNull();
            result.Rules.Should().HaveCount(expectedRules.Count);
            result.Rules.Should().BeEquivalentTo(expectedRules);
            result.TotalCount.Should().Be(expectedTotalCount);
            _mockContext.Verify(c => c.Rules, Times.AtLeastOnce);
        }

        /// <summary>
        /// Tests that CreateAsync adds a rule to the DbSet and saves changes
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldAddRuleToDbSetAndSaveChanges()
        {
            // Arrange
            var newRule = Rule.Create("GB", RuleType.VatRate, "Test Rule", "basePrice * 0.2", DateTime.UtcNow, "Test description");
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _repository.CreateAsync(newRule);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(newRule);
            _mockRuleDbSet.Verify(d => d.Add(newRule), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateAsync updates a rule and saves changes
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldUpdateRuleAndSaveChanges()
        {
            // Arrange
            var existingRule = _mockRules.First();
            existingRule.UpdateName("Updated Rule Name");
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _repository.UpdateAsync(existingRule);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(existingRule);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// Tests that ExistsByIdAsync returns true for an existing rule
        /// </summary>
        [Fact]
        public async Task ExistsByIdAsync_ShouldReturnTrueForExistingRule()
        {
            // Arrange
            var existingRuleId = _mockRules.First().RuleId;

            // Act
            var result = await _repository.ExistsByIdAsync(existingRuleId);

            // Assert
            result.Should().BeTrue();
            _mockContext.Verify(c => c.Rules, Times.Once);
        }

        /// <summary>
        /// Tests that ExistsByIdAsync returns false for a non-existing rule
        /// </summary>
        [Fact]
        public async Task ExistsByIdAsync_ShouldReturnFalseForNonExistingRule()
        {
            // Arrange
            var nonExistingRuleId = "non-existing-id";

            // Act
            var result = await _repository.ExistsByIdAsync(nonExistingRuleId);

            // Assert
            result.Should().BeFalse();
            _mockContext.Verify(c => c.Rules, Times.Once);
        }
    }
}
#nullable disable