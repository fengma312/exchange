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
    services.AddQuartz(q =>
    {
        q.UseMicrosoftDependencyInjectionJobFactory();
        q.UseDedicatedThreadPool();
        JobKey jobKey = new JobKey("key0");
        q.AddJob<BaseJobs>(j => j.WithIdentity(jobKey).UsingJobData("key", 0).Build());
        //启动执行
        q.AddTrigger(i => i.StartNow().ForJob(jobKey));
    });
    services.AddQuartz(q =>
    {
        q.UseMicrosoftDependencyInjectionJobFactory();
        q.UseDedicatedThreadPool();
        JobKey jobKey = new JobKey("key1");
        q.AddJob<BaseJobs>(j => j.WithIdentity(jobKey).UsingJobData("key", 1).Build());
        //5分钟执行一次
        q.AddTrigger(i => i.StartNow().WithCronSchedule("0 0/5 * * * ?", j => j.WithMisfireHandlingInstructionDoNothing()).ForJob(jobKey));
    });
    // services.AddQuartzServer(options =>
    // {
    //     options.WaitForJobsToComplete = true;
    // });
    services.BuildServiceProvider();
});
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.ConfigureLogging((hostContext, logging) =>
{
    logging.ClearProviders();
#if (DEBUG)
    logging.AddConsole();
#endif
    logging.AddNLog();
});
var app = builder.Build();
app.Run();

