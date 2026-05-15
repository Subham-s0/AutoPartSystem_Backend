using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiStock.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class RemovePaymentReceivedByStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_StaffsProfile_ReceivedByStaffId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ReceivedByStaffId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReceivedByStaffId",
                table: "Payments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceivedByStaffId",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ReceivedByStaffId",
                table: "Payments",
                column: "ReceivedByStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_StaffsProfile_ReceivedByStaffId",
                table: "Payments",
                column: "ReceivedByStaffId",
                principalTable: "StaffsProfile",
                principalColumn: "StaffMemberId");
        }
    }
}
