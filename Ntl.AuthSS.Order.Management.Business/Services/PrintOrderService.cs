using Microsoft.EntityFrameworkCore;
using Ntl.AuthSS.OrderManagement.Data;
using Ntl.AuthSS.OrderManagement.Data.Entities;
using Ntl.AuthSS.OrderManagement.Business.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public class PrintOrderService : IPrintOrderService
    {
        private readonly OrderManagementDbContext _orderManagementDbContext;
        public PrintOrderService(OrderManagementDbContext orderManagementDbContext)
        {
            _orderManagementDbContext = orderManagementDbContext;
        }

        public async Task<(PrintOrderQueueDto, int orgId)> ApprovePrintOrder(Guid printOrderId, string comments, int id)
        {
            var order = await _orderManagementDbContext.PrintOrders.Include(p => p.PrintPartner).Include(p => p.Product).Include(p => p.Warehouse).Include(p => p.ReelSize).SingleAsync(x => x.Id == printOrderId);
            order.Status = PrintOrderStatus.Processing;

            var orderHistory = new PrintOrderHistory();
            orderHistory.PrintOrder = order;
            orderHistory.Action = PrintOrderStatus.Processing;
            orderHistory.ActionedBy = OrgType.Ntl;
            orderHistory.Comments = comments;

            _orderManagementDbContext.PrintOrderHistories.Add(orderHistory);

            var printOrderQueueDto = new PrintOrderQueueDto();
            printOrderQueueDto.RequestGuid = Guid.NewGuid();
            printOrderQueueDto.PrintOrderGuid = order.Id;
            printOrderQueueDto.PrintOrderNo = order.Number;
            printOrderQueueDto.ProductType = order.Product.Id;
            printOrderQueueDto.ReelSize = order.ReelSize.Size;
            printOrderQueueDto.TotalReels = order.NoOfReels;
            printOrderQueueDto.ExcessPercentage = 0;
            printOrderQueueDto.IsExcessNewFile = false;
            printOrderQueueDto.WareHouseId = order.Warehouse.Id;
            printOrderQueueDto.PrintPartnerId = order.PrintPartner.Id;

            var printOrderRequest = new PrintOrderRequest();
            printOrderRequest.Id = printOrderQueueDto.RequestGuid;
            printOrderRequest.Status = StampGenerationStatus.Queued;
            printOrderRequest.PrintOrder = order;

            _orderManagementDbContext.PrintOrderRequests.Add(printOrderRequest);

            await _orderManagementDbContext.SaveChangesAsync();
            return (printOrderQueueDto, order.CreatedUser);
        }

        public async Task<PrintOrderDto> GetPrintOrderAsync(Guid printOrderId)
        {
            var printOrder = await _orderManagementDbContext.PrintOrders.Include(x => x.PrintPartner).Include(x => x.Product).Include(x => x.ReelSize).Include(x => x.Warehouse).Include(x => x.PrintOrderHistories).SingleAsync(x => x.Id == printOrderId);
            var printOrderDto = new PrintOrderDto();

            printOrderDto.Id = printOrder.Id;
            printOrderDto.NoOfReels = printOrder.NoOfReels;
            printOrderDto.PoNum = printOrder.PoNum;
            printOrderDto.Number = printOrder.Number;
            printOrderDto.Status = printOrder.Status;
            printOrderDto.StatusString = printOrder.Status.ToString();
            printOrderDto.ExpectedDate = printOrder.ExpectedDate;
            printOrderDto.NoOfOrders = 1;
            printOrderDto.PrintPartnerId = printOrder.PrintPartner.Id;
            printOrderDto.PrintPartnerName = printOrder.PrintPartner.Name;
            printOrderDto.ImageName = printOrder.Product.ImageName;
            printOrderDto.ProductId = printOrder.Product.Id;
            printOrderDto.ProductName = $"{printOrder.Product.Name} {printOrder.Product.Origin}";
            printOrderDto.ReelSize = printOrder.ReelSize.Size;
            printOrderDto.ReelSizeId = printOrder.ReelSize.Id;
            printOrderDto.TotalStamps = printOrder.TotalStamps;
            printOrderDto.WarehouseId = printOrder.Warehouse.Id;
            printOrderDto.WarehouseName = printOrder.Warehouse.Name;
            printOrderDto.PrintOrderHistories = new List<PrintOrderHistoryDto>();
            foreach (var printOrderHistory in printOrder.PrintOrderHistories.OrderByDescending(x => x.ModifiedDate))
            {
                var printOrderHistoryDto = new PrintOrderHistoryDto();
                printOrderHistoryDto.Id = printOrderHistory.Id;
                printOrderHistoryDto.Action = printOrderHistory.Action.ToString();
                printOrderHistoryDto.ActionedBy = printOrderHistory.ActionedBy.ToString();
                printOrderHistoryDto.Comments = printOrderHistory.Comments;
                printOrderHistoryDto.ModifiedDate = printOrderHistory.ModifiedDate.ToDisplayDate();

                printOrderDto.PrintOrderHistories.Add(printOrderHistoryDto);
            }

            return printOrderDto;
        }

        public async Task<PrintOrderListingDto> GetPrintOrdersAsync(PrintOrderSearchFilterOptions filterOptions)
        {
            var printOrders = GetFilteredPrintOrder(filterOptions);

            var allCount = printOrders.Count();
            var submittedOrdersCount = printOrders.Count(x => x.Status == PrintOrderStatus.Submitted);
            var processingOrdersCount = printOrders.Count(x => x.Status == PrintOrderStatus.Processing);
            var rejectedOrdersCount = printOrders.Count(x => x.Status == PrintOrderStatus.Rejected);
            var inTransitOrdersCount = printOrders.Count(x => x.Status == PrintOrderStatus.InTransit);
            var closedCount = printOrders.Count(x => x.Status == PrintOrderStatus.Closed);


            //Add any more status like payment status.
            printOrders = GetStatusFilteredPrintOrder(printOrders, filterOptions);

            var totalRows = printOrders.Count();

            filterOptions.PageNo = filterOptions.PageNo == 0 ? 1 : filterOptions.PageNo;
            filterOptions.PageSize = filterOptions.PageSize == 0 ? 10 : filterOptions.PageSize;
            printOrders = printOrders.Skip((filterOptions.PageNo - 1) * filterOptions.PageSize).Take(filterOptions.PageSize);

            var materialisedPrintOrders = await printOrders
                .Select(x => new
                {
                    PrintOrderId = x.Id,
                    PoNum = x.PoNum,
                    ProductName = x.Product.Name,
                    Origin = x.Product.Origin,
                    PrintOrderNumber = x.Number,
                    PrintPartnerName = x.PrintPartner.Name,
                    ExpectedDate = x.ExpectedDate,
                    RequestedOn = x.ModifiedDate,
                    Status = x.Status,
                    WarehouseName = x.Warehouse.Name
                }).ToListAsync();

            var miniPrintOrderDtoList = new List<MiniPrintOrderDto>();
            foreach (var materialisedPrintOrder in materialisedPrintOrders)
            {
                var miniPrintOrderDto = new MiniPrintOrderDto();
                miniPrintOrderDto.PrintOrderId = materialisedPrintOrder.PrintOrderId;
                miniPrintOrderDto.PoNum = materialisedPrintOrder.PoNum;
                miniPrintOrderDto.ProductName = materialisedPrintOrder.ProductName;
                miniPrintOrderDto.Origin = materialisedPrintOrder.Origin.ToString();
                miniPrintOrderDto.PrintOrderNum = materialisedPrintOrder.PrintOrderNumber;
                miniPrintOrderDto.PrintPartnerName = materialisedPrintOrder.PrintPartnerName;
                miniPrintOrderDto.ExpectedDate = materialisedPrintOrder.ExpectedDate.ToDisplayDate();
                miniPrintOrderDto.RequestedOn = materialisedPrintOrder.RequestedOn.ToDisplayDate();
                miniPrintOrderDto.Status = materialisedPrintOrder.Status.ToString();
                miniPrintOrderDto.Location = materialisedPrintOrder.WarehouseName;

                miniPrintOrderDtoList.Add(miniPrintOrderDto);
            }

            var printOrderlistDto = new PrintOrderListingDto();
            printOrderlistDto.MiniPrintOrderDtos = miniPrintOrderDtoList;
            printOrderlistDto.TotalCount = allCount;
            printOrderlistDto.SubmittedCount = submittedOrdersCount;
            printOrderlistDto.ProcessingCount = processingOrdersCount;
            printOrderlistDto.RejectedCount = rejectedOrdersCount;
            printOrderlistDto.InTransitCount = inTransitOrdersCount;
            printOrderlistDto.ClosedCount = closedCount;
            printOrderlistDto.TotalRows = totalRows;

            return printOrderlistDto;
        }
        public IList<MiniPrintOrderDto> GetPrintOrderDownloadList(PrintOrderSearchFilterOptions filterOptions)
        {
            var printOrders = GetFilteredPrintOrder(filterOptions);
            printOrders = GetStatusFilteredPrintOrder(printOrders, filterOptions);

            //Add any more status like payment status.

            var materialisedPrintOrders = printOrders
                .Select(x => new
                {
                    PrintOrderId = x.Id,
                    PoNum = x.PoNum,
                    ProductName = x.Product.Name,
                    Origin = x.Product.Origin,
                    PrintOrderNumber = x.Number,
                    PrintPartnerName = x.PrintPartner.Name,
                    ExpectedDate = x.ExpectedDate,
                    RequestedOn = x.ModifiedDate,
                    Status = x.Status,
                    WarehouseName = x.Warehouse.Name
                }).ToList();

            var miniPrintOrderDtoList = new List<MiniPrintOrderDto>();
            foreach (var materialisedPrintOrder in materialisedPrintOrders)
            {
                var miniPrintOrderDto = new MiniPrintOrderDto();
                miniPrintOrderDto.PrintOrderId = materialisedPrintOrder.PrintOrderId;
                miniPrintOrderDto.PoNum = materialisedPrintOrder.PoNum;
                miniPrintOrderDto.ProductName = materialisedPrintOrder.ProductName;
                miniPrintOrderDto.Origin = materialisedPrintOrder.Origin.ToString();
                miniPrintOrderDto.PrintOrderNum = materialisedPrintOrder.PrintOrderNumber;
                miniPrintOrderDto.PrintPartnerName = materialisedPrintOrder.PrintPartnerName;
                miniPrintOrderDto.ExpectedDate = materialisedPrintOrder.ExpectedDate.ToDisplayDate();
                miniPrintOrderDto.RequestedOn = materialisedPrintOrder.RequestedOn.ToDisplayDate();
                miniPrintOrderDto.Status = materialisedPrintOrder.Status.ToString();
                miniPrintOrderDto.Location = materialisedPrintOrder.WarehouseName;

                miniPrintOrderDtoList.Add(miniPrintOrderDto);
            }
            return miniPrintOrderDtoList;
        }

        public async Task<(string printOrderNumber, int warehouseId, int printPartnerId, int userId, string printPartnerName)> RejectPrintOrder(Guid printOrderId, string comments)
        {
            var order = await _orderManagementDbContext.PrintOrders.Include(p => p.PrintPartner).Include(o => o.Warehouse).SingleAsync(x => x.Id == printOrderId);
            order.Status = PrintOrderStatus.Rejected;

            var orderHistory = new PrintOrderHistory();
            orderHistory.PrintOrder = order;
            orderHistory.Action = PrintOrderStatus.Rejected;
            orderHistory.ActionedBy = OrgType.Ntl;
            orderHistory.Comments = comments;

            _orderManagementDbContext.PrintOrderHistories.Add(orderHistory);

            await _orderManagementDbContext.SaveChangesAsync();
            return (order.Number, order.Warehouse.Id, order.PrintPartner.Id, order.CreatedUser, order.PrintPartner.Name);
        }

        public async Task<IList<(string printOrderNumber, int warehouseId, int printPartnerId, Guid printOrderId)>> SavePrintOrderAsync(PrintOrderDto printOrderDto)
        {
            if (printOrderDto.Id != Guid.Empty)
                printOrderDto.NoOfOrders = 1;
            var responseData = new List<(string printOrderNumber, int warehouseId, int printPartnerId, Guid printOrderId)>();
            for (int i = 0; i < printOrderDto.NoOfOrders; i++)
            {
                var printOrder = await _orderManagementDbContext.PrintOrders.Include(p=>p.PrintPartner).Include(x => x.PrintOrderHistories).SingleOrDefaultAsync(x => x.Id == printOrderDto.Id) ?? await AddGetPrintOrder();

                printOrder.ExpectedDate = printOrderDto.ExpectedDate;
                printOrder.NoOfReels = printOrderDto.NoOfReels;
                printOrder.PoNum = printOrderDto.PoNum;
                printOrder.PrintPartner = _orderManagementDbContext.Organization.Find(printOrderDto.PrintPartnerId);
                printOrder.Product = _orderManagementDbContext.Products.Find(printOrderDto.ProductId);
                printOrder.ReelSize = _orderManagementDbContext.ReelSizes.Find(printOrderDto.ReelSizeId);
                printOrder.NoOfReels = printOrderDto.NoOfReels;
                printOrder.TotalStamps = printOrder.ReelSize.Size * printOrder.NoOfReels;
                printOrder.Warehouse = _orderManagementDbContext.Warehouses.Find(printOrderDto.WarehouseId);

                var printOrderHistory = new PrintOrderHistory();
                printOrderHistory.Action = printOrder.Status;
                printOrderHistory.ActionedBy = OrgType.Ntl;
                printOrderHistory.Comments = printOrderDto.Comments;
                printOrderHistory.PrintOrder = printOrder;
                printOrder.PrintOrderHistories = printOrder.PrintOrderHistories ?? new List<PrintOrderHistory>();
                printOrder.PrintOrderHistories.Add(printOrderHistory);

                responseData.Add((printOrder.Number, printOrder.Warehouse.Id, printOrder.PrintPartner.Id, printOrder.Id));

                //_orderManagementDbContext.PrintOrders.Add(printOrder);
            }
            await _orderManagementDbContext.SaveChangesAsync();
            return responseData;

        }

        public async Task<(Guid printOrderId, string printOrderNumber, int warehouseId, int printPartnerId)> ClosePrintOrder(Guid printOrderId, string comments)
        {
            var order = await _orderManagementDbContext.PrintOrders.Include(o=>o.PrintPartner).Include(x=>x.Warehouse).SingleAsync(x => x.Id == printOrderId);
            order.Status = PrintOrderStatus.Closed;

            var orderHistory = new PrintOrderHistory();
            orderHistory.PrintOrder = order;
            orderHistory.Action = PrintOrderStatus.Closed;
            orderHistory.ActionedBy = OrgType.Ntl;
            orderHistory.Comments = comments;

            _orderManagementDbContext.PrintOrderHistories.Add(orderHistory);

            await _orderManagementDbContext.SaveChangesAsync();
            return (order.Id,order.Number, order.Warehouse.Id,order.PrintPartner.Id);
        }
        private async Task<PrintOrder> AddGetPrintOrder()
        {
            var printOrder = new PrintOrder();
            printOrder.Status = PrintOrderStatus.Submitted;
            printOrder.Number = OrderEntityType.Ntl.GenerateOrderNumber();
            await _orderManagementDbContext.PrintOrders.AddAsync(printOrder);
            return printOrder;
        }
        private IQueryable<PrintOrder> GetFilteredPrintOrder(PrintOrderSearchFilterOptions filterOptions)
        {
            IQueryable<PrintOrder> printOrders = _orderManagementDbContext.PrintOrders.AsQueryable();

            if (filterOptions.PrintPartnerId != null)
            {
                printOrders = printOrders.Where(x => x.PrintPartner.Id == filterOptions.PrintPartnerId);
            }

            if (filterOptions.Locations != null && filterOptions.Locations.Length > 0)
            {
                printOrders = printOrders.Where(x => filterOptions.Locations.Contains(x.Warehouse.Id));
            }

            if (!string.IsNullOrEmpty(filterOptions.SearchText))
            {
                printOrders = printOrders.Where(x => x.Number.Contains(filterOptions.SearchText)
                || x.TotalStamps.ToString().Contains(filterOptions.SearchText)
                || x.PoNum.Contains(filterOptions.SearchText)
                || x.Product.Name.Contains(filterOptions.SearchText)
                );
            }

            if (filterOptions.OrdersFrom != null)
            {
                printOrders = printOrders.Where(x => x.ModifiedDate >= filterOptions.OrdersFrom.Value.Date);
            }

            if (filterOptions.OrdersTill != null)
            {
                printOrders = printOrders.Where(x => x.ModifiedDate <= filterOptions.OrdersTill.Value.Date);
            }
            return printOrders;
        }
        private IQueryable<PrintOrder> GetStatusFilteredPrintOrder(IQueryable<PrintOrder> printOrders, PrintOrderSearchFilterOptions filterOptions)
        {
            if (filterOptions.PrintOrderStatuses != null && filterOptions.PrintOrderStatuses.Length > 0)
            {
                printOrders = printOrders.Where(x => filterOptions.PrintOrderStatuses.Contains(x.Status));
            }

            if (filterOptions.SortBy != null)
            {
                switch (filterOptions.SortBy.ToLower())
                {
                    case "ponum":
                        printOrders = (filterOptions.SortByDesc ? printOrders.OrderByDescending(x => x.PoNum) : printOrders.OrderBy(x => x.PoNum));
                        break;
                    case "expecteddate":
                        printOrders = (filterOptions.SortByDesc ? printOrders.OrderByDescending(x => x.ExpectedDate) : printOrders.OrderBy(x => x.ExpectedDate));
                        break;
                    case "requestedon":
                        printOrders = filterOptions.SortByDesc ? printOrders.OrderByDescending(x => x.ModifiedDate) : printOrders.OrderBy(x => x.ModifiedDate);
                        break;
                    case "location":
                        printOrders = filterOptions.SortByDesc ? printOrders.OrderByDescending(x => x.Warehouse.Name) : printOrders.OrderBy(x => x.Warehouse.Name);
                        break;
                }
            }
            return printOrders;
        }
    }
}
