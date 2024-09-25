using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public interface IInternalStockService
    {
        Task<InternalStockRequest> SaveInternalStockRequest(InternalStockRequestDto internalStockRequestDto, string userName);
        Task<InternalStockRequestDto> GetInternalStockRequest(Guid internalStockRequestId);
        Task<StockRequestListingDto> GetInternalStockRequests(InternalStockRequestFilterOptions filterOptions, int? orgId);
        Task<InternalStockRequest> ApproveStockRequest(Guid requestId, string comments, string userName, int approvingFacility);
        ReelDto GetStockTransferReel(string reelCode, int locationId, int productId);
        CartonDto GetStockTransferCarton(string cartonCode, int locationId, int productId);
        Task<InternalStockRequest> SaveInternalStockTransfer(Guid requestId, PackageDto[] packageDtos, int shipperId, string trackingId, DateTime expectedDate, string comments, string userName);
        IList<MiniInternalStockRequestDto> GetInternalStockRequestDownloadList(InternalStockRequestFilterOptions filterOptions, int? orgId);
        Task<int> CloseStockRequest(Guid requestId, string userName, int CloseFacility);
    }
}
