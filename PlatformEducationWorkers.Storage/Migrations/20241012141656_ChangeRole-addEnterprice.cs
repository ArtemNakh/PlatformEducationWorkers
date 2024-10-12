using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlatformEducationWorkers.Storage.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRoleaddEnterprice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnterpriseId",
                table: "Roles",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_EnterpriseId",
                table: "Roles",
                column: "EnterpriseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Enterprises_EnterpriseId",
                table: "Roles",
                column: "EnterpriseId",
                principalTable: "Enterprises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Enterprises_EnterpriseId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_EnterpriseId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "EnterpriseId",
                table: "Roles");
        }
    }
}
