using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ntl.AuthSS.OrderManagement.Data.Migrations
{
    public partial class FullfillLockDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<DateTime>(
                name: "FulfillLockedDate",
                table: "Reel",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "FulfillLockedDate",
                table: "Reel");

        }
    }
}
