using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Security.Principal;
using System.Security.Claims;
using Ntl.AuthSS.OrderManagement.Queryable.Entities;

namespace Ntl.AuthSS.OrderManagement.Queryable
{
    public class OrderManagementQueryableDbContext : DbContext
    {
        private ClaimsPrincipal _principal;
        public OrderManagementQueryableDbContext()
        {
        }

        public OrderManagementQueryableDbContext(DbContextOptions<OrderManagementQueryableDbContext> options, IPrincipal principal)
            : base(options)
        {
            _principal = (ClaimsPrincipal)principal;
        }

        public virtual DbSet<DashboardOrderDetailsResponse> DashboardOrderDetailsResponses { get; set; }
        public virtual DbSet<MFStampDetailsForSuppliers> MFStampDetailsForSuppliers { get; set; }
        public virtual DbSet<MFStampCountForBrands> MFStampCountForBrands { get; set; }
        public virtual DbSet<MFStampDetailsBasedOnProductSku> MFStampDetailsBasedOnProductSkus { get; set; }
        public virtual DbSet<PrintOrderCountDetails> PrintOrderCountDetails { get; set; }
        public virtual DbSet<PrintOrderStampDetailsBasedOnProduct>  PrintOrderStampDetailsBasedOnProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                modelBuilder.Entity(entityType.ClrType).ToTable(entityType.ClrType.Name);
            }

            modelBuilder.Entity<DashboardOrderDetailsResponse>(e =>
            {
                e.HasNoKey();
            }); 
            modelBuilder.Entity<MFStampDetailsForSuppliers>(e =>
                 {
                     e.HasNoKey();
                 });
            modelBuilder.Entity<MFStampCountForBrands>(e =>
            {
                e.HasNoKey();
            });
            modelBuilder.Entity<MFStampDetailsBasedOnProductSku>(e =>
            {
                e.HasNoKey();
            });
            modelBuilder.Entity<PrintOrderCountDetails>(e =>
            {
                e.HasNoKey();
            });
            modelBuilder.Entity<PrintOrderStampDetailsBasedOnProduct>(e =>
            {
                e.HasNoKey();
            }); 
            base.OnModelCreating(modelBuilder);
        }

     
    }
}
