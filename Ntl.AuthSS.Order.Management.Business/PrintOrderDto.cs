using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class PrintOrderDto
    {
        public Guid Id { get; set; }
        public string PoNum { get; set; }
        public string Number { get; set; }
        public int NoOfOrders { get; set; }

        public PrintOrderStatus Status { get; set; }
        public string StatusString { get; set; }

        public DateTime ExpectedDate { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageName { get; set; }
        public int ReelSizeId { get; set; }
        public int ReelSize { get; set; }
        public int NoOfReels { get; set; }
        public int TotalStamps { get; set; }
        public int PrintPartnerId { get; set; }
        public string PrintPartnerName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string Comments { get; set; }
        public ICollection<PrintOrderHistoryDto> PrintOrderHistories { get; set; }
    }
}
