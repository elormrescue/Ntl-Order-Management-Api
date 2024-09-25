using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business.Helper
{
    public static class Extensions
    {
        public static String ToDisplayDate(this DateTime dt)
        {
            return dt.ToString("dd-MMM-yyyy");
        }
        public static String ToDisplayDate(this DateTime? dt)
        {
            if (dt.HasValue)
            {
                return dt.Value.ToDisplayDate();
            }
            return "";
        }
    }
}
