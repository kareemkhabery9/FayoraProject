using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fayora.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VerificationDocumentUrl",
                table: "HousingUnits",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerificationDocumentUrl",
                table: "HousingUnits");
        }
    }
}
