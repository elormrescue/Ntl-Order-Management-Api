using Microsoft.EntityFrameworkCore.Migrations;

namespace Ntl.AuthSS.OrderManagement.Data.Migrations
{
    public partial class ReelUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reel_Product_ProductId",
                table: "Reel");
            migrationBuilder.AddColumn<int>(
                name: "PrintPartnerId",
                table: "Reel",
                nullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_Reel_PrintPartnerId",
                table: "Reel",
                column: "PrintPartnerId");


            migrationBuilder.AddForeignKey(
                name: "FK_Reel_Organization_PrintPartnerId",
                table: "Reel",
                column: "PrintPartnerId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reel_Product_ProductId",
                table: "Reel",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.DropForeignKey(
                name: "FK_Reel_Organization_PrintPartnerId",
                table: "Reel");

            migrationBuilder.DropForeignKey(
                name: "FK_Reel_Product_ProductId",
                table: "Reel");

            
            migrationBuilder.DropIndex(
                name: "IX_Reel_PrintPartnerId",
                table: "Reel");

            migrationBuilder.DropColumn(
                name: "PrintPartnerId",
                table: "Reel");

            migrationBuilder.CreateTable(
                name: "ReelSize",
                columns: table => new
                {
                    TempId = table.Column<int>(nullable: false),
                    TempId1 = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.UniqueConstraint("AK_ReelSize_TempId", x => x.TempId);
                    table.UniqueConstraint("AK_ReelSize_TempId1", x => x.TempId1);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Reel_Product_ProductId",
                table: "Reel",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "TempId5",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
