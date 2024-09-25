using System;
using System.Collections.Generic;
using System.Text;
using Ntl.AuthSS.OrderManagement.Data.Entities;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class StampGenerationFilterDto
    {
        public StampGenerationStatus[] Status { get; set; }
        public string SearchText { get; set; }
        public string SortBy { get; set; }
        public bool SortByDesc { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int[] PrintPartners { get; set; }
    }
}
