using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public interface IOrderMetaService
    {
        Dictionary<string, string> GetSuppliers(int? orgId);
        List<MiniProductDto> GetProducts(int orgId, OrderEntityType orderEntityType);
        Dictionary<string, string> GetProductOrigns(int orgId, OrderEntityType orderEntityType);
        Dictionary<string, string> GetBrandProducts(int orgId, int productId);
        Dictionary<int, string> GetOrgBrandProducts(int orgId);
        Dictionary<string, string> GetReelSizes(int productId, OrderEntityType orderEntityType);
        Dictionary<string, string> GetSkus(int productId);
        decimal? GetStampPrice(int productId);
        Dictionary<string, string> GetWarehouses(int orgId);
        Dictionary<string, string> GetPrintPartners();
        decimal GetOrderShippingCharges();
        decimal GetOrderTax();
        Dictionary<string, string> GetShippers();
        Dictionary<string, string> GetNtlWarehouses();
        decimal orgBalance(int orgId);
        List<TaxSlab> TaxSlabs(int status);
        Task<int> AddTaxSlab(TaxSlabDto taxSlab);
    }
}
