using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alfred.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCreatedAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Index on orders.CreatedAt to support the monthly date-range query in SellAccountAsync
            // without a full table scan. This query runs on every order creation.
            migrationBuilder.CreateIndex(
                name: "IX_orders_CreatedAt",
                table: "orders",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_orders_CreatedAt",
                table: "orders");
        }
    }
}
