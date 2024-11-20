using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlatformEducationWorkers.Storage.Migrations
{
    /// <inheritdoc />
    public partial class addShowUserAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedAnswersJson",
                table: "UserResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ShowUserAnswers",
                table: "Cources",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedAnswersJson",
                table: "UserResults");

            migrationBuilder.DropColumn(
                name: "ShowUserAnswers",
                table: "Cources");
        }
    }
}
