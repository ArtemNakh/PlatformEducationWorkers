using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlatformEducationWorkers.Storage.Migrations
{
    /// <inheritdoc />
    public partial class addProfileAvatar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileAvatar",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileAvatar",
                table: "Users");
        }
    }
}
