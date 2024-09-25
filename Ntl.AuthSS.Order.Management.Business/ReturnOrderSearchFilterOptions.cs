using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class ReturnOrderSearchFilterOptions
    {
        public int?[] EntityIds { get; set; }
        public string SearchText { get; set; }
        public OrgType? OrgType { get; set; }

       // public int?[] Locations { get; set; }
        public ReturnOrderStatus[] OrderStatuses { get; set; }
        public DateTime? OrdersFrom { get; set; }
        public DateTime? OrdersTill { get; set; }
        public string SortBy { get; set; }
        public bool SortByDesc { get; set; } 
        public int PageNo { get; set; }
        public int PageSize { get; set; }
    }
}
