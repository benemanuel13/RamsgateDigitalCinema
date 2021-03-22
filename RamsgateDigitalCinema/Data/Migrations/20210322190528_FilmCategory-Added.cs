using Microsoft.EntityFrameworkCore.Migrations;

namespace RamsgateDigitalCinema.Data.Migrations
{
    public partial class FilmCategoryAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FilmCategoryID",
                table: "Films",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FilmID",
                table: "FilmCollections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsViewable",
                table: "FilmCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Films_FilmCategoryID",
                table: "Films",
                column: "FilmCategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Films_FilmCategories_FilmCategoryID",
                table: "Films",
                column: "FilmCategoryID",
                principalTable: "FilmCategories",
                principalColumn: "FilmCategoryID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Films_FilmCategories_FilmCategoryID",
                table: "Films");

            migrationBuilder.DropIndex(
                name: "IX_Films_FilmCategoryID",
                table: "Films");

            migrationBuilder.DropColumn(
                name: "FilmCategoryID",
                table: "Films");

            migrationBuilder.DropColumn(
                name: "FilmID",
                table: "FilmCollections");

            migrationBuilder.DropColumn(
                name: "IsViewable",
                table: "FilmCategories");
        }
    }
}
