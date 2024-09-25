using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public interface IReelChangeService
    {
        Task<ReelChangeRequest> SaveReelChangeRequest(ReelChangeRequestDto reelChangeRequestDto, int orgId);
        Task<ReelChangeRequestDto> GetReelChangeRequest(Guid reelChangeRequestId);
        Task<ReelProductDto> GetReelProduct(string reelCode, int orgId, int productId);
        Task<List<ReelProductDto>> GetReelProducts(string cartonCode, int orgId, int productId);
        Task<ReelChangeListingDto> GetReelChangeRequests(ReelChangeRequestSearchFilterOptions filterOptions);
        Task<(string number, string orgName, int orgId)> ApproveReelChangeRequest(Guid reelChangeRequestId, string comments);
        Task<(string number, string orgName, int orgId)> RejectReelChangeRequest(Guid reelChangeRequestId, string comments);
        IList<MiniReelChangeRequestDto> GetReelChangeDownloadList(ReelChangeRequestSearchFilterOptions filterOptions);
    }
}
