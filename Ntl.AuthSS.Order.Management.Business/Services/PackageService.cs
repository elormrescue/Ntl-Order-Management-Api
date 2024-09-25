using Microsoft.EntityFrameworkCore;
using Ntl.AuthSS.OrderManagement.Data;
using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public class PackageService : IPackageService
    {
        private readonly OrderManagementDbContext _orderManagementDbContext;

        public PackageService(OrderManagementDbContext orderManagementDbContext)
        {
            _orderManagementDbContext = orderManagementDbContext;
        }
        public (ReelDto reel, string errorMessage) GetReelDetailsFromCodeForFullfillment(string reelCode, Guid orderItemId)
        {
            var reel = _orderManagementDbContext.Reels.Include(p => p.Product).Where(a => a.Code == reelCode).FirstOrDefault();
            if (reel == null)
                return (null, "Reel not found");
            var orderItem = _orderManagementDbContext.OrderItems.Include(x => x.Product).Include(s => s.ReelSize).Where(x => x.Id == orderItemId).FirstOrDefault();
            if (orderItem == null)
                return (null, "Invalid order item");

            if (reel.Product.Id != orderItem.Product.Id)
            {
                return (null, "Product type of order item and added reel differ,Cannot add this reel for this order item");
            }
            else if (reel.ReelSize != orderItem.ReelSize.Size)
            {
                return (null, "Reel Size do not match");
            }
            if (reel.LockedOrderItemId != null)
            {
               // if (reel.FulfillLockedDate.Value.AddMinutes(30) > DateTime.UtcNow)
                return (null, "This reel is already locked for fullfilment.");
            }
            reel.FulfillLockedDate = DateTime.Now;
            reel.LockedOrderItemId = orderItemId;
            var reelDto = new ReelDto(reel);
            return (reelDto, null);
        }

        public (CartonDto carton, string errorMessage) GetCartonDetailsFromCodeForFulfillment(string cartonCode, Guid orderItemId)
        {
            var carton = _orderManagementDbContext.Cartons.Include(x => x.Reels).ThenInclude(p => p.Product).Where(a => a.Code == cartonCode).FirstOrDefault();
            if (carton == null)
                return (null, "Carton not found");
            var orderItem = _orderManagementDbContext.OrderItems.Include(x => x.Product).Include(s => s.ReelSize).Where(x => x.Id == orderItemId).FirstOrDefault();
            if (orderItem == null)
                return (null, "Invalid OrderItem");
            if (carton.Reels.Where(x => x.Product.Id != orderItem.Product.Id).Count() > 0)
                return (null, "Product type of order item and added carton differ,Cannot add this Carton for this order item");
            var notMatchingReelSizeCount = carton.Reels.Where(z => z.ReelSize != orderItem.ReelSize.Size).Any();
            if (notMatchingReelSizeCount)
                return (null, "Reel Size do not match");

            var reels = new List<ReelDto>();
            foreach (var reel in carton.Reels)
            {
                var reelDto = new ReelDto(reel);
                if (reel.LockedOrderItemId != null)
                {
                    reelDto.IsUsed = true;
                }
                //reel.LockedOrderItemId = orderItemId;
                //reel.FulfillLockedDate = DateTime.Now;
                reels.Add(reelDto);
            }
             _orderManagementDbContext.SaveChanges();
            var cartonDto = new CartonDto();
            cartonDto.Code = carton.Code;
            cartonDto.Id = carton.Id;
            cartonDto.Reels = reels;
            cartonDto.PackageType = PackageType.Carton;
            return (cartonDto, null);
        }

        public (ReelDto reel, string errorMessage) GetReelDetailsFromCodeForReturnOrder(string reelCode, int orgId)
        {
            var orderItemReel = _orderManagementDbContext.OrderItemReels.Include(p => p.Reel).Include(o => o.Organization).Include(o => o.Product).Where(a => a.Reel.Code == reelCode && a.IsReturned == false).FirstOrDefault();
            if (orderItemReel == null)
                return (null, "Reel not found");
            if (orderItemReel.Organization.Id != orgId)
                return (null, "Reel does not belong to the organization");
            if (orderItemReel.ReelConsumptionType == ReelConsumptionType.Consumed)
                return (null, "Reel has been consumed");
            if (_orderManagementDbContext.ReturnOrderReels.Where(r => r.ReelCode == reelCode).Any(r => r.ReturnOrder.Status != ReturnOrderStatus.Closed))
                return (null, "This reel has been already returned");
            var reelDto = new ReelDto(orderItemReel.Reel);
            reelDto.ProductName = orderItemReel.BrandProductName;
            return (reelDto, null);
        }

        public (CartonDto carton, string errorMessage) GetCartonDetailsFromCodeForReturnOrder(string cartonCode, int orgId)
        {
            var carton = _orderManagementDbContext.Cartons.Include(x => x.Reels).Where(a => a.Code == cartonCode).FirstOrDefault();
            if (carton == null)
                return (null, "Carton not found");
            var orderItemReels = _orderManagementDbContext.OrderItemReels.Include(o => o.Organization).Include(c => c.Carton).Where(o => o.Carton.Id == carton.Id && o.IsReturned == false).ToList();
            if (orderItemReels.Count() != carton.ReelCount)
            {
                return (null, "Cannot add the whole carton for return since some reels are not present");
            }
            if (orderItemReels.Where(o => o.Organization.Id != orgId).Any())
            {
                return (null, "Cannot add the whole carton since some reels do not belong to the organization");
            }
            if (orderItemReels.Where(o => o.ReelConsumptionType == ReelConsumptionType.Consumed).Any())
            {
                return (null, "Cannot add the whole carton since some reels are already consumed");
            }
            if (_orderManagementDbContext.ReturnOrderReels.Where(r => orderItemReels.Select(r => r.Reel).Contains(r.Reel)).Any(r => r.ReturnOrder.Status != ReturnOrderStatus.Closed))
                return (null, "Some reels in the carton are already submitted for a different return order");

            var reels = new List<ReelDto>();
            foreach (var reel in carton.Reels)
            {
                var reelDto = new ReelDto(reel);
                reels.Add(reelDto);
            }
            var cartonDto = new CartonDto();
            cartonDto.Code = carton.Code;
            cartonDto.Id = carton.Id;
            cartonDto.Reels = reels;
            cartonDto.PackageType = PackageType.Carton;
            cartonDto.ProductName = orderItemReels.FirstOrDefault().BrandProductName;
            cartonDto.ReelCount = carton.ReelCount;
            return (cartonDto, null);
        }


        public (ReelDto reel, string errorMessage) TraceReel(string reelCode)
        {
            var reel = _orderManagementDbContext.Reels
                .Include(p => p.PrintOrder)
                .Include(c => c.Carton)
                .Include(p => p.Product)
                .Where(a => a.Code == reelCode).FirstOrDefault();
            if (reel == null)
                return (null, "Reel not found");
            var reelDto = new ReelDto(reel);
            reelDto.ProductNameOrgin = reel.Product.Name + "-" + reel.Product.Origin.ToString();
            reelDto.CartonCode = reel.Carton.Code;
            reelDto.PrintOrderNumber = reel.PrintOrder.Number;
            reelDto.Status = reel.Status.ToString();

            var orderItemReel = _orderManagementDbContext.OrderItemReels
                .Include(o => o.OrderItem).ThenInclude(y => y.Order)
                .Include(p => p.Organization)
                .Include(z => z.InternalStockRequest).ThenInclude(x => x.RequestingFacility)
                .Include(z => z.InternalStockRequest).ThenInclude(x => x.ApprovingFacility)
                .Where(x => x.Reel.Id == reel.Id).ToList().LastOrDefault();

            if (orderItemReel != null)
            {
                TraceOrderDetailDto traceOrderDetailDto = new TraceOrderDetailDto();
                traceOrderDetailDto.BrandProductName = orderItemReel.BrandProductName;
                traceOrderDetailDto.IsReturned = orderItemReel.IsReturned;
                traceOrderDetailDto.NewBrandProductName = orderItemReel.NewBrandProductName;
                traceOrderDetailDto.NewSkuName = orderItemReel.NewSkuName;
                traceOrderDetailDto.OrderId = orderItemReel.OrderItem.Order.Id;
                traceOrderDetailDto.OrderNumber = orderItemReel.OrderItem.Order.Number;
                traceOrderDetailDto.OrganizationName = orderItemReel.OrganizationName;
                traceOrderDetailDto.SkuName = orderItemReel.SkuName;
                traceOrderDetailDto.WarehouseName = orderItemReel.WarehouseName;
                if (orderItemReel.InternalStockRequest != null)
                {
                    traceOrderDetailDto.StockRequestNumber = orderItemReel.InternalStockRequest.Number;
                    traceOrderDetailDto.StockEarlierFacility = orderItemReel.InternalStockRequest.ApprovingFacility.Name;
                    traceOrderDetailDto.StockCurrentFacility = orderItemReel.InternalStockRequest.RequestingFacility.Name;
                }
                reelDto.TraceOrderDetailDto = traceOrderDetailDto;

                var returnOrderitem = _orderManagementDbContext.ReturnOrderReels
                 .Include(r => r.ReturnOrder).ThenInclude(r => r.Organization)
                 .Where(r => r.Reel.Id == reel.Id).ToList().LastOrDefault();
                if (returnOrderitem != null)
                {
                    ReturnOrderDto returnOrderDto = new ReturnOrderDto();
                    returnOrderDto.Organization = returnOrderitem.ReturnOrder.Organization.Name;
                    returnOrderDto.Number = returnOrderitem.ReturnOrder.Number;
                    returnOrderDto.ExpectedDeliveryDate = returnOrderitem.ReturnOrder.ModifiedDate;
                    reelDto.ReturnOrderDto = returnOrderDto;
                }
            };

            return (reelDto, null);
        }

        public (CartonDto carton, string errorMessage) TraceCarton(string cartonCode)
        {
            var carton = _orderManagementDbContext.Cartons
               .Include(p => p.Reels).ThenInclude(p => p.PrintOrder)
               .Include(c => c.Pallet)
               .Include(p => p.Product)
               .Where(a => a.Code == cartonCode).FirstOrDefault();
            if (carton == null)
                return (null, "carton not found");
            var cartonDto = new CartonDto();
            cartonDto.ReelCount = carton.ReelCount;
            cartonDto.Code = carton.Code.ToString();
            cartonDto.ProductNameOrgin = carton.Product.Name + "-" + carton.Product.Origin.ToString();
            cartonDto.UsedReelCount = carton.Reels.Where(x => x.IsUsedForFulfillment == true).Count();
            cartonDto.UnUsedReelCount = cartonDto.ReelCount - cartonDto.UsedReelCount;
            cartonDto.PrintOrder = carton.Reels.FirstOrDefault().PrintOrder.Number;
            var reels = new List<ReelDto>();

            foreach (var reel in carton.Reels)
            {
                var reelDto = new ReelDto();
                reelDto.Code = reel.Code;
                reelDto.StampCount = reel.StampCount;
                reelDto.IsUsed = reel.IsUsedForFulfillment;
                cartonDto.ReelSize = reel.ReelSize;
                if (reel.IsUsedForFulfillment)
                {
                    var orderItemReel = _orderManagementDbContext.OrderItemReels
                                       .Include(o => o.OrderItem).ThenInclude(y => y.Order)
                                       .Include(p => p.Organization)
                                       .Where(r => r.Reel.Id == reel.Id)
                                       .ToList().LastOrDefault();
                    if (orderItemReel != null)
                    {

                        TraceOrderDetailDto traceOrderDetailDto = new TraceOrderDetailDto();
                        traceOrderDetailDto.OrderId = orderItemReel.OrderItem.Order.Id;
                        traceOrderDetailDto.OrderNumber = orderItemReel.OrderItem.Order.Number;
                        traceOrderDetailDto.OrganizationName = orderItemReel.OrganizationName;
                        traceOrderDetailDto.SkuName = orderItemReel.SkuName;
                        traceOrderDetailDto.WarehouseName = orderItemReel.WarehouseName;
                        traceOrderDetailDto.IsReturned = orderItemReel.IsReturned;
                        reelDto.TraceOrderDetailDto = traceOrderDetailDto;
                        reelDto.OrgType = orderItemReel.Organization.OrgType.ToString();
                        reelDto.Status = reel.Status.ToString();
                    }
                }
                else
                {
                    reelDto.OrgType = OrgType.Ntl.ToString();
                    reelDto.Status = reel.Status.ToString();
                }
                reels.Add(reelDto);
            }
            cartonDto.Reels = reels;
            return (cartonDto, null);
        }


        public (PalletDto pallet, string errorMessage) TracePallet(string palletCode)
        {
            var pallet = _orderManagementDbContext.Pallets
              .Include(p => p.Cartons).ThenInclude(c => c.Reels)
              .Include(p => p.Cartons).ThenInclude(p => p.Product)
               .Where(a => a.Code == palletCode).FirstOrDefault();
            if (pallet == null)
                return (null, "pallet not found");

            var palletDto = new PalletDto();
            palletDto.Code = pallet.Code;
            var cartonList = new List<CartonDto>();
            foreach (var carton in pallet.Cartons)
            {
                var cartonDto = new CartonDto();
                cartonDto.Code = carton.Code;
                cartonDto.ReelCount = carton.ReelCount;
                cartonDto.ProductNameOrgin = carton.Product.Name + "-" + carton.Product.Origin.ToString();
                cartonDto.UnUsedReelCount = carton.Reels.Where(x => x.IsUsedForFulfillment == false).Count();
                cartonDto.UsedReelCount = carton.Reels.Where(x => x.IsUsedForFulfillment == true).Count();
                cartonDto.ReelSize = carton.Reels.FirstOrDefault().ReelSize;
                cartonList.Add(cartonDto);
            }
            palletDto.Cartons = cartonList;
            return (palletDto, null);
        }


    }


}
