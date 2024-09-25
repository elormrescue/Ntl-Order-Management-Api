using Microsoft.EntityFrameworkCore.Migrations;

namespace Ntl.AuthSS.OrderManagement.Data.Migrations
{
    public partial class PrintOrderRequestUpdate1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrintPartnerId",
                table: "PrintOrderRequest",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrintOrderRequest_PrintPartnerId",
                table: "PrintOrderRequest",
                column: "PrintPartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrintOrderRequest_Organization_PrintPartnerId",
                table: "PrintOrderRequest",
                column: "PrintPartnerId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropForeignKey(
                name: "FK_PrintOrderRequest_Organization_PrintPartnerId",
                table: "PrintOrderRequest");

            migrationBuilder.DropIndex(
                name: "IX_PrintOrderRequest_PrintPartnerId",
                table: "PrintOrderRequest");

            migrationBuilder.DropColumn(
                name: "PrintPartnerId",
                table: "PrintOrderRequest");
        }
    }
}
