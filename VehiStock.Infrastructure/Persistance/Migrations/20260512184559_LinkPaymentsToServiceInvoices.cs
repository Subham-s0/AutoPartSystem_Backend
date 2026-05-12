using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiStock.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class LinkPaymentsToServiceInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_SalesInvoices_SalesInvoiceId",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "SalesInvoiceId",
                table: "Payments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ServiceInvoiceId",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ServiceInvoiceId",
                table: "Payments",
                column: "ServiceInvoiceId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_ExactlyOneInvoice",
                table: "Payments",
                sql: "(\"SalesInvoiceId\" IS NOT NULL AND \"ServiceInvoiceId\" IS NULL) OR (\"SalesInvoiceId\" IS NULL AND \"ServiceInvoiceId\" IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_SalesInvoices_SalesInvoiceId",
                table: "Payments",
                column: "SalesInvoiceId",
                principalTable: "SalesInvoices",
                principalColumn: "SalesInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_ServiceInvoices_ServiceInvoiceId",
                table: "Payments",
                column: "ServiceInvoiceId",
                principalTable: "ServiceInvoices",
                principalColumn: "ServiceInvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_SalesInvoices_SalesInvoiceId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_ServiceInvoices_ServiceInvoiceId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ServiceInvoiceId",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_ExactlyOneInvoice",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ServiceInvoiceId",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "SalesInvoiceId",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_SalesInvoices_SalesInvoiceId",
                table: "Payments",
                column: "SalesInvoiceId",
                principalTable: "SalesInvoices",
                principalColumn: "SalesInvoiceId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
