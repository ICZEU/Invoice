using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ICZEU.Invoice.WebApp
{
    public class Program
    {
        public static IConfiguration Configuration;

        public static void Main(string[] args)
        {
            IWebHost webHost = BuildWebHost(args);
            // Make configuration available as a static field for the TokenHelper class.
            Configuration = (IConfiguration) webHost.Services.GetService(typeof(IConfiguration));
            webHost.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureAppConfiguration(
                    builder => builder.AddEnvironmentVariables())
                .Build();
    }
}
