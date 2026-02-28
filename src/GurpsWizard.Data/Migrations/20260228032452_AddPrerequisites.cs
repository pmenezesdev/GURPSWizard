using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GurpsWizard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrerequisites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrerequisitesJson",
                table: "LibraryTraits",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrerequisitesJson",
                table: "LibrarySkills",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrerequisitesJson",
                table: "LibraryTraits");

            migrationBuilder.DropColumn(
                name: "PrerequisitesJson",
                table: "LibrarySkills");
        }
    }
}
