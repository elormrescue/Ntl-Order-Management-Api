using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ntl.AuthSS.OrderManagement.Data.Migrations
{
    public partial class InitialDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Container",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(nullable: true),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Container", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InternalStockRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Number = table.Column<string>(maxLength: 50, nullable: true),
                    ProductId = table.Column<int>(nullable: true),
                    NoOfStamps = table.Column<int>(nullable: false),
                    OrganizationId = table.Column<int>(nullable: true),
                    RequestingFacilityId = table.Column<int>(nullable: true),
                    ApprovingFacilityId = table.Column<int>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    ExpiredDate = table.Column<DateTime>(nullable: true),
                    ExpectedDate = table.Column<DateTime>(nullable: true),
                    TrackingId = table.Column<string>(maxLength: 50, nullable: true),
                    ShipperId = table.Column<int>(nullable: true),
                    FulfillmentComments = table.Column<string>(maxLength: 500, nullable: true),
                    DeliveryOtp = table.Column<int>(nullable: true),
                    DeliveryRemarks = table.Column<string>(maxLength: 50, nullable: true),
                    CartonCount = table.Column<int>(nullable: false),
                    ReelCount = table.Column<int>(nullable: false),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalStockRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternalStockRequest_Warehouse_ApprovingFacilityId",
                        column: x => x.ApprovingFacilityId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternalStockRequest_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternalStockRequest_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternalStockRequest_Warehouse_RequestingFacilityId",
                        column: x => x.RequestingFacilityId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternalStockRequest_Organization_ShipperId",
                        column: x => x.ShipperId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Number = table.Column<string>(maxLength: 50, nullable: false),
                    Status = table.Column<int>(nullable: false),
                    WarehouseId = table.Column<int>(nullable: true),
                    OrganizationId = table.Column<int>(nullable: true),
                    ShipperId = table.Column<int>(nullable: true),
                    TrackingId = table.Column<string>(nullable: true),
                    ExpectedDate = table.Column<DateTime>(nullable: true),
                    PaymentDueDate = table.Column<DateTime>(nullable: true),
                    DeliveryOtp = table.Column<int>(nullable: true),
                    DeliveryRemarks = table.Column<string>(maxLength: 50, nullable: true),
                    TotalCoils = table.Column<int>(nullable: false),
                    TotalStampsPrice = table.Column<decimal>(nullable: false),
                    TotalStamps = table.Column<decimal>(nullable: false),
                    ShippingCharges = table.Column<decimal>(nullable: false),
                    Tax = table.Column<decimal>(nullable: false),
                    TaxPercent = table.Column<decimal>(nullable: false),
                    TotalPrice = table.Column<decimal>(nullable: false),
                    CreditsApplied = table.Column<decimal>(nullable: false),
                    PayableAmount = table.Column<decimal>(nullable: false),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Order_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_Organization_ShipperId",
                        column: x => x.ShipperId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethod",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Provider = table.Column<string>(nullable: true),
                    ServiceName = table.Column<string>(nullable: true),
                    MethodName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethod", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrintOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PoNum = table.Column<string>(maxLength: 50, nullable: true),
                    Number = table.Column<string>(maxLength: 50, nullable: true),
                    ExpectedDate = table.Column<DateTime>(nullable: false),
                    ProductId = table.Column<int>(nullable: true),
                    ReelSizeId = table.Column<int>(nullable: true),
                    NoOfReels = table.Column<int>(nullable: false),
                    TotalStamps = table.Column<int>(nullable: false),
                    PrintPartnerId = table.Column<int>(nullable: true),
                    WarehouseId = table.Column<int>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrintOrder_Organization_PrintPartnerId",
                        column: x => x.PrintPartnerId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrintOrder_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrintOrder_ReelSize_ReelSizeId",
                        column: x => x.ReelSizeId,
                        principalTable: "ReelSize",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrintOrder_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReelChangeRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Number = table.Column<string>(maxLength: 50, nullable: true),
                    OrganizationId = table.Column<int>(nullable: true),
                    ReelChangeType = table.Column<int>(nullable: false),
                    ReelChangeProductId = table.Column<int>(nullable: true),
                    ChangeToProductId = table.Column<int>(nullable: true),
                    ChangeToSkuId = table.Column<int>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReelChangeRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReelChangeRequest_OrgBrandProduct_ChangeToProductId",
                        column: x => x.ChangeToProductId,
                        principalTable: "OrgBrandProduct",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReelChangeRequest_StockKeepingUnit_ChangeToSkuId",
                        column: x => x.ChangeToSkuId,
                        principalTable: "StockKeepingUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReelChangeRequest_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReelChangeRequest_Product_ReelChangeProductId",
                        column: x => x.ReelChangeProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReturnOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Number = table.Column<string>(maxLength: 50, nullable: false),
                    OrganizationId = table.Column<int>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    WarehouseId = table.Column<int>(nullable: true),
                    ShipperId = table.Column<int>(nullable: true),
                    TrackingId = table.Column<string>(maxLength: 50, nullable: true),
                    ExpectedDate = table.Column<DateTime>(nullable: true),
                    DeliveryOtp = table.Column<int>(nullable: true),
                    DeliveryRemarks = table.Column<string>(maxLength: 50, nullable: true),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnOrder_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnOrder_Organization_ShipperId",
                        column: x => x.ShipperId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnOrder_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pallet",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(nullable: true),
                    ContainerId = table.Column<Guid>(nullable: true),
                    CartonCount = table.Column<int>(nullable: false),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pallet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pallet_Container_ContainerId",
                        column: x => x.ContainerId,
                        principalTable: "Container",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InternalStockRequestHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Action = table.Column<int>(nullable: false),
                    Notes = table.Column<string>(maxLength: 1000, nullable: true),
                    InternalStockRequestId = table.Column<Guid>(nullable: true),
                    UserName = table.Column<string>(maxLength: 50, nullable: true),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalStockRequestHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternalStockRequestHistory_InternalStockRequest_InternalStockRequestId",
                        column: x => x.InternalStockRequestId,
                        principalTable: "InternalStockRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OrderId = table.Column<Guid>(nullable: true),
                    Action = table.Column<int>(nullable: false),
                    ActionedBy = table.Column<int>(nullable: false),
                    Comments = table.Column<string>(maxLength: 1000, nullable: true),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderHistory_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OrderId = table.Column<Guid>(nullable: true),
                    ProductId = table.Column<int>(nullable: true),
                    BrandProductId = table.Column<int>(nullable: true),
                    StockKeepingUnitId = table.Column<int>(nullable: true),
                    ReelSizeId = table.Column<int>(nullable: true),
                    SupplierId = table.Column<int>(nullable: true),
                    NoOfCoils = table.Column<int>(nullable: false),
                    NoOfStamps = table.Column<decimal>(nullable: false),
                    StampPrice = table.Column<decimal>(nullable: true),
                    TotalPrice = table.Column<decimal>(nullable: true),
                    UsedReelCount = table.Column<int>(nullable: false),
                    IsFulfilled = table.Column<bool>(nullable: false),
                    UsedCartonCount = table.Column<int>(nullable: false),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItem_OrgBrandProduct_BrandProductId",
                        column: x => x.BrandProductId,
                        principalTable: "OrgBrandProduct",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItem_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItem_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItem_ReelSize_ReelSizeId",
                        column: x => x.ReelSizeId,
                        principalTable: "ReelSize",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItem_StockKeepingUnit_StockKeepingUnitId",
                        column: x => x.StockKeepingUnitId,
                        principalTable: "StockKeepingUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItem_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PaymentMode = table.Column<int>(nullable: false),
                    PaymentStatus = table.Column<int>(nullable: false),
                    TransactionId = table.Column<string>(maxLength: 50, nullable: false),
                    PaymentInfo = table.Column<string>(nullable: true),
                    Amount = table.Column<decimal>(nullable: false),
                    PaymentDate = table.Column<DateTime>(nullable: true),
                    PaymentMethodId = table.Column<int>(nullable: true),
                    OrderId = table.Column<Guid>(nullable: false),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payment_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payment_PaymentMethod_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrintOrderHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PrintOrderId = table.Column<Guid>(nullable: true),
                    Action = table.Column<int>(nullable: false),
                    ActionedBy = table.Column<int>(nullable: false),
                    Comments = table.Column<string>(maxLength: 1000, nullable: true),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintOrderHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrintOrderHistory_PrintOrder_PrintOrderId",
                        column: x => x.PrintOrderId,
                        principalTable: "PrintOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReelChangeRequestHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ReelChangeRequestId = table.Column<Guid>(nullable: true),
                    Action = table.Column<int>(nullable: false),
                    ActionedBy = table.Column<int>(nullable: false),
                    Comments = table.Column<string>(maxLength: 500, nullable: true),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReelChangeRequestHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReelChangeRequestHistory_ReelChangeRequest_ReelChangeRequestId",
                        column: x => x.ReelChangeRequestId,
                        principalTable: "ReelChangeRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrgWallet",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OrganizationId = table.Column<int>(nullable: false),
                    TransactionType = table.Column<int>(nullable: false),
                    WalletOrderType = table.Column<int>(nullable: false),
                    TransactionAmount = table.Column<decimal>(nullable: false),
                    BalanceAmount = table.Column<decimal>(nullable: false),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    ReturnOrderId = table.Column<Guid>(nullable: true),
                    OrderId = table.Column<Guid>(nullable: true),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgWallet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrgWallet_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrgWallet_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrgWallet_ReturnOrder_ReturnOrderId",
                        column: x => x.ReturnOrderId,
                        principalTable: "ReturnOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReturnOrderHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ReturnOrderId = table.Column<Guid>(nullable: true),
                    Action = table.Column<int>(nullable: false),
                    ActionedBy = table.Column<int>(nullable: false),
                    Comments = table.Column<string>(maxLength: 500, nullable: true),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnOrderHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnOrderHistory_ReturnOrder_ReturnOrderId",
                        column: x => x.ReturnOrderId,
                        principalTable: "ReturnOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Carton",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(nullable: true),
                    ProductId = table.Column<int>(nullable: true),
                    ReelCount = table.Column<int>(nullable: false),
                    PalletId = table.Column<Guid>(nullable: false),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carton", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carton_Pallet_PalletId",
                        column: x => x.PalletId,
                        principalTable: "Pallet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Carton_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PaymentId = table.Column<Guid>(nullable: true),
                    Response = table.Column<string>(nullable: true),
                    Success = table.Column<bool>(nullable: false),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentHistory_Payment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reel",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(nullable: true),
                    StampCount = table.Column<int>(nullable: false),
                    PrintOrderId = table.Column<Guid>(nullable: false),
                    ProductId = table.Column<int>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    CartonId = table.Column<Guid>(nullable: false),
                    ReelSize = table.Column<int>(nullable: false),
                    IsUsedForFulfillment = table.Column<bool>(nullable: false),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reel_Carton_CartonId",
                        column: x => x.CartonId,
                        principalTable: "Carton",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reel_PrintOrder_PrintOrderId",
                        column: x => x.PrintOrderId,
                        principalTable: "PrintOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reel_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InternalStockRequestReel",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    InternalStockRequestId = table.Column<Guid>(nullable: true),
                    ReelId = table.Column<Guid>(nullable: true),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalStockRequestReel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternalStockRequestReel_InternalStockRequest_InternalStockRequestId",
                        column: x => x.InternalStockRequestId,
                        principalTable: "InternalStockRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternalStockRequestReel_Reel_ReelId",
                        column: x => x.ReelId,
                        principalTable: "Reel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemReel",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OrderItemId = table.Column<Guid>(nullable: true),
                    ReelId = table.Column<Guid>(nullable: true),
                    CartonId = table.Column<Guid>(nullable: true),
                    CartonCode = table.Column<string>(nullable: true),
                    IsReturned = table.Column<bool>(nullable: false),
                    ReelConsumptionType = table.Column<int>(nullable: false),
                    InternalStockRequestId = table.Column<Guid>(nullable: true),
                    ReturnOrderId = table.Column<Guid>(nullable: true),
                    PrintOrderId = table.Column<Guid>(nullable: true),
                    OrganizationId = table.Column<int>(nullable: true),
                    OrganizationName = table.Column<string>(maxLength: 50, nullable: true),
                    WarehouseId = table.Column<int>(nullable: true),
                    ProductId = table.Column<int>(nullable: true),
                    BrandProductId = table.Column<int>(nullable: true),
                    BrandProductName = table.Column<string>(maxLength: 50, nullable: true),
                    WarehouseName = table.Column<string>(maxLength: 50, nullable: true),
                    SkuId = table.Column<int>(nullable: true),
                    SkuName = table.Column<string>(maxLength: 50, nullable: true),
                    NewBrandProductId = table.Column<int>(nullable: true),
                    NewBrandProductName = table.Column<string>(maxLength: 50, nullable: true),
                    NewSkuId = table.Column<int>(nullable: true),
                    NewSkuName = table.Column<string>(maxLength: 50, nullable: true),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemReel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemReel_OrgBrandProduct_BrandProductId",
                        column: x => x.BrandProductId,
                        principalTable: "OrgBrandProduct",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemReel_Carton_CartonId",
                        column: x => x.CartonId,
                        principalTable: "Carton",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemReel_InternalStockRequest_InternalStockRequestId",
                        column: x => x.InternalStockRequestId,
                        principalTable: "InternalStockRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemReel_OrgBrandProduct_NewBrandProductId",
                        column: x => x.NewBrandProductId,
                        principalTable: "OrgBrandProduct",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemReel_StockKeepingUnit_NewSkuId",
                        column: x => x.NewSkuId,
                        principalTable: "StockKeepingUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemReel_OrderItem_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemReel_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemReel_PrintOrder_PrintOrderId",
                        column: x => x.PrintOrderId,
                        principalTable: "PrintOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemReel_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemReel_Reel_ReelId",
                        column: x => x.ReelId,
                        principalTable: "Reel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemReel_ReturnOrder_ReturnOrderId",
                        column: x => x.ReturnOrderId,
                        principalTable: "ReturnOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemReel_StockKeepingUnit_SkuId",
                        column: x => x.SkuId,
                        principalTable: "StockKeepingUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemReel_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReelChangeRequestReel",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ReelChangeRequestId = table.Column<Guid>(nullable: true),
                    ReelId = table.Column<Guid>(nullable: true),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReelChangeRequestReel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReelChangeRequestReel_ReelChangeRequest_ReelChangeRequestId",
                        column: x => x.ReelChangeRequestId,
                        principalTable: "ReelChangeRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReelChangeRequestReel_Reel_ReelId",
                        column: x => x.ReelId,
                        principalTable: "Reel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReturnOrderReels",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ReturnOrderId = table.Column<Guid>(nullable: true),
                    ReelId = table.Column<Guid>(nullable: true),
                    ReelCode = table.Column<string>(nullable: true),
                    CartonId = table.Column<Guid>(nullable: true),
                    CartonCode = table.Column<string>(nullable: true),
                    ProductId = table.Column<int>(nullable: true),
                    ProductName = table.Column<string>(nullable: true),
                    CreatedUser = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUser = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnOrderReels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnOrderReels_Carton_CartonId",
                        column: x => x.CartonId,
                        principalTable: "Carton",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnOrderReels_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnOrderReels_Reel_ReelId",
                        column: x => x.ReelId,
                        principalTable: "Reel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnOrderReels_ReturnOrder_ReturnOrderId",
                        column: x => x.ReturnOrderId,
                        principalTable: "ReturnOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Carton_PalletId",
                table: "Carton",
                column: "PalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Carton_ProductId",
                table: "Carton",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalStockRequest_ApprovingFacilityId",
                table: "InternalStockRequest",
                column: "ApprovingFacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalStockRequest_OrganizationId",
                table: "InternalStockRequest",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalStockRequest_ProductId",
                table: "InternalStockRequest",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalStockRequest_RequestingFacilityId",
                table: "InternalStockRequest",
                column: "RequestingFacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalStockRequest_ShipperId",
                table: "InternalStockRequest",
                column: "ShipperId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalStockRequestHistory_InternalStockRequestId",
                table: "InternalStockRequestHistory",
                column: "InternalStockRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalStockRequestReel_InternalStockRequestId",
                table: "InternalStockRequestReel",
                column: "InternalStockRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalStockRequestReel_ReelId",
                table: "InternalStockRequestReel",
                column: "ReelId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_OrganizationId",
                table: "Order",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_ShipperId",
                table: "Order",
                column: "ShipperId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_WarehouseId",
                table: "Order",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHistory_OrderId",
                table: "OrderHistory",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_BrandProductId",
                table: "OrderItem",
                column: "BrandProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItem",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_ProductId",
                table: "OrderItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_ReelSizeId",
                table: "OrderItem",
                column: "ReelSizeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_StockKeepingUnitId",
                table: "OrderItem",
                column: "StockKeepingUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_SupplierId",
                table: "OrderItem",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReel_BrandProductId",
                table: "OrderItemReel",
                column: "BrandProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReel_CartonId",
                table: "OrderItemReel",
                column: "CartonId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReel_InternalStockRequestId",
                table: "OrderItemReel",
                column: "InternalStockRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReel_NewBrandProductId",
                table: "OrderItemReel",
                column: "NewBrandProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReel_NewSkuId",
                table: "OrderItemReel",
                column: "NewSkuId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReel_OrderItemId",
                table: "OrderItemReel",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReel_OrganizationId",
                table: "OrderItemReel",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReel_PrintOrderId",
                table: "OrderItemReel",
                column: "PrintOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReel_ProductId",
                table: "OrderItemReel",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReel_ReelId",
                table: "OrderItemReel",
                column: "ReelId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReel_ReturnOrderId",
                table: "OrderItemReel",
                column: "ReturnOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReel_SkuId",
                table: "OrderItemReel",
                column: "SkuId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReel_WarehouseId",
                table: "OrderItemReel",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgWallet_OrderId",
                table: "OrgWallet",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgWallet_OrganizationId",
                table: "OrgWallet",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgWallet_ReturnOrderId",
                table: "OrgWallet",
                column: "ReturnOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Pallet_ContainerId",
                table: "Pallet",
                column: "ContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_OrderId",
                table: "Payment",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentMethodId",
                table: "Payment",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistory_PaymentId",
                table: "PaymentHistory",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintOrder_PrintPartnerId",
                table: "PrintOrder",
                column: "PrintPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintOrder_ProductId",
                table: "PrintOrder",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintOrder_ReelSizeId",
                table: "PrintOrder",
                column: "ReelSizeId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintOrder_WarehouseId",
                table: "PrintOrder",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintOrderHistory_PrintOrderId",
                table: "PrintOrderHistory",
                column: "PrintOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Reel_CartonId",
                table: "Reel",
                column: "CartonId");

            migrationBuilder.CreateIndex(
                name: "IX_Reel_PrintOrderId",
                table: "Reel",
                column: "PrintOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Reel_ProductId",
                table: "Reel",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReelChangeRequest_ChangeToProductId",
                table: "ReelChangeRequest",
                column: "ChangeToProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReelChangeRequest_ChangeToSkuId",
                table: "ReelChangeRequest",
                column: "ChangeToSkuId");

            migrationBuilder.CreateIndex(
                name: "IX_ReelChangeRequest_OrganizationId",
                table: "ReelChangeRequest",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReelChangeRequest_ReelChangeProductId",
                table: "ReelChangeRequest",
                column: "ReelChangeProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReelChangeRequestHistory_ReelChangeRequestId",
                table: "ReelChangeRequestHistory",
                column: "ReelChangeRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReelChangeRequestReel_ReelChangeRequestId",
                table: "ReelChangeRequestReel",
                column: "ReelChangeRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReelChangeRequestReel_ReelId",
                table: "ReelChangeRequestReel",
                column: "ReelId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnOrder_OrganizationId",
                table: "ReturnOrder",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnOrder_ShipperId",
                table: "ReturnOrder",
                column: "ShipperId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnOrder_WarehouseId",
                table: "ReturnOrder",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnOrderHistory_ReturnOrderId",
                table: "ReturnOrderHistory",
                column: "ReturnOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnOrderReels_CartonId",
                table: "ReturnOrderReels",
                column: "CartonId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnOrderReels_ProductId",
                table: "ReturnOrderReels",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnOrderReels_ReelId",
                table: "ReturnOrderReels",
                column: "ReelId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnOrderReels_ReturnOrderId",
                table: "ReturnOrderReels",
                column: "ReturnOrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InternalStockRequestHistory");

            migrationBuilder.DropTable(
                name: "InternalStockRequestReel");

            migrationBuilder.DropTable(
                name: "OrderHistory");

            migrationBuilder.DropTable(
                name: "OrderItemReel");

            migrationBuilder.DropTable(
                name: "OrgWallet");

            migrationBuilder.DropTable(
                name: "PaymentHistory");

            migrationBuilder.DropTable(
                name: "PrintOrderHistory");

            migrationBuilder.DropTable(
                name: "ReelChangeRequestHistory");

            migrationBuilder.DropTable(
                name: "ReelChangeRequestReel");

            migrationBuilder.DropTable(
                name: "ReturnOrderHistory");

            migrationBuilder.DropTable(
                name: "ReturnOrderReels");

            migrationBuilder.DropTable(
                name: "InternalStockRequest");

            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "ReelChangeRequest");

            migrationBuilder.DropTable(
                name: "Reel");

            migrationBuilder.DropTable(
                name: "ReturnOrder");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "PaymentMethod");

            migrationBuilder.DropTable(
                name: "Carton");

            migrationBuilder.DropTable(
                name: "PrintOrder");

            migrationBuilder.DropTable(
                name: "Pallet");

            migrationBuilder.DropTable(
                name: "Container");
        }
    }
}
