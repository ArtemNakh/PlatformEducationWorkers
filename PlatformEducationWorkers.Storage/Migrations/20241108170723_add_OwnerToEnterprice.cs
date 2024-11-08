using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlatformEducationWorkers.Storage.Migrations
{
    /// <inheritdoc />
    public partial class add_OwnerToEnterprice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Enterprises",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enterprises_OwnerId",
                table: "Enterprises",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enterprises_Users_OwnerId",
                table: "Enterprises",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enterprises_Users_OwnerId",
                table: "Enterprises");

            migrationBuilder.DropIndex(
                name: "IX_Enterprises_OwnerId",
                table: "Enterprises");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Enterprises");
        }
    }
}
