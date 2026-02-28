using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GurpsWizard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTechniques : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LibraryTechniques",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GcsId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Difficulty = table.Column<string>(type: "TEXT", nullable: false),
                    ParentSkillName = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultModifier = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxAboveDefault = table.Column<int>(type: "INTEGER", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Reference = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryTechniques", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LibraryTechniques_GcsId",
                table: "LibraryTechniques",
                column: "GcsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibraryTechniques_Name",
                table: "LibraryTechniques",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibraryTechniques");
        }
    }
}
