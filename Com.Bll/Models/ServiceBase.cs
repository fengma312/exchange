using Com.Models.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Com.Bll.Models;

/// <summary>
/// 基础服务
/// </summary>
public class ServiceBase
{
    /// <summary>
    /// 驱动接口
    /// </summary>
    public readonly IServiceProvider provider;
    /// <summary>
    /// db上下文工厂
    /// </summary>
    public readonly IDbContextFactory<DbContextEF> db_factory;
    /// <summary>
    /// 配置接口
    /// </summary>
    public readonly IConfiguration configuration;
    /// <summary>
    /// 环境接口
    /// </summary>
    public readonly IHostEnvironment environment;
    /// <summary>
    /// 日志接口
    /// </summary>
    public readonly ILogger logger;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="provider">驱动接口</param>
    /// <param name="db_factory">db上下文工厂</param>
    /// <param name="configuration">配置接口</param>
    /// <param name="environment">环境接口</param>
    /// <param name="logger">日志接口</param>
    public ServiceBase(IServiceProvider provider, IDbContextFactory<DbContextEF> db_factory, IConfiguration configuration, IHostEnvironment environment, ILogger? logger)
    {
        this.provider = provider;
        this.db_factory = db_factory;
        this.configuration = configuration;
        this.environment = environment;
        this.logger = logger ?? NullLogger.Instance;
    }
}

