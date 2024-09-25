using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class DBPaymentPagination
    {
        public string SortBy { get; set; }
        public bool SortByDesc { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
