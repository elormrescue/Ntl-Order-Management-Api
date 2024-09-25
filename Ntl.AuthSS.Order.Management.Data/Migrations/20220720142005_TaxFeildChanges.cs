using Microsoft.EntityFrameworkCore.Migrations;

namespace Ntl.AuthSS.OrderManagement.Data.Migrations
{
    public partial class TaxFeildChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TaxSlab");

            migrationBuilder.DropColumn(
                name: "TaxTypeOrSubTotal",
                table: "OrderPaymentBreakdown");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TaxSlab",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsTax",
                table: "OrderPaymentBreakdown",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ItemPriceType",
                table: "OrderPaymentBreakdown",
                nullable: true);
           
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TaxSlab");

            migrationBuilder.DropColumn(
                name: "IsTax",
                table: "OrderPaymentBreakdown");

            migrationBuilder.DropColumn(
                name: "ItemPriceType",
                table: "OrderPaymentBreakdown");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TaxSlab",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TaxTypeOrSubTotal",
                table: "OrderPaymentBreakdown",
                type: "nvarchar(max)",
                nullable: true);

            
        }
    }
}
