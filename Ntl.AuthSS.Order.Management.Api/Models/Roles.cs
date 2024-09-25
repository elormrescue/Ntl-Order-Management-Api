using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ntl.AuthSS.Order_Management.Api.Models
{
    public enum Roles
    {
        MfAdmin,
        MfAccountManager,
        MfWarehouseIncharge,
        TaxAuthAdmin,
        TaxAuthRevenueOfficer,
        TpsafAdmin,
        TpsafFacilityAdmin,
        TpsafFacilityIncharge,
        TsspAdmin,
        TsspIntermediate,
        TsspWarehouseIncharge,
        PrintPartner,
        ShipperAdmin,
        ShipperAgent
    }
}
