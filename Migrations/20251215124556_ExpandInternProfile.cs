using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajyerPlatformu.Migrations
{
    /// <inheritdoc />
    public partial class ExpandInternProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "InternProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "InternProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "InternProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedIn",
                table: "InternProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoPath",
                table: "InternProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentId",
                table: "InternProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Experiences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyName = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    TotalHours = table.Column<int>(type: "INTEGER", nullable: false),
                    InternProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experiences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Experiences_InternProfiles_InternProfileId",
                        column: x => x.InternProfileId,
                        principalTable: "InternProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Experiences_InternProfileId",
                table: "Experiences",
                column: "InternProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Experiences");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "InternProfiles");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "InternProfiles");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "InternProfiles");

            migrationBuilder.DropColumn(
                name: "LinkedIn",
                table: "InternProfiles");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoPath",
                table: "InternProfiles");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "InternProfiles");
        }
    }
}
