using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace CoinService.Migrations
{
    public partial class AddInvoiceEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_InvoiceId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Users",
                newName: "Id");

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    DateAndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "OwnerId");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceId",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_InvoiceId",
                table: "Users",
                column: "InvoiceId",
                unique: true);
        }
    }
}
