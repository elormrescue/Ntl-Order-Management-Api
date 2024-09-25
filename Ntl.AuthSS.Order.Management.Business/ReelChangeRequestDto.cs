using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class ReelChangeRequestDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public int OrgId { get; set; }
        public string OrgName { get; set; }
        public ReelChangeType ReelChangeType { get; set; }
        public int ReelChangeProductId { get; set; }
        public string ReelChangeProductName { get; set; }
        public int? ChangeToProductId { get; set; }
        public string ChangeToProductName { get; set; }
        public int? ChangeToSkuId { get; set; }
        public string ChangeToSkuName { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
        public ICollection<ReelChangeRequestReelDto> ReelChangeRequestReels { get; set; }

        public ICollection<ReelChangeRequestHistoryDto> ReelChangeRequestHistories { get; set; }
    }

    public class ReelChangeRequestReelDto
    {
        public Guid Id { get; set; }
        public Guid ReelChangeRequestId { get; set; }
        public Guid ReelId { get; set; }
        public string ReelCode { get; set; }
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public string OldProductName { get; set; }
        public string OldSku { get; set; }
    }

    public class ReelChangeRequestHistoryDto
    {
        public Guid Id { get; set; }
        public Guid ReelChangeRequestId { get; set; }
        public string Action { get; set; }
        public string ActionnedBy { get; set; }
        public string Comments { get; set; }
        public string ModifiedDate { get; set; }
    }
}
