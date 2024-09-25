using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class PalletDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public PackageType PackageType { get; set; }
        public IEnumerable<CartonDto> Cartons { get; set; }
        public string Container { get; set; }
        public string PrintOrder { get; set; }

    }
}
