using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Moka.Sdk.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocalId = table.Column<Guid>(nullable: false),
                    Data = table.Column<byte[]>(nullable: true),
                    MessageType = table.Column<int>(nullable: false),
                    From = table.Column<Guid>(nullable: false),
                    To = table.Column<Guid>(nullable: false),
                    Created_at = table.Column<DateTime>(nullable: false),
                    Delivered_at = table.Column<DateTime>(nullable: true),
                    Read_at = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Guid = table.Column<Guid>(nullable: false),
                    NickName = table.Column<string>(nullable: true),
                    Username = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
