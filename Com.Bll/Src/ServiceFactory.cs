using System.Diagnostics;
using Com.Bll.Models;
using Com.Bll.Util;
using Com.Models.Db;
using Com.Models.Enum;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using RabbitMQ.Client;
using Snowflake.Core;
using StackExchange.Redis;

namespace Com.Bll;

/// <summary>
/// 服务工厂
/// </summary>
public class ServiceFactory
{
    public static readonly ServiceFactory instance = new ServiceFactory();
    /// <summary>
    /// 系统初始化时间  初始化  注:2017-1-1 此时是一年第一天，一年第一月，一年第一个星期日(星期日是一个星期开始的第一天)
    /// </summary>   
    public DateTimeOffset system_init = new DateTimeOffset(2017, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// 基础服务
    /// </summary>
    public ServiceBase service_base = null!;
    /// <summary>
    /// Service:关键字
    /// </summary>
    public ServiceKey service_key = null!;

    /// <summary>
    /// redis连接对象
    /// </summary>
    public ConnectionMultiplexer redisMultiplexer = null!;
    /// <summary>
    /// redis操作接口
    /// </summary>
    public IDatabase redis = null!;
    /// <summary>
    /// mq 连接工厂
    /// </summary>
    public ConnectionFactory connection_factory = null!;
    /// <summary>
    /// es客户端
    /// </summary>
    public ElasticClient elastic_client = null!;
    /// <summary>
    /// 雪花算法
    /// </summary>
    /// <returns></returns>
    public IdWorker worker = new IdWorker(1, 1);
    /// <summary>
    /// 随机数
    /// </summary>
    /// <returns></returns>
    public Random random = new Random();
    /// <summary>
    /// 秒表
    /// </summary>
    /// <returns></returns>
    public Stopwatch stopwatch = new Stopwatch();
    /// <summary>
    /// mq帮助类
    /// </summary>
    public HelperMq mq_helper = null!;
    

    private ServiceFactory()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="service_base"></param>
    public void Init(ServiceBase service_base)
    {
        this.service_base = service_base;
        this.service_key = new ServiceKey(service_base);
    }

    /// <summary>
    /// 初始化雪花算法
    /// </summary>
    /// <param name="service_type"></param>
    public void InitSnowflake(E_ServiceType service_type)
    {
        if (this.redis == null)
        {
            throw new Exception("请先初始化redis,再初始化雪花算法");
        }
        long worker_id = 0;
        do
        {
            worker_id = this.redis.HashIncrement(this.service_key.GetWorkerId(), service_type.ToString());
            if (worker_id > 31)
            {
                worker_id %= 32;
            }
        } while (worker_id == 0);
        this.worker = new IdWorker(worker_id, (int)service_type);
    }


    /// <summary>
    /// 初始化Db
    /// </summary>
    public void InitDb()
    {
        try
        {
            using DbContextEF db = this.service_base.db_factory.CreateDbContext();
            db.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            this.service_base.logger.LogError(ex, $"DB服务器连接不上");
        }
    }

    /// <summary>
    /// 初始化redis
    /// </summary>
    public void InitRedis()
    {
        try
        {
            string? redisConnection = this.service_base.configuration.GetConnectionString("Redis");
            if (!string.IsNullOrWhiteSpace(redisConnection))
            {
                this.redisMultiplexer = ConnectionMultiplexer.Connect(redisConnection);
                this.redis = redisMultiplexer.GetDatabase();
            }
            else
            {
                this.service_base.logger.LogError($"Redis服务器地址没有找到");
            }
        }
        catch (Exception ex)
        {
            this.service_base.logger.LogError(ex, $"redis服务器连接不上");
        }
    }

    /// <summary>
    /// 初始化mq
    /// </summary>
    public void InitMq()
    {
        try
        {
            this.connection_factory = this.service_base.configuration.GetSection("RabbitMQ").Get<ConnectionFactory>();
            if (this.connection_factory != null)
            {
                mq_helper = new HelperMq(this.service_base, this.connection_factory);
            }
            else
            {
                this.service_base.logger.LogError($"mq服务器地址没有找到");
            }
        }
        catch (Exception ex)
        {
            this.service_base.logger.LogError(ex, $"MQ服务器连接不上");
        }
    }

    /// <summary>
    /// 初始化ElasticSearch
    /// </summary>
    public void InitEs()
    {
        try
        {
            string connect = this.service_base.configuration.GetConnectionString("ElasticUrl");
            if (!string.IsNullOrWhiteSpace(connect))
            {
                var node = new Uri(connect);
                var settings = new ConnectionSettings(node);
                this.elastic_client = new ElasticClient(settings);
            }
            else
            {
                this.service_base.logger.LogError($"ElasticSearch服务器地址没有找到");
            }
        }
        catch (Exception ex)
        {
            this.service_base.logger.LogError(ex, $"ElasticSearch服务器连接不上");
        }
    }

    /// <summary>
    /// 创建ES索引
    /// </summary>
    /// <param name="index">索引名称</param>
    /// <param name="rebuild">是否重建索引</param>
    /// <typeparam name="T"></typeparam>
    public void EsCreateMapping<T>(string index, bool rebuild) where T : class
    {
        if (rebuild)
        {
            this.elastic_client.Indices.Create(index, c => c
                               .Map<T>(m => m
                               .AutoMap()
                               ));
        }
        else
        {
            ExistsResponse indexResponse = this.elastic_client.Indices.Exists(index);
            if (!indexResponse.Exists)
            {
                this.elastic_client.Indices.Create(index, c => c
                                    .Map<T>(m => m
                                    .AutoMap()));
            }
        }
    }



}
