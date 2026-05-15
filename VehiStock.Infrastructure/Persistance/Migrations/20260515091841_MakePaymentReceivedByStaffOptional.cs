using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiStock.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class MakePaymentReceivedByStaffOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_StaffsProfile_ReceivedByStaffId",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "ReceivedByStaffId",
                table: "Payments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_StaffsProfile_ReceivedByStaffId",
                table: "Payments",
                column: "ReceivedByStaffId",
                principalTable: "StaffsProfile",
                principalColumn: "StaffMemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_StaffsProfile_ReceivedByStaffId",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "ReceivedByStaffId",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_StaffsProfile_ReceivedByStaffId",
                table: "Payments",
                column: "ReceivedByStaffId",
                principalTable: "StaffsProfile",
                principalColumn: "StaffMemberId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
