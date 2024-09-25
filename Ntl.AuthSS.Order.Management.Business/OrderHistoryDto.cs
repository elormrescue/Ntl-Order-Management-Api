using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class OrderHistoryDto
    {
        public Guid Id { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string ActionedBy { get; set; }
        public string StatusString { get; set; }
        public string Comments { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedDateString { get; set; }
        
    }
}
