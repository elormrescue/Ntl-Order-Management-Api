using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
   public class ReelsDataForFullFill
    {
        public Guid Id { get; set; }
        public string ReelCode { get; set; }
        public int ReelSize { get; set; }
        public string CartonCode { get; set; }
        public int CartonReelCount { get; set; }
        public Guid CartonId { get; set; }
        public int ProductId { get; set; }
    }
}
