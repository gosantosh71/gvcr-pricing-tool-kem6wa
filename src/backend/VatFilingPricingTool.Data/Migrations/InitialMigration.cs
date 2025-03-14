using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using VatFilingPricingTool.Data.Migrations;

namespace VatFilingPricingTool.Data.Migrations
{
    /// <summary>
    /// Initial migration to create the database schema for VAT Filing Pricing Tool
    /// </summary>
    public partial class InitialMigration : Migration
    {
        /// <summary>
        /// Applies the migration to create the database schema and seed initial data
        /// </summary>
        /// <param name="migrationBuilder">The migration builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Users table
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    Email = table.Column<string>(maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(maxLength: 100, nullable: false),
                    LastName = table.Column<string>(maxLength: 100, nullable: false),
                    Roles = table.Column<string>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    LastLoginDate = table.Column<DateTime>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    AzureAdObjectId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            // Create Countries table
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Code = table.Column<string>(maxLength: 2, nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    StandardVatRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CurrencyCode = table.Column<string>(maxLength: 3, nullable: false),
                    AvailableFilingFrequencies = table.Column<string>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Code);
                });

            // Create Services table
            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServiceId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyCode = table.Column<string>(maxLength: 3, nullable: false),
                    ServiceType = table.Column<int>(nullable: false),
                    ComplexityLevel = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.ServiceId);
                });

            // Create AdditionalServices table
            migrationBuilder.CreateTable(
                name: "AdditionalServices",
                columns: table => new
                {
                    ServiceId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyCode = table.Column<string>(maxLength: 3, nullable: false),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalServices", x => x.ServiceId);
                });

            // Create Rules table
            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    RuleId = table.Column<string>(nullable: false),
                    CountryCode = table.Column<string>(maxLength: 2, nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    Expression = table.Column<string>(maxLength: 2000, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(nullable: false),
                    EffectiveTo = table.Column<DateTime>(nullable: true),
                    Priority = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.RuleId);
                    table.ForeignKey(
                        name: "FK_Rules_Countries_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Countries",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create RuleParameters table
            migrationBuilder.CreateTable(
                name: "RuleParameters",
                columns: table => new
                {
                    ParameterId = table.Column<string>(nullable: false),
                    RuleId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    DataType = table.Column<string>(maxLength: 50, nullable: false),
                    DefaultValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleParameters", x => x.ParameterId);
                    table.ForeignKey(
                        name: "FK_RuleParameters_Rules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "Rules",
                        principalColumn: "RuleId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create Calculations table
            migrationBuilder.CreateTable(
                name: "Calculations",
                columns: table => new
                {
                    CalculationId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    ServiceId = table.Column<string>(nullable: false),
                    TransactionVolume = table.Column<int>(nullable: false),
                    FilingFrequency = table.Column<int>(nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CalculationDate = table.Column<DateTime>(nullable: false),
                    CurrencyCode = table.Column<string>(maxLength: 3, nullable: false),
                    IsArchived = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calculations", x => x.CalculationId);
                    table.ForeignKey(
                        name: "FK_Calculations_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Calculations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create CalculationCountries table (junction table for many-to-many relationship)
            migrationBuilder.CreateTable(
                name: "CalculationCountries",
                columns: table => new
                {
                    CalculationId = table.Column<string>(nullable: false),
                    CountryCode = table.Column<string>(maxLength: 2, nullable: false),
                    CountryCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AppliedRules = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculationCountries", x => new { x.CalculationId, x.CountryCode });
                    table.ForeignKey(
                        name: "FK_CalculationCountries_Calculations_CalculationId",
                        column: x => x.CalculationId,
                        principalTable: "Calculations",
                        principalColumn: "CalculationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalculationCountries_Countries_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Countries",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create CalculationAdditionalServices table (junction table for many-to-many relationship)
            migrationBuilder.CreateTable(
                name: "CalculationAdditionalServices",
                columns: table => new
                {
                    CalculationId = table.Column<string>(nullable: false),
                    AdditionalServiceId = table.Column<string>(nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyCode = table.Column<string>(maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculationAdditionalServices", x => new { x.CalculationId, x.AdditionalServiceId });
                    table.ForeignKey(
                        name: "FK_CalculationAdditionalServices_AdditionalServices_AdditionalServiceId",
                        column: x => x.AdditionalServiceId,
                        principalTable: "AdditionalServices",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalculationAdditionalServices_Calculations_CalculationId",
                        column: x => x.CalculationId,
                        principalTable: "Calculations",
                        principalColumn: "CalculationId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create Reports table
            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    CalculationId = table.Column<string>(nullable: false),
                    ReportType = table.Column<string>(maxLength: 50, nullable: false),
                    Format = table.Column<string>(maxLength: 10, nullable: false),
                    StorageUrl = table.Column<string>(maxLength: 2000, nullable: false),
                    GenerationDate = table.Column<DateTime>(nullable: false),
                    FileSize = table.Column<long>(nullable: false),
                    IsArchived = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_Reports_Calculations_CalculationId",
                        column: x => x.CalculationId,
                        principalTable: "Calculations",
                        principalColumn: "CalculationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create Integrations table
            migrationBuilder.CreateTable(
                name: "Integrations",
                columns: table => new
                {
                    IntegrationId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    SystemType = table.Column<string>(maxLength: 50, nullable: false),
                    ConnectionString = table.Column<string>(maxLength: 1000, nullable: false),
                    LastSyncDate = table.Column<DateTime>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    RetryCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Integrations", x => x.IntegrationId);
                    table.ForeignKey(
                        name: "FK_Integrations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create indexes for frequently queried columns

            // Users
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            // Rules
            migrationBuilder.CreateIndex(
                name: "IX_Rules_CountryCode",
                table: "Rules",
                column: "CountryCode");

            // RuleParameters
            migrationBuilder.CreateIndex(
                name: "IX_RuleParameters_RuleId",
                table: "RuleParameters",
                column: "RuleId");

            // Calculations
            migrationBuilder.CreateIndex(
                name: "IX_Calculations_ServiceId",
                table: "Calculations",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Calculations_UserId",
                table: "Calculations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Calculations_CalculationDate",
                table: "Calculations",
                column: "CalculationDate");

            // CalculationCountries
            migrationBuilder.CreateIndex(
                name: "IX_CalculationCountries_CountryCode",
                table: "CalculationCountries",
                column: "CountryCode");

            // CalculationAdditionalServices
            migrationBuilder.CreateIndex(
                name: "IX_CalculationAdditionalServices_AdditionalServiceId",
                table: "CalculationAdditionalServices",
                column: "AdditionalServiceId");

            // Reports
            migrationBuilder.CreateIndex(
                name: "IX_Reports_CalculationId",
                table: "Reports",
                column: "CalculationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId",
                table: "Reports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_GenerationDate",
                table: "Reports",
                column: "GenerationDate");

            // Integrations
            migrationBuilder.CreateIndex(
                name: "IX_Integrations_UserId",
                table: "Integrations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_SystemType",
                table: "Integrations",
                column: "SystemType");

            // Seed initial data
            SeedData.SeedDatabase(migrationBuilder);
        }

        /// <summary>
        /// Reverts the migration by dropping all tables in the reverse order of creation
        /// </summary>
        /// <param name="migrationBuilder">The migration builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Integrations");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "CalculationAdditionalServices");

            migrationBuilder.DropTable(
                name: "CalculationCountries");

            migrationBuilder.DropTable(
                name: "RuleParameters");

            migrationBuilder.DropTable(
                name: "AdditionalServices");

            migrationBuilder.DropTable(
                name: "Calculations");

            migrationBuilder.DropTable(
                name: "Rules");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Countries");
        }

        /// <summary>
        /// Builds the database model with entity configurations, relationships, and constraints
        /// </summary>
        /// <param name="modelBuilder">The model builder</param>
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.AdditionalService", b =>
            {
                b.Property<string>("ServiceId")
                    .HasColumnType("nvarchar(450)");

                b.Property<decimal>("Cost")
                    .HasColumnType("decimal(18,2)");

                b.Property<string>("CurrencyCode")
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnType("nvarchar(3)");

                b.Property<string>("Description")
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.Property<bool>("IsActive")
                    .HasColumnType("bit");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.HasKey("ServiceId");

                b.ToTable("AdditionalServices");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.Calculation", b =>
            {
                b.Property<string>("CalculationId")
                    .HasColumnType("nvarchar(450)");

                b.Property<DateTime>("CalculationDate")
                    .HasColumnType("datetime2");

                b.Property<string>("CurrencyCode")
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnType("nvarchar(3)");

                b.Property<int>("FilingFrequency")
                    .HasColumnType("int");

                b.Property<bool>("IsArchived")
                    .HasColumnType("bit");

                b.Property<string>("ServiceId")
                    .IsRequired()
                    .HasColumnType("nvarchar(450)");

                b.Property<decimal>("TotalCost")
                    .HasColumnType("decimal(18,2)");

                b.Property<int>("TransactionVolume")
                    .HasColumnType("int");

                b.Property<string>("UserId")
                    .IsRequired()
                    .HasColumnType("nvarchar(450)");

                b.HasKey("CalculationId");

                b.HasIndex("CalculationDate");

                b.HasIndex("ServiceId");

                b.HasIndex("UserId");

                b.ToTable("Calculations");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.CalculationAdditionalService", b =>
            {
                b.Property<string>("CalculationId")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("AdditionalServiceId")
                    .HasColumnType("nvarchar(450)");

                b.Property<decimal>("Cost")
                    .HasColumnType("decimal(18,2)");

                b.Property<string>("CurrencyCode")
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnType("nvarchar(3)");

                b.HasKey("CalculationId", "AdditionalServiceId");

                b.HasIndex("AdditionalServiceId");

                b.ToTable("CalculationAdditionalServices");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.CalculationCountry", b =>
            {
                b.Property<string>("CalculationId")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("CountryCode")
                    .HasMaxLength(2)
                    .HasColumnType("nvarchar(2)");

                b.Property<string>("AppliedRules")
                    .HasColumnType("nvarchar(max)");

                b.Property<decimal>("CountryCost")
                    .HasColumnType("decimal(18,2)");

                b.HasKey("CalculationId", "CountryCode");

                b.HasIndex("CountryCode");

                b.ToTable("CalculationCountries");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.Country", b =>
            {
                b.Property<string>("Code")
                    .HasMaxLength(2)
                    .HasColumnType("nvarchar(2)");

                b.Property<string>("AvailableFilingFrequencies")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("CurrencyCode")
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnType("nvarchar(3)");

                b.Property<bool>("IsActive")
                    .HasColumnType("bit");

                b.Property<DateTime>("LastUpdated")
                    .HasColumnType("datetime2");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<decimal>("StandardVatRate")
                    .HasColumnType("decimal(5,2)");

                b.HasKey("Code");

                b.ToTable("Countries");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.Integration", b =>
            {
                b.Property<string>("IntegrationId")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("ConnectionString")
                    .IsRequired()
                    .HasMaxLength(1000)
                    .HasColumnType("nvarchar(1000)");

                b.Property<bool>("IsActive")
                    .HasColumnType("bit");

                b.Property<DateTime?>("LastSyncDate")
                    .HasColumnType("datetime2");

                b.Property<int>("RetryCount")
                    .HasColumnType("int");

                b.Property<string>("SystemType")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("UserId")
                    .IsRequired()
                    .HasColumnType("nvarchar(450)");

                b.HasKey("IntegrationId");

                b.HasIndex("SystemType");

                b.HasIndex("UserId");

                b.ToTable("Integrations");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.Report", b =>
            {
                b.Property<string>("ReportId")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("CalculationId")
                    .IsRequired()
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("Format")
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnType("nvarchar(10)");

                b.Property<long>("FileSize")
                    .HasColumnType("bigint");

                b.Property<DateTime>("GenerationDate")
                    .HasColumnType("datetime2");

                b.Property<bool>("IsArchived")
                    .HasColumnType("bit");

                b.Property<string>("ReportType")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("StorageUrl")
                    .IsRequired()
                    .HasMaxLength(2000)
                    .HasColumnType("nvarchar(2000)");

                b.Property<string>("UserId")
                    .IsRequired()
                    .HasColumnType("nvarchar(450)");

                b.HasKey("ReportId");

                b.HasIndex("CalculationId");

                b.HasIndex("GenerationDate");

                b.HasIndex("UserId");

                b.ToTable("Reports");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.Rule", b =>
            {
                b.Property<string>("RuleId")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("CountryCode")
                    .IsRequired()
                    .HasMaxLength(2)
                    .HasColumnType("nvarchar(2)");

                b.Property<string>("Description")
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.Property<DateTime>("EffectiveFrom")
                    .HasColumnType("datetime2");

                b.Property<DateTime?>("EffectiveTo")
                    .HasColumnType("datetime2");

                b.Property<string>("Expression")
                    .IsRequired()
                    .HasMaxLength(2000)
                    .HasColumnType("nvarchar(2000)");

                b.Property<bool>("IsActive")
                    .HasColumnType("bit");

                b.Property<DateTime>("LastUpdated")
                    .HasColumnType("datetime2");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<int>("Priority")
                    .HasColumnType("int");

                b.Property<int>("Type")
                    .HasColumnType("int");

                b.HasKey("RuleId");

                b.HasIndex("CountryCode");

                b.ToTable("Rules");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.RuleParameter", b =>
            {
                b.Property<string>("ParameterId")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("DataType")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("DefaultValue")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("RuleId")
                    .IsRequired()
                    .HasColumnType("nvarchar(450)");

                b.HasKey("ParameterId");

                b.HasIndex("RuleId");

                b.ToTable("RuleParameters");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.Service", b =>
            {
                b.Property<string>("ServiceId")
                    .HasColumnType("nvarchar(450)");

                b.Property<decimal>("BasePrice")
                    .HasColumnType("decimal(18,2)");

                b.Property<int>("ComplexityLevel")
                    .HasColumnType("int");

                b.Property<string>("CurrencyCode")
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnType("nvarchar(3)");

                b.Property<string>("Description")
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.Property<bool>("IsActive")
                    .HasColumnType("bit");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<int>("ServiceType")
                    .HasColumnType("int");

                b.HasKey("ServiceId");

                b.ToTable("Services");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.User", b =>
            {
                b.Property<string>("UserId")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("AzureAdObjectId")
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTime>("CreatedDate")
                    .HasColumnType("datetime2");

                b.Property<string>("Email")
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<string>("FirstName")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<bool>("IsActive")
                    .HasColumnType("bit");

                b.Property<DateTime?>("LastLoginDate")
                    .HasColumnType("datetime2");

                b.Property<string>("LastName")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Roles")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.HasKey("UserId");

                b.HasIndex("Email")
                    .IsUnique();

                b.ToTable("Users");
            });

            // Configure relationships

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.Calculation", b =>
            {
                b.HasOne("VatFilingPricingTool.Domain.Entities.Service", "Service")
                    .WithMany("Calculations")
                    .HasForeignKey("ServiceId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("VatFilingPricingTool.Domain.Entities.User", "User")
                    .WithMany("Calculations")
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Service");

                b.Navigation("User");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.CalculationAdditionalService", b =>
            {
                b.HasOne("VatFilingPricingTool.Domain.Entities.AdditionalService", "AdditionalService")
                    .WithMany("CalculationAdditionalServices")
                    .HasForeignKey("AdditionalServiceId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("VatFilingPricingTool.Domain.Entities.Calculation", "Calculation")
                    .WithMany("CalculationAdditionalServices")
                    .HasForeignKey("CalculationId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("AdditionalService");

                b.Navigation("Calculation");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.CalculationCountry", b =>
            {
                b.HasOne("VatFilingPricingTool.Domain.Entities.Calculation", "Calculation")
                    .WithMany("CalculationCountries")
                    .HasForeignKey("CalculationId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("VatFilingPricingTool.Domain.Entities.Country", "Country")
                    .WithMany("CalculationCountries")
                    .HasForeignKey("CountryCode")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Calculation");

                b.Navigation("Country");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.Integration", b =>
            {
                b.HasOne("VatFilingPricingTool.Domain.Entities.User", "User")
                    .WithMany("Integrations")
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("User");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.Report", b =>
            {
                b.HasOne("VatFilingPricingTool.Domain.Entities.Calculation", "Calculation")
                    .WithMany("Reports")
                    .HasForeignKey("CalculationId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("VatFilingPricingTool.Domain.Entities.User", "User")
                    .WithMany("Reports")
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Calculation");

                b.Navigation("User");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.Rule", b =>
            {
                b.HasOne("VatFilingPricingTool.Domain.Entities.Country", "Country")
                    .WithMany("Rules")
                    .HasForeignKey("CountryCode")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Country");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.RuleParameter", b =>
            {
                b.HasOne("VatFilingPricingTool.Domain.Entities.Rule", null)
                    .WithMany("Parameters")
                    .HasForeignKey("RuleId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            // Define navigation properties

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.AdditionalService", b =>
            {
                b.Navigation("CalculationAdditionalServices");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.Calculation", b =>
            {
                b.Navigation("CalculationAdditionalServices");

                b.Navigation("CalculationCountries");

                b.Navigation("Reports");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.Country", b =>
            {
                b.Navigation("CalculationCountries");

                b.Navigation("Rules");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.Rule", b =>
            {
                b.Navigation("Parameters");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.Service", b =>
            {
                b.Navigation("Calculations");
            });

            modelBuilder.Entity("VatFilingPricingTool.Domain.Entities.User", b =>
            {
                b.Navigation("Calculations");

                b.Navigation("Integrations");

                b.Navigation("Reports");
            });
        }
    }
}