using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using VatFilingPricingTool.Domain.Entities;

namespace VatFilingPricingTool.Data.Context
{
    /// <summary>
    /// Interface for the Entity Framework Core DbContext used in the VAT Filing Pricing Tool
    /// </summary>
    public interface IVatFilingDbContext : IDisposable
    {
        /// <summary>
        /// Gets the DbSet for User entities
        /// </summary>
        DbSet<User> Users { get; }

        /// <summary>
        /// Gets the DbSet for Country entities
        /// </summary>
        DbSet<Country> Countries { get; }

        /// <summary>
        /// Gets the DbSet for Service entities
        /// </summary>
        DbSet<Service> Services { get; }

        /// <summary>
        /// Gets the DbSet for AdditionalService entities
        /// </summary>
        DbSet<AdditionalService> AdditionalServices { get; }

        /// <summary>
        /// Gets the DbSet for Calculation entities
        /// </summary>
        DbSet<Calculation> Calculations { get; }

        /// <summary>
        /// Gets the DbSet for CalculationCountry entities
        /// </summary>
        DbSet<CalculationCountry> CalculationCountries { get; }

        /// <summary>
        /// Gets the DbSet for Rule entities
        /// </summary>
        DbSet<Rule> Rules { get; }

        /// <summary>
        /// Gets the DbSet for RuleParameter entities
        /// </summary>
        DbSet<RuleParameter> RuleParameters { get; }

        /// <summary>
        /// Gets the DbSet for Report entities
        /// </summary>
        DbSet<Report> Reports { get; }

        /// <summary>
        /// Gets the DbSet for Integration entities
        /// </summary>
        DbSet<Integration> Integrations { get; }

        /// <summary>
        /// Saves all changes made in this context to the database
        /// </summary>
        /// <returns>The number of state entries written to the database</returns>
        int SaveChanges();

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database
        /// </summary>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a DbSet<TEntity> that can be used to query and save instances of TEntity
        /// </summary>
        /// <typeparam name="TEntity">The type of entity for which a set should be returned</typeparam>
        /// <returns>A set for the given entity type</returns>
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}