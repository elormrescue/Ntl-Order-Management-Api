using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ntl.AuthSS.OrderManagement.Data.Migrations
{
    public partial class TaxSlabChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderPaymentBreakdown",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<Guid>(nullable: true),
                    TaxTypeOrSubTotal = table.Column<string>(nullable: true),
                    Percentage = table.Column<decimal>(nullable: true),
                    Amount = table.Column<decimal>(nullable: true),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderPaymentBreakdown", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderPaymentBreakdown_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxSlab",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxType = table.Column<string>(nullable: true),
                    Percentage = table.Column<decimal>(nullable: true),
                    IsCumulative = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxSlab", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderPaymentBreakdown_OrderId",
                table: "OrderPaymentBreakdown",
                column: "OrderId");

            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropTable(
                name: "OrderPaymentBreakdown");

            migrationBuilder.DropTable(
                name: "TaxSlab");
        }
    }
}
