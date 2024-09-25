using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class InternalStockRequestDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProdOrigin { get; set; }
        public string ImageName { get; set; }
        public int? OrgId { get; set; }
        public string OrgName { get; set; }
        public int NoOfStamps { get; set; }
        public int RequestingFacilityId { get; set; }
        public string RequestingFacilityName { get; set; }
        public int? ApprovingFacilityId { get; set; }
        public string ApprovingFacilityName { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public ICollection<InterStockRequestHistoryDto> InternalStockRequestHistories { get; set; }
    }
}
