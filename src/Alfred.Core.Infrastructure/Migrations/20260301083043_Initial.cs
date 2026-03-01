using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alfred.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ObjectKey = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Attachment"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "brands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SupportPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FormSchema = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_categories_categories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "units",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaseUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConversionRate = table.Column<decimal>(type: "numeric(18,8)", nullable: false, defaultValue: 1m),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_units_units_BaseUnitId",
                        column: x => x.BaseUnitId,
                        principalTable: "units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "date", nullable: true),
                    InitialCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    WarrantyExpiryDate = table.Column<DateTime>(type: "date", nullable: true),
                    Specs = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assets_brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_assets_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "brand_categories",
                columns: table => new
                {
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brand_categories", x => new { x.BrandId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_brand_categories_brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_brand_categories_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "commodities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AssetClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DefaultUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commodities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_commodities_units_DefaultUnitId",
                        column: x => x.DefaultUnitId,
                        principalTable: "units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "asset_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Cost = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    Note = table.Column<string>(type: "text", nullable: true),
                    FinanceTxnId = table.Column<Guid>(type: "uuid", nullable: true),
                    NextDueDate = table.Column<DateTime>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_asset_logs_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_asset_logs_brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "investment_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommodityId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TransactionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Quantity = table.Column<decimal>(type: "numeric(15,4)", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FeeAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    FinanceTxnId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_investment_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_investment_transactions_commodities_CommodityId",
                        column: x => x.CommodityId,
                        principalTable: "commodities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_investment_transactions_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "market_prices",
                columns: table => new
                {
                    Time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CommodityId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SellPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_market_prices", x => new { x.Time, x.CommodityId });
                    table.ForeignKey(
                        name: "FK_market_prices_commodities_CommodityId",
                        column: x => x.CommodityId,
                        principalTable: "commodities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_asset_logs_AssetId",
                table: "asset_logs",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_asset_logs_BrandId",
                table: "asset_logs",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_assets_BrandId",
                table: "assets",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_assets_CategoryId",
                table: "assets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_assets_Specs",
                table: "assets",
                column: "Specs")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_attachments_TargetId_TargetType",
                table: "attachments",
                columns: new[] { "TargetId", "TargetType" });

            migrationBuilder.CreateIndex(
                name: "IX_brand_categories_CategoryId",
                table: "brand_categories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_categories_Code",
                table: "categories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_ParentId",
                table: "categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_commodities_Code",
                table: "commodities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_commodities_DefaultUnitId",
                table: "commodities",
                column: "DefaultUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_investment_transactions_CommodityId",
                table: "investment_transactions",
                column: "CommodityId");

            migrationBuilder.CreateIndex(
                name: "IX_investment_transactions_UnitId",
                table: "investment_transactions",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_market_prices_CommodityId",
                table: "market_prices",
                column: "CommodityId");

            migrationBuilder.CreateIndex(
                name: "IX_units_BaseUnitId",
                table: "units",
                column: "BaseUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_units_Code",
                table: "units",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asset_logs");

            migrationBuilder.DropTable(
                name: "attachments");

            migrationBuilder.DropTable(
                name: "brand_categories");

            migrationBuilder.DropTable(
                name: "investment_transactions");

            migrationBuilder.DropTable(
                name: "market_prices");

            migrationBuilder.DropTable(
                name: "assets");

            migrationBuilder.DropTable(
                name: "commodities");

            migrationBuilder.DropTable(
                name: "brands");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "units");
        }
    }
}
