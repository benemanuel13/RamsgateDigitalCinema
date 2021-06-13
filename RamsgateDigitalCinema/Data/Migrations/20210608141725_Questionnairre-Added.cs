using Microsoft.EntityFrameworkCore.Migrations;

namespace RamsgateDigitalCinema.Data.Migrations
{
    public partial class QuestionnairreAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuestionnaireID",
                table: "Films",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Questionnaires",
                columns: table => new
                {
                    QuestionnaireID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FilmID = table.Column<int>(type: "int", nullable: false),
                    MemberID = table.Column<int>(type: "int", nullable: false),
                    IsCollection = table.Column<bool>(type: "bit", nullable: false),
                    FavouriteFilmID = table.Column<int>(type: "int", nullable: true),
                    FilmTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Like = table.Column<int>(type: "int", nullable: false),
                    LikeFestival = table.Column<int>(type: "int", nullable: false),
                    Booked = table.Column<int>(type: "int", nullable: false),
                    BookedEvent = table.Column<bool>(type: "bit", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Overall = table.Column<int>(type: "int", nullable: false),
                    TheFilms = table.Column<int>(type: "int", nullable: false),
                    TheEvents = table.Column<int>(type: "int", nullable: false),
                    Networking = table.Column<int>(type: "int", nullable: false),
                    DoBetter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComeNext = table.Column<bool>(type: "bit", nullable: false),
                    WhereFrom = table.Column<int>(type: "int", nullable: false),
                    Industry = table.Column<bool>(type: "bit", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questionnaires", x => x.QuestionnaireID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Films_QuestionnaireID",
                table: "Films",
                column: "QuestionnaireID");

            migrationBuilder.AddForeignKey(
                name: "FK_Films_Questionnaires_QuestionnaireID",
                table: "Films",
                column: "QuestionnaireID",
                principalTable: "Questionnaires",
                principalColumn: "QuestionnaireID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Films_Questionnaires_QuestionnaireID",
                table: "Films");

            migrationBuilder.DropTable(
                name: "Questionnaires");

            migrationBuilder.DropIndex(
                name: "IX_Films_QuestionnaireID",
                table: "Films");

            migrationBuilder.DropColumn(
                name: "QuestionnaireID",
                table: "Films");
        }
    }
}
