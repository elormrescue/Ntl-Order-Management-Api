using Microsoft.EntityFrameworkCore;
using Ntl.AuthSS.OrderManagement.Data;
using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public class OrderMetaService : IOrderMetaService
    {
        private readonly OrderManagementDbContext _orderManagementDbContext;
        public OrderMetaService(OrderManagementDbContext orderManagementDbContext)
        {
            _orderManagementDbContext = orderManagementDbContext;
        }
        public Dictionary<string, string> GetBrandProducts(int orgId, int productId)
        {
            return _orderManagementDbContext.OrgBrandProducts.Where(x => x.Organization.Id == orgId && x.Product.Id == productId && x.IsActive && x.IsApproved)
                         .Select(x => new { x.Id, x.Name }).ToList().ToDictionary(x => x.Id.ToString(), x => x.Name);

        }

        public List<MiniProductDto> GetProducts(int orgId, OrderEntityType orderEntityType)
        {
            switch (orderEntityType)
            {
                case OrderEntityType.Manufacturer:
                    return _orderManagementDbContext.OrgProducts.Where(x => x.Organization.Id == orgId && x.IsActive)
                         .Select(x => new MiniProductDto() { Id = x.Product.Id, Name = x.Product.Name + "-" + x.Product.Origin.ToString(), ImageName = x.Product.ImageName }).ToList();
                case OrderEntityType.Ntl:
                    return _orderManagementDbContext.Products.Where(x => x.IsActive)
                         .Select(x => new MiniProductDto() { Id = x.Id, Name = x.Name + "-" + x.Origin.ToString(), ImageName = x.ImageName }).ToList();
                case OrderEntityType.Tpsaf:
                    return _orderManagementDbContext.Products.Where(x => x.IsActive && x.Origin == Origin.Imported)
                         .Select(x => new MiniProductDto() { Id = x.Id, Name = x.Name + "-" + x.Origin.ToString(), ImageName = x.ImageName }).ToList();
                default:
                    return null;
            }

        }
        public Dictionary<string, string> GetReelSizes(int productId, OrderEntityType orderEntityType)
        {
            var productReelSizes = _orderManagementDbContext.ProductReelSizes.Where(x => x.Product.Id == productId && x.IsActive);
            switch (orderEntityType)
            {
                case OrderEntityType.Manufacturer:
                    productReelSizes = productReelSizes.Where(x => x.CanManufacturerOrder);
                    break;
                case OrderEntityType.Tpsaf:
                    productReelSizes = productReelSizes.Where(x => x.CanTpsafOrder);
                    break;
            }
            return productReelSizes
                    .Select(x => new { x.ReelSize.Id, x.ReelSize.Size }).ToList().ToDictionary(x => x.Id.ToString(), x => x.Size.ToString());
        }

        public Dictionary<string, string> GetSkus(int productId)
        {
            return _orderManagementDbContext.ProductStockKeepingUnits.Where(x => x.Product.Id == productId && x.IsActive)
                    .Select(x => new { x.StockKeepingUnit.Id, x.StockKeepingUnit.Unit }).ToList().ToDictionary(x => x.Id.ToString(), x => x.Unit.ToString());
        }

        public decimal? GetStampPrice(int productId)
        {
            return _orderManagementDbContext.ProductPriceHistories.SingleOrDefault(x => x.Product.Id == productId && x.Status == PriceStatus.Active)?.Price;
        }

        public Dictionary<string, string> GetSuppliers(int? orgId)
        {
            if (orgId == null)
                return _orderManagementDbContext.Suppliers.Select(x => new { x.Id, x.Name }).ToList().ToDictionary(x => x.Id.ToString(), x => x.Name);
            return _orderManagementDbContext.OrgSuppliers.Where(x => x.Organization.Id == orgId && x.IsActive)
                    .Select(x => new { x.Supplier.Id, x.Supplier.Name }).ToList().ToDictionary(x => x.Id.ToString(), x => x.Name);
        }

        public Dictionary<string, string> GetWarehouses(int orgId)
        {
            return _orderManagementDbContext.OrgWarehouses.Where(x => x.Organization.Id == orgId)
                .Select(x => new { x.Warehouse.Id, x.Warehouse.Name }).ToList().ToDictionary(x => x.Id.ToString(), x => x.Name);
        }

        public Dictionary<string, string> GetPrintPartners()
        {
            return _orderManagementDbContext.Organization.Where(x => x.OrgType == OrgType.PrintPartner)
                .Select(x => new { x.Id, x.Name }).ToList().ToDictionary(x => x.Id.ToString(), x => x.Name);
        }

        public decimal GetOrderShippingCharges()
        {
            return 0;
        }

        public decimal GetOrderTax()
        {
            return 0;
        }

        public Dictionary<string, string> GetShippers()
        {
            return _orderManagementDbContext.Organization.Where(x => x.OrgType == OrgType.Shipper && x.Status == OrgStatus.Active)
                .Select(x => new { x.Id, x.Name }).ToList().ToDictionary(x => x.Id.ToString(), x => x.Name);
        }

        public Dictionary<string, string> GetNtlWarehouses()
        {
            var ntlOrganization = _orderManagementDbContext.Organization.Where(x => x.OrgType == OrgType.Ntl && x.Status == OrgStatus.Active).FirstOrDefault();
            return GetWarehouses(ntlOrganization.Id);
        }
        public decimal orgBalance(int orgId)
        {
            return _orderManagementDbContext.OrgWallets.Where(x => x.Organization.Id == orgId).OrderByDescending(x => x.ModifiedDate).Select(x => x.BalanceAmount).FirstOrDefault();
        }

        public Dictionary<string, string> GetProductOrigns(int orgId, OrderEntityType orderEntityType)
        {
            switch (orderEntityType)
            {
                case OrderEntityType.Manufacturer:
                    return _orderManagementDbContext.OrgProducts.Where(x => x.Organization.Id == orgId && x.IsActive)
                         .Select(x => new { x.Product.Id, x.Product.Origin }).ToList().ToDictionary(x => x.Id.ToString(), x => x.Origin.ToString());
                case OrderEntityType.Ntl:
                case OrderEntityType.Tpsaf:
                    return _orderManagementDbContext.Products
                         .Select(x => new { x.Id, x.Origin }).ToList().ToDictionary(x => x.Id.ToString(), x => x.Origin.ToString());
                default:
                    return null;
            }

        }

        public Dictionary<int, string> GetOrgBrandProducts(int orgId)
        {
            return _orderManagementDbContext.OrgBrandProducts.Where(b => b.Organization.Id == orgId).Select(b => new { b.Id, b.Name }).ToDictionary(b => b.Id, b => b.Name);
        }
        public List<TaxSlab> TaxSlabs(int status)
        {
            var taxSlabs = _orderManagementDbContext.TaxSlabs.ToList();
            if (status == 1)
                return taxSlabs.Where(x => x.Status == TaxSlabStatus.Active).ToList();
            else if (status == 2)
                return taxSlabs.Where(x => x.Status == TaxSlabStatus.Queued).ToList();
            else 
                return taxSlabs;
        }
        public async Task<int> AddTaxSlab(TaxSlabDto taxSlab)
        {
            var newTaxSlab = taxSlab.Id == 0 ? new TaxSlab() : _orderManagementDbContext.TaxSlabs.Where(x => x.Id == taxSlab.Id).FirstOrDefault();
            newTaxSlab.TaxType = taxSlab.TaxType;
            newTaxSlab.Percentage = taxSlab.NewPercentage;
            newTaxSlab.Status = TaxSlabStatus.Queued;
            newTaxSlab.IsCumulative = taxSlab.IsCumulative;
            newTaxSlab.ReflectingFrom = taxSlab.EffectiveFrom;

            _orderManagementDbContext.TaxSlabs.Add(newTaxSlab);

            return await _orderManagementDbContext.SaveChangesAsync();

        }
    }
}
