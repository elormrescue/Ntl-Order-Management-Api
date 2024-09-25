using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class DeliveryFilterOptions
    {
        public Guid OrderId { get; set; }
        public DeliveryType DeliveryType { get; set; } 
        public string SearchText { get; set; }
        public string SortBy { get; set; }
        public bool SortByDesc { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
    }

    public enum DeliveryType
    {
        Mf=1,
        Tpsaf=2,
        //Transfer=3,
        Return=4
    }

    public enum DeliveryStatus
    {
        ReadyToBeDelivered=1,
        InTransit=2,
        Delivered=3
    }
}
