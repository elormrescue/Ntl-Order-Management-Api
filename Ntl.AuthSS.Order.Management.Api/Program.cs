using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EnvironmentName = Microsoft.AspNetCore.Hosting.EnvironmentName;

namespace Ntl.AuthSS.OrderManagement.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                var builtConfig = config.Build();
                var uRl = builtConfig["AzureKeyVault:Url"].ToString();
                string clientId = builtConfig["AZURE_CLIENT_ID"].ToString();
                var options = new DefaultAzureCredentialOptions() { ManagedIdentityClientId = builtConfig["AZURE_CLIENT_ID"].ToString() };
                config.AddAzureKeyVault(new SecretClient(new Uri(uRl), new DefaultAzureCredential(options)), new AzureKeyVaultConfigurationOptions() { });
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
