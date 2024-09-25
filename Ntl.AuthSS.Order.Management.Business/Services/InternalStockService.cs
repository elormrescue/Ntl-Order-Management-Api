using Microsoft.EntityFrameworkCore;
using Ntl.AuthSS.OrderManagement.Data;
using Ntl.AuthSS.OrderManagement.Data.Entities;
using Ntl.AuthSS.OrderManagement.Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public class InternalStockService : IInternalStockService
    {
        private readonly OrderManagementDbContext _orderManagementDbContext;
        private readonly MetaDataQueueService _metaDataQueueService;
        private readonly IConfiguration _configuration;
        public InternalStockService(OrderManagementDbContext orderManagementDbContext, IConfiguration configuration)
        {
            _orderManagementDbContext = orderManagementDbContext;
            _configuration = configuration;
            _metaDataQueueService = new MetaDataQueueService(_configuration.GetConnectionString("StorageAccount"), _configuration.GetValue<string>("StorageAccount:MetaDataUpdateQueue"));
        }

        public async Task<InternalStockRequestDto> GetInternalStockRequest(Guid internalStockRequestId)
        {
            var internalStockRequest = await _orderManagementDbContext.InternalStockRequests.Include(x => x.Product).Include(x => x.RequestingFacility).Include(x => x.ApprovingFacility).Include(x => x.InternalStockRequestHistories).SingleAsync(x => x.Id == internalStockRequestId);

            var internalStockRequestDto = new InternalStockRequestDto();
            internalStockRequestDto.Id = internalStockRequest.Id;
            internalStockRequestDto.Number = internalStockRequest.Number;
            internalStockRequestDto.NoOfStamps = internalStockRequest.NoOfStamps;
            internalStockRequestDto.ProductId = internalStockRequest.Product.Id;
            internalStockRequestDto.ProductName = internalStockRequest.Product.Name;
            internalStockRequestDto.ProdOrigin = internalStockRequest.Product.Origin.ToString();
            internalStockRequestDto.ImageName = internalStockRequest.Product.ImageName;
            internalStockRequestDto.OrgId = internalStockRequest.Organization?.Id;
            internalStockRequestDto.OrgName = internalStockRequest.Organization?.Name;
            internalStockRequestDto.RequestingFacilityId = internalStockRequest.RequestingFacility.Id;
            internalStockRequestDto.RequestingFacilityName = internalStockRequest.RequestingFacility.Name;
            internalStockRequestDto.ApprovingFacilityId = internalStockRequest.ApprovingFacility?.Id;
            internalStockRequestDto.ApprovingFacilityName = internalStockRequest.ApprovingFacility?.Name;
            internalStockRequestDto.ExpiredDate = internalStockRequest.ExpiredDate;
            internalStockRequestDto.Status = internalStockRequest.Status.ToString();
            internalStockRequestDto.InternalStockRequestHistories = new List<InterStockRequestHistoryDto>();

            foreach (var history in internalStockRequest.InternalStockRequestHistories.OrderByDescending(x => x.ModifiedDate))
            {
                var internalStockRequestHistoryDto = new InterStockRequestHistoryDto();
                internalStockRequestHistoryDto.Id = history.Id;
                internalStockRequestHistoryDto.Action = history.Action.ToString();
                internalStockRequestHistoryDto.UserName = history.UserName;
                internalStockRequestHistoryDto.Notes = history.Notes;
                internalStockRequestHistoryDto.ModifiedDate = history.ModifiedDate.ToDisplayDate();

                internalStockRequestDto.InternalStockRequestHistories.Add(internalStockRequestHistoryDto);
            }

            return internalStockRequestDto;

        }

        public async Task<InternalStockRequest> SaveInternalStockRequest(InternalStockRequestDto internalStockRequestDto, string userName)
        {
            var internalStockRequest = _orderManagementDbContext.InternalStockRequests.Include(x => x.InternalStockRequestHistories).SingleOrDefault(x => x.Id == internalStockRequestDto.Id) ?? AddGetInternalStockRequest();

            internalStockRequest.NoOfStamps = internalStockRequestDto.NoOfStamps;
            internalStockRequest.Product = _orderManagementDbContext.Products.Find(internalStockRequestDto.ProductId);
            internalStockRequest.RequestingFacility = _orderManagementDbContext.Warehouses.Find(internalStockRequestDto.RequestingFacilityId);
            internalStockRequest.Organization = _orderManagementDbContext.Organization.Find(internalStockRequestDto.OrgId);
            internalStockRequest.InternalStockRequestHistories ??= new List<InternalStockRequestHistory>();

            internalStockRequest.InternalStockRequestHistories.Add(new InternalStockRequestHistory { Action = InternalStockTransferStatus.Requested, InternalStockRequest = internalStockRequest, Notes = internalStockRequestDto.Notes, UserName = userName });

            await _orderManagementDbContext.SaveChangesAsync();

            return internalStockRequest;

        }

        public async Task<StockRequestListingDto> GetInternalStockRequests(InternalStockRequestFilterOptions filterOptions, int? orgId)
        {
            var stockRequests = GetFilteredInternalStockRequest(filterOptions, orgId);

            var allCount = stockRequests.Count();
            var requestedCount = stockRequests.Count(x => x.Status == InternalStockTransferStatus.Requested);
            var approvedCount = stockRequests.Count(x => x.Status == InternalStockTransferStatus.Approved);
            var transitCount = stockRequests.Count(x => x.Status == InternalStockTransferStatus.InTransit);
            var closedCount = stockRequests.Count(x => x.Status == InternalStockTransferStatus.Closed);
            var expiredCount = stockRequests.Count(x => x.Status == InternalStockTransferStatus.Expired);

            //Add any more status like payment status.
            stockRequests = GetStatusFilteredInternalStockRequest(stockRequests, filterOptions, orgId);

            var totalRows = stockRequests.Count();

            filterOptions.PageNo = filterOptions.PageNo == 0 ? 1 : filterOptions.PageNo;
            filterOptions.PageSize = filterOptions.PageSize == 0 ? 10 : filterOptions.PageSize;
            stockRequests = stockRequests.Skip((filterOptions.PageNo - 1) * filterOptions.PageSize).Take(filterOptions.PageSize);

            var materialisedRequests = await stockRequests
                .Select(x => new
                {
                    StockRequestId = x.Id,
                    Number = x.Number,
                    ProductName = x.Product.Name,
                    RequestedStamps = x.NoOfStamps,
                    FulfilledStamps = x.InternalStockRequestReels.Sum(x => x.Reel.StampCount),
                    RequestedOn = x.ModifiedDate,
                    Status = x.Status,
                    RequestingFaclityId = x.RequestingFacility.Id,
                    RequestingFacility = x.RequestingFacility.Name,
                    ApprovingFacilityId = x.ApprovingFacility.Id,
                    ApprovingFacility = x.ApprovingFacility.Name,
                }).ToListAsync();

            var miniInternalStockRequestDtoList = new List<MiniInternalStockRequestDto>();
            foreach (var materialisedRequest in materialisedRequests)
            {
                var miniInternalStockRequestDto = new MiniInternalStockRequestDto();
                miniInternalStockRequestDto.StockRequestId = materialisedRequest.StockRequestId;
                miniInternalStockRequestDto.Number = materialisedRequest.Number;
                miniInternalStockRequestDto.ProductName = materialisedRequest.ProductName;
                miniInternalStockRequestDto.RequestedStamps = materialisedRequest.RequestedStamps;
                miniInternalStockRequestDto.FulfilledStamps = materialisedRequest.FulfilledStamps;
                miniInternalStockRequestDto.RequestedOn = materialisedRequest.RequestedOn.ToDisplayDate();
                miniInternalStockRequestDto.Status = materialisedRequest.Status.ToString();
                miniInternalStockRequestDto.RequestingFacilityId = materialisedRequest.RequestingFaclityId;
                miniInternalStockRequestDto.ApprovingFacilityId = materialisedRequest.ApprovingFacilityId;
                miniInternalStockRequestDto.RequestingFacility = materialisedRequest.RequestingFacility;
                miniInternalStockRequestDto.ApprovingFacility = materialisedRequest.ApprovingFacility;

                miniInternalStockRequestDtoList.Add(miniInternalStockRequestDto);
            }

            var stockRequestListingDto = new StockRequestListingDto();
            stockRequestListingDto.MiniStockRequestDtos = miniInternalStockRequestDtoList;
            stockRequestListingDto.TotalCount = allCount;
            stockRequestListingDto.RequestedCount = requestedCount;
            stockRequestListingDto.ApprovedCount = approvedCount;
            stockRequestListingDto.TransitCount = transitCount;
            stockRequestListingDto.ClosedCount = closedCount;
            stockRequestListingDto.ExpiredCount = expiredCount;
            stockRequestListingDto.TotalRows = totalRows;

            return stockRequestListingDto;
        }

        public async Task<InternalStockRequest> ApproveStockRequest(Guid requestId, string comments, string userName, int approvingFacility)
        {
            var stockRequest = await _orderManagementDbContext.InternalStockRequests.Include(x => x.Organization).Include(x => x.RequestingFacility).SingleAsync(x => x.Id == requestId);
            stockRequest.Status = InternalStockTransferStatus.Approved;
            stockRequest.ApprovingFacility = _orderManagementDbContext.Warehouses.Find(approvingFacility);

            var stockRequestHistory = new InternalStockRequestHistory();
            stockRequestHistory.InternalStockRequest = stockRequest;
            stockRequestHistory.Action = InternalStockTransferStatus.Approved;
            stockRequestHistory.UserName = userName;
            stockRequestHistory.Notes = comments;

            _orderManagementDbContext.InternalStockRequestHistories.Add(stockRequestHistory);

            await _orderManagementDbContext.SaveChangesAsync();
            var metaDataQueueModel = new MetaDataQueueModel();
            metaDataQueueModel.Id = stockRequest.Id;
            metaDataQueueModel.EventName = MetaDataUpdateEvent.OrderFulfillment;
            await _metaDataQueueService.AddMsgToQueue(metaDataQueueModel);
            return stockRequest;

        }
        public async Task<int> CloseStockRequest(Guid requestId, string userName, int closeFacility)
        {
            var stockRequest = await _orderManagementDbContext.InternalStockRequests.SingleAsync(x => x.Id == requestId);
            stockRequest.Status = InternalStockTransferStatus.Closed;
            stockRequest.ApprovingFacility = _orderManagementDbContext.Warehouses.Find(closeFacility);

            var stockRequestHistory = new InternalStockRequestHistory();
            stockRequestHistory.InternalStockRequest = stockRequest;
            stockRequestHistory.Action = InternalStockTransferStatus.Closed;
            stockRequestHistory.UserName = userName;
            stockRequestHistory.Notes = "Closed";

            _orderManagementDbContext.InternalStockRequestHistories.Add(stockRequestHistory);

            await _orderManagementDbContext.SaveChangesAsync();
            return 1;
        }

        private InternalStockRequest AddGetInternalStockRequest()
        {
            var internalStockRequest = new InternalStockRequest();
            internalStockRequest.Status = InternalStockTransferStatus.Requested;
            internalStockRequest.Number = $"ISR{DateTime.Now.Ticks}";
            _orderManagementDbContext.InternalStockRequests.Add(internalStockRequest);
            return internalStockRequest;
        }

        public ReelDto GetStockTransferReel(string reelCode, int locationId, int productId)
        {
            var reel = _orderManagementDbContext.OrderItemReels.Include(x => x.Reel).Include(x => x.Warehouse).Include(x => x.InternalStockRequest).Include(x => x.Product).SingleOrDefault(x =>
                    x.Reel.Code == reelCode
                    && x.InternalStockRequest == null
                    && x.Warehouse.Id == locationId
                    && x.Product.Id == productId
                    && x.ReelConsumptionType == ReelConsumptionType.NotConsumed)
                ?.Reel;
            if (reel != null)
            {
                return new ReelDto(reel);
            }
            return null;
        }

        public CartonDto GetStockTransferCarton(string cartonCode, int locationId, int productId)
        {
            var carton = _orderManagementDbContext.Cartons.SingleOrDefault(x => x.Code == cartonCode);
            if (carton == null)
                return null;

            var reels = _orderManagementDbContext.OrderItemReels.Include(x => x.Carton).Include(x => x.InternalStockRequest).Include(x => x.Warehouse).Include(x => x.Product).Where(
                x => x.Carton.Code == cartonCode
                && x.InternalStockRequest == null
                && x.Warehouse.Id == locationId
                && x.Product.Id == productId
                && x.ReelConsumptionType == ReelConsumptionType.NotConsumed).Select(x => x.Reel);

            if (carton.ReelCount != reels.Count())
                return null; //used carton

            var reelDtos = new List<ReelDto>();
            foreach (var reel in reels)
            {
                var reelDto = new ReelDto(reel);
                reelDtos.Add(reelDto);
            }

            var cartonDto = new CartonDto();
            cartonDto.Code = carton.Code;
            cartonDto.Id = carton.Id;
            cartonDto.Reels = reelDtos;
            cartonDto.PackageType = PackageType.Carton;
            return cartonDto;
        }

        public async Task<InternalStockRequest> SaveInternalStockTransfer(Guid requestId, PackageDto[] packageDtos, int shipperId, string trackingId, DateTime expectedDate, string comments, string userName)
        {
            var stockRequest = _orderManagementDbContext.InternalStockRequests.Include(x => x.ApprovingFacility).Include(x => x.Product).Include(x => x.Organization).Single(x => x.Id == requestId);

            foreach (var package in packageDtos)
            {
                if (package.packageType == PackageType.Carton)
                {
                    var carton = GetStockTransferCarton(package.Code, stockRequest.ApprovingFacility.Id, stockRequest.Product.Id);
                    if (carton == null)
                        throw new Exception("Carton not found. It may have become used right now.");
                    foreach (var reel in carton.Reels)
                    {
                        var stockRequestReel = new InternalStockRequestReel();
                        stockRequestReel.InternalStockRequest = stockRequest;
                        stockRequestReel.Reel = _orderManagementDbContext.Reels.Find(reel.Id);

                        _orderManagementDbContext.InternalStockRequestReels.Add(stockRequestReel);

                        var orderItemReel = _orderManagementDbContext.OrderItemReels.Single(x => x.Reel.Id == reel.Id);
                        orderItemReel.InternalStockRequest = stockRequest;
                    }
                }
                else if (package.packageType == PackageType.Reel)
                {
                    var reel = GetStockTransferReel(package.Code, stockRequest.ApprovingFacility.Id, stockRequest.Product.Id);
                    if (reel == null)
                        throw new Exception("Reel not found. It may have become used right now.");

                    var stockRequestReel = new InternalStockRequestReel();
                    stockRequestReel.InternalStockRequest = stockRequest;
                    stockRequestReel.Reel = _orderManagementDbContext.Reels.Find(reel.Id);

                    _orderManagementDbContext.InternalStockRequestReels.Add(stockRequestReel);

                    var orderItemReel = _orderManagementDbContext.OrderItemReels.Single(x => x.Reel.Id == reel.Id);
                    orderItemReel.InternalStockRequest = stockRequest;

                }
            }
            stockRequest.Status = InternalStockTransferStatus.InTransit;
            stockRequest.Shipper = _orderManagementDbContext.Organization.Find(shipperId);
            stockRequest.TrackingId = trackingId;
            stockRequest.ExpectedDate = expectedDate;
            stockRequest.FulfillmentComments = comments;
            stockRequest.ReelCount = packageDtos.Count(x => x.packageType == PackageType.Reel);
            stockRequest.CartonCount = packageDtos.Count(x => x.packageType == PackageType.Carton);
            stockRequest.InternalStockRequestHistories ??= new List<InternalStockRequestHistory>();

            stockRequest.InternalStockRequestHistories.Add(new InternalStockRequestHistory { Action = InternalStockTransferStatus.InTransit, InternalStockRequest = stockRequest, Notes = comments, UserName = userName });
            await _orderManagementDbContext.SaveChangesAsync();
            return stockRequest;
        }
        public IList<MiniInternalStockRequestDto> GetInternalStockRequestDownloadList(InternalStockRequestFilterOptions filterOptions, int? orgId)
        {
            var stockRequests = GetFilteredInternalStockRequest(filterOptions, orgId);
            stockRequests = GetStatusFilteredInternalStockRequest(stockRequests, filterOptions, orgId);

            var materialisedRequests = stockRequests
                .Select(x => new
                {
                    StockRequestId = x.Id,
                    Number = x.Number,
                    ProductName = x.Product.Name,
                    RequestedStamps = x.NoOfStamps,
                    FulfilledStamps = x.InternalStockRequestReels.Sum(x => x.Reel.StampCount),
                    RequestedOn = x.ModifiedDate,
                    Status = x.Status,
                    RequestingFaclityId = x.RequestingFacility.Id,
                    RequestingFacility = x.RequestingFacility.Name,
                    ApprovingFacilityId = x.ApprovingFacility.Id,
                    ApprovingFacility = x.ApprovingFacility.Name,
                }).ToList();

            var miniInternalStockRequestDtoList = new List<MiniInternalStockRequestDto>();
            foreach (var materialisedRequest in materialisedRequests)
            {
                var miniInternalStockRequestDto = new MiniInternalStockRequestDto();
                miniInternalStockRequestDto.StockRequestId = materialisedRequest.StockRequestId;
                miniInternalStockRequestDto.Number = materialisedRequest.Number;
                miniInternalStockRequestDto.ProductName = materialisedRequest.ProductName;
                miniInternalStockRequestDto.RequestedStamps = materialisedRequest.RequestedStamps;
                miniInternalStockRequestDto.FulfilledStamps = materialisedRequest.FulfilledStamps;
                miniInternalStockRequestDto.RequestedOn = materialisedRequest.RequestedOn.ToDisplayDate();
                miniInternalStockRequestDto.Status = materialisedRequest.Status.ToString();
                miniInternalStockRequestDto.RequestingFacilityId = materialisedRequest.RequestingFaclityId;
                miniInternalStockRequestDto.ApprovingFacilityId = materialisedRequest.ApprovingFacilityId;
                miniInternalStockRequestDto.RequestingFacility = materialisedRequest.RequestingFacility;
                miniInternalStockRequestDto.ApprovingFacility = materialisedRequest.ApprovingFacility;

                miniInternalStockRequestDtoList.Add(miniInternalStockRequestDto);
            }
            return miniInternalStockRequestDtoList;
        }
        private IQueryable<InternalStockRequest> GetFilteredInternalStockRequest(InternalStockRequestFilterOptions filterOptions, int? orgId)
        {
            IQueryable<InternalStockRequest> stockRequests = _orderManagementDbContext.InternalStockRequests.AsQueryable();

            if (orgId != null)
            {
                stockRequests = stockRequests.Where(x => x.Organization.Id == orgId.Value);
            }

            if (filterOptions.Locations != null && filterOptions.Locations.Length > 0)
            {
                stockRequests = stockRequests.Where(x => filterOptions.Locations.Contains(x.RequestingFacility.Id));
            }

            if (!string.IsNullOrEmpty(filterOptions.SearchText))
            {
                stockRequests = stockRequests.Where(x => x.Product.Name.Contains(filterOptions.SearchText) || x.NoOfStamps.ToString().Contains(filterOptions.SearchText) || x.RequestingFacility.Name.Contains(filterOptions.SearchText));
            }

            if (filterOptions.RequestsFrom != null)
            {
                stockRequests = stockRequests.Where(x => x.ModifiedDate >= filterOptions.RequestsFrom.Value.Date);
            }

            if (filterOptions.RequestsTill != null)
            {
                stockRequests = stockRequests.Where(x => x.ModifiedDate <= filterOptions.RequestsTill.Value.Date);
            }
            return stockRequests;
        }
        private IQueryable<InternalStockRequest> GetStatusFilteredInternalStockRequest(IQueryable<InternalStockRequest> stockRequests, InternalStockRequestFilterOptions filterOptions, int? orgId)
        {

            if (filterOptions.Statuses != null && filterOptions.Statuses.Length > 0)
            {
                stockRequests = stockRequests.Where(x => filterOptions.Statuses.Contains(x.Status));
            }

            if (filterOptions.SortBy != null)
            {
                switch (filterOptions.SortBy.ToLower())
                {
                    case "requestingfacility":
                        stockRequests = (filterOptions.SortByDesc ? stockRequests.OrderByDescending(x => x.RequestingFacility.Name) : stockRequests.OrderBy(x => x.RequestingFacility.Name));
                        break;
                    case "requestedon":
                        stockRequests = filterOptions.SortByDesc ? stockRequests.OrderByDescending(x => x.ModifiedDate) : stockRequests.OrderBy(x => x.ModifiedDate);
                        break;
                    case "noofstamps":
                        stockRequests = filterOptions.SortByDesc ? stockRequests.OrderByDescending(x => x.NoOfStamps) : stockRequests.OrderBy(x => x.NoOfStamps);
                        break;
                }
            }
            return stockRequests;
        }
    }
}
