using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiStock.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceInvoiceDiscountPercent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "ServiceInvoices",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "ServiceInvoices");
        }
    }
}
