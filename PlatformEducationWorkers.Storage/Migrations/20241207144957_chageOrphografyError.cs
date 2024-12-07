using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlatformEducationWorkers.Storage.Migrations
{
    /// <inheritdoc />
    public partial class chageOrphografyError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserResults_Cources_CourceId",
                table: "UserResults");

            migrationBuilder.DropTable(
                name: "CourcesJobTitle");

            migrationBuilder.RenameColumn(
                name: "CourceId",
                table: "UserResults",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_UserResults_CourceId",
                table: "UserResults",
                newName: "IX_UserResults_CourseId");

            migrationBuilder.CreateTable(
                name: "CoursesJobTitle",
                columns: table => new
                {
                    AccessRolesId = table.Column<int>(type: "int", nullable: false),
                    CoursesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursesJobTitle", x => new { x.AccessRolesId, x.CoursesId });
                    table.ForeignKey(
                        name: "FK_CoursesJobTitle_Cources_CoursesId",
                        column: x => x.CoursesId,
                        principalTable: "Cources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoursesJobTitle_JobTitles_AccessRolesId",
                        column: x => x.AccessRolesId,
                        principalTable: "JobTitles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoursesJobTitle_CoursesId",
                table: "CoursesJobTitle",
                column: "CoursesId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserResults_Cources_CourseId",
                table: "UserResults",
                column: "CourseId",
                principalTable: "Cources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserResults_Cources_CourseId",
                table: "UserResults");

            migrationBuilder.DropTable(
                name: "CoursesJobTitle");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "UserResults",
                newName: "CourceId");

            migrationBuilder.RenameIndex(
                name: "IX_UserResults_CourseId",
                table: "UserResults",
                newName: "IX_UserResults_CourceId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_UserResults_Cources_CourceId",
                table: "UserResults",
                column: "CourceId",
                principalTable: "Cources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
