using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Ntl.AuthSS.OrderManagement.Data;
using Ntl.AuthSS.OrderManagement.Data.Entities;
using Ntl.AuthSS.OrderManagement.Queryable;
using Ntl.AuthSS.OrderManagement.Queryable.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
   public class OrderDashboardService: IOrderDashboardService
    {
        private readonly OrderManagementQueryableDbContext _queryableDbContext;
        private readonly OrderManagementDbContext _orderManagementDbContext;
        public OrderDashboardService(OrderManagementQueryableDbContext queryableDbContext, OrderManagementDbContext orderManagementDbContext)
        {
            _queryableDbContext = queryableDbContext;
            _orderManagementDbContext = orderManagementDbContext;
        }

        public async Task<List<DashboardOrderDetailsResponse>> GetOrderDetails(int? orgId, int? orgType)
        {
            var searchKey = new SqlParameter("@n_orgId", orgId);
            var searchTypeKey = new SqlParameter("@n_orgType", orgType);
            var result = await _queryableDbContext.DashboardOrderDetailsResponses.FromSqlRaw("EXEC report_usp_dashboardorderdetails @n_orgId,@n_orgType", parameters: new[] { searchKey, searchTypeKey }).ToListAsync();
            return result;
        }

        public async Task<List<MFStampDetailsForSuppliers>> GetMFStampDetailsForSuppliers(int? orgId)
        {
            var orgKey = new SqlParameter("@orgId", orgId);
            var result = await _queryableDbContext.MFStampDetailsForSuppliers.FromSqlRaw("EXEC report_usp_dashboardOrgToSupplierStampDetails @orgId", orgKey).ToListAsync();
            return result;
        }
        public async Task<List<MFStampCountForBrands>> GetMFStampDetailsForBrands(int? orgId)
        {
            var orgKey = new SqlParameter("@orgId", orgId);
            var result = await _queryableDbContext.MFStampCountForBrands.FromSqlRaw("EXEC report_usp_dashboardOrgToBrandStampDetails @orgId", orgKey).ToListAsync();
            return result;
        }

        public async Task<List<MFStampDetailsBasedOnProductSku>> GetMFStampDetailsBasedOnProductSku(int? orgId)
        {
            var orgKey = new SqlParameter("@orgId", orgId);
            var result = await _queryableDbContext.MFStampDetailsBasedOnProductSkus.FromSqlRaw("Exec [dbo].[report_usp_dashboardOrgToProductSkuStampDetails] @orgId", orgKey).ToListAsync();
            return result;
        }

        public async Task<List<PrintOrderCountDetails>> GetPrintOrderDetails()
        {
            var result = await _queryableDbContext.PrintOrderCountDetails.FromSqlRaw("Exec report_usp_dashboardPrintOrderDetails").ToListAsync();
            return result;
        }
        public async Task<List<PrintOrderStampDetailsBasedOnProduct>> GetPrintOrderStampDetails()
        {
            var result = await _queryableDbContext.PrintOrderStampDetailsBasedOnProducts.FromSqlRaw("Exec [dbo].[report_usp_dashboardPrintOrderToProductStampDetails]").ToListAsync();
            return result;
        }
        public OrganizationCountDto GetOrganizationCountDetails()
        {
            var manufacturers= _orderManagementDbContext.Organization.Where(x => x.OrgType == OrgType.Manufacturer).AsQueryable();
            OrganizationCountDto organizationCountDto = new OrganizationCountDto();
            organizationCountDto.TotalMfCount = manufacturers.Count();
            organizationCountDto.ActiveMfCount = manufacturers.Where(x=>x.Status == OrgStatus.Active).Count();
            organizationCountDto.MfDomesticCount = manufacturers.Where(x=>x.IsImporter==false && x.Status == OrgStatus.Active).Count();
            organizationCountDto.MfImporterCount = manufacturers.Where(x => x.IsImporter==true && x.Status == OrgStatus.Active).Count();
            organizationCountDto.TpsafCount= _orderManagementDbContext.Organization.Where(x => x.OrgType == OrgType.Tpsaf).Count();
            organizationCountDto.PrintPartnerCount= _orderManagementDbContext.Organization.Where(x => x.OrgType == OrgType.PrintPartner).Count();
            organizationCountDto.ShipperCount = _orderManagementDbContext.Organization.Where(x => x.OrgType == OrgType.Shipper).Count();
            return organizationCountDto;
        }
        public TotalPymentDetailsDto GetTotalPaymentDetails(DBPaymentPagination pagination)
        {
            var payment = pagination.FromDate == null ? _orderManagementDbContext.Payments.Include(x=>x.Order).Include(x=>x.Order.Organization).ToList() : _orderManagementDbContext.Payments.Include(x => x.Order).Include(x => x.Order.Organization).Where(x=>x.ModifiedDate >= pagination.FromDate && x.ModifiedDate <= pagination.ToDate).ToList();

            var consumptionDetails = _orderManagementDbContext.Consumptions.Include(x => x.Product).GroupBy(x => x.Product.Origin).Select(x => new
            {
                ProductOrigin = x.Key,
                TotalStamps = x.Sum(s => s.TotalStamps)
            });

            var paymentDetails = new TotalPymentDetailsDto();
            paymentDetails.TotalAmount = Math.Round(payment.Sum(x => x.Amount),2).ToString("#,##0.00");
            paymentDetails.PaidAmount = Math.Round(payment.Where(x => x.PaymentStatus == PaymentStatus.Paid).Sum(x => x.Amount),2).ToString("#,##0.00");
            paymentDetails.UnpaidAmount = Math.Round(payment.Where(x => x.PaymentStatus != PaymentStatus.Paid).Sum(x => x.Amount),2).ToString("#,##0.00");
            paymentDetails.DomesticAffixAccount = consumptionDetails.Where(x => x.ProductOrigin == Origin.Domestic).SingleOrDefault() == null ? 0 : consumptionDetails.Where(x => x.ProductOrigin == Origin.Domestic).SingleOrDefault().TotalStamps;
            paymentDetails.ImportedAffixCount = consumptionDetails.Where(x => x.ProductOrigin == Origin.Imported).SingleOrDefault() == null ? 0 : consumptionDetails.Where(x => x.ProductOrigin == Origin.Imported).SingleOrDefault().TotalStamps;
            paymentDetails.TransitionAffixCount = consumptionDetails.Where(x => x.ProductOrigin == Origin.Transition).SingleOrDefault() == null ? 0 : consumptionDetails.Where(x => x.ProductOrigin == Origin.Transition).SingleOrDefault().TotalStamps;


            var orgPaymentDetails = payment.GroupBy(x => new { x.Order.OrganizationId, x.Order.Organization.Name}).Select(x=> new OrgPaymentDetails { OrganizationId = x.Key.OrganizationId,Name = x.Key.Name, totalAmount= Math.Round(x.Sum(x=>x.Amount),2).ToString("#,##0.00"), PaidAmount = Math.Round(x.Where(x=>x.PaymentStatus == PaymentStatus.Paid).Sum(a=>a.Amount),2).ToString("#,##0.00"), UnpaidAmount = Math.Round(x.Where(x=>x.PaymentStatus != PaymentStatus.Paid).Sum(a=>a.Amount),2).ToString("#,##0.00") }).ToList();

            paymentDetails.TotalRows = orgPaymentDetails.Count();

            pagination.PageNo = pagination.PageNo == 0 ? 1 : pagination.PageNo;
            pagination.PageSize = pagination.PageSize == 0 ? 10 : pagination.PageSize;
            orgPaymentDetails = orgPaymentDetails.Skip((pagination.PageNo - 1) * pagination.PageSize).Take(pagination.PageSize).ToList();

            paymentDetails.OrgPaymentDetails = orgPaymentDetails;

            return paymentDetails;
        }

    }
}
