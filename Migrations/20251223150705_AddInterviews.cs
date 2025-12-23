using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajyerPlatformu.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Interviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployerProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    InternProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    InternshipPostId = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interviews_EmployerProfiles_EmployerProfileId",
                        column: x => x.EmployerProfileId,
                        principalTable: "EmployerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Interviews_InternProfiles_InternProfileId",
                        column: x => x.InternProfileId,
                        principalTable: "InternProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Interviews_InternshipPosts_InternshipPostId",
                        column: x => x.InternshipPostId,
                        principalTable: "InternshipPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_EmployerProfileId",
                table: "Interviews",
                column: "EmployerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_InternProfileId",
                table: "Interviews",
                column: "InternProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_InternshipPostId",
                table: "Interviews",
                column: "InternshipPostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Interviews");
        }
    }
}
