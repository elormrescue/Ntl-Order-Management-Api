using Ntl.AuthSS.OrderManagement.Queryable;
using Ntl.AuthSS.OrderManagement.Queryable.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public interface IOrderDashboardService
    {
        Task<List<DashboardOrderDetailsResponse>> GetOrderDetails(int? orgId, int? orgType);
        Task<List<MFStampDetailsForSuppliers>> GetMFStampDetailsForSuppliers(int? orgId);
        Task<List<MFStampCountForBrands>> GetMFStampDetailsForBrands(int? orgId);
        Task<List<MFStampDetailsBasedOnProductSku>> GetMFStampDetailsBasedOnProductSku(int? orgId);
        Task<List<PrintOrderCountDetails>> GetPrintOrderDetails();
        Task<List<PrintOrderStampDetailsBasedOnProduct>> GetPrintOrderStampDetails();
        OrganizationCountDto GetOrganizationCountDetails();
        TotalPymentDetailsDto GetTotalPaymentDetails(DBPaymentPagination pagination);

    }
}