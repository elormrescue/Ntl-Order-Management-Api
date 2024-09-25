using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ntl.AuthSS.OrderManagement.Data;
using Ntl.AuthSS.OrderManagement.Data.Entities;
using Ntl.AuthSS.OrderManagement.Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Ntl.AuthSS.OrderManagement.Business.FileDtos;
using Microsoft.Data.SqlClient;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderManagementDbContext _orderManagementDbContext;
        private readonly IOrderMetaService _orderMetaService;
        private readonly MetaDataQueueService _metaDataQueueService;
        private readonly IConfiguration _configuration;
        public OrderService(OrderManagementDbContext orderManagementDbContext, IOrderMetaService orderMetaService, IConfiguration configuration)
        {
            _orderManagementDbContext = orderManagementDbContext;
            _orderMetaService = orderMetaService;
            _configuration = configuration;
            _metaDataQueueService = new MetaDataQueueService(_configuration.GetConnectionString("StorageAccount"), _configuration.GetValue<string>("StorageAccount:MetaDataUpdateQueue"));
        }
        public async Task<Order> SaveOrderAsync(OrderDto orderDto, int orgId, OrderEntityType orderEntityType)
        {
            var order = await _orderManagementDbContext.Orders.Include(x => x.OrderItems).Include(x => x.OrderPaymentBreakdowns).SingleOrDefaultAsync(x => x.Id == orderDto.Id) ?? await AddGetOrder(orderEntityType, orgId);

            if (orderDto.Id != Guid.Empty && order.Status != OrderStatus.Rejected)
                throw new Exception("We don't consider order resubmission unless order was rejected");

            order.ShippingCharges = _orderMetaService.GetOrderShippingCharges();
            order.Warehouse = _orderManagementDbContext.Warehouses.Include(w => w.Contact).Where(w => w.Id == orderDto.WarehouseId).SingleOrDefault();
            //order.Organization = _orderManagementDbContext.Organization.Include(o => o.contact).Where(w => w.Id == orderDto.OrgId).SingleOrDefault();
            order.Organization = _orderManagementDbContext.Organization.Include(o => o.Contact).Where(w => w.Id == orgId).SingleOrDefault();
            order.TotalStamps = GetTotalStamps(orderDto.OrderItems, orderEntityType);
            order.TotalCoils = orderDto.OrderItems.Sum(x => x.NoOfCoils);
            //order.TotalStampsPrice = orderEntityType == OrderEntityType.Manufacturer ? GetTotalStampsPrice(orderDto.OrderItems, orderEntityType) : 0;


            order.TotalStampsPrice = GetTotalStampsPrice(orderDto.OrderItems, orderEntityType);
            order.Tax = (order.TotalStampsPrice * _orderMetaService.GetOrderTax()) / 100;
            order.TotalPrice = order.TotalStampsPrice; //subtotal without any tax /*+ order.Tax + order.ShippingCharges;*/
            var credits = _orderMetaService.orgBalance(orgId);
            order.CreditsApplied = credits >= order.TotalPrice ? order.TotalPrice : credits;

            decimal? taxAmount = 0;

            if (order.OrderPaymentBreakdowns == null)
                order.OrderPaymentBreakdowns = new List<OrderPaymentBreakdown>();

            var existingPaymentBreakdownIds = order.OrderPaymentBreakdowns.Select(x => x.Id).ToArray();
            var selectedPaymentBreakdownIds = orderDto.OrderPaymentBreakdowns.Where(x => x.Id != 0).Select(x => x.Id).ToArray();

            var paymentBreakdownToBeDeleted = _orderManagementDbContext.OrderPaymentBreakdowns.Where(x => (existingPaymentBreakdownIds.Except(selectedPaymentBreakdownIds)).Contains(x.Id)).ToList();
            var paymentBreakdownToBeAddedOrUpdated = orderDto.OrderPaymentBreakdowns.Where(x => x.Id == 0 || existingPaymentBreakdownIds.Contains(x.Id));

            _orderManagementDbContext.OrderPaymentBreakdowns.RemoveRange(paymentBreakdownToBeDeleted);


            foreach (var slabs in paymentBreakdownToBeAddedOrUpdated)
            {
                taxAmount += slabs.IsTax ? slabs.SlabAmount : 0;
                var orderbreakdown = new OrderPaymentBreakdown();
                orderbreakdown.Order = order;
                orderbreakdown.ItemPriceType = slabs.Type;
                orderbreakdown.Percentage = slabs.Percentage;
                orderbreakdown.IsTax = slabs.IsTax;
                orderbreakdown.Amount = slabs.SlabAmount;
                orderbreakdown.IsCumulative = slabs.IsCumulative;
                //if(order.Status != OrderStatus.Rejected)
                order.OrderPaymentBreakdowns.Add(orderbreakdown);
            }

            order.PayableAmount = Math.Round(((order.TotalPrice + taxAmount) - order.CreditsApplied) ?? 0, 2);

            var orgWallet = new OrgWallet();
            orgWallet.BalanceAmount = credits - order.CreditsApplied;

            orgWallet.Organization = order.Organization;
            orgWallet.TransactionType = TransactionType.Debit;
            orgWallet.TransactionAmount = order.TotalPrice;
            orgWallet.Order = order;
            orgWallet.WalletOrderType = WalletOrderType.RegularOrder;


            if (order.Status == OrderStatus.Rejected)
                order.Status = OrderStatus.Resubmitted;

            order.OrderHistories ??= new List<OrderHistory>();
            order.OrderHistories.Add(new OrderHistory { Action = order.Status, ActionedBy = GetActionedBy(orderEntityType), Comments = orderDto.Notes });


            if (order.OrderItems == null)
                order.OrderItems = new List<OrderItem>();

            var existingOrderItemIds = order.OrderItems.Select(x => x.Id).ToArray();
            var selectedOrderItemIds = orderDto.OrderItems.Where(x => x.Id != Guid.Empty).Select(x => x.Id).ToArray();

            var orderItemsToBeDeleted = _orderManagementDbContext.OrderItems.Where(x => (existingOrderItemIds.Except(selectedOrderItemIds)).Contains(x.Id)).ToList();
            var orderItemsToBeAddedOrUpdated = orderDto.OrderItems.Where(x => x.Id == Guid.Empty || existingOrderItemIds.Contains(x.Id));

            _orderManagementDbContext.OrderItems.RemoveRange(orderItemsToBeDeleted);

            foreach (var addUpdateOrderItem in orderItemsToBeAddedOrUpdated)
            {
                var orderItem = addUpdateOrderItem.Id == Guid.Empty ? AddGetOrderItem(order) : order.OrderItems.Single(x => x.Id == addUpdateOrderItem.Id);
                orderItem.BrandProduct = _orderManagementDbContext.OrgBrandProducts.Find(addUpdateOrderItem.BrandProductId);
                orderItem.NoOfCoils = addUpdateOrderItem.NoOfCoils;
                orderItem.NoOfStamps = addUpdateOrderItem.NoOfCoils * GetReelSize(addUpdateOrderItem.ReelSizeId, orderEntityType, addUpdateOrderItem.ProductId);
                orderItem.Product = _orderManagementDbContext.Products.Find(addUpdateOrderItem.ProductId);
                orderItem.ReelSize = _orderManagementDbContext.ReelSizes.Find(addUpdateOrderItem.ReelSizeId);
                orderItem.StampPrice = _orderManagementDbContext.ProductPriceHistories.SingleOrDefault(x => x.Product.Id == addUpdateOrderItem.ProductId && x.Status == PriceStatus.Active)?.Price;
                orderItem.StockKeepingUnit = _orderManagementDbContext.StockKeepingUnits.Find(addUpdateOrderItem.StockKeepingUnitId);
                orderItem.Supplier = _orderManagementDbContext.Suppliers.Find(addUpdateOrderItem.SupplierId);
                orderItem.TotalPrice = addUpdateOrderItem.NoOfCoils * GetReelSize(addUpdateOrderItem.ReelSizeId, orderEntityType, addUpdateOrderItem.ProductId) * GetStampPrice(addUpdateOrderItem.ProductId);
            }

            await _orderManagementDbContext.OrgWallets.AddAsync(orgWallet);
            await _orderManagementDbContext.SaveChangesAsync();
            return order;

        }


        private OrderItem AddGetOrderItem(Order order)
        {
            var orderItem = new OrderItem();
            order.OrderItems.Add(orderItem);
            return orderItem;
        }
        private async Task<Order> AddGetOrder(OrderEntityType orderEntityType, int entityId)
        {
            var order = new Order();
            order.Number = orderEntityType.GenerateOrderNumber();
            order.Organization = _orderManagementDbContext.Organization.Find(entityId);
            order.Status = OrderStatus.Submitted;

            await _orderManagementDbContext.Orders.AddAsync(order);
            return order;
        }

        private OrgType GetActionedBy(OrderEntityType? orderEntityType)
        {
            switch (orderEntityType)
            {
                case OrderEntityType.Manufacturer:
                    return OrgType.Manufacturer;
                case OrderEntityType.Ntl:
                    return OrgType.Ntl;
                case OrderEntityType.Tpsaf:
                    return OrgType.Tpsaf;
                case OrderEntityType.TaxAuth:
                    return OrgType.TaxAuthority;
                default: throw new Exception("Cannot match Org type for Order entity");
            }
        }


        private decimal GetStampPrice(int productId)
        {
            var productPrice = _orderManagementDbContext.ProductPriceHistories.SingleOrDefault(x => x.Product.Id == productId && x.Status == PriceStatus.Active);
            if (productPrice == null)
                throw new Exception("Cannot create the order for the product which doesn't have associted price");
            return productPrice.Price;
        }

        private int GetReelSize(int reelSizeId, OrderEntityType orderEntityType, int productId)
        {
            var productReelSizes = _orderManagementDbContext.ProductReelSizes.Where(x => x.Product.Id == productId).AsQueryable();
            switch (orderEntityType)
            {
                case OrderEntityType.Manufacturer:
                    productReelSizes = productReelSizes.Where(x => x.CanManufacturerOrder);
                    break;
                case OrderEntityType.Tpsaf:
                    productReelSizes = productReelSizes.Where(x => x.CanTpsafOrder);
                    break;
            }

            var productReelSize = productReelSizes.Include(x => x.ReelSize).SingleOrDefault(x => x.ReelSize.Id == reelSizeId && x.IsActive);
            if (productReelSize == null)
                throw new Exception("Cannot create the order for the product which doesn't have associated reel size");

            return productReelSize.ReelSize.Size;
        }

        private decimal GetTotalStampsPrice(IEnumerable<OrderItemDto> orderItems, OrderEntityType orderEntityType)
        {
            decimal totalPrice = 0;
            foreach (var item in orderItems)
            {
                totalPrice = totalPrice + (item.NoOfCoils * GetReelSize(item.ReelSizeId, orderEntityType, item.ProductId) * GetStampPrice(item.ProductId));
            }

            return totalPrice;
        }

        private int GetTotalStamps(IEnumerable<OrderItemDto> orderItems, OrderEntityType orderEntityType)
        {
            int totalStamps = 0;
            foreach (var item in orderItems)
            {
                totalStamps = totalStamps + (item.NoOfCoils * GetReelSize(item.ReelSizeId, orderEntityType, item.ProductId));
            }

            return totalStamps;
        }

        public async Task<OrderDto> GetOrderAsync(Guid orderId)
        {
            var order = await _orderManagementDbContext.Orders
                                                 .Include(x => x.Organization).Include(o=>o.Organization.Contact).Include(x=>x.Organization.Address)
                                                 .Include(x => x.OrderItems).ThenInclude(x => x.Product)
                                                 .Include(x => x.OrderItems).ThenInclude(x => x.BrandProduct)
                                                 .Include(x => x.OrderItems).ThenInclude(x => x.ReelSize)
                                                 .Include(x => x.OrderItems).ThenInclude(x => x.Supplier)
                                                 .Include(x => x.OrderItems).ThenInclude(x => x.StockKeepingUnit)
                                                 .Include(x => x.Warehouse)
                                                 .Include(x => x.Payment)
                                                 .Include(x => x.OrderHistories)
                                                 .Include(x => x.OrderPaymentBreakdowns)
                                                 .Include(x=>x.Shipper)
                                                 .SingleAsync(x => x.Id == orderId);
            var orderDto = new OrderDto();
            orderDto.Id = order.Id;
            orderDto.OrderNumber = order.Number;
            orderDto.ShippingCharges = order.ShippingCharges;
            orderDto.Tax = order.Tax;
            orderDto.TotalCoils = order.TotalCoils;
            orderDto.TotalStampsPrice = order.TotalStampsPrice;
            orderDto.CreditsApplied = order.CreditsApplied;
            orderDto.PayableAmount = order.PayableAmount;
            orderDto.TotalPrice = order.TotalPrice;
            orderDto.TotalStamps = order.TotalStamps;
            orderDto.WarehouseId = order.Warehouse.Id;
            orderDto.WarehouseName = order.Warehouse.Name;
            orderDto.StatusString = order.Status.ToString();
            orderDto.OrgId = order.Organization.Id;
            orderDto.OrgType = (int)order.Organization.OrgType;
            orderDto.OrgName = order.Organization.Name;
            orderDto.Organization = order.Organization;

            orderDto.Shipper = order.Shipper;
            orderDto.ExpectedDate = order.ExpectedDate;
            orderDto.CreatedDate = order.CreatedDate;
            orderDto.ModifiedDate = order.ModifiedDate;

            orderDto.OrderItems = new List<OrderItemDto>();

            foreach (var orderItem in order.OrderItems)
            {
                var orderItemDto = new OrderItemDto();
                orderItemDto.Id = orderItem.Id;
                orderItemDto.NoOfCoils = orderItem.NoOfCoils;
                orderItemDto.NoOfStamps = orderItem.NoOfStamps;
                orderItemDto.ProductId = orderItem.Product.Id;
                orderItemDto.ProductName = $"{orderItem.Product.Name} {orderItem.Product.Origin}";
                orderItemDto.ImageName = orderItem.Product.ImageName;
                orderItemDto.BrandProductId = orderItem.BrandProduct?.Id;
                orderItemDto.BrandProductName = orderItem.BrandProduct?.Name;
                orderItemDto.ReelSizeId = orderItem.ReelSize.Id;
                orderItemDto.ReelSize = orderItem.ReelSize.Size.ToString();
                orderItemDto.StockKeepingUnitId = orderItem.StockKeepingUnit?.Id;
                orderItemDto.StockKeepingUnitName = orderItem.StockKeepingUnit?.Unit;
                orderItemDto.SupplierId = orderItem.Supplier?.Id;
                orderItemDto.SupplierName = orderItem.Supplier?.Name;
                orderItemDto.StampPrice = orderItem.StampPrice;
                orderItemDto.TotalPrice = orderItem.TotalPrice;
                orderItemDto.IsFulfilled = orderItem.IsFulfilled;

                var resultPackages = await FetchPackagesOfOrderItemId(orderItem.Id.ToString(), orderItem);
                if (orderItem.IsFulfilled)
                {
                    orderItemDto.UsedReelCount = orderItem.UsedReelCount;
                    orderItemDto.UsedCartonCount = orderItem.UsedCartonCount;
                    orderItemDto.FullfilledPackages = resultPackages.packageDtos;
                }
                if (resultPackages.unFullfilledPackages != null)
                    orderItemDto.UnFullfilledPackages = resultPackages.unFullfilledPackages;
                orderDto.OrderItems.Add(orderItemDto);
            }

            var paymentDto = new PaymentDto();
            paymentDto.Id = order.Payment?.Id;
            paymentDto.TransactionId = order.Payment?.TransactionId ?? "NA";
            paymentDto.PaymentStatus = order.Payment?.PaymentStatus;
            paymentDto.StatusString = order.Payment?.PaymentStatus == null ? "NA" : order.Payment?.PaymentStatus.ToString();
            orderDto.Payment = paymentDto;
            orderDto.OrderHistories = new List<OrderHistoryDto>();
            foreach (var orderHistory in order.OrderHistories.OrderByDescending(x => x.ModifiedDate))
            {
                var orderHistoryDto = new OrderHistoryDto();
                orderHistoryDto.Id = orderHistory.Id;
                orderHistoryDto.OrderStatus = orderHistory.Action;
                orderHistoryDto.StatusString = orderHistory.Action.ToString();
                orderHistoryDto.ActionedBy = orderHistory.ActionedBy.ToString();
                orderHistoryDto.Comments = orderHistory.Comments;
                orderHistoryDto.CreateDate = orderHistory.CreatedDate;
                orderHistoryDto.CreatedDateString = orderHistory.CreatedDate.ToDisplayDate();

                orderDto.OrderHistories.Add(orderHistoryDto);
            }

            //new feature w.r.t taxes
            orderDto.OrderPaymentBreakdowns = new List<OrderPaymentBreakdownDto>();
            foreach (var breakdown in order.OrderPaymentBreakdowns)
            {
                var item = new OrderPaymentBreakdownDto();
                item.Id = breakdown.Id;
                item.Type = breakdown.ItemPriceType;
                item.IsTax = breakdown.IsTax;
                item.Percentage = breakdown.Percentage;
                item.SlabAmount = breakdown.Amount;
                item.IsCumulative = breakdown.IsCumulative;

                orderDto.OrderPaymentBreakdowns.Add(item);
            }

            return orderDto;
        }
        public async Task<OrderListingDto> GetOrders(OrderSearchFilterOptions filterOptions)
        {
            var orders = GetFilteredOrders(filterOptions);
            var allCount = orders.Count();
            var inConsiderationOrdersCount = orders.Count(x => x.Status == OrderStatus.InConsideration);
            var rejectedOrdersCount = orders.Count(x => x.Status == OrderStatus.Rejected);
            var submittedOrdersCount = orders.Count(x => x.Status == OrderStatus.Submitted);
            var resubmittedOrdersCount = orders.Count(x => x.Status == OrderStatus.Resubmitted);
            var fullfilledOrdersCount = orders.Count(x => x.Status == OrderStatus.Fullfilled);
            var inTransitOrdersCount = orders.Count(x => x.Status == OrderStatus.InTransit);
            var deliveredOrderCount = orders.Count(x => x.Status == OrderStatus.Closed);

            //Add any more status like payment status.

            orders = GetStatusFilteredOrders(orders, filterOptions);

            var totalRows = orders.Count();

            filterOptions.PageNo = filterOptions.PageNo == 0 ? 1 : filterOptions.PageNo;
            filterOptions.PageSize = filterOptions.PageSize == 0 ? 10 : filterOptions.PageSize;
            orders = orders.Skip((filterOptions.PageNo - 1) * filterOptions.PageSize).Take(filterOptions.PageSize);

            var materialisedOrders = await orders
                .Select(x => new
                {
                    OrderId = x.Id,
                    OrderNumber = x.Number,
                    OrgName = x.Organization.Name,
                    RequestedOn = x.ModifiedDate,
                    Status = x.Status,
                    PaymentStatus = x.Payment.PaymentStatus,
                    WarehouseName = x.Warehouse.Name,
                    IsImporter = x.Organization.IsImporter,
                    orgId = x.Organization.Id,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate
                }).ToListAsync();

            var miniOrderDtoList = new List<MiniOrderDto>();
            foreach (var materialisedOrder in materialisedOrders)
            {
                var miniOrderDto = new MiniOrderDto();
                miniOrderDto.OrderId = materialisedOrder.OrderId;
                miniOrderDto.OrderNumber = materialisedOrder.OrderNumber;
                miniOrderDto.OrgName = materialisedOrder.OrgName;
                miniOrderDto.PaymentStatus = materialisedOrder.PaymentStatus == 0 ? "NA" : materialisedOrder.PaymentStatus.ToString();
                miniOrderDto.RequestedOn = materialisedOrder.RequestedOn.ToDisplayDate();
                miniOrderDto.Status = materialisedOrder.Status.ToString();
                miniOrderDto.Location = materialisedOrder.WarehouseName;
                miniOrderDto.IsImporter = materialisedOrder.IsImporter;
                miniOrderDto.ManufactureHasDomesticProducts = getDomesticProducts(materialisedOrder.orgId);
                miniOrderDto.DueDays = materialisedOrder.PaymentStatus == 0 || materialisedOrder.PaymentStatus == PaymentStatus.Paid ? 0 : (DateTime.Now - materialisedOrder.ModifiedDate).Days > 30 ? (DateTime.Now - materialisedOrder.ModifiedDate).Days : 0;
                miniOrderDtoList.Add(miniOrderDto);
            }

            var orderlistDto = new OrderListingDto();
            orderlistDto.MiniOrderDtos = miniOrderDtoList;
            orderlistDto.TotalCount = allCount;
            orderlistDto.InConsiderationCount = inConsiderationOrdersCount;
            orderlistDto.RejectedCount = rejectedOrdersCount;
            orderlistDto.SubmittedCount = submittedOrdersCount;
            orderlistDto.ResubmittedCount = resubmittedOrdersCount;
            orderlistDto.FullFilledCount = fullfilledOrdersCount;
            orderlistDto.InTransitCount = inTransitOrdersCount;
            orderlistDto.DeliveredCount = deliveredOrderCount;
            orderlistDto.TotalRows = totalRows;

            return orderlistDto;

        }

        public async Task<(int orgId, string ordNumber, OrgType? orgType, string orgName, string ordWareHouseEmail, int wareHouseId)> ApproveOrder(Guid orderId, string comments, OrderEntityType? orderEntityType)
        {
            var order = await _orderManagementDbContext.Orders.Include(o => o.OrderItems).ThenInclude(s => s.StockKeepingUnit).Include(o => o.OrderItems).ThenInclude(p => p.Product).Include(o => o.Organization).Include(w => w.Warehouse).ThenInclude(x => x.Address).ThenInclude(c => c.Country).Include(w => w.Warehouse.Contact).SingleOrDefaultAsync(x => x.Id == orderId);
            order.Status = OrderStatus.InConsideration;
            if (order.TotalPrice == 0)
            {
                //implement payment logic and status to paid
            }
            var orderHistory = new OrderHistory();
            orderHistory.Order = order;
            orderHistory.Action = OrderStatus.InConsideration;
            orderHistory.ActionedBy = GetActionedBy(orderEntityType);
            orderHistory.Comments = comments;

            _orderManagementDbContext.OrderHistories.Add(orderHistory);
            await ProcessInvoiceDataForFileCreationQueue(order);
            await ProcessPayment(order);
            await _orderManagementDbContext.SaveChangesAsync();
            return (order.Organization.Id, order.Number, order.Organization.OrgType, order.Organization.Name, order.Warehouse.Contact.Email, order.Warehouse.Id);
        }

        public async Task ProcessInvoiceDataForFileCreationQueue(Order order)
        {
            InvoiceFileDto invoiceFileDto = new InvoiceFileDto();
            invoiceFileDto.Address = order.Warehouse.Address.Description;

            invoiceFileDto.Country = order.Warehouse.Address.Country.Name;
            //invoiceFileDto.DueDate = DateTime.Now.Date.AddDays(Convert.ToInt32(_configuration.GetConnectionString("PaymentDueDays")));
            invoiceFileDto.InvoiceDate = DateTime.Now.Date;
            invoiceFileDto.DueDate = invoiceFileDto.InvoiceDate.AddDays(30);
            invoiceFileDto.InvoiceNumber = order.Number;
            invoiceFileDto.Organization = order.Organization.Name;
            invoiceFileDto.PostalCode = order.Warehouse.Address.PostalAddress;
            invoiceFileDto.StampTotalPrice = order.TotalStampsPrice;
            invoiceFileDto.TinNumber = order.Organization.TinNumber;
            invoiceFileDto.IsOrgImporter = order.Organization.IsImporter;
            var orderItemList = new List<OrderItemFileDto>();

            foreach (var item in order.OrderItems)
            {
                OrderItemFileDto orderItemFileDto = new OrderItemFileDto();
                orderItemFileDto.ProductDescription = item.Product.Name.ToString() + "-" + item.Product.Origin.ToString() + "-" + (int)(item.NoOfStamps / item.NoOfCoils) + "-" + item.StockKeepingUnit?.Description;
                orderItemFileDto.Quantity = item.NoOfCoils;
                orderItemFileDto.StampPrice = item.StampPrice;
                orderItemFileDto.TotalPrice = item.TotalPrice;
                orderItemFileDto.TotalStamps = item.NoOfStamps;
                orderItemList.Add(orderItemFileDto);
            }
            invoiceFileDto.OrderItems = orderItemList;


            //new implementation for taxes and orderbreakdown feature
            var breakDownItems = new List<OrderPaymentBreakdownDto>();
            var orderBreakdown = _orderManagementDbContext.OrderPaymentBreakdowns.Where(x => x.Order.Id == order.Id).ToList();

            var isBreakDownData = orderBreakdown.Count() > 0 ? true : false;

            //if (!isBreakDownData)
            //{
            decimal totalNonCumulativeAmount = 0;
            var slabs = _orderMetaService.TaxSlabs(1); //1 status = active
            foreach (var slab in slabs)
            {
                if (!slab.IsCumulative)
                {
                    var item = new OrderPaymentBreakdownDto();
                    item.Type = slab.TaxType;
                    item.Percentage = slab.Percentage;
                    item.IsCumulative = slab.IsCumulative;
                    item.IsTax = true;
                    item.SlabAmount = Math.Round((slab.Percentage * order.TotalPrice / 100) ?? 0, 2);
                    totalNonCumulativeAmount += item.SlabAmount ?? 0;

                    breakDownItems.Add(item);

                }
            }

            invoiceFileDto.TotalAfterTaxes = Math.Round((order.TotalPrice + totalNonCumulativeAmount), 2);

            decimal totalCumulativeAmount = 0;
            foreach (var slab in slabs)
            {
                if (slab.IsCumulative)
                {
                    var item = new OrderPaymentBreakdownDto();
                    item.Type = slab.TaxType;
                    item.Percentage = slab.Percentage;
                    item.IsCumulative = slab.IsCumulative;
                    item.IsTax = true;
                    item.SlabAmount = Math.Round((slab.Percentage * invoiceFileDto.TotalAfterTaxes / 100) ?? 0, 2);
                    totalCumulativeAmount += item.SlabAmount ?? 0;

                    breakDownItems.Add(item);

                }
            }
            invoiceFileDto.TotalAfterCumulativeTax = Math.Round((invoiceFileDto.TotalAfterTaxes + totalCumulativeAmount), 2);
            //}

            invoiceFileDto.BreakdownDtos = breakDownItems;

            if (!isBreakDownData)
            {
                invoiceFileDto.AmountDue = Math.Round((invoiceFileDto.TotalAfterCumulativeTax - order.CreditsApplied), 2);
                order.OrderPaymentBreakdowns = new List<OrderPaymentBreakdown>();
                foreach (var item in invoiceFileDto.BreakdownDtos)
                {
                    var data = new OrderPaymentBreakdown();
                    data.Order = order;
                    data.ItemPriceType = item.Type;
                    data.Amount = item.SlabAmount;
                    data.Percentage = item.Percentage;
                    data.IsTax = item.IsTax;
                    data.IsCumulative = item.IsCumulative;
                    order.OrderPaymentBreakdowns.Add(data);
                }
                var subTotal = new OrderPaymentBreakdown { ItemPriceType = "Sub Total", IsTax = false, IsCumulative = false, Amount = order.TotalStampsPrice, Percentage = 0 };
                order.OrderPaymentBreakdowns.Add(subTotal);
                order.PayableAmount = invoiceFileDto.AmountDue;
                await _orderManagementDbContext.SaveChangesAsync();
            }
            else
            {
                invoiceFileDto.AmountDue = order.PayableAmount;
            }

            invoiceFileDto.OrderCreditAdded = order.CreditsApplied;
            invoiceFileDto.ShippingCharges = order.ShippingCharges;
            invoiceFileDto.TaxAmount = (decimal)order.Tax;

            FileDetailsDto fileDetailsDto = new FileDetailsDto();
            fileDetailsDto.FileDataDto = invoiceFileDto;
            fileDetailsDto.ContentModelType = invoiceFileDto.GetType().Name;
            await Utility.SendToQueue(fileDetailsDto, _configuration.GetConnectionString("StorageConnectionString"), _configuration.GetValue<string>("FileProcessingQueue"));
        }

        private async Task ProcessPayment(Order order)
        {
            var payment = new Payment();
            payment.PaymentMode = order.Organization.IsImporter ? PaymentMode.Online : PaymentMode.Offline;
            payment.TransactionId = order.Organization.IsImporter ? "0" : DateTime.Now.Ticks.ToString();
            payment.Amount = order.PayableAmount;
            var transOrder = order.OrderItems.Where(x => x.Product.Origin == Origin.Transition).Count();
            payment.PaymentStatus = transOrder > 0 ? PaymentStatus.Paid : order.PayableAmount > 0 ? PaymentStatus.Unpaid : PaymentStatus.Paid;
            //payment.PaymentStatus = order.Organization.IsImporter ? PaymentStatus.Unpaid : PaymentStatus.Paid;
            payment.PaymentDate = DateTime.Now;
            payment.OrderId = order.Id;
            payment.Order = order;
            await _orderManagementDbContext.Payments.AddAsync(payment);
        }

        public async Task<(int orgId, string ordNumber, string orgName, OrgType? orgType, string ordWareHouseEmail, int userId, int wareHouseId)> RejectOrder(Guid orderId, string comments, OrderEntityType? orderEntityType)
        {
            var order = await _orderManagementDbContext.Orders.Include(x => x.Organization).Include(x => x.Warehouse).Include(x => x.Warehouse.Contact).SingleOrDefaultAsync(x => x.Id == orderId);
            order.Status = OrderStatus.Rejected;

            var orderHistory = new OrderHistory();
            orderHistory.Order = order;
            orderHistory.Action = OrderStatus.Rejected;
            orderHistory.ActionedBy = GetActionedBy(orderEntityType);
            orderHistory.Comments = comments;

            var orgWallet = new OrgWallet();
            orgWallet.TransactionAmount = order.CreditsApplied;
            orgWallet.TransactionType = TransactionType.Credit;
            var credits = _orderMetaService.orgBalance(order.Organization.Id);
            orgWallet.BalanceAmount = credits + order.CreditsApplied;
            orgWallet.Order = order;
            orgWallet.Organization = order.Organization;
            orgWallet.WalletOrderType = WalletOrderType.AddMoney;
            orgWallet.Description = "Rejected";


            _orderManagementDbContext.OrderHistories.Add(orderHistory);
            await _orderManagementDbContext.OrgWallets.AddAsync(orgWallet);

            await _orderManagementDbContext.SaveChangesAsync();
            return (order.Organization.Id, order.Number, order.Organization.Name, order.Organization.OrgType, order.Warehouse.Contact.Email, order.CreatedUser, order.Warehouse.Id);

        }

        public async Task<(OrderDto orderDto, string errorMessage)> FullFillOrderItem(FullFillOrderDto fullFillOrderDto, OrderEntityType? orderEntityType)
        {
            var orderReels = new List<Reel>();

            var order = await _orderManagementDbContext.Orders
                .Include(o => o.Organization)
                .Include(w => w.Warehouse).ThenInclude(a => a.Address).ThenInclude(c => c.Country)
                .Include(x => x.OrderItems).ThenInclude(x => x.ReelSize)
                .Include(x => x.OrderItems).ThenInclude(p => p.Product)
                .Include(x => x.OrderItems).ThenInclude(p => p.StockKeepingUnit)
                .Include(x => x.OrderItems).ThenInclude(p => p.BrandProduct)
                .SingleOrDefaultAsync(y => y.Id == fullFillOrderDto.OrderId);

            if (order == null)
                return (null, "Ordernumber does not exists");

            foreach (var fullFillOrderItemDto in fullFillOrderDto.fullFillOrderItemDtos)
            {

                var orderItem = order.OrderItems.Where(x => x.Id == fullFillOrderItemDto.OrderItemId).FirstOrDefault();
                var orderItemLevelData = await GetReelsUsedAndFullFillmentData(fullFillOrderItemDto, order, orderItem);
                orderItem.UsedCartonCount = orderItemLevelData.UsedCartonCount;
                orderItem.UsedReelCount = orderItemLevelData.UsedReelCount;
                orderItem.IsFulfilled = true;

                var notMatchingProductCount = orderItemLevelData.Reels.Where(z => z.Product.Id != orderItem.Product.Id).Any();
                var notMatchingReelSizeCount = orderItemLevelData.Reels.Where(z => z.ReelSize != orderItem.ReelSize.Size).Any();
                if (orderItemLevelData.Reels.Sum(z => z.ReelSize) != orderItem.NoOfStamps)
                {
                    return (null, "Quantity of stamps required does not match with fulfilled quantity");
                }
                if (notMatchingProductCount)
                {
                    return (null, "Product type of order item and added reels differ");
                }
                if (notMatchingReelSizeCount)
                {
                    return (null, "Reel size of order item and added Reels differ");
                }

                foreach (var reel in orderItemLevelData.Reels)
                {
                    reel.IsUsedForFulfillment = true;
                    reel.FulfillLockedDate = null;
                    reel.LockedOrderItemId = null;
                }
                orderReels.AddRange(orderItemLevelData.Reels);

                //await CreateOrderItemReelAsync(order, orderItemLevelData.Reels, orderItem);
            }
            var duplicateCodesPresent = orderReels.GroupBy(x => x.Code).Any(g => g.Count() > 1);
            if (duplicateCodesPresent)
                return (null, "Some reels are used multiple times");

            order.ShipperId = fullFillOrderDto.shipper.ShipperId;
            order.TrackingId = fullFillOrderDto.TrackingId;
            order.ExpectedDate = fullFillOrderDto.ExpectedDeliveryDate;

            order.Status = OrderStatus.Fullfilled;
            var orderHistory = new OrderHistory();
            orderHistory.Order = order;
            orderHistory.Action = OrderStatus.Fullfilled;
            orderHistory.ActionedBy = GetActionedBy(orderEntityType);
            orderHistory.Comments = fullFillOrderDto.Comment;
            _orderManagementDbContext.OrderHistories.Add(orderHistory);

            await _orderManagementDbContext.SaveChangesAsync();
            await ProcessFullfillDataForFileCreationQueue(fullFillOrderDto, order);
            var orderDto = new OrderDto();
            orderDto.OrgName = order.Organization.Name;
            orderDto.OrderNumber = order.Number;
            orderDto.OrgId = order.Organization.Id;
            orderDto.WarehouseId = order.Warehouse.Id;
            orderDto.OrgType = (int)order.Organization.OrgType;
            var metaDataQueueModel = new MetaDataQueueModel();
            metaDataQueueModel.Id = order.Id;
            metaDataQueueModel.EventName = MetaDataUpdateEvent.OrderFulfillment;
            await _metaDataQueueService.AddMsgToQueue(metaDataQueueModel);
            return (orderDto, null);
        }
        public async Task ProcessFullfillDataForFileCreationQueue(FullFillOrderDto fullFillOrderDto, Order order)
        {
            FullFillFileDto fullFillFileDto = new FullFillFileDto();
            fullFillFileDto.Organization = order.Organization.Name;
            fullFillFileDto.Comment = fullFillOrderDto.Comment;
            fullFillFileDto.OrderNumber = order.Number;
            fullFillFileDto.ExpectedDeliveryDate = fullFillOrderDto.ExpectedDeliveryDate;
            fullFillFileDto.Shipper = fullFillOrderDto.shipper.ShipperName;
            fullFillFileDto.TrackingId = fullFillOrderDto.TrackingId;
            if (order.Warehouse.Address != null)
            {
                fullFillFileDto.Address = order.Warehouse.Address.Description;
                fullFillFileDto.Country = order.Warehouse.Address.Country.Name;
                fullFillFileDto.PostalCode = order.Warehouse.Address.PostalAddress;
            }

            List<PackageDto> orderPackages = new List<PackageDto>();
            foreach (var item in fullFillOrderDto.fullFillOrderItemDtos)
            {
                orderPackages.AddRange(item.packageDtos);
            }
            fullFillFileDto.PackageDtos = orderPackages;
            FileDetailsDto fileDetailsDto = new FileDetailsDto();
            fileDetailsDto.FileDataDto = fullFillFileDto;
            fileDetailsDto.ContentModelType = fullFillFileDto.GetType().Name;
            await Utility.SendToQueue(fileDetailsDto, _configuration.GetConnectionString("StorageConnectionString"), _configuration.GetValue<string>("FileProcessingQueue"));


        }

        private async Task<(List<Reel> Reels, int UsedReelCount, int UsedCartonCount)> GetReelsUsedAndFullFillmentData(FullFillOrderItemDto fullFillOrderItemDto, Order order, OrderItem orderItem)
        {
            int usedReelCount = 0, usedCartonCount = 0;
            var reels = new List<Reel>();
            foreach (var package in fullFillOrderItemDto.packageDtos)
            {
                if (package.packageType == PackageType.Carton)
                {
                    var carton = await _orderManagementDbContext.Cartons
                        //.Include("Reels.PrintOrder")
                        //.Include("Reels.Product")
                        .Include(x => x.Reels)
                        .ThenInclude(p => p.PrintOrder)
                        .Include(p => p.Reels)
                        .ThenInclude(h => h.Product)
                        .Where(x => x.Code == package.Code).FirstOrDefaultAsync();
                    package.ProductNameOrigin = carton.Product.Name + "-" + carton.Product.Origin.ToString();
                    package.StampCount = carton.Reels.Where(x => x.IsUsedForFulfillment == false).Sum(x => x.StampCount);
                    if (carton.Reels.Count() == carton.Reels.Where(x => x.IsUsedForFulfillment == false).Count())
                    {
                        usedCartonCount += 1;
                        foreach (var reel in carton.Reels.ToList())
                        {
                            await CreateOrderItemReelAsync(order, reel, orderItem, carton.Code);
                        }

                    }
                    else
                    {
                        usedReelCount += carton.Reels.Where(x => x.IsUsedForFulfillment == false).Count();
                        foreach (var reel in carton.Reels.Where(x => x.IsUsedForFulfillment == false).ToList())
                        {
                            await CreateOrderItemReelAsync(order, reel, orderItem, null);
                        }
                    }

                    reels.AddRange(carton.Reels.Where(x => x.IsUsedForFulfillment == false));

                }
                else
                {
                    var reel = await _orderManagementDbContext.Reels.Include(c => c.Carton).Include(p => p.PrintOrder).Include(p => p.Product).Where(x => x.Code == package.Code).FirstOrDefaultAsync();
                    package.ProductNameOrigin = reel.Product.Name + "-" + reel.Product.Origin.ToString();
                    package.StampCount = reel.StampCount;
                    usedReelCount += 1;
                    await CreateOrderItemReelAsync(order, reel, orderItem, null);
                    reels.Add(reel);
                }
            }
            return (reels, usedReelCount, usedCartonCount);
        }

        private async Task CreateOrderItemReelAsync(Order order, Reel reel, OrderItem orderItem, string cartonCode)
        {
            //foreach (var reel in reels)
            //{
            var orderItemReel = new OrderItemReel();
            await _orderManagementDbContext.OrderItemReels.AddAsync(orderItemReel);
            orderItemReel.OrderItem = orderItem;
            orderItemReel.Reel = reel;
            orderItemReel.Carton = reel.Carton;
            orderItemReel.CartonCode = cartonCode;
            orderItemReel.PrintOrder = reel.PrintOrder;
            orderItemReel.Organization = order.Organization;
            orderItemReel.OrganizationName = order.Organization.Name;
            orderItemReel.Product = orderItem.Product;
            orderItemReel.Warehouse = order.Warehouse;
            orderItemReel.WarehouseName = order.Warehouse.Name;
            orderItemReel.ReelConsumptionType = ReelConsumptionType.NotConsumed;
            orderItemReel.BrandProduct = orderItem.BrandProduct;
            orderItemReel.BrandProductName = orderItem.BrandProduct?.Name;
            orderItemReel.Sku = orderItem.StockKeepingUnit;
            orderItemReel.SkuName = orderItem.StockKeepingUnit?.Unit;
            // }
        }

        public Boolean GetOrderForDeactivation(int id)
        {
            IQueryable<Order> orders = _orderManagementDbContext.Orders.AsQueryable();
            var order = orders.Where(x => x.Organization.Id == id && x.Status != OrderStatus.Closed && x.Status != OrderStatus.Rejected);
            var count = order.Count();
            var result = count > 0 ? true : false;
            return result;

        }

        public Boolean GetOrderForProductDeactivation(int productId)
        {
            IQueryable<OrderItem> orders = _orderManagementDbContext.OrderItems.AsQueryable();
            IQueryable<PrintOrder> printorders = _orderManagementDbContext.PrintOrders.AsQueryable();
            var printorder = printorders.Where(x => x.Product.Id == productId && x.Status != PrintOrderStatus.Closed && x.Status != PrintOrderStatus.Rejected);
            var order = orders.Where(x => x.Product.Id == productId && x.Order.Status != OrderStatus.Closed && x.Order.Status != OrderStatus.Rejected);
            var result = (order.Count() | printorder.Count()) > 0 ? true : false;
            return result;
        }
        public Boolean GetOrderForReelSizeDeactivation(int reelSizeId)
        {
            IQueryable<OrderItem> orders = _orderManagementDbContext.OrderItems.AsQueryable();
            IQueryable<PrintOrder> printorders = _orderManagementDbContext.PrintOrders.AsQueryable();
            var printorder = printorders.Where(x => x.ReelSize.Id == reelSizeId && x.Status != PrintOrderStatus.Closed && x.Status != PrintOrderStatus.Rejected);
            var order = orders.Where(x => x.ReelSize.Id == reelSizeId && x.Order.Status != OrderStatus.Closed && x.Order.Status != OrderStatus.Rejected);
            var result = (order.Count() | printorder.Count()) > 0 ? true : false;
            return result;
        }
        public Boolean GetOrderForSKUDeactivation(int SkuId)
        {
            IQueryable<OrderItem> orders = _orderManagementDbContext.OrderItems.AsQueryable();
            var order = orders.Where(x => x.StockKeepingUnit.Id == SkuId && x.Order.Status != OrderStatus.Closed && x.Order.Status != OrderStatus.Rejected);
            var result = order.Count() > 0 ? true : false;
            return result;
        }
        public List<MiniOrderDto> GetOrderDownloadList(OrderSearchFilterOptions filterOptions)
        {

            var orders = GetFilteredOrders(filterOptions);
            orders = GetStatusFilteredOrders(orders, filterOptions);
            //Add any more status like payment status.

            var materialisedOrders = orders
                .Select(x => new
                {
                    OrderId = x.Id,
                    OrderNumber = x.Number,
                    OrgName = x.Organization.Name,
                    RequestedOn = x.ModifiedDate,
                    Status = x.Status,
                    PaymentStatus = x.Payment.PaymentStatus,
                    WarehouseName = x.Warehouse.Name,
                    PayableAmount = x.PayableAmount
                }).ToList();

            var miniOrderDtoList = new List<MiniOrderDto>();
            foreach (var materialisedOrder in materialisedOrders)
            {
                var miniOrderDto = new MiniOrderDto();
                miniOrderDto.OrderId = materialisedOrder.OrderId;
                miniOrderDto.OrderNumber = materialisedOrder.OrderNumber;
                miniOrderDto.OrgName = materialisedOrder.OrgName;
                miniOrderDto.PaymentStatus = materialisedOrder.PaymentStatus == 0 ? "NA" : materialisedOrder.PaymentStatus.ToString();
                miniOrderDto.RequestedOn = materialisedOrder.RequestedOn.ToDisplayDate();
                miniOrderDto.Status = materialisedOrder.Status.ToString();
                miniOrderDto.Location = materialisedOrder.WarehouseName;
                miniOrderDto.PayableAmount = materialisedOrder.PayableAmount;
                miniOrderDtoList.Add(miniOrderDto);
            }
            return miniOrderDtoList;

        }
        private IQueryable<Order> GetFilteredOrders(OrderSearchFilterOptions filterOptions)
        {
            IQueryable<Order> orders = _orderManagementDbContext.Orders.AsQueryable();

            if (filterOptions.EntityIds != null && filterOptions.EntityIds.Length > 0)
            {
                orders = orders.Where(x => filterOptions.EntityIds.Contains(x.Organization.Id));

            }

            if (filterOptions.OrgType != null)
            {
                orders = orders.Where(x => x.Organization.OrgType == filterOptions.OrgType.Value);
            }

            if (filterOptions.Locations != null && filterOptions.Locations.Length > 0)
            {
                orders = orders.Where(x => filterOptions.Locations.Contains(x.Warehouse.Id));
            }

            if (!string.IsNullOrEmpty(filterOptions.SearchText))
            {
                orders = orders.Where(x => x.Number.Contains(filterOptions.SearchText) || x.TotalStamps.ToString().Contains(filterOptions.SearchText) || x.TotalPrice.ToString().Contains(filterOptions.SearchText));
            }

            if (filterOptions.OrdersFrom != null)
            {
                orders = orders.Where(x => x.ModifiedDate >= filterOptions.OrdersFrom.Value.Date);
            }

            if (filterOptions.OrdersTill != null)
            {
                orders = orders.Where(x => x.ModifiedDate <= filterOptions.OrdersTill.Value.Date);
            }
            return orders;
        }
        private IQueryable<Order> GetStatusFilteredOrders(IQueryable<Order> orders, OrderSearchFilterOptions filterOptions)
        {
            if (filterOptions.OrgType != null)
            {
                orders = orders.Where(x => x.Organization.OrgType == filterOptions.OrgType.Value);
            }
            if (filterOptions.OrderStatuses != null && filterOptions.OrderStatuses.Length > 0)
            {
                orders = orders.Where(x => filterOptions.OrderStatuses.Contains(x.Status));
            }

            if (filterOptions.SortBy != null)
            {
                switch (filterOptions.SortBy.ToLower())
                {
                    case "manufacturer":
                        orders = (filterOptions.SortByDesc ? orders.OrderByDescending(x => x.Organization.Name) : orders.OrderBy(x => x.Organization.Name));
                        break;
                    case "requestedon":
                        orders = filterOptions.SortByDesc ? orders.OrderByDescending(x => x.ModifiedDate) : orders.OrderBy(x => x.ModifiedDate);
                        break;
                    case "location":
                        orders = filterOptions.SortByDesc ? orders.OrderByDescending(x => x.Warehouse.Name) : orders.OrderBy(x => x.Warehouse.Name);
                        break;
                }
            }
            return orders;
        }
        private async Task<Microsoft.Azure.Cosmos.Container> GetContainer(Database db, string container)
        {
            FeedIterator<ContainerProperties> resultSetIterator = db.GetContainerQueryIterator<ContainerProperties>();
            while (resultSetIterator.HasMoreResults)
            {
                foreach (ContainerProperties candidate in resultSetIterator.ReadNextAsync().Result)
                {
                    if (candidate.Id == container)
                        return db.GetContainer(container);
                }
            }
            return null;
        }
        public async Task<(string orgName, string orderNumber, int warehouseId, int orgId)> CloseOrder(Guid orderId, OrderEntityType? orderEntityType)
        {
            var order = await _orderManagementDbContext.Orders.Include(o => o.Organization).Include(w => w.Warehouse).SingleOrDefaultAsync(x => x.Id == orderId);
            order.Status = OrderStatus.Closed;
            var orderHistory = new OrderHistory();
            orderHistory.Order = order;
            orderHistory.Action = OrderStatus.Closed;
            orderHistory.ActionedBy = GetActionedBy(orderEntityType);
            orderHistory.Comments = "Closed";
            _orderManagementDbContext.OrderHistories.Add(orderHistory);
            await _orderManagementDbContext.SaveChangesAsync();
            return (order.Organization.Name, order.Number, order.Warehouse.Id, order.Organization.Id);
        }

        public async Task<(AutoFillFullfillOrderItemDto, string Error)> AutoFillFullfillmentForOrderItem(string orderItemId)
        {
            var lockedReels = await _orderManagementDbContext.Reels.Where(x => x.LockedOrderItemId == Guid.Parse(orderItemId)).ToListAsync();
            if (lockedReels.Count != 0)
            {
                foreach (var reel in lockedReels)
                {
                    reel.FulfillLockedDate = null;
                    reel.LockedOrderItemId = null;
                }
                await _orderManagementDbContext.SaveChangesAsync();
            }

            var orderItemDetails = await _orderManagementDbContext.OrderItems.Include(x => x.Product).Include(x => x.ReelSize).Where(x => x.Id == Guid.Parse(orderItemId)).FirstOrDefaultAsync();
            if (orderItemDetails == null)
                return (null, "Order item id is not available");
            AutoFillFullfillOrderItemDto autoFillFullfillOrderItemDto = new AutoFillFullfillOrderItemDto();
            List<CartonDto> cartonDtos = new List<CartonDto>();
            List<ReelDto> reelDtos = new List<ReelDto>();
            //string productNameOrigin = orderItemDetails.Product.Name + "-" + orderItemDetails.Product.Origin.ToString();
            var noOfReels = new SqlParameter("@reelRequiredCount", orderItemDetails.NoOfCoils);
            var productId = new SqlParameter("@productId", orderItemDetails.Product.Id);
            var reelSize = new SqlParameter("@reelSize", orderItemDetails.ReelSize.Size);
            var orderItemKey = new SqlParameter("@Orderitemid", orderItemDetails.Id);
            var result = await _orderManagementDbContext.ReelsDataForFullFills.FromSqlRaw("EXEC [dbo].[usp_GetReelsForFullfilment] @reelRequiredCount,@productId,@reelSize,@Orderitemid", parameters: new[] { noOfReels, productId, reelSize, orderItemKey }).ToListAsync();
            if (result.Count != orderItemDetails.NoOfCoils)
                return (null, "Not enough reels of this product to autofill");
            var groupByCartonResult = result.GroupBy(x => new { x.CartonCode }).Select(r => new { CartonCode = r.Key.CartonCode, reelCount = r.Count() });
            foreach (var item in groupByCartonResult)
            {
                var cartonReels = result.Where(x => x.CartonCode == item.CartonCode).ToList();
                var reelsInCurrentCarton = new List<ReelDto>();
                foreach (var reel in cartonReels)
                {
                    var reelDto = new ReelDto();
                    reelDto.Code = reel.ReelCode;
                    reelDto.Id = reel.Id;
                    reelDto.StampCount = reel.ReelSize;
                    reelDto.CartonCode = reel.CartonCode;
                    reelDto.PackageType = PackageType.Reel;
                    reelDto.IsUsed = false;
                    reelDto.ProductName = orderItemDetails.Product.Name;
                    reelsInCurrentCarton.Add(reelDto);
                }

                if (cartonReels.FirstOrDefault().CartonReelCount == item.reelCount)
                {
                    CartonDto cartonDto = new CartonDto();
                    cartonDto.Code = item.CartonCode;
                    cartonDto.Id = cartonReels.FirstOrDefault().CartonId;
                    cartonDto.PackageType = PackageType.Carton;
                    cartonDto.ReelCount = item.reelCount;
                    cartonDto.Reels = reelsInCurrentCarton;
                    cartonDtos.Add(cartonDto);
                }
                else
                {
                    reelDtos.AddRange(reelsInCurrentCarton);
                }
            }
            autoFillFullfillOrderItemDto.CartonDtos = cartonDtos;
            autoFillFullfillOrderItemDto.ReelDtos = reelDtos;
            autoFillFullfillOrderItemDto.OrderItemId = orderItemDetails.Id.ToString();
            return (autoFillFullfillOrderItemDto, null);
        }

        public async Task<string> RemovePackageLock(PackageDto packageDto)
        {
            if (packageDto.packageType == PackageType.Reel)
            {
                var reel = await _orderManagementDbContext.Reels.Where(x => x.Code == packageDto.Code).FirstOrDefaultAsync();
                if (reel == null)
                    return ("Package does not exist");
                reel.FulfillLockedDate = null;
                reel.LockedOrderItemId = null;
            }
            else
            {
                var reels = await _orderManagementDbContext.Reels.Where(x => x.CartonId == packageDto.PackageId).ToListAsync();
                if (reels.Count == 0)
                    return ("Package does not exist");
                foreach (var reel in reels)
                {
                    reel.FulfillLockedDate = null;
                    reel.LockedOrderItemId = null;
                }
            }
            await _orderManagementDbContext.SaveChangesAsync();
            return null;
        }

        public async Task<(List<PackageDto> packageDtos, AutoFillFullfillOrderItemDto unFullfilledPackages, OrderItemDto orderItemDto)> FetchPackagesOfOrderItemId(string orderItemId, OrderItem orderItem)
        {
            var packageDto = new List<PackageDto>();
            var orderItemDto = new OrderItemDto();
            if (orderItem == null)
            {
                orderItem = await _orderManagementDbContext.OrderItems.Include(p => p.Product).Include(x => x.StockKeepingUnit).Include(x => x.Order).Include(x => x.Order.Organization).Include(x => x.Order.Warehouse).Include(x => x.Order.Warehouse.Address).Include(x => x.Order.Warehouse.Address.Country).Where(x => x.Id == Guid.Parse(orderItemId)).FirstOrDefaultAsync();
                //newly added
                orderItemDto.ManufactureName = orderItem.Order.Organization.Name;
                orderItemDto.WarehouseName = orderItem.Order.Warehouse.Name;
                orderItemDto.PostalAddress = orderItem.Order.Warehouse.Address.PostalAddress;
                orderItemDto.Description = orderItem.Order.Warehouse.Address.Description;
                orderItemDto.City = orderItem.Order.Warehouse.Address.City;
                orderItemDto.Country = orderItem.Order.Warehouse.Address.Country.Name;
            }
            orderItemDto.ProductName = orderItem.Product.Name;
            orderItemDto.NoOfCoils = orderItem.NoOfCoils;
            orderItemDto.NoOfStamps = orderItem.NoOfStamps;

            if (orderItem.StockKeepingUnit != null)
                orderItemDto.StockKeepingUnitName = orderItem.StockKeepingUnit.Unit;
            if (orderItem.IsFulfilled)
            {
                orderItemDto.IsFulfilled = true;
                var orderItemReels = await _orderManagementDbContext.OrderItemReels.Include(x => x.Reel).Include(c => c.Carton).ThenInclude(r => r.Reels).Include(c => c.Carton).ThenInclude(x => x.Pallet).Where(y => y.OrderItem.Id == orderItem.Id).ToListAsync();
                if (orderItemReels.Where(x => x.CartonCode != null).Any())
                {
                    packageDto.AddRange(orderItemReels.Where(x => x.CartonCode != null)
                                            .GroupBy(x => new { x.CartonCode })
                                            .Select(x => new PackageDto { Code = x.Key.CartonCode, packageType = PackageType.Carton, StampCount = orderItemReels.Where(u => u.CartonCode == x.Key.CartonCode).FirstOrDefault().Carton.Reels.Sum(z => z.ReelSize), ReelCount = orderItemReels.Where(u => u.CartonCode == x.Key.CartonCode).FirstOrDefault().Carton.ReelCount, PalletCode = orderItemReels.Where(u => u.CartonCode == x.Key.CartonCode).FirstOrDefault().Carton.Pallet.Code })
                                            .ToList());
                }

                foreach (var item in orderItemReels.Where(x => x.CartonCode == null))
                {
                    PackageDto package = new PackageDto();
                    package.PackageId = item.Reel.Id;
                    package.Code = item.Reel.Code;
                    package.packageType = PackageType.Reel;
                    package.StampCount = item.Reel.ReelSize;
                    package.PalletCode = item.Carton.Pallet.Code;
                    packageDto.Add(package);
                }
                orderItemDto.FullfilledPackages = packageDto;
                return (packageDto, null, orderItemDto);
            }
            else
            {
                orderItemDto.IsFulfilled = false;
                var reels = await _orderManagementDbContext.Reels.Include(c => c.Carton).ThenInclude(x => x.Pallet).Where(x => x.LockedOrderItemId == orderItem.Id).ToListAsync();
                if (reels.Count != 0)
                {
                    AutoFillFullfillOrderItemDto autoFillFullfillOrderItemDto = new AutoFillFullfillOrderItemDto();
                    List<CartonDto> cartonDtos = new List<CartonDto>();
                    List<ReelDto> reelDtos = new List<ReelDto>();
                    var groupByCartonResult = reels.GroupBy(x => new { x.CartonId }).Select(r => new { CartonId = r.Key.CartonId, reelCount = r.Count() });
                    foreach (var item in groupByCartonResult)
                    {
                        var cartonReels = reels.Where(x => x.CartonId == item.CartonId).ToList();
                        var reelsInCurrentCarton = new List<ReelDto>();
                        foreach (var reel in cartonReels)
                        {
                            var reelDto = new ReelDto();
                            reelDto.Code = reel.Code;
                            reelDto.Id = reel.Id;
                            reelDto.StampCount = reel.ReelSize;
                            reelDto.CartonCode = reel.Carton.Code;
                            reelDto.PackageType = PackageType.Reel;
                            reelDto.IsUsed = false;
                            reelDto.ProductName = orderItem.Product.Name;
                            reelDto.PalletCode = reel.Carton.Pallet.Code;
                            reelsInCurrentCarton.Add(reelDto);
                        }

                        if (cartonReels.FirstOrDefault().Carton.ReelCount == item.reelCount)
                        {
                            CartonDto cartonDto = new CartonDto();
                            cartonDto.Code = cartonReels.FirstOrDefault().Carton.Code;
                            cartonDto.Id = cartonReels.FirstOrDefault().CartonId;
                            cartonDto.PackageType = PackageType.Carton;
                            cartonDto.ReelCount = item.reelCount;
                            cartonDto.Reels = reelsInCurrentCarton;
                            cartonDto.PalletCode = cartonReels.FirstOrDefault().Carton.Pallet.Code;
                            cartonDtos.Add(cartonDto);
                        }
                        else
                        {
                            reelDtos.AddRange(reelsInCurrentCarton);
                        }
                    }
                    autoFillFullfillOrderItemDto.CartonDtos = cartonDtos;
                    autoFillFullfillOrderItemDto.ReelDtos = reelDtos;
                    autoFillFullfillOrderItemDto.OrderItemId = orderItem.Id.ToString();
                    orderItemDto.UnFullfilledPackages = autoFillFullfillOrderItemDto;
                    return (null, autoFillFullfillOrderItemDto, orderItemDto);
                }
                return (null, null, null);
            }
            //return (null, null,null);
        }


        public async Task<List<string>> AddPackageLock(LockPackageDto lockPackageDto)
        {
            var errorList = new List<string>();
            foreach (var packageDto in lockPackageDto.PackageDtos)
            {
                if (packageDto.packageType == PackageType.Reel)
                {
                    var reel = await _orderManagementDbContext.Reels.Where(x => x.Code == packageDto.Code).FirstOrDefaultAsync();
                    if (reel == null)
                        errorList.Add($"Reel does not exist {packageDto.Code}");
                    reel.FulfillLockedDate = DateTime.Now;
                    reel.LockedOrderItemId = lockPackageDto.OrderItemId;
                }
                else
                {
                    var reels = await _orderManagementDbContext.Reels.Where(x => x.CartonId == packageDto.PackageId).ToListAsync();
                    if (reels.Count == 0)
                        errorList.Add($"Carton does not exist{packageDto.Code}");
                    foreach (var reel in reels)
                    {
                        reel.FulfillLockedDate = DateTime.Now;
                        reel.LockedOrderItemId = lockPackageDto.OrderItemId;
                    }
                }
            }
            await _orderManagementDbContext.SaveChangesAsync();
            return errorList;
        }
        public async Task<bool> MakePayment(Guid orderId, PaymentRepsonseDto paymentRepsonseDto)
        {
            var order = await _orderManagementDbContext.Orders.Include(o => o.OrderItems).ThenInclude(s => s.StockKeepingUnit).Include(o => o.OrderItems).ThenInclude(p => p.Product).Include(o => o.Organization).Include(w => w.Warehouse).ThenInclude(x => x.Address).ThenInclude(c => c.Country).Include(w => w.Warehouse.Contact).SingleOrDefaultAsync(x => x.Id == orderId);
            await ProcessMakePayment(order, paymentRepsonseDto);
            return true;
        }
        public async Task<bool> MakeOfflinePayment(Guid orderId, PaymentRepsonseDto paymentRepsonseDto)
        {
            var order = await _orderManagementDbContext.Orders.Include(o => o.OrderItems).ThenInclude(s => s.StockKeepingUnit).Include(o => o.OrderItems).ThenInclude(p => p.Product).Include(o => o.Organization).Include(w => w.Warehouse).ThenInclude(x => x.Address).ThenInclude(c => c.Country).Include(w => w.Warehouse.Contact).SingleOrDefaultAsync(x => x.Id == orderId);
            await ProcessMakePayment(order, paymentRepsonseDto);
            return true;
        }
        private async Task ProcessMakePayment(Order order, PaymentRepsonseDto paymentRepsonseDto)
        {
            var payment = _orderManagementDbContext.Payments.Where(x => x.OrderId == order.Id).SingleOrDefault();

            //var payment = new Payment();

            payment.PaymentMode = paymentRepsonseDto.PaymentMode == (int)PaymentMode.Online ? PaymentMode.Online : PaymentMode.Offline;
            payment.TransactionId = paymentRepsonseDto.TransactionId;
            payment.Amount = order.PayableAmount;
            //payment.PaymentStatus = order.PayableAmount > 0 ? PaymentStatus.Unpaid : PaymentStatus.Paid;
            payment.PaymentStatus = PaymentStatus.InProcess;
            payment.PaymentDate = DateTime.Now;
            payment.OrderId = order.Id;
            payment.Order = order;
            payment.PaymentInfo = paymentRepsonseDto.ResponseInfo;
            //await _orderManagementDbContext.Payments.AddAsync(payment);
            await _orderManagementDbContext.SaveChangesAsync();
        }

        public (int count, bool IsImporter) GetOrderExpiredStatusDetails(int orgId)
        {
            var organization = _orderManagementDbContext.Organization.Where(x => x.Id == orgId).FirstOrDefault();
            var data = _orderManagementDbContext.Orders.Include(o => o.Organization).Where(x => x.OrganizationId == orgId && x.Status == OrderStatus.Expired).ToList();
            return (data.Count(), organization.IsImporter);
        }
        public bool ImporterHasDomesticProducts(int orgId)
        {
            return getDomesticProducts(orgId);
        }

        private bool getDomesticProducts(int id)
        {
            var products = _orderManagementDbContext.OrgProducts.Include(p => p.Product).Where(x => x.Organization.Id == id && x.IsActive).Select(x => x.Product.Origin).ToList();
            return products.Contains(Origin.Domestic) || products.Contains(Origin.Transition);
        }
    }
}
