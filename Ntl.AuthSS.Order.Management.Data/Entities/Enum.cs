using System.ComponentModel.DataAnnotations;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public enum Origin
    {
        Domestic = 1,
        Imported = 2,
        Transition = 3
    }
    public enum OrderEntityType
    {
        Manufacturer=1,
        Tpsaf=2,
        Ntl=3,
        Shipper=4,
        TaxAuth=5
        //Add more entities here
    }

    public enum OrderStatus
    {
        Submitted=1,
        InConsideration = 2,
        Rejected=3,
        Resubmitted=4,
        InProcessing=5,
        Fullfilled=6,
        InTransit=7,
        Delivered=8,
        Closed=9,
        Expired
    }

    public enum PaymentStatus
    {
        Paid = 1,
        Unpaid = 2,
        InProcess = 3

    }

    public enum PaymentMode
    {
        Online=1,
        Offline=2
    }

    public enum PriceStatus
    {
        Active = 1,
        Queued = 2,
        Obsolete = 3
    }

    public enum ReelStatus
    {
        Partial = 1,
        Printed = 2,
        InStock = 3,
        //Delivered = 4,
        //Reconciled = 5,
        Archived = 6
    }

    public enum PrintOrderStatus
    {
        Submitted=1,
        Processing=2,
        Rejected=3,
        InTransit=4,
        Closed=5
    }

    public enum OrgType
    {
        Ntl = 1,
        TaxAuthority = 2,
        Manufacturer = 3,
        Tpsaf = 4,
        PrintPartner = 5, //Print Partner
        Shipper = 6
    }

    public enum InternalStockTransferStatus
    {
        Requested = 1,
        Approved = 2,
        InTransit = 3,
        Closed = 4,
        Expired = 5
    }

    public enum ReturnOrderStatus
    {
        Submitted = 1,
        InTransit = 2,
        Delivered = 3,
        Closed = 4
    }
    public enum OrgStatus
    {
        Active = 1,
        Inactive = 2
    }

    public enum PackageType
    {
        Reel=1,
        Carton=2,
        Pallet=3
    }

    public enum ReelChangeType
    {
        Sku=1,
        Product=2,
        Both =3,
    }

    public enum ReelChangeRequestStatus
    {
        Submitted=1,
        Approved=2,
        Rejected=3
    }
    public enum TransactionType
    {
        Credit = 1,
        Debit = 2
    }
    public enum WalletOrderType
    {
        RegularOrder = 1,
        ReturnOrder = 2,
        AddMoney=3,
        DebitMoney=4
    }

    public enum ReelConsumptionType
    {
        NotConsumed=1,
        Consumed=2,
        PartiallyConsumed=3
    }
    public enum StampGenerationStatus
    {
        Queued = 1,
        InProcess = 2,
        Generated = 3
    }
    public enum ntlNotificationType
    {
        ForceLogout = 1,
        OrderFullFill = 2,
        ReturnOrder = 3,
    }
    public enum TaxSlabStatus
    {
        Active = 1,
        Queued = 2,
        Obsolete = 3
    }

}
