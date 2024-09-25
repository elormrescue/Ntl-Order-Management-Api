using Microsoft.EntityFrameworkCore;
using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System.Threading;
using System.Security.Principal;
using System.Security.Claims;

namespace Ntl.AuthSS.OrderManagement.Data
{
    public class OrderManagementDbContext: DbContext
    {
        private ClaimsPrincipal _principal;
        public OrderManagementDbContext()
        {
        }

        public OrderManagementDbContext(DbContextOptions<OrderManagementDbContext> options, IPrincipal principal)
            : base(options)
        {
            _principal = (ClaimsPrincipal)principal;
        }

        public DbSet<Organization> Organization { get; set; } //View 
        public DbSet<OrgBrandProduct> OrgBrandProducts { get; set; } //View
        public DbSet<OrgProduct> OrgProducts { get; set; } //View
        public DbSet<OrgSupplier> OrgSuppliers { get; set; } //View
        public DbSet<OrgWarehouse> OrgWarehouses { get; set; } //View
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderHistory> OrderHistories { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderItemReel> OrderItemReels { get; set; }
        public DbSet<Container> Containers { get; set; }
        public DbSet<Carton> Cartons { get; set; }
        public DbSet<Pallet> Pallets { get; set; }
        public DbSet<Reel> Reels { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentHistory> PaymentHistories { get; set; }
        public DbSet<Product> Products { get; set; } //View
        public DbSet<ProductPriceHistory> ProductPriceHistories { get; set; } //View
        public DbSet<ProductReelSize> ProductReelSizes { get; set; } //View
        public DbSet<ProductStockKeepingUnit> ProductStockKeepingUnits { get; set; } //View
        public DbSet<ReelSize> ReelSizes { get; set; } //View
        public DbSet<StockKeepingUnit> StockKeepingUnits { get; set; } //View
        public DbSet<Supplier> Suppliers { get; set; } //View        
        public DbSet<Warehouse> Warehouses { get; set; }//View
        public DbSet<PrintOrder> PrintOrders { get; set; }
        public DbSet<PrintOrderHistory> PrintOrderHistories { get; set; }
        public DbSet<InternalStockRequest> InternalStockRequests { get; set; }
        public DbSet<InternalStockRequestHistory> InternalStockRequestHistories { get; set; }
        public DbSet<InternalStockRequestReel> InternalStockRequestReels { get; set; }
        public DbSet<ReturnOrder> ReturnOrders { get; set; }
        public DbSet<ReturnOrderHistory> ReturnOrderHistories { get; set; }
        public DbSet<ReturnOrderReels> ReturnOrderReels { get; set; }
        public DbSet<ReelChangeRequest> ReelChangeRequests { get; set; }
        public DbSet<ReelChangeRequestReel> ReelChangeRequestReels { get; set; }
        public DbSet<ReelChangeRequestHistory> ReelChangeRequestHistories { get; set; }
        public DbSet<OrgWallet> OrgWallets { get; set; }
        public DbSet<Address> Addresses { get; set; }  //View
        public DbSet<Contact> Contacts { get; set; }  //View
        public DbSet<Region> Regions { get; set; }  //View
        public DbSet<Country> Countries { get; set; }  //View
        public DbSet<PrintOrderRequest> PrintOrderRequests { get; set; }
        public virtual DbSet<ReelsDataForFullFill> ReelsDataForFullFills { get; set; }
        public DbSet<TaxSlab> TaxSlabs { get; set; }
        public DbSet<OrderPaymentBreakdown> OrderPaymentBreakdowns { get; set; }
        public DbSet<Consumption> Consumptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Use the entity name instead of the Context.DbSet<T> name
                // refs https://docs.microsoft.com/en-us/ef/core/modeling/relational/tables#conventions
                modelBuilder.Entity(entityType.ClrType).ToTable(entityType.ClrType.Name);
            }

            modelBuilder.Entity<Organization>(b =>
            {
                b.ToView("Organization");
            });
            modelBuilder.Entity<OrgBrandProduct>(b =>
            {
                b.ToView("OrgBrandProduct");
            });
            modelBuilder.Entity<OrgProduct>(b =>
            {
                b.ToView("OrgProduct");
            });
            modelBuilder.Entity<OrgSupplier>(b =>
            {
                b.ToView("OrgSupplier");
            });
            modelBuilder.Entity<Product>(b =>
            {
                b.ToView("Product");
            });
            modelBuilder.Entity<ProductPriceHistory>(b =>
            {
                b.ToView("ProductPriceHistory");
            });
            modelBuilder.Entity<ProductReelSize>(b =>
            {
                b.ToView("ProductReelSize");
            });
            modelBuilder.Entity<ProductStockKeepingUnit>(b =>
            {
                b.ToView("ProductStockKeepingUnit");
            });

            modelBuilder.Entity<ReelSize>(b =>
            {
                b.ToView("ReelSize");
            });
            modelBuilder.Entity<StockKeepingUnit>(b =>
            {
                b.ToView("StockKeepingUnit");
            });
            modelBuilder.Entity<Supplier>(b =>
            {
                b.ToView("Supplier");
            });
            
            modelBuilder.Entity<Warehouse>(b =>
            {
                b.ToView("Warehouse");
            });
            modelBuilder.Entity<OrgWarehouse>(b =>
            {
                b.ToView("OrgWarehouse");
            });
            modelBuilder.Entity<Address>(b =>
            {
                b.ToView("Address");
            });
            modelBuilder.Entity<Contact>(b =>
            {
                b.ToView("Contact");
            });
            modelBuilder.Entity<Region>(b =>
            {
                b.ToView("Region");
            });
            modelBuilder.Entity<Country>(b =>
            {
                b.ToView("Country");
            });

            modelBuilder.Entity<ReelsDataForFullFill>(e =>
            {
                e.ToView("ReelsDataForFullFill");
                e.HasNoKey();
            });
            modelBuilder.Entity<Consumption>(e =>
            {
                e.ToView("Consumption");
            });

            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            SetAuditingFields();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override int SaveChanges()
        {
            SetAuditingFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            SetAuditingFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        public int? GetLoggedInUser()
        {
            var userClaim = _principal.Claims.SingleOrDefault(x=>x.Type == ClaimTypes.NameIdentifier);
            if (userClaim != null)
                return Convert.ToInt32(userClaim.Value);

            return 1; //Return 1 if user is not found. Ideally this shouldn't happen
        }

        public void SetAuditingFields()
        {
            var user = GetLoggedInUser();
            if (user == null)
                throw new InvalidOperationException("Seems like user has not logged in the system. Make sure you set the current principal of the main thread appropriately and add user Id claim.");

            ChangeTracker.Entries().Where(x => (x.State == EntityState.Added || x.State == EntityState.Modified) && x.Entity is IAuditable).ToList().ForEach(x => SetUserInfo((IAuditable)x.Entity, user.Value));
        }
        private void SetUserInfo(IAuditable auditable, int userId)
        {
            if (auditable.CreatedUser == 0)
                auditable.CreatedUser = userId;
            if (auditable.CreatedDate == DateTime.MinValue)
                auditable.CreatedDate = DateTime.Now;

            auditable.ModifiedUser = userId;
            auditable.ModifiedDate = DateTime.Now;
        }
    }
}
