using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ntl.AuthSS.OrderManagement.Data.Migrations
{
    public partial class AddedLockOrderItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LockOrderItemId",
                table: "Reel",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockOrderItemId",
                table: "Reel");
        }
    }
}
