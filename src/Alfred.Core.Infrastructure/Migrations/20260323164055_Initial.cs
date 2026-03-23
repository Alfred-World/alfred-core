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
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION generate_uuid_v7()
RETURNS uuid
LANGUAGE plpgsql
AS $$
DECLARE
    v_millis BIGINT;
    v_bytes BYTEA;
BEGIN
    v_millis := (extract(epoch from clock_timestamp()) * 1000)::BIGINT;
    v_bytes := uuid_send(gen_random_uuid());
    v_bytes := set_byte(v_bytes, 0, ((v_millis >> 40) & 255)::int);
    v_bytes := set_byte(v_bytes, 1, ((v_millis >> 32) & 255)::int);
    v_bytes := set_byte(v_bytes, 2, ((v_millis >> 24) & 255)::int);
    v_bytes := set_byte(v_bytes, 3, ((v_millis >> 16) & 255)::int);
    v_bytes := set_byte(v_bytes, 4, ((v_millis >> 8) & 255)::int);
    v_bytes := set_byte(v_bytes, 5, (v_millis & 255)::int);
    v_bytes := set_byte(v_bytes, 6, ((get_byte(v_bytes, 6) & 15) | 112)::int);
    v_bytes := set_byte(v_bytes, 8, ((get_byte(v_bytes, 8) & 63) | 128)::int);
    RETURN encode(v_bytes, 'hex')::uuid;
END;
$$;
");

            migrationBuilder.CreateTable(
                name: "access_permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
                    Code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Resource = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Action = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "access_roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Icon = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsImmutable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SupportPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FormSchema = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Zalo"),
                    SourceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CustomerNote = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ProductType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Other"),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "referral_commission_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
                    CommissionPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referral_commission_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "replicated_users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_replicated_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "units",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaseUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConversionRate = table.Column<decimal>(type: "numeric(18,8)", nullable: false, defaultValue: 1m),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "access_role_permissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_role_permissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_access_role_permissions_access_permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "access_permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_access_role_permissions_access_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "access_roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
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
                name: "account_clones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalAccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    TwoFaSecret = table.Column<string>(type: "text", nullable: true),
                    ExtraInfo = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Init"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SoldAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_clones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_account_clones_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_variants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    WarrantyDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_variants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_variants_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "access_user_roles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_user_roles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_access_user_roles_access_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "access_roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_access_user_roles_replicated_users_UserId",
                        column: x => x.UserId,
                        principalTable: "replicated_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "referral_commission_setting_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
                    ReferralCommissionSettingId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousCommissionPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    NewCommissionPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referral_commission_setting_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_referral_commission_setting_histories_referral_commission_s~",
                        column: x => x.ReferralCommissionSettingId,
                        principalTable: "referral_commission_settings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_referral_commission_setting_histories_replicated_users_Chan~",
                        column: x => x.ChangedByUserId,
                        principalTable: "replicated_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "commodities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Cost = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 1m),
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
                name: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
                    OrderCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    SoldByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountCloneId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarrantySourceAccountCloneId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantNameSnapshot = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    UnitPriceSnapshot = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    WarrantyDaysSnapshot = table.Column<int>(type: "integer", nullable: false),
                    ReferrerMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferralCommissionPercentSnapshot = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    ReferralCommissionAmountSnapshot = table.Column<decimal>(type: "numeric(15,2)", nullable: false, defaultValue: 0m),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    WarrantyExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderNote = table.Column<string>(type: "text", nullable: true),
                    WarrantyIssueNote = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_orders_account_clones_AccountCloneId",
                        column: x => x.AccountCloneId,
                        principalTable: "account_clones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_account_clones_WarrantySourceAccountCloneId",
                        column: x => x.WarrantySourceAccountCloneId,
                        principalTable: "account_clones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_members_ReferrerMemberId",
                        column: x => x.ReferrerMemberId,
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_product_variants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "product_variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "investment_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
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
                name: "IX_access_permissions_Code",
                table: "access_permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_access_permissions_Resource",
                table: "access_permissions",
                column: "Resource");

            migrationBuilder.CreateIndex(
                name: "IX_access_role_permissions_PermissionId",
                table: "access_role_permissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_access_role_permissions_RoleId",
                table: "access_role_permissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_access_roles_NormalizedName",
                table: "access_roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_access_user_roles_RoleId",
                table: "access_user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_account_clones_ExternalAccountId",
                table: "account_clones",
                column: "ExternalAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_account_clones_ProductId_Status",
                table: "account_clones",
                columns: new[] { "ProductId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_account_clones_ProductId_Username",
                table: "account_clones",
                columns: new[] { "ProductId", "Username" },
                unique: true);

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
                unique: true,
                filter: "\"IsDeleted\" = false");

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
                name: "IX_members_DisplayName",
                table: "members",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_members_SourceId",
                table: "members",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_AccountCloneId",
                table: "orders",
                column: "AccountCloneId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_MemberId",
                table: "orders",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_OrderCode",
                table: "orders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_ProductId",
                table: "orders",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_ProductVariantId",
                table: "orders",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_ReferrerMemberId",
                table: "orders",
                column: "ReferrerMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_SoldByUserId",
                table: "orders",
                column: "SoldByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_WarrantyExpiry",
                table: "orders",
                column: "WarrantyExpiry");

            migrationBuilder.CreateIndex(
                name: "IX_orders_WarrantySourceAccountCloneId",
                table: "orders",
                column: "WarrantySourceAccountCloneId");

            migrationBuilder.CreateIndex(
                name: "IX_product_variants_ProductId",
                table: "product_variants",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_variants_ProductId_Name",
                table: "product_variants",
                columns: new[] { "ProductId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_ProductType",
                table: "products",
                column: "ProductType");

            migrationBuilder.CreateIndex(
                name: "IX_referral_commission_setting_histories_ChangedByUserId",
                table: "referral_commission_setting_histories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_referral_commission_setting_histories_ReferralCommissionSet~",
                table: "referral_commission_setting_histories",
                columns: new[] { "ReferralCommissionSettingId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_referral_commission_settings_CreatedAt",
                table: "referral_commission_settings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_replicated_users_Email",
                table: "replicated_users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_replicated_users_UserName",
                table: "replicated_users",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_units_BaseUnitId",
                table: "units",
                column: "BaseUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_units_Code",
                table: "units",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "access_role_permissions");

            migrationBuilder.DropTable(
                name: "access_user_roles");

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

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS generate_uuid_v7();");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "referral_commission_setting_histories");

            migrationBuilder.DropTable(
                name: "access_permissions");

            migrationBuilder.DropTable(
                name: "access_roles");

            migrationBuilder.DropTable(
                name: "assets");

            migrationBuilder.DropTable(
                name: "commodities");

            migrationBuilder.DropTable(
                name: "account_clones");

            migrationBuilder.DropTable(
                name: "members");

            migrationBuilder.DropTable(
                name: "product_variants");

            migrationBuilder.DropTable(
                name: "referral_commission_settings");

            migrationBuilder.DropTable(
                name: "replicated_users");

            migrationBuilder.DropTable(
                name: "brands");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "units");

            migrationBuilder.DropTable(
                name: "products");
        }
    }
}
