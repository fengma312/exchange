// using GrpcExchange;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Grpc.Core;
using Com.Bll;
using Microsoft.EntityFrameworkCore;
using Com.Models.Db;
using Com.Bll.Models;
// using GrpcExchange;
using Com.Models.Enum;

namespace Com.Service.Match;

/// <summary>
/// 网站后台服务
/// </summary>
public class MainService : BackgroundService
{  
    public readonly ServiceBase service_base;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="provider">服务驱动</param>
    /// <param name="configuration">配置接口</param>
    /// <param name="environment">环境接口</param>
    /// <param name="logger">日志接口</param>
    public MainService(IServiceProvider provider, IDbContextFactory<DbContextEF> db_factory, IConfiguration configuration, IHostEnvironment environment, ILogger<MainService> logger)
    {        
        this.service_base = new ServiceBase(provider, db_factory, configuration, environment, logger);
    }

    /// <summary>
    /// 任务执行方法
    /// </summary>
    /// <param name="stoppingToken">后台任务取消令牌</param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.service_base.logger.LogInformation("准备启动业务后台服务");
        try
        {
            ServiceFactory.instance.Init(this.service_base);
            // ServiceFactory.instance.InitDb();
            // ServiceFactory.instance.InitRedis();
            // ServiceFactory.instance.InitSnowflake(E_ServiceType.collector);
            // ServiceFactory.instance.InitMq();
            // ServiceFactory.instance.InitEs();
            FactoryMatch.instance.Init(this.service_base);
            this.service_base.logger.LogInformation("启动业务后台服务成功");
        }
        catch (Exception ex)
        {
            this.service_base.logger.LogError(ex, "启动业务后台服务异常");
        }
        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
    }
}