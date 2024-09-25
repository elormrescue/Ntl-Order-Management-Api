using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business.StampEntities
{
    public class StampItem
    {
        public string id { get; set; }

        public string StampId { get; set; }

        public int StampType { get; set; }

        public string SerialCode { get; set; }

        public long SequenceNo { get; set; }

        public int StampStatus { get; set; }
    }

    public class GeneratedStampItem : StampItem
    {
        public DateTime GeneratedDateTime { get; set; }

        public string PrintOrderNo { get; set; }

        public string PrintOrderGuid { get; set; }
    }

    public class DownloadedStampItem : GeneratedStampItem
    {
        public DateTime DownloadedDateTime { get; set; } = DateTime.Now;

        public string CollectionName { get; set; }

        public string RequestGuid { get; set; }
    }

    public class UploadedStampItem : DownloadedStampItem
    {
        public DateTime UploadedDateTime { get; set; }

        public string ReelGuid { get; set; }

        public string ReelCode { get; set; }

        public int StampPosition { get; set; }

        public int ReelSize { get; set; }

        public string CartonCode { get; set; }

        public string CartonGuid { get; set; }

        public string PalletCode { get; set; }

        public string PalletGuid { get; set; }
    }

    public class DeliveredStampItem : UploadedStampItem
    {
        public string OrderGuid { get; set; }
        public string OrderNumber { get; set; }
        public string OrderItemGuid { get; set; }
        public int? OrganizationId { get; set; }
        public int? StockKeepingUnitId { get; set; }
        public int? ProductBrandID { get; set; }
        public int? WarehouseId { get; set; }

    }
}
