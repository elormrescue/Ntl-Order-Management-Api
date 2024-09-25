using Ntl.AuthSS.OrderManagement.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ntl.AuthSS.OrderManagement.Data.Entities;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public class StampGenerationService : IStampGenerationService
    {
        private readonly OrderManagementDbContext _orderManagementDbContext;
        public StampGenerationService(OrderManagementDbContext orderManagementDbContext)
        {
            _orderManagementDbContext = orderManagementDbContext;
        }
        public StampGenerationListingDto GetStampsGenerate(StampGenerationFilterDto filter)
        {
            var stampGeneration = _orderManagementDbContext.PrintOrderRequests.Include(p => p.PrintOrder).Include(p=>p.PrintPartner).AsQueryable();

            var allCount = stampGeneration.Count();
            var generatedCount = stampGeneration.Count(c => c.Status == StampGenerationStatus.Generated);
            var inProcessCount = stampGeneration.Count(c => c.Status == StampGenerationStatus.InProcess);
            var queuedCount = stampGeneration.Count(c => c.Status == StampGenerationStatus.Queued);

            stampGeneration = GetFilteredStampGeneration(stampGeneration, filter);

            var totalRows = stampGeneration.Count();

            filter.PageNo = filter.PageNo == 0 ? 1 : filter.PageNo;
            filter.PageSize = filter.PageSize == 0 ? 10 : filter.PageSize;
            stampGeneration = stampGeneration.Skip((filter.PageNo - 1) * filter.PageSize).Take(filter.PageSize);

            var stampGenerationDto = new List<StampGenerationDto>();

            foreach (var stamp in stampGeneration)
            {
                var lstStampGeneration = new StampGenerationDto();
                lstStampGeneration.Id = stamp.Id;
                lstStampGeneration.PrintOrderNumber = stamp.PrintOrder.Number;
                lstStampGeneration.RequestedDate = stamp.CreatedDate;
                lstStampGeneration.Status = stamp.Status.ToString();
                if(stamp.PrintPartner != null)
                {
                    lstStampGeneration.PrintPartner = stamp.PrintPartner.Name;
                }
                stampGenerationDto.Add(lstStampGeneration);
            }
            var stampGenerationListing = new StampGenerationListingDto();
            stampGenerationListing.StampGenerationDtos = stampGenerationDto;
            stampGenerationListing.TotalCount = allCount;
            stampGenerationListing.QueuedCount = queuedCount;
            stampGenerationListing.InProcessCount = inProcessCount;
            stampGenerationListing.GeneratedCount = generatedCount;
            stampGenerationListing.TotalRows = totalRows;

            return stampGenerationListing;
        }
        public List<StampGenerationDto> GetFilteredStampGenerationDownloadList(StampGenerationFilterDto filter)
        {
            var stampGeneration = _orderManagementDbContext.PrintOrderRequests.Include(p => p.PrintOrder).AsQueryable();

            stampGeneration = GetFilteredStampGeneration(stampGeneration, filter);

            var materialisedStamps = stampGeneration
              .Select(x => new
              {
                  Id = x.Id,
                  PrintOrderNumber = x.PrintOrder.Number,
                  RequestedDate = x.CreatedDate,
                  Status = x.Status
              }).ToList();
            var lstStampGeneration = new List<StampGenerationDto>();
            foreach(var stamp in materialisedStamps)
            {
                var stampGenerated = new StampGenerationDto();
                stampGenerated.PrintOrderNumber = stamp.PrintOrderNumber;
                stampGenerated.RequestedDate = stamp.RequestedDate;
                stampGenerated.Status = stamp.Status.ToString();
                lstStampGeneration.Add(stampGenerated);
            }
            return lstStampGeneration;
        }
        private IQueryable<PrintOrderRequest> GetFilteredStampGeneration(IQueryable<PrintOrderRequest> stampGeneration, StampGenerationFilterDto filter)
        {
            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                //stampGeneration = stampGeneration.Where(x=>x.Status.ToString().Contains(filter.SearchText));
                stampGeneration = stampGeneration.Where(x => x.PrintOrder.Number.Contains(filter.SearchText));
            }
            if (filter.Status != null && filter.Status.Length > 0)
            {
                stampGeneration = stampGeneration.Where(s => filter.Status.Contains(s.Status));
            }
            if(filter.PrintPartners != null && filter.PrintPartners.Length > 0)
            {
                stampGeneration = stampGeneration.Where(p => filter.PrintPartners.Contains(p.PrintPartner.Id));
            }
            if (filter.SortBy != null)
            {
                switch (filter.SortBy)
                {
                    case "requestedDate":
                        stampGeneration = (filter.SortByDesc ? stampGeneration.OrderByDescending(r => r.ModifiedDate) : stampGeneration.OrderBy(r => r.ModifiedDate));
                        break;
                }
            }
            return stampGeneration;
        }
    }
}
