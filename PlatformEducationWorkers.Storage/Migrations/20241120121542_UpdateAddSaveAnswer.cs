using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlatformEducationWorkers.Storage.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAddSaveAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowSelectedAnswers",
                table: "Cources",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowSelectedAnswers",
                table: "Cources");
        }
    }
}
