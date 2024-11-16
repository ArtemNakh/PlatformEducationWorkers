using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlatformEducationWorkers.Storage.Migrations
{
    /// <inheritdoc />
    public partial class addCourceRowsChangeName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShowQuestions",
                table: "Cources",
                newName: "ShowUserAnswers");

            migrationBuilder.RenameColumn(
                name: "ShowAnswer",
                table: "Cources",
                newName: "ShowCorrectAnswers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShowUserAnswers",
                table: "Cources",
                newName: "ShowQuestions");

            migrationBuilder.RenameColumn(
                name: "ShowCorrectAnswers",
                table: "Cources",
                newName: "ShowAnswer");
        }
    }
}
