using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RamsgateDigitalCinema.Data.Migrations
{
    public partial class InitialFilmsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FilmCategories",
                columns: table => new
                {
                    FilmCategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmCategories", x => x.FilmCategoryID);
                });

            migrationBuilder.CreateTable(
                name: "FilmCollections",
                columns: table => new
                {
                    FilmCollectionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmCollections", x => x.FilmCollectionID);
                });

            migrationBuilder.CreateTable(
                name: "Films",
                columns: table => new
                {
                    FilmID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Showing = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilmCollectionID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Films", x => x.FilmID);
                    table.ForeignKey(
                        name: "FK_Films_FilmCollections_FilmCollectionID",
                        column: x => x.FilmCollectionID,
                        principalTable: "FilmCollections",
                        principalColumn: "FilmCollectionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberFilms",
                columns: table => new
                {
                    MemberFilmID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberID = table.Column<int>(type: "int", nullable: false),
                    FilmID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberFilms", x => x.MemberFilmID);
                    table.ForeignKey(
                        name: "FK_MemberFilms_Films_FilmID",
                        column: x => x.FilmID,
                        principalTable: "Films",
                        principalColumn: "FilmID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberFilms_Members_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Films_FilmCollectionID",
                table: "Films",
                column: "FilmCollectionID");

            migrationBuilder.CreateIndex(
                name: "IX_MemberFilms_FilmID",
                table: "MemberFilms",
                column: "FilmID");

            migrationBuilder.CreateIndex(
                name: "IX_MemberFilms_MemberID",
                table: "MemberFilms",
                column: "MemberID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilmCategories");

            migrationBuilder.DropTable(
                name: "MemberFilms");

            migrationBuilder.DropTable(
                name: "Films");

            migrationBuilder.DropTable(
                name: "FilmCollections");
        }
    }
}
