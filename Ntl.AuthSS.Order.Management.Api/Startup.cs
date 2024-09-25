using System;
using CorePush.Google;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ntl.AuthSS.Order_Management.Api.Authorization;
using Ntl.AuthSS.Order_Management.Api.ConfigModels;
using Ntl.AuthSS.Order_Management.Api.Models;
using Ntl.AuthSS.OrderManagement.Business;
using Ntl.AuthSS.OrderManagement.Business.Services;
using Ntl.AuthSS.OrderManagement.Data;
using Ntl.AuthSS.OrderManagement.Queryable;
using Ntl.Tss.Identity.Data;
using Utilities.NotificationHelper;
using Twilio;
using System.Net.Http;
using Ntl.AuthSS.Order_Management.Api.HttpClients;

namespace Ntl.AuthSS.OrderManagement.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _currentEnvironment;
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            _currentEnvironment = environment;
        }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddDbContext<OrderManagementDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("AuthSSDb"), sql =>
                //options.UseSqlServer("Server=tcp:ntl-authss.database.windows.net,1433;Initial Catalog=tssp_db_dev1;Persist Security Info=False;User ID=ntladmin;Password=admin@564;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;", sql =>
                {
                    sql.MigrationsHistoryTable("_OrderManagementMigrations");
                });
            });
            services.AddDbContext<TssIdentityDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("IdentityDb"),
                    b => b.MigrationsHistoryTable("_Migrations"));
            });
            services.AddDbContext<OrderManagementQueryableDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("AuthSSDb"));
            });
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = Configuration.GetValue<string>("HostingApp:Url");
                    options.RequireHttpsMetadata = false;
                    options.Audience = Configuration.GetValue<string>("ApiName");
                });
            services.AddCors(options => options.AddPolicy("AllowCors", builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); }));
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
                });
            });

            services.AddAuthorization(options =>
            {
                /*Order Policies*/
                options.AddPolicy("CanCreateOrders", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.MfAccountManager.ToString(), Roles.MfAdmin.ToString(), Roles.MfWarehouseIncharge.ToString(), Roles.TpsafAdmin.ToString(), Roles.TpsafFacilityAdmin.ToString(), Roles.TpsafFacilityIncharge.ToString() }));
                options.AddPolicy("CanViewOrders", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.MfAccountManager.ToString(), Roles.MfAdmin.ToString(), Roles.MfWarehouseIncharge.ToString(), Roles.TpsafAdmin.ToString(), Roles.TpsafFacilityAdmin.ToString(), Roles.TpsafFacilityIncharge.ToString(), Roles.TaxAuthAdmin.ToString(), Roles.TaxAuthRevenueOfficer.ToString(), Roles.TsspAdmin.ToString(), Roles.TsspIntermediate.ToString(), Roles.TsspWarehouseIncharge.ToString() }));
                options.AddPolicy("CanApproveRejectOrders", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.TaxAuthAdmin.ToString(), Roles.TaxAuthRevenueOfficer.ToString(), Roles.TsspAdmin.ToString(), Roles.TsspIntermediate.ToString(), Roles.TsspWarehouseIncharge.ToString(), Roles.TaxAuthAdmin.ToString(), Roles.TaxAuthRevenueOfficer.ToString() }));
                options.AddPolicy("CanFulfillOrders", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.TsspAdmin.ToString(), Roles.TsspIntermediate.ToString(), Roles.TsspWarehouseIncharge.ToString(), Roles.TaxAuthAdmin.ToString(), Roles.TaxAuthRevenueOfficer.ToString() }));

                /*Print Order Policies*/
                options.AddPolicy("CanCreateClosePrintOrders", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.TsspAdmin.ToString(), Roles.TsspIntermediate.ToString(), Roles.TsspWarehouseIncharge.ToString(), Roles.TaxAuthAdmin.ToString(), Roles.TaxAuthRevenueOfficer.ToString() }));
                options.AddPolicy("CanViewPrintOrders", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.TsspAdmin.ToString(), Roles.TsspIntermediate.ToString(), Roles.TsspWarehouseIncharge.ToString(), Roles.PrintPartner.ToString(), Roles.TaxAuthAdmin.ToString(), Roles.TaxAuthRevenueOfficer.ToString() }));
                options.AddPolicy("CanApproveRejectPrintOrders", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.PrintPartner.ToString() }));

                /*Reel Change Policies*/
                options.AddPolicy("CanCreateReelChangeRequest", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.MfAccountManager.ToString(), Roles.MfAdmin.ToString(), Roles.MfWarehouseIncharge.ToString() }));
                options.AddPolicy("CanApproveRejectReelChangeRequest", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.TsspAdmin.ToString(), Roles.TsspIntermediate.ToString(), Roles.TsspWarehouseIncharge.ToString(), Roles.TaxAuthAdmin.ToString(), Roles.TaxAuthRevenueOfficer.ToString() }));
                options.AddPolicy("CanViewReelChangeRequest", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.TsspAdmin.ToString(), Roles.TsspIntermediate.ToString(), Roles.TsspWarehouseIncharge.ToString(), Roles.MfAccountManager.ToString(), Roles.MfAdmin.ToString(), Roles.MfWarehouseIncharge.ToString(), Roles.TaxAuthAdmin.ToString(), Roles.TaxAuthRevenueOfficer.ToString() }));

                /*Internal Stock Transfer Policies*/
                options.AddPolicy("InternalStockTransfer", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.TpsafAdmin.ToString(), Roles.TpsafFacilityAdmin.ToString(), Roles.TpsafFacilityIncharge.ToString() }));
                options.AddPolicy("ViewInternalStockTransfers", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.TsspAdmin.ToString(), Roles.TsspIntermediate.ToString(), Roles.TsspWarehouseIncharge.ToString(), Roles.TpsafAdmin.ToString(), Roles.TpsafFacilityAdmin.ToString(), Roles.TpsafFacilityIncharge.ToString(), Roles.TaxAuthAdmin.ToString(), Roles.TaxAuthRevenueOfficer.ToString() }));

                /*Packages Policies*/
                options.AddPolicy("OrderFulfillment", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.TsspAdmin.ToString(), Roles.TsspIntermediate.ToString(), Roles.TsspWarehouseIncharge.ToString(), Roles.TaxAuthAdmin.ToString(), Roles.TaxAuthRevenueOfficer.ToString() }));
                options.AddPolicy("ReturnOrderFulfillment", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.MfAccountManager.ToString(), Roles.MfAdmin.ToString(), Roles.MfWarehouseIncharge.ToString() }));
                options.AddPolicy("ViewPackageTypes", policy =>
                    policy.RequireClaim("Role", new string[] { Roles.MfAccountManager.ToString(), Roles.MfAdmin.ToString(), Roles.MfWarehouseIncharge.ToString(), Roles.TsspAdmin.ToString(), Roles.TsspIntermediate.ToString(), Roles.TsspWarehouseIncharge.ToString(), Roles.TpsafAdmin.ToString(), Roles.TpsafFacilityAdmin.ToString(), Roles.TpsafFacilityIncharge.ToString(), Roles.TaxAuthAdmin.ToString(), Roles.TaxAuthRevenueOfficer.ToString() }));

                /*Return Order Policies*/
                options.AddPolicy("CanCreateReturnOrders", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.MfAccountManager.ToString(), Roles.MfAdmin.ToString(), Roles.MfWarehouseIncharge.ToString() }));
                options.AddPolicy("CanApproveRejectReturnOrders", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.TsspAdmin.ToString(), Roles.TsspIntermediate.ToString(), Roles.TsspWarehouseIncharge.ToString(), Roles.TaxAuthAdmin.ToString(), Roles.TaxAuthRevenueOfficer.ToString() }));
                options.AddPolicy("CanDownloadPackageList", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.MfAccountManager.ToString(), Roles.MfAdmin.ToString(), Roles.MfWarehouseIncharge.ToString(), Roles.TsspAdmin.ToString(), Roles.TsspIntermediate.ToString(), Roles.TsspWarehouseIncharge.ToString(), Roles.ShipperAgent.ToString(), Roles.TaxAuthAdmin.ToString(), Roles.TaxAuthRevenueOfficer.ToString() }));
                options.AddPolicy("CanViewReturnOrders", policy =>
                     policy.RequireClaim("Role", new string[] { Roles.MfAccountManager.ToString(), Roles.MfAdmin.ToString(), Roles.MfWarehouseIncharge.ToString(), Roles.TsspAdmin.ToString(), Roles.TsspIntermediate.ToString(), Roles.TsspWarehouseIncharge.ToString(), Roles.TaxAuthAdmin.ToString(), Roles.TaxAuthRevenueOfficer.ToString() }));

                /*Delivery Policies*/
                options.AddPolicy("Delivery", policy =>
                     policy.AddRequirements(new DeliveryAppRequirement()));

                /*Dashboard*/
                options.AddPolicy("Admin", policy =>
                    policy.RequireClaim("Role", new string[] { Roles.TsspAdmin.ToString(),Roles.TaxAuthAdmin.ToString() }));
                options.AddPolicy("Dashboard", policy =>
                policy.RequireClaim("Role", new string[] { Roles.MfAccountManager.ToString(), Roles.MfAdmin.ToString(), Roles.MfWarehouseIncharge.ToString(), Roles.TsspAdmin.ToString(), Roles.TsspIntermediate.ToString(), Roles.TsspWarehouseIncharge.ToString(), Roles.TpsafAdmin.ToString(), Roles.TpsafFacilityAdmin.ToString(), Roles.TpsafFacilityIncharge.ToString(), Roles.TaxAuthAdmin.ToString(), Roles.TaxAuthRevenueOfficer.ToString() }));
            });

            services.AddSingleton<IAuthorizationHandler, RoleClaimHandler>();
            services.AddSingleton<IAuthorizationHandler, HeaderValueHandler>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderMetaService, OrderMetaService>();
            services.AddScoped<IPrintOrderService, PrintOrderService>();
            services.AddScoped<IDeliveryService, DeliveryService>();
            services.AddScoped<IPackageService, PackageService>();
            services.AddScoped<IReelChangeService, ReelChangeService>();
            services.AddScoped<IReturnOrderService, ReturnOrderService>();
            services.AddScoped<IInternalStockService, InternalStockService>();
            services.AddScoped<IOrderDashboardService, OrderDashboardService>();
            services.AddScoped<IStampGenerationService, StampGenerationService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<INotificationHelper, NotificationHelper>();
            services.AddHttpClient<UserClient>("UserClient", client =>
            {
                //client.BaseAddress = new Uri("https://localhost:44322/api/");
                client.BaseAddress = new Uri(Configuration.GetValue<string>("UserApiUrl"));
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                if (_currentEnvironment.IsDevelopment())
                {
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                }
                return handler;
            });

            services.AddHttpClient<IdServerClient>("IdServerClient", client =>
            {
                //client.BaseAddress = new Uri("https://localhost:44370/");
                client.BaseAddress = new Uri(Configuration.GetValue<string>("HostingApp:Url"));
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                if (_currentEnvironment.IsDevelopment())
                {
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                }
                return handler;
            });


            services.AddTransient<IPrincipal>(
                provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);

            var storageAccountConfig = Configuration.GetSection("StorageAccount").Get<StorageAccountConfig>();
            services.AddSingleton<StorageAccountConfig>(storageAccountConfig);
            var cosmosConfig = Configuration.GetSection("Cosmos").Get<CosmosAccessConfig>();
            services.AddSingleton(cosmosConfig);
            services.AddSingleton<ICosmosDbService>((InitializeNotificationCosmosClientAsync(cosmosConfig)).GetAwaiter().GetResult());           

            var fcmConfig = Configuration.GetSection("Fcm").Get<FcmConfig>();
            services.AddSingleton(fcmConfig);
            var fcmSettings = new FcmSettings();
            fcmSettings.ServerKey = fcmConfig.ServerKey;
            fcmSettings.SenderId = fcmConfig.ServerId;
            services.AddSingleton(fcmSettings); 
            services.AddHttpClient<FcmClient>("FcmClient", client =>
            {

            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                if (_currentEnvironment.IsDevelopment())
                {
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                }
                return handler;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();


            app.UseRouting();
            app.UseCors("AllowCors");
            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Management API");
                c.RoutePrefix = "swagger";
            });
        }

        private static async Task<NotifcationCosmosDbService> InitializeNotificationCosmosClientAsync(CosmosAccessConfig cosmosAccessConfig)
        {
            string account = cosmosAccessConfig.Account;
            string key = cosmosAccessConfig.Key;
            string notificationDatabase = cosmosAccessConfig.NotificationDatabase;
            string notificationContainer = cosmosAccessConfig.NotificationContainer; 
            string notificationUSerDetail = cosmosAccessConfig.NotificationUserDetailContainer;
            Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
            Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(notificationDatabase);
            await database.Database.CreateContainerIfNotExistsAsync(notificationUSerDetail, "/UserToken");
            await database.Database.CreateContainerIfNotExistsAsync(notificationContainer, "/id");
            NotifcationCosmosDbService notifcationCosmosDb = new NotifcationCosmosDbService(client, notificationDatabase, notificationUSerDetail);

            return notifcationCosmosDb;
        }
    }
}
