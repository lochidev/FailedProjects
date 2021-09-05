using Microsoft.EntityFrameworkCore.Migrations;

namespace CoinService.Migrations
{
    public partial class IndexinvoiceId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_InvoiceId",
                table: "Users",
                column: "InvoiceId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_InvoiceId",
                table: "Users");
        }
    }
}
