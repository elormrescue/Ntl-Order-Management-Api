using System;

namespace Ntl.AuthSS.OrderManagement.Data.Interfaces
{
    public interface IAuditable
    {
        int CreatedUser { get; set; }
        DateTime CreatedDate { get; set; }
        int ModifiedUser { get; set; }
        DateTime ModifiedDate { get; set; }
    }
}
