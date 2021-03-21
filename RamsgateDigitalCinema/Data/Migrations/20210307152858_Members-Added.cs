using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RamsgateDigitalCinema.Data.Migrations
{
    public partial class MembersAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    MemberID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ASPID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    TimeDiff = table.Column<TimeSpan>(type: "time", nullable: false),
                    TimeDiffPositive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoggedIn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DontEmail = table.Column<bool>(type: "bit", nullable: false),
                    UnsubscribeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.MemberID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Members");
        }
    }
}
