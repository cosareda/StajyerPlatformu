using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajyerPlatformu.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicationStatusAndSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "InternshipPosts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "InternshipPosts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkType",
                table: "InternshipPosts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Github",
                table: "InternProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "EmployerProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoPath",
                table: "EmployerProfiles",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "InternshipPosts");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "InternshipPosts");

            migrationBuilder.DropColumn(
                name: "WorkType",
                table: "InternshipPosts");

            migrationBuilder.DropColumn(
                name: "Github",
                table: "InternProfiles");

            migrationBuilder.DropColumn(
                name: "City",
                table: "EmployerProfiles");

            migrationBuilder.DropColumn(
                name: "LogoPath",
                table: "EmployerProfiles");
        }
    }
}
