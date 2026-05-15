using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiStock.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueReviewPerCustomerService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_CustomerId",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CustomerId_ServiceRecordId",
                table: "Reviews",
                columns: new[] { "CustomerId", "ServiceRecordId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_CustomerId_ServiceRecordId",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CustomerId",
                table: "Reviews",
                column: "CustomerId");
        }
    }
}
