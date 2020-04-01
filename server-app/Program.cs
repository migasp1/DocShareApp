using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DocShareApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    .ConfigureAppConfiguration((context, builder) =>
                     {
                         IWebHostEnvironment env = context.HostingEnvironment;
                         //where the app is locally
                         builder.SetBasePath(env.ContentRootPath);
                         builder.AddJsonFile("appsettings.json", false, true);
                         builder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
                         builder.AddEnvironmentVariables();
                     });
                });
    }
}
