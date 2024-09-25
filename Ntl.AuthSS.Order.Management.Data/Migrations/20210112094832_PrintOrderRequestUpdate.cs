using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ntl.AuthSS.OrderManagement.Data.Migrations
{
    public partial class PrintOrderRequestUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropTable(
                name: "PoRequest");

            migrationBuilder.CreateTable(
                name: "PrintOrderRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PrintOrderId = table.Column<Guid>(nullable: true),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintOrderRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrintOrderRequest_PrintOrder_PrintOrderId",
                        column: x => x.PrintOrderId,
                        principalTable: "PrintOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrintOrderRequest_PrintOrderId",
                table: "PrintOrderRequest",
                column: "PrintOrderId");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrintOrderRequest");

            migrationBuilder.CreateTable(
                name: "PoRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUser = table.Column<int>(type: "int", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUser = table.Column<int>(type: "int", nullable: false),
                    PrintOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoRequest_PrintOrder_PrintOrderId",
                        column: x => x.PrintOrderId,
                        principalTable: "PrintOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PoRequest_PrintOrderId",
                table: "PoRequest",
                column: "PrintOrderId");
        }
    }
}
