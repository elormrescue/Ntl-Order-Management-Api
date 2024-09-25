using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ntl.AuthSS.OrderManagement.Data.Migrations
{
    public partial class ReelUpdate1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carton_Product_ProductId",
                table: "Carton");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalStockRequest_Warehouse_ApprovingFacilityId",
                table: "InternalStockRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalStockRequest_Organization_OrganizationId",
                table: "InternalStockRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalStockRequest_Product_ProductId",
                table: "InternalStockRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalStockRequest_Warehouse_RequestingFacilityId",
                table: "InternalStockRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalStockRequest_Organization_ShipperId",
                table: "InternalStockRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Organization_OrganizationId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Organization_ShipperId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Warehouse_WarehouseId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_OrgBrandProduct_BrandProductId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Product_ProductId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_ReelSize_ReelSizeId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_StockKeepingUnit_StockKeepingUnitId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Supplier_SupplierId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReel_OrgBrandProduct_BrandProductId",
                table: "OrderItemReel");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReel_OrgBrandProduct_NewBrandProductId",
                table: "OrderItemReel");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReel_StockKeepingUnit_NewSkuId",
                table: "OrderItemReel");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReel_Organization_OrganizationId",
                table: "OrderItemReel");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReel_Product_ProductId",
                table: "OrderItemReel");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReel_StockKeepingUnit_SkuId",
                table: "OrderItemReel");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReel_Warehouse_WarehouseId",
                table: "OrderItemReel");

            migrationBuilder.DropForeignKey(
                name: "FK_OrgWallet_Organization_OrganizationId",
                table: "OrgWallet");

            migrationBuilder.DropForeignKey(
                name: "FK_PrintOrder_Organization_PrintPartnerId",
                table: "PrintOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_PrintOrder_Product_ProductId",
                table: "PrintOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_PrintOrder_ReelSize_ReelSizeId",
                table: "PrintOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_PrintOrder_Warehouse_WarehouseId",
                table: "PrintOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_Reel_PrintOrder_PrintOrderId",
                table: "Reel");

            migrationBuilder.DropForeignKey(
                name: "FK_Reel_Organization_PrintPartnerId",
                table: "Reel");

            migrationBuilder.DropForeignKey(
                name: "FK_Reel_Product_ProductId",
                table: "Reel");

            migrationBuilder.DropForeignKey(
                name: "FK_ReelChangeRequest_OrgBrandProduct_ChangeToProductId",
                table: "ReelChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_ReelChangeRequest_StockKeepingUnit_ChangeToSkuId",
                table: "ReelChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_ReelChangeRequest_Organization_OrganizationId",
                table: "ReelChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_ReelChangeRequest_Product_ReelChangeProductId",
                table: "ReelChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnOrder_Organization_OrganizationId",
                table: "ReturnOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnOrder_Organization_ShipperId",
                table: "ReturnOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnOrder_Warehouse_WarehouseId",
                table: "ReturnOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnOrderReels_Product_ProductId",
                table: "ReturnOrderReels");

            //migrationBuilder.DropTable(
            //    name: "Organization");

            //migrationBuilder.DropTable(
            //    name: "OrgBrandProduct");

            //migrationBuilder.DropTable(
            //    name: "Product");

            //migrationBuilder.DropTable(
            //    name: "ReelSize");

            //migrationBuilder.DropTable(
            //    name: "StockKeepingUnit");

            //migrationBuilder.DropTable(
            //    name: "Supplier");

            //migrationBuilder.DropTable(
            //    name: "Warehouse");

            migrationBuilder.AlterColumn<Guid>(
                name: "PrintOrderId",
                table: "Reel",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Carton_Product_ProductId",
                table: "Carton",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalStockRequest_Warehouse_ApprovingFacilityId",
                table: "InternalStockRequest",
                column: "ApprovingFacilityId",
                principalTable: "Warehouse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalStockRequest_Organization_OrganizationId",
                table: "InternalStockRequest",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalStockRequest_Product_ProductId",
                table: "InternalStockRequest",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalStockRequest_Warehouse_RequestingFacilityId",
                table: "InternalStockRequest",
                column: "RequestingFacilityId",
                principalTable: "Warehouse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalStockRequest_Organization_ShipperId",
                table: "InternalStockRequest",
                column: "ShipperId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Organization_OrganizationId",
                table: "Order",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Organization_ShipperId",
                table: "Order",
                column: "ShipperId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Warehouse_WarehouseId",
                table: "Order",
                column: "WarehouseId",
                principalTable: "Warehouse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_OrgBrandProduct_BrandProductId",
                table: "OrderItem",
                column: "BrandProductId",
                principalTable: "OrgBrandProduct",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Product_ProductId",
                table: "OrderItem",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_ReelSize_ReelSizeId",
                table: "OrderItem",
                column: "ReelSizeId",
                principalTable: "ReelSize",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_StockKeepingUnit_StockKeepingUnitId",
                table: "OrderItem",
                column: "StockKeepingUnitId",
                principalTable: "StockKeepingUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Supplier_SupplierId",
                table: "OrderItem",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReel_OrgBrandProduct_BrandProductId",
                table: "OrderItemReel",
                column: "BrandProductId",
                principalTable: "OrgBrandProduct",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReel_OrgBrandProduct_NewBrandProductId",
                table: "OrderItemReel",
                column: "NewBrandProductId",
                principalTable: "OrgBrandProduct",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReel_StockKeepingUnit_NewSkuId",
                table: "OrderItemReel",
                column: "NewSkuId",
                principalTable: "StockKeepingUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReel_Organization_OrganizationId",
                table: "OrderItemReel",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReel_Product_ProductId",
                table: "OrderItemReel",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReel_StockKeepingUnit_SkuId",
                table: "OrderItemReel",
                column: "SkuId",
                principalTable: "StockKeepingUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReel_Warehouse_WarehouseId",
                table: "OrderItemReel",
                column: "WarehouseId",
                principalTable: "Warehouse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrgWallet_Organization_OrganizationId",
                table: "OrgWallet",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PrintOrder_Organization_PrintPartnerId",
                table: "PrintOrder",
                column: "PrintPartnerId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrintOrder_Product_ProductId",
                table: "PrintOrder",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrintOrder_ReelSize_ReelSizeId",
                table: "PrintOrder",
                column: "ReelSizeId",
                principalTable: "ReelSize",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrintOrder_Warehouse_WarehouseId",
                table: "PrintOrder",
                column: "WarehouseId",
                principalTable: "Warehouse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reel_PrintOrder_PrintOrderId",
                table: "Reel",
                column: "PrintOrderId",
                principalTable: "PrintOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.AddForeignKey(
                name: "FK_ReelChangeRequest_OrgBrandProduct_ChangeToProductId",
                table: "ReelChangeRequest",
                column: "ChangeToProductId",
                principalTable: "OrgBrandProduct",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReelChangeRequest_StockKeepingUnit_ChangeToSkuId",
                table: "ReelChangeRequest",
                column: "ChangeToSkuId",
                principalTable: "StockKeepingUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReelChangeRequest_Organization_OrganizationId",
                table: "ReelChangeRequest",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReelChangeRequest_Product_ReelChangeProductId",
                table: "ReelChangeRequest",
                column: "ReelChangeProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnOrder_Organization_OrganizationId",
                table: "ReturnOrder",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnOrder_Organization_ShipperId",
                table: "ReturnOrder",
                column: "ShipperId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnOrder_Warehouse_WarehouseId",
                table: "ReturnOrder",
                column: "WarehouseId",
                principalTable: "Warehouse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnOrderReels_Product_ProductId",
                table: "ReturnOrderReels",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carton_Product_ProductId",
                table: "Carton");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalStockRequest_Warehouse_ApprovingFacilityId",
                table: "InternalStockRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalStockRequest_Organization_OrganizationId",
                table: "InternalStockRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalStockRequest_Product_ProductId",
                table: "InternalStockRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalStockRequest_Warehouse_RequestingFacilityId",
                table: "InternalStockRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalStockRequest_Organization_ShipperId",
                table: "InternalStockRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Organization_OrganizationId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Organization_ShipperId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Warehouse_WarehouseId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_OrgBrandProduct_BrandProductId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Product_ProductId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_ReelSize_ReelSizeId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_StockKeepingUnit_StockKeepingUnitId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Supplier_SupplierId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReel_OrgBrandProduct_BrandProductId",
                table: "OrderItemReel");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReel_OrgBrandProduct_NewBrandProductId",
                table: "OrderItemReel");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReel_StockKeepingUnit_NewSkuId",
                table: "OrderItemReel");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReel_Organization_OrganizationId",
                table: "OrderItemReel");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReel_Product_ProductId",
                table: "OrderItemReel");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReel_StockKeepingUnit_SkuId",
                table: "OrderItemReel");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemReel_Warehouse_WarehouseId",
                table: "OrderItemReel");

            migrationBuilder.DropForeignKey(
                name: "FK_OrgWallet_Organization_OrganizationId",
                table: "OrgWallet");

            migrationBuilder.DropForeignKey(
                name: "FK_PrintOrder_Organization_PrintPartnerId",
                table: "PrintOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_PrintOrder_Product_ProductId",
                table: "PrintOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_PrintOrder_ReelSize_ReelSizeId",
                table: "PrintOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_PrintOrder_Warehouse_WarehouseId",
                table: "PrintOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_Reel_PrintOrder_PrintOrderId",
                table: "Reel");

            migrationBuilder.DropForeignKey(
                name: "FK_Reel_Organization_PrintPartnerId",
                table: "Reel");

            migrationBuilder.DropForeignKey(
                name: "FK_Reel_Product_ProductId",
                table: "Reel");

            migrationBuilder.DropForeignKey(
                name: "FK_ReelChangeRequest_OrgBrandProduct_ChangeToProductId",
                table: "ReelChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_ReelChangeRequest_StockKeepingUnit_ChangeToSkuId",
                table: "ReelChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_ReelChangeRequest_Organization_OrganizationId",
                table: "ReelChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_ReelChangeRequest_Product_ReelChangeProductId",
                table: "ReelChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnOrder_Organization_OrganizationId",
                table: "ReturnOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnOrder_Organization_ShipperId",
                table: "ReturnOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnOrder_Warehouse_WarehouseId",
                table: "ReturnOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnOrderReels_Product_ProductId",
                table: "ReturnOrderReels");

            migrationBuilder.AlterColumn<Guid>(
                name: "PrintOrderId",
                table: "Reel",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            //migrationBuilder.CreateTable(
            //    name: "Organization",
            //    columns: table => new
            //    {
            //        TempId = table.Column<int>(nullable: false),
            //        TempId1 = table.Column<int>(nullable: false),
            //        TempId10 = table.Column<int>(nullable: false),
            //        TempId2 = table.Column<int>(nullable: false),
            //        TempId3 = table.Column<int>(nullable: false),
            //        TempId4 = table.Column<int>(nullable: false),
            //        TempId5 = table.Column<int>(nullable: false),
            //        TempId6 = table.Column<int>(nullable: false),
            //        TempId7 = table.Column<int>(nullable: false),
            //        TempId8 = table.Column<int>(nullable: false),
            //        TempId9 = table.Column<int>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.UniqueConstraint("AK_Organization_TempId", x => x.TempId);
            //        table.UniqueConstraint("AK_Organization_TempId1", x => x.TempId1);
            //        table.UniqueConstraint("AK_Organization_TempId10", x => x.TempId10);
            //        table.UniqueConstraint("AK_Organization_TempId2", x => x.TempId2);
            //        table.UniqueConstraint("AK_Organization_TempId3", x => x.TempId3);
            //        table.UniqueConstraint("AK_Organization_TempId4", x => x.TempId4);
            //        table.UniqueConstraint("AK_Organization_TempId5", x => x.TempId5);
            //        table.UniqueConstraint("AK_Organization_TempId6", x => x.TempId6);
            //        table.UniqueConstraint("AK_Organization_TempId7", x => x.TempId7);
            //        table.UniqueConstraint("AK_Organization_TempId8", x => x.TempId8);
            //        table.UniqueConstraint("AK_Organization_TempId9", x => x.TempId9);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "OrgBrandProduct",
            //    columns: table => new
            //    {
            //        TempId = table.Column<int>(nullable: false),
            //        TempId1 = table.Column<int>(nullable: false),
            //        TempId2 = table.Column<int>(nullable: false),
            //        TempId3 = table.Column<int>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.UniqueConstraint("AK_OrgBrandProduct_TempId", x => x.TempId);
            //        table.UniqueConstraint("AK_OrgBrandProduct_TempId1", x => x.TempId1);
            //        table.UniqueConstraint("AK_OrgBrandProduct_TempId2", x => x.TempId2);
            //        table.UniqueConstraint("AK_OrgBrandProduct_TempId3", x => x.TempId3);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Product",
            //    columns: table => new
            //    {
            //        TempId = table.Column<int>(nullable: false),
            //        TempId1 = table.Column<int>(nullable: false),
            //        TempId2 = table.Column<int>(nullable: false),
            //        TempId3 = table.Column<int>(nullable: false),
            //        TempId4 = table.Column<int>(nullable: false),
            //        TempId5 = table.Column<int>(nullable: false),
            //        TempId6 = table.Column<int>(nullable: false),
            //        TempId7 = table.Column<int>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.UniqueConstraint("AK_Product_TempId", x => x.TempId);
            //        table.UniqueConstraint("AK_Product_TempId1", x => x.TempId1);
            //        table.UniqueConstraint("AK_Product_TempId2", x => x.TempId2);
            //        table.UniqueConstraint("AK_Product_TempId3", x => x.TempId3);
            //        table.UniqueConstraint("AK_Product_TempId4", x => x.TempId4);
            //        table.UniqueConstraint("AK_Product_TempId5", x => x.TempId5);
            //        table.UniqueConstraint("AK_Product_TempId6", x => x.TempId6);
            //        table.UniqueConstraint("AK_Product_TempId7", x => x.TempId7);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ReelSize",
            //    columns: table => new
            //    {
            //        TempId = table.Column<int>(nullable: false),
            //        TempId1 = table.Column<int>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.UniqueConstraint("AK_ReelSize_TempId", x => x.TempId);
            //        table.UniqueConstraint("AK_ReelSize_TempId1", x => x.TempId1);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "StockKeepingUnit",
            //    columns: table => new
            //    {
            //        TempId = table.Column<int>(nullable: false),
            //        TempId1 = table.Column<int>(nullable: false),
            //        TempId2 = table.Column<int>(nullable: false),
            //        TempId3 = table.Column<int>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.UniqueConstraint("AK_StockKeepingUnit_TempId", x => x.TempId);
            //        table.UniqueConstraint("AK_StockKeepingUnit_TempId1", x => x.TempId1);
            //        table.UniqueConstraint("AK_StockKeepingUnit_TempId2", x => x.TempId2);
            //        table.UniqueConstraint("AK_StockKeepingUnit_TempId3", x => x.TempId3);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Supplier",
            //    columns: table => new
            //    {
            //        TempId = table.Column<int>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.UniqueConstraint("AK_Supplier_TempId", x => x.TempId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Warehouse",
            //    columns: table => new
            //    {
            //        TempId = table.Column<int>(nullable: false),
            //        TempId1 = table.Column<int>(nullable: false),
            //        TempId2 = table.Column<int>(nullable: false),
            //        TempId3 = table.Column<int>(nullable: false),
            //        TempId4 = table.Column<int>(nullable: false),
            //        TempId5 = table.Column<int>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.UniqueConstraint("AK_Warehouse_TempId", x => x.TempId);
            //        table.UniqueConstraint("AK_Warehouse_TempId1", x => x.TempId1);
            //        table.UniqueConstraint("AK_Warehouse_TempId2", x => x.TempId2);
            //        table.UniqueConstraint("AK_Warehouse_TempId3", x => x.TempId3);
            //        table.UniqueConstraint("AK_Warehouse_TempId4", x => x.TempId4);
            //        table.UniqueConstraint("AK_Warehouse_TempId5", x => x.TempId5);
            //    });

            migrationBuilder.AddForeignKey(
                name: "FK_Carton_Product_ProductId",
                table: "Carton",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "TempId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalStockRequest_Warehouse_ApprovingFacilityId",
                table: "InternalStockRequest",
                column: "ApprovingFacilityId",
                principalTable: "Warehouse",
                principalColumn: "TempId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalStockRequest_Organization_OrganizationId",
                table: "InternalStockRequest",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "TempId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalStockRequest_Product_ProductId",
                table: "InternalStockRequest",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "TempId1",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalStockRequest_Warehouse_RequestingFacilityId",
                table: "InternalStockRequest",
                column: "RequestingFacilityId",
                principalTable: "Warehouse",
                principalColumn: "TempId1",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalStockRequest_Organization_ShipperId",
                table: "InternalStockRequest",
                column: "ShipperId",
                principalTable: "Organization",
                principalColumn: "TempId1",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Organization_OrganizationId",
                table: "Order",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "TempId2",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Organization_ShipperId",
                table: "Order",
                column: "ShipperId",
                principalTable: "Organization",
                principalColumn: "TempId3",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Warehouse_WarehouseId",
                table: "Order",
                column: "WarehouseId",
                principalTable: "Warehouse",
                principalColumn: "TempId2",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_OrgBrandProduct_BrandProductId",
                table: "OrderItem",
                column: "BrandProductId",
                principalTable: "OrgBrandProduct",
                principalColumn: "TempId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Product_ProductId",
                table: "OrderItem",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "TempId2",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_ReelSize_ReelSizeId",
                table: "OrderItem",
                column: "ReelSizeId",
                principalTable: "ReelSize",
                principalColumn: "TempId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_StockKeepingUnit_StockKeepingUnitId",
                table: "OrderItem",
                column: "StockKeepingUnitId",
                principalTable: "StockKeepingUnit",
                principalColumn: "TempId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Supplier_SupplierId",
                table: "OrderItem",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "TempId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReel_OrgBrandProduct_BrandProductId",
                table: "OrderItemReel",
                column: "BrandProductId",
                principalTable: "OrgBrandProduct",
                principalColumn: "TempId1",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReel_OrgBrandProduct_NewBrandProductId",
                table: "OrderItemReel",
                column: "NewBrandProductId",
                principalTable: "OrgBrandProduct",
                principalColumn: "TempId2",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReel_StockKeepingUnit_NewSkuId",
                table: "OrderItemReel",
                column: "NewSkuId",
                principalTable: "StockKeepingUnit",
                principalColumn: "TempId1",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReel_Organization_OrganizationId",
                table: "OrderItemReel",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "TempId4",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReel_Product_ProductId",
                table: "OrderItemReel",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "TempId3",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReel_StockKeepingUnit_SkuId",
                table: "OrderItemReel",
                column: "SkuId",
                principalTable: "StockKeepingUnit",
                principalColumn: "TempId2",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemReel_Warehouse_WarehouseId",
                table: "OrderItemReel",
                column: "WarehouseId",
                principalTable: "Warehouse",
                principalColumn: "TempId3",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrgWallet_Organization_OrganizationId",
                table: "OrgWallet",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "TempId5",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PrintOrder_Organization_PrintPartnerId",
                table: "PrintOrder",
                column: "PrintPartnerId",
                principalTable: "Organization",
                principalColumn: "TempId6",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrintOrder_Product_ProductId",
                table: "PrintOrder",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "TempId4",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrintOrder_ReelSize_ReelSizeId",
                table: "PrintOrder",
                column: "ReelSizeId",
                principalTable: "ReelSize",
                principalColumn: "TempId1",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrintOrder_Warehouse_WarehouseId",
                table: "PrintOrder",
                column: "WarehouseId",
                principalTable: "Warehouse",
                principalColumn: "TempId4",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reel_PrintOrder_PrintOrderId",
                table: "Reel",
                column: "PrintOrderId",
                principalTable: "PrintOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reel_Organization_PrintPartnerId",
                table: "Reel",
                column: "PrintPartnerId",
                principalTable: "Organization",
                principalColumn: "TempId7",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reel_Product_ProductId",
                table: "Reel",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "TempId5",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReelChangeRequest_OrgBrandProduct_ChangeToProductId",
                table: "ReelChangeRequest",
                column: "ChangeToProductId",
                principalTable: "OrgBrandProduct",
                principalColumn: "TempId3",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReelChangeRequest_StockKeepingUnit_ChangeToSkuId",
                table: "ReelChangeRequest",
                column: "ChangeToSkuId",
                principalTable: "StockKeepingUnit",
                principalColumn: "TempId3",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReelChangeRequest_Organization_OrganizationId",
                table: "ReelChangeRequest",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "TempId8",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReelChangeRequest_Product_ReelChangeProductId",
                table: "ReelChangeRequest",
                column: "ReelChangeProductId",
                principalTable: "Product",
                principalColumn: "TempId6",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnOrder_Organization_OrganizationId",
                table: "ReturnOrder",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "TempId9",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnOrder_Organization_ShipperId",
                table: "ReturnOrder",
                column: "ShipperId",
                principalTable: "Organization",
                principalColumn: "TempId10",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnOrder_Warehouse_WarehouseId",
                table: "ReturnOrder",
                column: "WarehouseId",
                principalTable: "Warehouse",
                principalColumn: "TempId5",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnOrderReels_Product_ProductId",
                table: "ReturnOrderReels",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "TempId7",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
