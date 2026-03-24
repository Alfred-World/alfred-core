using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alfred.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceAccountId",
                table: "account_clones",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "source_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "generate_uuid_v7()"),
                    AccountType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Other"),
                    Username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    TwoFaSecret = table.Column<string>(type: "text", nullable: true),
                    RecoveryEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RecoveryPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_source_accounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_clones_SourceAccountId",
                table: "account_clones",
                column: "SourceAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_source_accounts_AccountType",
                table: "source_accounts",
                column: "AccountType");

            migrationBuilder.CreateIndex(
                name: "IX_source_accounts_AccountType_Username",
                table: "source_accounts",
                columns: new[] { "AccountType", "Username" });

            migrationBuilder.AddForeignKey(
                name: "FK_account_clones_source_accounts_SourceAccountId",
                table: "account_clones",
                column: "SourceAccountId",
                principalTable: "source_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_account_clones_source_accounts_SourceAccountId",
                table: "account_clones");

            migrationBuilder.DropTable(
                name: "source_accounts");

            migrationBuilder.DropIndex(
                name: "IX_account_clones_SourceAccountId",
                table: "account_clones");

            migrationBuilder.DropColumn(
                name: "SourceAccountId",
                table: "account_clones");
        }
    }
}
