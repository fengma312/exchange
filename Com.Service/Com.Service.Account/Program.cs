using Com.Models.Db;
using Com.Service.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Quartz;

// ThreadPool.SetMinThreads(300, 200);
IHostBuilder builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((hostContext, services) =>
{
    services.AddPooledDbContextFactory<DbContextEF>(options =>
    {
        options.UseLoggerFactory(LoggerFactory.Create(builder => { builder.AddConsole(); }));
        // options.EnableSensitiveDataLogging();
        DbContextOptions options1 = options.UseNpgsql(hostContext.Configuration.GetConnectionString("pgsql"), builder =>
        {
            // builder.EnableRetryOnFailure(
            //     maxRetryCount: 5,
            //     maxRetryDelay: TimeSpan.FromSeconds(30),
            //     errorNumbersToAdd: new int[] { 2 });
        }).Options;

    });
    services.AddHostedService<MainService>();
   
    services.BuildServiceProvider();
});
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.ConfigureLogging((hostContext, logging) =>
{
    logging.ClearProviders();
// #if (DEBUG)
    logging.AddConsole();
// #endif
    logging.AddNLog();
});
var app = builder.Build();
app.Run();

