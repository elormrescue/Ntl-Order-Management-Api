using Microsoft.EntityFrameworkCore;
using Ntl.AuthSS.OrderManagement.Data;
using Ntl.AuthSS.OrderManagement.Data.Entities;
using Ntl.AuthSS.OrderManagement.Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public class ReelChangeService : IReelChangeService
    {
        private readonly OrderManagementDbContext _orderManagementDbContext;
        private readonly MetaDataQueueService _metaDataQueueService;
        private readonly IConfiguration _configuration;
        public ReelChangeService(OrderManagementDbContext orderManagementDbContext, IConfiguration configuration)
        {
            _orderManagementDbContext = orderManagementDbContext;
            _configuration = configuration;
            _metaDataQueueService = new MetaDataQueueService(_configuration.GetConnectionString("StorageAccount"), _configuration.GetValue<string>("StorageAccount:MetaDataUpdateQueue"));
        }

        public async Task<ReelChangeRequest> SaveReelChangeRequest(ReelChangeRequestDto reelChangeRequestDto, int orgId)
        {
            var reelChangeRequest = await _orderManagementDbContext.ReelChangeRequests.Include(x => x.Organization).Include(x => x.ReelChangeRequestHistories).Include(x => x.ReelChangeRequestReels).SingleOrDefaultAsync(x => x.Id == reelChangeRequestDto.Id) ?? await AddGetReelChangeRequest();
            reelChangeRequest.Organization = _orderManagementDbContext.Organization.Find(orgId);
            reelChangeRequest.ReelChangeProduct = _orderManagementDbContext.Products.Find(reelChangeRequestDto.ReelChangeProductId);
            reelChangeRequest.ChangeToProduct = reelChangeRequestDto.ChangeToProductId != null ? _orderManagementDbContext.OrgBrandProducts.Find(reelChangeRequestDto.ChangeToProductId) : null;
            reelChangeRequest.ChangeToSku = reelChangeRequestDto.ChangeToSkuId != null ? _orderManagementDbContext.StockKeepingUnits.Find(reelChangeRequestDto.ChangeToSkuId) : null;
            reelChangeRequest.ReelChangeType = reelChangeRequestDto.ReelChangeType;
            reelChangeRequest.ReelChangeRequestReels ??= new List<ReelChangeRequestReel>();

            var existingRequestIds = reelChangeRequest.ReelChangeRequestReels.Select(x => x.Id).ToArray();
            var selectedRequestIds = reelChangeRequestDto.ReelChangeRequestReels.Where(x => x.Id != Guid.Empty).Select(x => x.Id).ToArray();

            var requestReelsToBeDeleted = _orderManagementDbContext.ReelChangeRequestReels.Where(x => (existingRequestIds.Except(selectedRequestIds)).Contains(x.Id)).ToList();
            var requestReelsToBeAddedOrUpdated = reelChangeRequestDto.ReelChangeRequestReels.Where(x => x.Id == Guid.Empty || existingRequestIds.Contains(x.Id));

            _orderManagementDbContext.ReelChangeRequestReels.RemoveRange(requestReelsToBeDeleted);

            foreach (var addUpdateRequestReel in requestReelsToBeAddedOrUpdated)
            {
                var requestReel = addUpdateRequestReel.Id == Guid.Empty ? await AddGetReelChangeRequestReel() : reelChangeRequest.ReelChangeRequestReels.Single(x => x.Id == addUpdateRequestReel.Id);
                requestReel.Reel = _orderManagementDbContext.Reels.Find(addUpdateRequestReel.ReelId);
                requestReel.ReelChangeRequest = reelChangeRequest;
            }

            reelChangeRequest.ReelChangeRequestHistories ??= new List<ReelChangeRequestHistory>();
            reelChangeRequest.ReelChangeRequestHistories.Add(new ReelChangeRequestHistory { Action = reelChangeRequest.Status, ActionedBy = OrgType.Manufacturer, Comments = reelChangeRequestDto.Comments });

            await _orderManagementDbContext.SaveChangesAsync();
            return reelChangeRequest;
            //return new List<string>() { reelChangeRequest.Id.ToString(), reelChangeRequest.Number.ToString(), reelChangeRequest.ReelChangeProduct.Name, reelChangeRequest.ChangeToSku.Unit };
        }

        public async Task<ReelChangeRequestDto> GetReelChangeRequest(Guid reelChangeRequestId)
        {
            var reelChangeRequest = await _orderManagementDbContext.ReelChangeRequests
                .Include(x => x.ChangeToProduct)
                .Include(x => x.ChangeToSku)
                .Include(x => x.Organization)
                .Include(x => x.ReelChangeProduct)
                .Include(x => x.ReelChangeRequestHistories)
                .Include(x => x.ReelChangeRequestReels).ThenInclude(x => x.Reel)
                .SingleAsync(x => x.Id == reelChangeRequestId);

            var reelChangeRequestDto = new ReelChangeRequestDto();
            reelChangeRequestDto.Id = reelChangeRequest.Id;
            reelChangeRequestDto.Number = reelChangeRequest.Number;
            reelChangeRequestDto.OrgId = reelChangeRequest.Organization.Id;
            reelChangeRequestDto.OrgName = reelChangeRequest.Organization.Name;
            reelChangeRequestDto.ReelChangeProductId = reelChangeRequest.ReelChangeProduct.Id;
            reelChangeRequestDto.ReelChangeProductName = $"{reelChangeRequest.ReelChangeProduct.Name} {reelChangeRequest.ReelChangeProduct.Origin}";
            reelChangeRequestDto.ReelChangeType = reelChangeRequest.ReelChangeType;
            reelChangeRequestDto.Status = reelChangeRequest.Status.ToString();
            reelChangeRequestDto.ChangeToProductId = reelChangeRequest.ChangeToProduct?.Id;
            reelChangeRequestDto.ChangeToProductName = reelChangeRequest.ChangeToProduct?.Name;
            reelChangeRequestDto.ChangeToSkuId = reelChangeRequest.ChangeToSku?.Id;
            reelChangeRequestDto.ChangeToSkuName = reelChangeRequest.ChangeToSku?.Unit;
            reelChangeRequestDto.ReelChangeRequestHistories = new List<ReelChangeRequestHistoryDto>();
            reelChangeRequestDto.ReelChangeRequestReels = new List<ReelChangeRequestReelDto>();

            foreach (var reelChangeRequestHistory in reelChangeRequest.ReelChangeRequestHistories.OrderByDescending(x => x.ModifiedDate))
            {
                var reelChangeHistoryDto = new ReelChangeRequestHistoryDto();
                reelChangeHistoryDto.Id = reelChangeRequestHistory.Id;
                reelChangeHistoryDto.Action = reelChangeRequestHistory.Action.ToString();
                reelChangeHistoryDto.ActionnedBy = reelChangeRequestHistory.ActionedBy.ToString();
                reelChangeHistoryDto.Comments = reelChangeRequestHistory.Comments;
                reelChangeHistoryDto.ModifiedDate = reelChangeRequestHistory.ModifiedDate.ToDisplayDate();
                reelChangeRequestDto.ReelChangeRequestHistories.Add(reelChangeHistoryDto);
            }

            foreach (var reelChangeRequestReel in reelChangeRequest.ReelChangeRequestReels)
            {
                var reelChangeRequestReelDto = new ReelChangeRequestReelDto();
                reelChangeRequestReelDto.Id = reelChangeRequestReel.Id;
                reelChangeRequestReelDto.ReelCode = reelChangeRequestReel.Reel.Code;
                reelChangeRequestReelDto.ReelId = reelChangeRequestReel.Reel.Id;
                var reelProduct = await GetReelProduct(reelChangeRequestReel.Reel.Code, reelChangeRequest.Organization.Id, reelChangeRequest.ReelChangeProduct.Id);
                reelChangeRequestReelDto.ProductName = reelProduct.ProductName;
                reelChangeRequestReelDto.Sku = reelProduct.Sku;
                reelChangeRequestReelDto.OldProductName = reelProduct.OldProductName;
                reelChangeRequestReelDto.OldSku = reelProduct.OldSku;
                reelChangeRequestDto.ReelChangeRequestReels.Add(reelChangeRequestReelDto);
            }

            return reelChangeRequestDto;
        }

        public async Task<ReelProductDto> GetReelProduct(string reelCode, int orgId, int productId)
        {
            var orderItemReel = await _orderManagementDbContext.OrderItemReels.Include(x => x.Reel).Include(x => x.Product).Include(x => x.BrandProduct).Include(x => x.NewBrandProduct).Include(x => x.Sku).Include(x => x.NewSku).SingleOrDefaultAsync(x => x.Reel.Code == reelCode && x.Organization.Id == orgId && x.Product.Id == productId && x.Organization.OrgType == OrgType.Manufacturer && x.ReelConsumptionType == ReelConsumptionType.NotConsumed && !x.IsReturned);
            if (orderItemReel != null)
            {
                return new ReelProductDto { Code = reelCode, Id = orderItemReel.Reel.Id, ProductName = orderItemReel.NewBrandProduct == null ? orderItemReel.BrandProduct.Name : orderItemReel.NewBrandProduct.Name, ProductType = $"{orderItemReel.Product.Name} {orderItemReel.Product.Origin}", Sku = orderItemReel.NewSku == null ? orderItemReel.Sku.Unit : orderItemReel.NewSku.Unit ,OldProductName= orderItemReel.BrandProductName,OldSku=orderItemReel.SkuName };
            }
            return null;

        }

        public async Task<List<ReelProductDto>> GetReelProducts(string cartonCode, int orgId, int productId)
        {
            var orderItemReels = await _orderManagementDbContext.OrderItemReels.Include(x => x.Reel).Include(x => x.Product).Include(x => x.BrandProduct).Include(x => x.NewBrandProduct).Include(x => x.Sku).Include(x => x.NewSku).Where(x => x.Reel.Carton.Code == cartonCode && x.Organization.Id == orgId && x.Product.Id == productId && x.Organization.OrgType == OrgType.Manufacturer && x.ReelConsumptionType == ReelConsumptionType.NotConsumed && !x.IsReturned).ToListAsync();
            var reelProductDtos = new List<ReelProductDto>();
            foreach (var orderItemReel in orderItemReels)
            {
                reelProductDtos.Add(new ReelProductDto { Code = orderItemReel.Reel.Code, Id = orderItemReel.Reel.Id, ProductName = orderItemReel.NewBrandProduct == null ? orderItemReel.BrandProduct.Name : orderItemReel.NewBrandProduct.Name, ProductType = $"{orderItemReel.Product.Name} {orderItemReel.Product.Origin}", Sku = orderItemReel.NewSku == null ? orderItemReel.Sku.Unit : orderItemReel.NewSku.Unit });
            }

            return reelProductDtos;
        }

        public async Task<(string number, string orgName, int orgId)> ApproveReelChangeRequest(Guid reelChangeRequestId, string comments)
        {
            var reelChangeRequest = _orderManagementDbContext.ReelChangeRequests
                .Include(x => x.ReelChangeRequestReels).ThenInclude(x => x.Reel)
                .Include(x => x.Organization)
                .Include(x => x.Organization.Contact)
                .Include(x => x.ReelChangeRequestHistories)
                .Include(x => x.ChangeToProduct)
                .Include(x => x.ChangeToSku)
                .Single(x => x.Id == reelChangeRequestId);

            foreach (var reelChangeReel in reelChangeRequest.ReelChangeRequestReels)
            {
                var orderItemReel = _orderManagementDbContext.OrderItemReels.Single(x => x.Reel.Id == reelChangeReel.Reel.Id && x.Organization.Id == reelChangeRequest.Organization.Id && x.Organization.OrgType == OrgType.Manufacturer && x.ReelConsumptionType == ReelConsumptionType.NotConsumed && !x.IsReturned);
                orderItemReel.NewBrandProduct = reelChangeRequest.ChangeToProduct;
                orderItemReel.NewBrandProductName = reelChangeRequest.ChangeToProduct?.Name;
                orderItemReel.NewSku = reelChangeRequest.ChangeToSku;
                orderItemReel.NewSkuName = reelChangeRequest.ChangeToSku?.Unit;
                //Add ChangeRequestId is required
            }
            reelChangeRequest.Status = ReelChangeRequestStatus.Approved;
            reelChangeRequest.ReelChangeRequestHistories.Add(new ReelChangeRequestHistory { Action = ReelChangeRequestStatus.Approved, ActionedBy = OrgType.Ntl, ReelChangeRequest = reelChangeRequest, Comments = comments });

            var metaDataQueueModel = new MetaDataQueueModel();
            metaDataQueueModel.Id = reelChangeRequest.Id;
            metaDataQueueModel.EventName = MetaDataUpdateEvent.ChangeSkuApproval;
            await _metaDataQueueService.AddMsgToQueue(metaDataQueueModel);
            await _orderManagementDbContext.SaveChangesAsync();

            return (reelChangeRequest.Number, reelChangeRequest.Organization.Name, reelChangeRequest.Organization.Id);

        }

        public async Task<(string number, string orgName, int orgId)> RejectReelChangeRequest(Guid reelChangeRequestId, string comments)
        {
            var reelChangeRequest = _orderManagementDbContext.ReelChangeRequests
                .Include(x => x.ReelChangeRequestHistories)
                .Include(x => x.Organization.Contact)
                .Single(x => x.Id == reelChangeRequestId);

            reelChangeRequest.Status = ReelChangeRequestStatus.Rejected;
            reelChangeRequest.ReelChangeRequestHistories.Add(new ReelChangeRequestHistory { Action = ReelChangeRequestStatus.Rejected, ActionedBy = OrgType.Ntl, ReelChangeRequest = reelChangeRequest, Comments = comments });

            await _orderManagementDbContext.SaveChangesAsync();
            return (reelChangeRequest.Number, reelChangeRequest.Organization.Name, reelChangeRequest.Organization.Id);
        }

        public async Task<ReelChangeListingDto> GetReelChangeRequests(ReelChangeRequestSearchFilterOptions filterOptions)
        {
            var reelChangeRequests = GetFilteredReelChange(filterOptions);
            var allCount = reelChangeRequests.Count();
            var submittedCount = reelChangeRequests.Count(x => x.Status == ReelChangeRequestStatus.Submitted);
            var approvedCount = reelChangeRequests.Count(x => x.Status == ReelChangeRequestStatus.Approved);
            var rejectedCount = reelChangeRequests.Count(x => x.Status == ReelChangeRequestStatus.Rejected);

            reelChangeRequests = GetStatusFilteredReelChange(reelChangeRequests, filterOptions);

            var totalRows = reelChangeRequests.Count();

            filterOptions.PageNo = filterOptions.PageNo == 0 ? 1 : filterOptions.PageNo;
            filterOptions.PageSize = filterOptions.PageSize == 0 ? 10 : filterOptions.PageSize;
            reelChangeRequests = reelChangeRequests.Skip((filterOptions.PageNo - 1) * filterOptions.PageSize).Take(filterOptions.PageSize);

            var materialisedChangeRequests = await reelChangeRequests
                .Select(x => new
                {
                    Id = x.Id,
                    Number = x.Number,
                    OrgName = x.Organization.Name,
                    RequestedOn = x.ModifiedDate,
                    Status = x.Status
                }).ToListAsync();

            var miniReelChangeRequestDtoList = new List<MiniReelChangeRequestDto>();
            foreach (var materialisedChangeRequest in materialisedChangeRequests)
            {
                var miniChangeRequestDto = new MiniReelChangeRequestDto();
                miniChangeRequestDto.Id = materialisedChangeRequest.Id;
                miniChangeRequestDto.Number = materialisedChangeRequest.Number;
                miniChangeRequestDto.OrgName = materialisedChangeRequest.OrgName;
                miniChangeRequestDto.RequestedDate = materialisedChangeRequest.RequestedOn.ToDisplayDate();
                miniChangeRequestDto.Status = materialisedChangeRequest.Status.ToString();
                miniReelChangeRequestDtoList.Add(miniChangeRequestDto);
            }

            var reelChangeRequestDto = new ReelChangeListingDto();
            reelChangeRequestDto.MiniReelChangeRequeustDtos = miniReelChangeRequestDtoList;
            reelChangeRequestDto.TotalCount = allCount;
            reelChangeRequestDto.SubmittedCount = submittedCount;
            reelChangeRequestDto.ApprovedCount = approvedCount;
            reelChangeRequestDto.RejectedCount = rejectedCount;
            reelChangeRequestDto.TotalRows = totalRows;

            return reelChangeRequestDto;

        }
        private async Task<ReelChangeRequestReel> AddGetReelChangeRequestReel()
        {
            var reelChangeRequestReel = new ReelChangeRequestReel();
            await _orderManagementDbContext.ReelChangeRequestReels.AddAsync(reelChangeRequestReel);
            return reelChangeRequestReel;
        }

        private async Task<ReelChangeRequest> AddGetReelChangeRequest()
        {
            var reelChangeRequest = new ReelChangeRequest();
            reelChangeRequest.Number = $"RCR{DateTime.Now.Ticks}";
            reelChangeRequest.Status = ReelChangeRequestStatus.Submitted;
            await _orderManagementDbContext.ReelChangeRequests.AddAsync(reelChangeRequest);
            return reelChangeRequest;
        }
        public IList<MiniReelChangeRequestDto> GetReelChangeDownloadList(ReelChangeRequestSearchFilterOptions filterOptions)
        {
            var reelChangeRequests = GetFilteredReelChange(filterOptions);
            reelChangeRequests = GetStatusFilteredReelChange(reelChangeRequests, filterOptions);
            var materialisedChangeRequests = reelChangeRequests
                .Select(x => new
                {
                    Id = x.Id,
                    Number = x.Number,
                    OrgName = x.Organization.Name,
                    RequestedOn = x.ModifiedDate,
                    Status = x.Status
                }).ToList();

            var miniReelChangeRequestDtoList = new List<MiniReelChangeRequestDto>();
            foreach (var materialisedChangeRequest in materialisedChangeRequests)
            {
                var miniChangeRequestDto = new MiniReelChangeRequestDto();
                miniChangeRequestDto.Id = materialisedChangeRequest.Id;
                miniChangeRequestDto.Number = materialisedChangeRequest.Number;
                miniChangeRequestDto.OrgName = materialisedChangeRequest.OrgName;
                miniChangeRequestDto.RequestedDate = materialisedChangeRequest.RequestedOn.ToDisplayDate();
                miniChangeRequestDto.Status = materialisedChangeRequest.Status.ToString();
                miniReelChangeRequestDtoList.Add(miniChangeRequestDto);
            }
            return miniReelChangeRequestDtoList;
        }
        //filtered common method
        private IQueryable<ReelChangeRequest> GetFilteredReelChange(ReelChangeRequestSearchFilterOptions filterOptions)
        {
            IQueryable<ReelChangeRequest> reelChangeRequests = _orderManagementDbContext.ReelChangeRequests.AsQueryable();

            if(filterOptions.EntityIds !=null && filterOptions.EntityIds.Length > 0)
            {
                reelChangeRequests = reelChangeRequests.Where(x => filterOptions.EntityIds.Contains(x.Organization.Id));
            }

            if (!string.IsNullOrEmpty(filterOptions.SearchText))
            {
                reelChangeRequests = reelChangeRequests.Where(x => x.Number.Contains(filterOptions.SearchText)
                || x.Organization.Name.ToString().Contains(filterOptions.SearchText)
                || x.ChangeToProduct.Name.Contains(filterOptions.SearchText)
                || x.ChangeToSku.Unit.Contains(filterOptions.SearchText)
                || x.ReelChangeProduct.Name.Contains(filterOptions.SearchText)
                );
            }

            if (filterOptions.RequestsFrom != null)
            {
                reelChangeRequests = reelChangeRequests.Where(x => x.ModifiedDate >= filterOptions.RequestsFrom.Value.Date);
            }

            if (filterOptions.RequestsTill != null)
            {
                reelChangeRequests = reelChangeRequests.Where(x => x.ModifiedDate <= filterOptions.RequestsTill.Value.Date);
            }
            return reelChangeRequests;
        }
        private IQueryable<ReelChangeRequest> GetStatusFilteredReelChange(IQueryable<ReelChangeRequest> reelChangeRequests, ReelChangeRequestSearchFilterOptions filterOptions)
        {

            if (filterOptions.Statuses != null && filterOptions.Statuses.Length > 0)
            {
                reelChangeRequests = reelChangeRequests.Where(x => filterOptions.Statuses.Contains(x.Status));
            }

            if (filterOptions.SortBy != null)
            {
                switch (filterOptions.SortBy.ToLower())
                {
                    case "orgname":
                        reelChangeRequests = (filterOptions.SortByDesc ? reelChangeRequests.OrderByDescending(x => x.Organization.Name) : reelChangeRequests.OrderBy(x => x.Organization.Name));
                        break;
                    case "requesteddate":
                        reelChangeRequests = filterOptions.SortByDesc ? reelChangeRequests.OrderByDescending(x => x.ModifiedDate) : reelChangeRequests.OrderBy(x => x.ModifiedDate);
                        break;
                }
            }
            return reelChangeRequests;
        }

    }
}

