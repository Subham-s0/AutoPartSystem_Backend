using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiStock.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoUrlToPartRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "PartRequests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "PartRequests");
        }
    }
}
