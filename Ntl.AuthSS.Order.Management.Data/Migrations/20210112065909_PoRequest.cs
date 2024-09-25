using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ntl.AuthSS.OrderManagement.Data.Migrations
{
    public partial class PoRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrintOrderRequest");

            migrationBuilder.CreateTable(
                name: "PoRequest",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropTable(
                name: "PoRequest");

            migrationBuilder.CreateTable(
                name: "PrintOrderRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrintOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    createdUser = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintOrderRequest", x => x.Id);
                });

        }
    }
}
