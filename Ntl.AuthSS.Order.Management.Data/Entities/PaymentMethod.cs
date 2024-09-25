namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class PaymentMethod
    {
        public virtual int Id { get; set; }

        public virtual string Provider { get; set; }

        public virtual string ServiceName { get; set; }

        public virtual string MethodName { get; set; }
    }
}