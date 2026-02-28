using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GurpsWizard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpells : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LibrarySpells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GcsId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    College = table.Column<string>(type: "TEXT", nullable: false),
                    PowerSource = table.Column<string>(type: "TEXT", nullable: true),
                    SpellClass = table.Column<string>(type: "TEXT", nullable: true),
                    Difficulty = table.Column<string>(type: "TEXT", nullable: false),
                    Resist = table.Column<string>(type: "TEXT", nullable: true),
                    CastingCost = table.Column<string>(type: "TEXT", nullable: true),
                    MaintenanceCost = table.Column<string>(type: "TEXT", nullable: true),
                    CastingTime = table.Column<string>(type: "TEXT", nullable: true),
                    Duration = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Reference = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibrarySpells", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LibrarySpells_College",
                table: "LibrarySpells",
                column: "College");

            migrationBuilder.CreateIndex(
                name: "IX_LibrarySpells_GcsId",
                table: "LibrarySpells",
                column: "GcsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibrarySpells_Name",
                table: "LibrarySpells",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibrarySpells");
        }
    }
}
