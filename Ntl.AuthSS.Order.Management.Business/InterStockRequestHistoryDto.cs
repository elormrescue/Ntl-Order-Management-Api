using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class InterStockRequestHistoryDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; }
        public string UserName { get; set; }
        public string Notes { get; set; }
        public string ModifiedDate { get; set; }
    }
}
