using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class PrintOrderSearchFilterOptions
    {
        public int? PrintPartnerId { get; set; }
        public string SearchText { get; set; }
        public int?[] Locations { get; set; }
        public PrintOrderStatus[] PrintOrderStatuses { get; set; }
        public DateTime? OrdersFrom { get; set; }
        public DateTime? OrdersTill { get; set; }
        public string SortBy { get; set; }
        public bool SortByDesc { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
    }
}
