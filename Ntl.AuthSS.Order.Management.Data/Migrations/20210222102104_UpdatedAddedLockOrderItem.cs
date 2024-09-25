using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ntl.AuthSS.OrderManagement.Data.Migrations
{
    public partial class UpdatedAddedLockOrderItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockOrderItemId",
                table: "Reel");

            migrationBuilder.AddColumn<Guid>(
                name: "LockedOrderItemId",
                table: "Reel",
                nullable: true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropColumn(
                name: "LockedOrderItemId",
                table: "Reel");

            migrationBuilder.AddColumn<int>(
                name: "LockOrderItemId",
                table: "Reel",
                type: "int",
                nullable: true);
        }
    }
}
