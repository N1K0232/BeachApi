using Serilog;

namespace BeachApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = CreateHostBuilder(args);
        var host = builder.Build();

        await host.RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .UseSerilog((hostingContext, loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
            })
            .ConfigureWebHostDefaults(builder =>
            {
                builder.UseStartup<Startup>();
            });

        return builder;
    }
}