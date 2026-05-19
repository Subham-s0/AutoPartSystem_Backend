using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiStock.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceRecordStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ServiceRecords",
                type: "text",
                nullable: false,
                defaultValue: "Open");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ServiceRecords");
        }
    }
}
