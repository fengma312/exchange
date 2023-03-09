using Com.Bll;
using Com.Bll.Models;
using Com.Models.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Com.Service.Trade;

/// <summary>
/// 定时任务
/// </summary>
public class BaseJobs : IJob
{
    public readonly ServiceBase service_base;
    /// <summary>
    /// 日志编号
    /// </summary>
    /// <returns></returns>
    private readonly EventId eventId = new EventId(101, "(bll)执行任务");
    /// <summary>
    /// 服务列表
    /// </summary>
    public readonly ServiceList server_list;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="config"></param>
    /// <param name="environment"></param>
    /// <param name="logger"></param>
    public BaseJobs(IServiceProvider provider, IDbContextFactory<DbContextEF> db_factory, IConfiguration configuration, IHostEnvironment environment, ILogger<BaseJobs> logger)
    {
        this.service_base = new ServiceBase(provider, db_factory, configuration, environment, logger);
        server_list = new ServiceList(this.service_base);
    }

    /// <summary>
    /// 执行方法
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Execute(IJobExecutionContext context)
    {
        JobDataMap data = context.MergedJobDataMap;
        int key = data.GetInt("key");
        try
        {
            DateTimeOffset time_trigger = context.ScheduledFireTimeUtc ?? DateTimeOffset.UtcNow;
            TimeSpan interval = (context.NextFireTimeUtc ?? time_trigger) - time_trigger;
            this.service_base.logger.LogInformation(eventId, $"定时任务,类型:{key},时间:{time_trigger.ToString("yyyy-MM-dd HH:mm:ss.fffK")},间隔:{interval}");
            if (key == 0)
            {
                // 启动执行              


                // for (int i = 0; i < 20; i++)
                // {
                //     a.service_admin.test("BaseJobs");

                // }
                // for (int i = 0; i < 20; i++)
                // {
                //     a.service_admin.test("BaseJobs");

                // }
                // for (int i = 0; i < 50; i++)
                // {
                //     a.service_admin.test("BaseJobs");

                // }

            }
            else if (key == 1)
            {
                //5分钟执行一次
            }
            else if (key == 2)
            {
                // 15分钟执行一次

            }
            else if (key == 3)
            {
                //1小时执行一次


            }
            else if (key == 4)
            {
                //2小时执行一次
            }
            else if (key == 5)
            {
                //1日执行一次


                //每日清理数据

            }
            else if (key == 6)
            {
                //1周执行一次

            }
            else if (key == 7)
            {
                //1月执行一次

            }
            await Task.Delay(0);
        }
        catch (System.Exception ex)
        {
            this.service_base.logger.LogError(eventId, ex, $"执行定时任务异常,类型为:{key}");
        }
        return;
    }

}
