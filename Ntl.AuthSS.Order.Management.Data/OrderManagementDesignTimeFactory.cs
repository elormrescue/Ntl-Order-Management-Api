using System.Reflection;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ntl.AuthSS.OrderManagement.Data
{
    public class OrderManagementDesignTimeFactory : IDesignTimeDbContextFactory<OrderManagementDbContext>
    {
        public OrderManagementDbContext CreateDbContext(string[] args)
        {
            var migrationsAssembly = typeof(OrderManagementDesignTimeFactory).GetTypeInfo().Assembly.GetName().Name;
            var optionsBuilder = new DbContextOptionsBuilder<OrderManagementDbContext>();
            //optionsBuilder.UseSqlServer("Server=tcp:ntlshield.database.windows.net,1433;Initial Catalog=ExciseMeta;Persist Security Info=False;User ID=ntlshieldadmin;Password=%3zJQi10qwr!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;", sql =>
            //optionsBuilder.UseSqlServer("Server=tcp:authss.database.windows.net,1433;Initial Catalog=AuthSS_QA_Db;Persist Security Info=False;User ID=authss_admin;Password=Welcome1@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=10000;", sql =>
            optionsBuilder.UseSqlServer("Server=tcp:authss.database.windows.net,1433;Initial Catalog=AuthSS_Db;Persist Security Info=False;User ID=authss_admin;Password=Welcome1@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=10000;", sql =>
            //optionsBuilder.UseSqlServer("Server=tcp:kyb.database.windows.net,1433;Initial Catalog=KYB_Db;Persist Security Info=False;User ID=kybadmin;Password=@run@564;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;", sql =>
            {
                sql.MigrationsAssembly(migrationsAssembly);
                sql.MigrationsHistoryTable("_OrderManagementMigrations");
                sql.EnableRetryOnFailure();
            });

            return new OrderManagementDbContext(optionsBuilder.Options, new ClaimsPrincipal());
        }
    }
}
