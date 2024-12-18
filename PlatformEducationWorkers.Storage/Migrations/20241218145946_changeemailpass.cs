using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlatformEducationWorkers.Storage.Migrations
{
    /// <inheritdoc />
    public partial class changeemailpass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HashPasswordEmail",
                table: "Enterprises");

            migrationBuilder.RenameColumn(
                name: "SaltPassword",
                table: "Enterprises",
                newName: "PasswordEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordEmail",
                table: "Enterprises",
                newName: "SaltPassword");

            migrationBuilder.AddColumn<string>(
                name: "HashPasswordEmail",
                table: "Enterprises",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
