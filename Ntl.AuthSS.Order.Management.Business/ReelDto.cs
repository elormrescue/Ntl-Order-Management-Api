using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class ReelDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public int StampCount { get; set; }
        public Guid CartonId { get; set; }
        public int ReelSize { get; set; }
        public bool IsUsed { get; set; }
        public PackageType PackageType { get; set; }
        public string ProductName { get; set; }
        //added for Trace Coil
        public string ProductNameOrgin { get; set; }
        public string CartonCode { get; set; }
        public string PrintOrderNumber { get; set; }
        public string Status { get; set; }
        public string OrgType { get; set; }
        public TraceOrderDetailDto TraceOrderDetailDto { get; set; }
        public ReturnOrderDto ReturnOrderDto { get; set; }
        //for print
        public string PalletCode { get; set; }
        public ReelDto(Reel reel)
        {
            if (reel != null)
            {
                Id = reel.Id;
                Code = reel.Code;
                StampCount = reel.StampCount;
                CartonId = reel.CartonId;
                ReelSize = reel.ReelSize;
                ProductNameOrgin = reel.Product.Name + " " + reel.Product.Origin;
                IsUsed = reel.IsUsedForFulfillment;
                PackageType = PackageType.Reel;
            }
        }
        public ReelDto()
        {

        }
    }

}
