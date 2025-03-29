using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookShop.Migrations
{
    public partial class AddSellerIdAndBalance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Autors",
                schema: "Products",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Autors",
                schema: "Autors");

            migrationBuilder.EnsureSchema(
                name: "Authors");

            migrationBuilder.RenameColumn(
                name: "AutorID",
                schema: "Products",
                table: "Products",
                newName: "AuthorID");

            migrationBuilder.RenameIndex(
                name: "IX_Products_AutorID",
                schema: "Products",
                table: "Products",
                newName: "IX_Products_AuthorID");

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                schema: "Users",
                table: "Users",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SellerID",
                schema: "Products",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Authors",
                schema: "Authors",
                columns: table => new
                {
                    AuthorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    DeathDate = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.AuthorID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_SellerID",
                schema: "Products",
                table: "Products",
                column: "SellerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Authors",
                schema: "Products",
                table: "Products",
                column: "AuthorID",
                principalSchema: "Authors",
                principalTable: "Authors",
                principalColumn: "AuthorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_Seller",
                schema: "Products",
                table: "Products",
                column: "SellerID",
                principalSchema: "Users",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Authors",
                schema: "Products",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_Seller",
                schema: "Products",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Authors",
                schema: "Authors");

            migrationBuilder.DropIndex(
                name: "IX_Products_SellerID",
                schema: "Products",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Balance",
                schema: "Users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SellerID",
                schema: "Products",
                table: "Products");

            migrationBuilder.EnsureSchema(
                name: "Autors");

            migrationBuilder.RenameColumn(
                name: "AuthorID",
                schema: "Products",
                table: "Products",
                newName: "AutorID");

            migrationBuilder.RenameIndex(
                name: "IX_Products_AuthorID",
                schema: "Products",
                table: "Products",
                newName: "IX_Products_AutorID");

            migrationBuilder.CreateTable(
                name: "Autors",
                schema: "Autors",
                columns: table => new
                {
                    AutorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeathDate = table.Column<DateTime>(type: "date", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Autors", x => x.AutorID);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Autors",
                schema: "Products",
                table: "Products",
                column: "AutorID",
                principalSchema: "Autors",
                principalTable: "Autors",
                principalColumn: "AutorID");
        }
    }
}
