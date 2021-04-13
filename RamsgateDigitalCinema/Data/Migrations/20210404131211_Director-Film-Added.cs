using Microsoft.EntityFrameworkCore.Migrations;

namespace RamsgateDigitalCinema.Data.Migrations
{
    public partial class DirectorFilmAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Director",
                table: "Films",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Director",
                table: "Films");
        }
    }
}
