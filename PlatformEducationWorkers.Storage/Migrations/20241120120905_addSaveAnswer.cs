using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlatformEducationWorkers.Storage.Migrations
{
    /// <inheritdoc />
    public partial class addSaveAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "answerJson",
                table: "UserResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "answerJson",
                table: "UserResults");
        }
    }
}
