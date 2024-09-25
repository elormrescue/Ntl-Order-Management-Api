using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ntl.AuthSS.OrderManagement.Data;
using Ntl.AuthSS.OrderManagement.Data.Entities;
using Ntl.AuthSS.OrderManagement.Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Ntl.AuthSS.OrderManagement.Business.FileDtos;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public class ReturnOrderService : IReturnOrderService
    {
        private readonly OrderManagementDbContext _orderManagementDbContext;
        private readonly IOrderMetaService _orderMetaService;
        private readonly MetaDataQueueService _metaDataQueueService;
        private readonly IConfiguration _configuration;
        public ReturnOrderService(OrderManagementDbContext orderManagementDbContext, IOrderMetaService orderMetaService, IConfiguration configuration)
        {
            _orderManagementDbContext = orderManagementDbContext;
            _orderMetaService = orderMetaService;
            _configuration = configuration;
            _metaDataQueueService = new MetaDataQueueService(_configuration.GetConnectionString("StorageAccount"), _configuration.GetValue<string>("StorageAccount:MetaDataUpdateQueue"));
        }

        public async Task<(string orderNumber, string orgName, string returnOrderNumber, string errorMessage)> ReturnOrder(ReturnOrderDto returnOrderDto, OrderEntityType? orderEntityType, int orgId)
        {
            var returnOrder = await AddReturnOrderAsync();

            var processedReelsData = await ProcessReturnOrderReels(returnOrderDto, returnOrder);
            if (processedReelsData.orderItemReels == null)
                return (null, null, null, processedReelsData.errorMessage);

            var organization = _orderManagementDbContext.Organization.Include(x=>x.Address).Include(x=>x.Address.Country).Where(x => x.Id == orgId).FirstOrDefault();
            var shipper = _orderManagementDbContext.Organization.Where(x => x.Id == returnOrderDto.shipper.ShipperId).FirstOrDefault();
            var warehouse = _orderManagementDbContext.Warehouses.Include(a => a.Address).ThenInclude(c => c.Country).Where(y => y.Id == returnOrderDto.Warehouse.WarehouseId).FirstOrDefault();
            returnOrder.TrackingId = returnOrderDto.TrackingId;
            returnOrder.ExpectedDate = returnOrderDto.ExpectedDeliveryDate;
            returnOrder.Number = Utility.GenerateReturnOrderNumber();
            returnOrder.Shipper = shipper;
            returnOrder.Organization = organization;
            returnOrder.Status = ReturnOrderStatus.Submitted;
            returnOrder.Warehouse = warehouse;

            var returnOrderHistory = new ReturnOrderHistory
            {
                Action = ReturnOrderStatus.Submitted,
                ActionedBy = GetActionedBy(orderEntityType),
                Comments = returnOrderDto.Comment,
                ReturnOrder = returnOrder
            };
            _orderManagementDbContext.ReturnOrderHistories.Add(returnOrderHistory);
            await _orderManagementDbContext.SaveChangesAsync();
            await ProcessReturnOrderDataForFileCreationQueue(returnOrderDto, returnOrder, organization, shipper, warehouse);
            return (returnOrder.Number, organization.Name, returnOrder.Number, null);
        }

        public async Task ProcessReturnOrderDataForFileCreationQueue(ReturnOrderDto returnOrderDto, ReturnOrder returnOrder, Organization organization, Organization shipper, Warehouse warehouse)
        {
            ReturnOrderFileDto returnOrderFileDto = new ReturnOrderFileDto();
            returnOrderFileDto.Comment = returnOrderDto.Comment;
            returnOrderFileDto.ExpectedDeliveryDate = returnOrderDto.ExpectedDeliveryDate;
            returnOrderFileDto.Number = returnOrder.Number;
            returnOrderFileDto.Organization = organization.Name;
            returnOrderFileDto.Address = organization.Address.Description;
            returnOrderFileDto.PostalCode = organization.Address.PostalAddress;
            returnOrderFileDto.Country = organization.Address.Country.Name;
            returnOrderFileDto.PackageDtos = returnOrderDto.packageDtos;
            returnOrderFileDto.ShipperName = shipper.Name;
            returnOrderFileDto.TrackingId = returnOrderDto.TrackingId;
            returnOrderFileDto.WarehouseName = warehouse.Name;
            if (warehouse.Address != null)
            {
                returnOrderFileDto.Address = warehouse.Address.Description;
                returnOrderFileDto.Country = warehouse.Address.Country.Name;
                returnOrderFileDto.PostalCode = warehouse.Address.PostalAddress;
            }
            FileDetailsDto fileDetailsDto = new FileDetailsDto();
            fileDetailsDto.FileDataDto = returnOrderFileDto;
            fileDetailsDto.ContentModelType = returnOrderFileDto.GetType().Name;
            await Utility.SendToQueue(fileDetailsDto, _configuration.GetConnectionString("StorageConnectionString"), _configuration.GetValue<string>("FileProcessingQueue"));


        }

        private async Task<ReturnOrder> AddReturnOrderAsync()
        {
            var returnOrder = new ReturnOrder();
            await _orderManagementDbContext.ReturnOrders.AddAsync(returnOrder);
            return returnOrder;
        }

        private async Task<(List<OrderItemReel> orderItemReels, string errorMessage)> ProcessReturnOrderReels(ReturnOrderDto returnOrderDto, ReturnOrder returnOrder)
        {
            var orderItemReels = new List<OrderItemReel>();
            foreach (var package in returnOrderDto.packageDtos)
            {
                if (package.packageType == PackageType.Carton)
                {
                    var orderItemReelList = await _orderManagementDbContext.OrderItemReels
                        .Include(c => c.Carton)
                        .Include(p => p.Reel)
                        .Include(h => h.Product)
                        .Where(x => x.Carton.Code == package.Code).ToListAsync();
                    if (orderItemReelList.Count() == 0)
                        return (null, "Carton not found");
                    if (orderItemReelList.Count() != orderItemReelList.FirstOrDefault().Carton.ReelCount)
                        return (null, "The carton has some reels which are not Used");
                    if (orderItemReelList.Where(i => i.ReelConsumptionType == ReelConsumptionType.Consumed).Any())
                        return (null, "The carton has some reels which are consumed");
                    orderItemReels.AddRange(orderItemReelList);
                    foreach (var orderItemReel in orderItemReelList)
                    {
                        await CreateReturnOrderReelAsync(returnOrder, orderItemReel, orderItemReel.Carton.Code);
                    }
                }
                else
                {
                    var orderItemReel = await _orderManagementDbContext.OrderItemReels
                        .Include(c => c.Carton)
                        .Include(p => p.Reel)
                        .Include(h => h.Product)
                        .Where(x => x.Reel.Code == package.Code).FirstOrDefaultAsync();
                    if (orderItemReel == null)
                        return (null, "Reel not found");
                    if (orderItemReel.ReelConsumptionType == ReelConsumptionType.Consumed)
                        return (null, "The reel is already consumed");
                    if (orderItemReel.ReelConsumptionType == ReelConsumptionType.PartiallyConsumed)
                        return (null, "The reel is partially consumed");
                    orderItemReels.Add(orderItemReel);
                    await CreateReturnOrderReelAsync(returnOrder, orderItemReel, null);
                }
            }
            return (orderItemReels, null);
        }

        private async Task CreateReturnOrderReelAsync(ReturnOrder returnOrder, OrderItemReel orderItemReel, string cartonCode)
        {
            var returnOrderReel = new ReturnOrderReels();
            await _orderManagementDbContext.ReturnOrderReels.AddAsync(returnOrderReel);
            returnOrderReel.CartonCode = cartonCode;
            returnOrderReel.Carton = orderItemReel.Carton;
            returnOrderReel.ReturnOrder = returnOrder;
            returnOrderReel.Reel = orderItemReel.Reel;
            returnOrderReel.ReelCode = orderItemReel.Reel.Code;
            returnOrderReel.Product = orderItemReel.Product;
            returnOrderReel.ProductName = orderItemReel.Product.Name;
        }

        public async Task<ReturnOrderListingDto> GetReturnOrders(ReturnOrderSearchFilterOptions filterOptions)
        {
            var returnOrders = GetFilteredReturnOrders(filterOptions);

            var allCount = returnOrders.Count();
            var closedOrdersCount = returnOrders.Count(x => x.Status == ReturnOrderStatus.Closed);
            var deliveredOrdersCount = returnOrders.Count(x => x.Status == ReturnOrderStatus.Closed);
            var submittedOrdersCount = returnOrders.Count(x => x.Status == ReturnOrderStatus.Submitted);
            var inTransitOrdersCount = returnOrders.Count(x => x.Status == ReturnOrderStatus.InTransit);

            //Add any more status like payment status.
            returnOrders = GetStatusFilteredReturnOrder(returnOrders, filterOptions);

            var totalRows = returnOrders.Count();

            filterOptions.PageNo = filterOptions.PageNo == 0 ? 1 : filterOptions.PageNo;
            filterOptions.PageSize = filterOptions.PageSize == 0 ? 10 : filterOptions.PageSize;
            returnOrders = returnOrders.Skip((filterOptions.PageNo - 1) * filterOptions.PageSize).Take(filterOptions.PageSize);

            var materialisedOrders = await returnOrders
                .Select(x => new
                {
                    OrderId = x.Id,
                    OrderNumber = x.Number,
                    OrgName = x.Organization.Name,
                    RequestedOn = x.ModifiedDate,
                    Status = x.Status,
                }).ToListAsync();

            var miniReturnOrderDtoList = new List<MiniReturnOrderDto>();
            foreach (var materialisedOrder in materialisedOrders)
            {
                var miniOrderDto = new MiniReturnOrderDto();
                miniOrderDto.OrderId = materialisedOrder.OrderId;
                miniOrderDto.OrderNumber = materialisedOrder.OrderNumber;
                miniOrderDto.OrgName = materialisedOrder.OrgName;
                miniOrderDto.RequestedOn = materialisedOrder.RequestedOn.ToDisplayDate();
                miniOrderDto.Status = materialisedOrder.Status.ToString();
                miniReturnOrderDtoList.Add(miniOrderDto);
            }

            var returnOrderlistDto = new ReturnOrderListingDto();
            returnOrderlistDto.MiniReturnOrderDtos = miniReturnOrderDtoList;
            returnOrderlistDto.TotalCount = allCount;
            returnOrderlistDto.ClosedCount = closedOrdersCount;
            returnOrderlistDto.DeliveredCount = deliveredOrdersCount;
            returnOrderlistDto.SubmittedCount = submittedOrdersCount;
            returnOrderlistDto.InTransitCount = inTransitOrdersCount;
            returnOrderlistDto.TotalRows = totalRows;

            return returnOrderlistDto;

        }

        public async Task<ReturnOrderDto> GetReturnOrderDetailsAsync(Guid returnOrderId)
        {
            var packageDto = new List<PackageDto>();
            var returnOrder = await _orderManagementDbContext.ReturnOrders
                                    .Include(o => o.Organization)
                                    .Include(y => y.Warehouse)
                                    .Include(y => y.ReturnOrderHistories)
                                    .Include(y => y.Shipper)
                                    .Where(x => x.Id == returnOrderId).FirstOrDefaultAsync();
            var returnOrderReels = await _orderManagementDbContext.ReturnOrderReels
                .Include(x => x.Carton).ThenInclude(p => p.Product)
                .Include(x => x.Carton).ThenInclude(x => x.Reels)
                .Include(r => r.Reel).ThenInclude(p => p.Product).Where(y => y.ReturnOrder.Id == returnOrder.Id).ToListAsync();

            if (returnOrderReels.Where(x => x.CartonCode != null).Any())
            {
                packageDto = returnOrderReels.Where(x => x.CartonCode != null)
                                        .GroupBy(x => new { x.CartonCode })
                                        .Select(x => new PackageDto
                                        {
                                            Code = x.Key.CartonCode,
                                            packageType = PackageType.Carton,
                                            StampCount = returnOrderReels.Where(u => u.CartonCode == x.Key.CartonCode).FirstOrDefault().Carton.Reels.Sum(z => z.ReelSize),
                                            ReelCount = returnOrderReels.Where(u => u.CartonCode == x.Key.CartonCode).FirstOrDefault().Carton.ReelCount,
                                            ProductNameOrigin = returnOrderReels.Where(u => u.CartonCode == x.Key.CartonCode).FirstOrDefault().Carton.Product.Name + "-" + returnOrderReels.Where(u => u.CartonCode == x.Key.CartonCode).FirstOrDefault().Carton.Product.Origin.ToString()
                                        })
                                        .ToList();
            }
            foreach (var returnOrderReel in returnOrderReels.Where(x => x.CartonCode == null))
            {
                var package = new PackageDto()
                {
                    Code = returnOrderReel.Reel.Code,
                    packageType = PackageType.Reel,
                    StampCount = returnOrderReel.Reel.StampCount,
                    ProductNameOrigin = returnOrderReel.ProductName + "-" + returnOrderReel.Product.Origin.ToString()
                };
                packageDto.Add(package);
            }

            var returnOrderDto = new ReturnOrderDto();
            returnOrderDto.ExpectedDeliveryDate = returnOrder.ExpectedDate;
            returnOrderDto.Number = returnOrder.Number;
            returnOrderDto.Organization = returnOrder.Organization.Name;
            returnOrderDto.shipper = new ShipperDto() { ShipperId = returnOrder.Shipper.Id, ShipperName = returnOrder.Shipper.Name };
            returnOrderDto.TrackingId = returnOrder.TrackingId;
            returnOrderDto.Warehouse = new WarehouseDto() { WarehouseId = returnOrder.Warehouse.Id, WarehouseName = returnOrder.Warehouse.Name };
            returnOrderDto.packageDtos = packageDto;
            returnOrderDto.ReturnOrderStatus = returnOrder.Status;
            returnOrderDto.ReturnOrderHistories = new List<ReturnOrderHistoryDto>();

            foreach (var orderHistory in returnOrder.ReturnOrderHistories.OrderByDescending(x => x.ModifiedDate))
            {
                var returnOrderHistoryDto = new ReturnOrderHistoryDto();
                returnOrderHistoryDto.Id = orderHistory.Id;
                returnOrderHistoryDto.OrderStatus = orderHistory.Action;
                returnOrderHistoryDto.StatusString = orderHistory.Action.ToString();
                returnOrderHistoryDto.ActionedBy = orderHistory.ActionedBy.ToString();
                returnOrderHistoryDto.Comments = orderHistory.Comments;
                returnOrderHistoryDto.CreateDate = orderHistory.CreatedDate;
                returnOrderHistoryDto.CreatedDateString = orderHistory.CreatedDate.ToShortDateString();
                returnOrderDto.ReturnOrderHistories.Add(returnOrderHistoryDto);
            }

            return returnOrderDto;
        }

        public async Task<(string mfName, string returnOrderNumber, int orgId)> ApproveReturnOrder(Guid orderId, string comments, OrderEntityType? orderEntityType)
        {
            var returnOrder = await _orderManagementDbContext.ReturnOrders.Include(o => o.Organization).Where(x => x.Id == orderId).SingleAsync();
            returnOrder.Status = ReturnOrderStatus.Closed;

            var orderReturnHistory = new ReturnOrderHistory();
            orderReturnHistory.ReturnOrder = returnOrder;
            orderReturnHistory.Action = ReturnOrderStatus.Closed;
            orderReturnHistory.ActionedBy = GetActionedBy(orderEntityType);
            orderReturnHistory.Comments = comments;
            _orderManagementDbContext.ReturnOrderHistories.Add(orderReturnHistory);

            OrgWallet orgWallet = new OrgWallet();
            orgWallet.Organization = returnOrder.Organization;
            orgWallet.ReturnOrder = returnOrder;
            orgWallet.TransactionType = TransactionType.Credit;
            orgWallet.WalletOrderType = WalletOrderType.ReturnOrder;
            var orgWalletBalanceDetails = await _orderManagementDbContext.OrgWallets.Where(x => x.Organization.Id == returnOrder.Organization.Id)
                                                        .OrderByDescending(p => p.ModifiedDate).FirstOrDefaultAsync();

            var returnOrderReels = await _orderManagementDbContext.ReturnOrderReels.Include(r => r.Reel).Where(x => x.ReturnOrder.Id == orderId).ToListAsync();
            foreach (var item in returnOrderReels)
            {
                var orderItemReel = await _orderManagementDbContext.OrderItemReels.Include(o => o.OrderItem).Where(x => x.Reel.Id == item.Reel.Id && x.IsReturned == false).SingleOrDefaultAsync();
                orderItemReel.ReturnOrder = returnOrder;
                orderItemReel.IsReturned = true;
                item.Reel.IsUsedForFulfillment = false;
                item.Reel.Status = ReelStatus.InStock;

                var calculatedAmt = (orderItemReel.Reel.ReelSize * orderItemReel.OrderItem.StampPrice);
                orgWallet.TransactionAmount += (decimal)calculatedAmt;
            }

            if (orgWalletBalanceDetails != null)
                orgWallet.BalanceAmount = orgWallet.TransactionAmount + orgWalletBalanceDetails.BalanceAmount;

            await _orderManagementDbContext.OrgWallets.AddAsync(orgWallet);
            await _orderManagementDbContext.SaveChangesAsync();
            var metaDataQueueModel = new MetaDataQueueModel();
            metaDataQueueModel.Id = returnOrder.Id;
            metaDataQueueModel.EventName = MetaDataUpdateEvent.ReturnOrderApproval;
            await _metaDataQueueService.AddMsgToQueue(metaDataQueueModel);
            return (returnOrder.Organization.Name, returnOrder.Number, returnOrder.Organization.Id);

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
        public IList<MiniReturnOrderDto> GetReturnOrderDownloadList(ReturnOrderSearchFilterOptions filterOptions)
        {
            var returnOrders = GetFilteredReturnOrders(filterOptions);
            returnOrders = GetStatusFilteredReturnOrder(returnOrders, filterOptions);


            var materialisedOrders = returnOrders
                .Select(x => new
                {
                    OrderId = x.Id,
                    OrderNumber = x.Number,
                    OrgName = x.Organization.Name,
                    RequestedOn = x.ModifiedDate,
                    Status = x.Status,
                }).ToList();

            var miniReturnOrderDtoList = new List<MiniReturnOrderDto>();
            foreach (var materialisedOrder in materialisedOrders)
            {
                var miniOrderDto = new MiniReturnOrderDto();
                miniOrderDto.OrderId = materialisedOrder.OrderId;
                miniOrderDto.OrderNumber = materialisedOrder.OrderNumber;
                miniOrderDto.OrgName = materialisedOrder.OrgName;
                miniOrderDto.RequestedOn = materialisedOrder.RequestedOn.ToDisplayDate();
                miniOrderDto.Status = materialisedOrder.Status.ToString();
                miniReturnOrderDtoList.Add(miniOrderDto);
            }

            return miniReturnOrderDtoList;

        }
        private IQueryable<ReturnOrder> GetFilteredReturnOrders(ReturnOrderSearchFilterOptions filterOptions)
        {
            IQueryable<ReturnOrder> returnOrders = _orderManagementDbContext.ReturnOrders.AsQueryable();

            if (filterOptions.EntityIds != null && filterOptions.EntityIds.Length > 0)
            {
                returnOrders = returnOrders.Where(x => filterOptions.EntityIds.Contains(x.Organization.Id));
            }

            if (filterOptions.OrgType != null)
            {
                returnOrders = returnOrders.Where(x => x.Organization.OrgType == filterOptions.OrgType.Value);
            }
            if (filterOptions.OrdersFrom != null)
            {
                returnOrders = returnOrders.Where(x => x.ModifiedDate >= filterOptions.OrdersFrom.Value.Date);
            }

            if (filterOptions.OrdersTill != null)
            {
                returnOrders = returnOrders.Where(x => x.ModifiedDate <= filterOptions.OrdersTill.Value.Date);
            }
            if (!string.IsNullOrEmpty(filterOptions.SearchText))
            {
                returnOrders = returnOrders.Where(x => x.Number.Contains(filterOptions.SearchText));
            }
            return returnOrders;
        }
        private IQueryable<ReturnOrder> GetStatusFilteredReturnOrder(IQueryable<ReturnOrder> returnOrders, ReturnOrderSearchFilterOptions filterOptions)
        {

            if (filterOptions.OrderStatuses != null && filterOptions.OrderStatuses.Length > 0)
            {
                returnOrders = returnOrders.Where(x => filterOptions.OrderStatuses.Contains(x.Status));
            }
            if (filterOptions.SortBy != null)
            {
                switch (filterOptions.SortBy.ToLower())
                {
                    case "manufacturer":
                        returnOrders = (filterOptions.SortByDesc ? returnOrders.OrderByDescending(x => x.Organization.Name) : returnOrders.OrderBy(x => x.Organization.Name));
                        break;
                    case "actiontakenon":
                        returnOrders = filterOptions.SortByDesc ? returnOrders.OrderByDescending(x => x.ModifiedDate) : returnOrders.OrderBy(x => x.ModifiedDate);
                        break;
                }
            }
            return returnOrders;
        }


    }
}
