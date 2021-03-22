using Microsoft.EntityFrameworkCore.Migrations;

namespace RamsgateDigitalCinema.Data.Migrations
{
    public partial class FilmDetailsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FilmDetails",
                columns: table => new
                {
                    FilmDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FilmID = table.Column<int>(type: "int", nullable: false),
                    PosterUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrailerUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectorPicUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectorPicUrl2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryOfOrigin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilmLength = table.Column<int>(type: "int", nullable: false),
                    Synopsis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectorOrigin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectorBio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectorsFirstFilm = table.Column<bool>(type: "bit", nullable: false),
                    Screen = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmDetails", x => x.FilmDetailsID);
                    table.ForeignKey(
                        name: "FK_FilmDetails_Films_FilmID",
                        column: x => x.FilmID,
                        principalTable: "Films",
                        principalColumn: "FilmID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StillUrl",
                columns: table => new
                {
                    StillUrlID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilmDetailsID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StillUrl", x => x.StillUrlID);
                    table.ForeignKey(
                        name: "FK_StillUrl_FilmDetails_FilmDetailsID",
                        column: x => x.FilmDetailsID,
                        principalTable: "FilmDetails",
                        principalColumn: "FilmDetailsID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FilmDetails_FilmID",
                table: "FilmDetails",
                column: "FilmID");

            migrationBuilder.CreateIndex(
                name: "IX_StillUrl_FilmDetailsID",
                table: "StillUrl",
                column: "FilmDetailsID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StillUrl");

            migrationBuilder.DropTable(
                name: "FilmDetails");
        }
    }
}
