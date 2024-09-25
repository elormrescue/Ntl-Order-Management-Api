using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class InternalStockRequestFilterOptions
    {
        public string SearchText { get; set; }
        public int?[] Locations { get; set; }
        public InternalStockTransferStatus[] Statuses { get; set; }
        public DateTime? RequestsFrom { get; set; }
        public DateTime? RequestsTill { get; set; }
        public string SortBy { get; set; }
        public bool SortByDesc { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
    }
}
