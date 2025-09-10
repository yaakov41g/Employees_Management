using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace EmployeeManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
.ConfigureLogging((hostingContext, logging) =>
{// 
    //If you want only NLog as the logging provider, Remove all the default logging providers (comment all lines but 'logging.AddNLog();')
    logging.ClearProviders();
    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
    logging.AddConsole(); // add log when using CLI (command line interface - the black command window
    logging.AddDebug();
    logging.AddEventSourceLogger();
    // Enable NLog as one of the Logging Provider
    logging.AddNLog();//  by Nlog it will log to the file which registered in Nlog.config ( the path\file name is  'c:\DemoLogs\nlog-all-{date}.log')   
})
.ConfigureWebHostDefaults(webBuilder =>

{
    webBuilder.UseStartup<Startup>();
});
        }
    }
}
