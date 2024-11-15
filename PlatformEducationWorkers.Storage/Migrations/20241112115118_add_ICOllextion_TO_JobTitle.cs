using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlatformEducationWorkers.Storage.Migrations
{
    /// <inheritdoc />
    public partial class add_ICOllextion_TO_JobTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobTitles_Cources_CourcesId",
                table: "JobTitles");

            migrationBuilder.DropIndex(
                name: "IX_JobTitles_CourcesId",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "CourcesId",
                table: "JobTitles");

            migrationBuilder.CreateTable(
                name: "CourcesJobTitle",
                columns: table => new
                {
                    AccessRolesId = table.Column<int>(type: "int", nullable: false),
                    CoursesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourcesJobTitle", x => new { x.AccessRolesId, x.CoursesId });
                    table.ForeignKey(
                        name: "FK_CourcesJobTitle_Cources_CoursesId",
                        column: x => x.CoursesId,
                        principalTable: "Cources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourcesJobTitle_JobTitles_AccessRolesId",
                        column: x => x.AccessRolesId,
                        principalTable: "JobTitles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourcesJobTitle_CoursesId",
                table: "CourcesJobTitle",
                column: "CoursesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourcesJobTitle");

            migrationBuilder.AddColumn<int>(
                name: "CourcesId",
                table: "JobTitles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobTitles_CourcesId",
                table: "JobTitles",
                column: "CourcesId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobTitles_Cources_CourcesId",
                table: "JobTitles",
                column: "CourcesId",
                principalTable: "Cources",
                principalColumn: "Id");
        }
    }
}
