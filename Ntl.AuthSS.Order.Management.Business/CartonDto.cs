using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class CartonDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public PackageType PackageType { get; set; }
        public IEnumerable<ReelDto> Reels { get; set; }
        public string ProductName { get; set; }
        public int ReelCount { get; set; }
        //Added for trace Package
        public int UsedReelCount { get; set; }
        public int UnUsedReelCount { get; set; }
        public string PalletName { get; set; }
        public string ProductNameOrgin { get; set; }
        public string PrintOrder { get; set; }
        public int ReelSize { get; set; }
        //added for print
        public string PalletCode { get; set; }

    }
}
