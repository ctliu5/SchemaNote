//#define SSL
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
#if SSL
#else
using System.Net;
#endif
namespace SchemaNote
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
#if SSL
#else
                .UseKestrel(option =>
                {
                    option.Listen(IPAddress.Loopback, 8010);
                })
#endif
                .UseStartup<Startup>();
    }
}
