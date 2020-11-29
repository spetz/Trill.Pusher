using System.Threading.Tasks;
using Convey.Logging;
using Convey.Secrets.Vault;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;

namespace Trill.Pusher
{
    public class Program
    {
        public static async Task Main(string[] args)
            => await CreateHostBuilder(args)
                .Build()
                .RunAsync();

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.ConfigureKestrel((_, k) =>
                    {
                        k.ListenLocalhost(5010, o => o.Protocols = HttpProtocols.Http1);
                        k.ListenLocalhost(5011, o => o.Protocols = HttpProtocols.Http2);
                    })
                    .UseStartup<Startup>())
                .UseLogging()
                .UseVault();
    }
}
