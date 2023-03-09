
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Grpc.Core;
using Com.Bll;
using Microsoft.EntityFrameworkCore;
using Com.Models.Db;
using Com.Bll.Models;

using Com.Models.Enum;
using Grpc.Net.Client;

using Com.Service.Match.Models;
using System.Diagnostics;
using System.Collections.Concurrent;
using ServiceMatchGrpc;


namespace Com.Service.Match;

/// <summary>
/// 用来管理撮合服务
/// </summary>
public class FactoryMatch
{
    /// <summary>
    /// 单例类的实例
    /// </summary>
    /// <returns></returns>
    public static readonly FactoryMatch instance = new FactoryMatch();
    /// <summary>
    /// 服务列表
    /// </summary>
    public ServiceList service_list = null!;
    /// <summary>
    /// 服务:行情处理
    /// </summary>
    public MatchDepth service_depth = null!;
    /// <summary>
    /// 互斥锁
    /// </summary>
    /// <returns></returns>
    private Mutex mutex = new Mutex(false);
    /// <summary>
    /// 撮合服务集合
    /// </summary>
    /// <typeparam name="string">交易对</typeparam>
    /// <typeparam name="Core">撮合对象</typeparam>
    /// <returns></returns>
    public ConcurrentDictionary<long, MatchModel> service = new ConcurrentDictionary<long, MatchModel>();
    /// <summary>
    /// 秒表
    /// </summary>
    /// <returns></returns>
    public Stopwatch stopwatch = new Stopwatch();
    /// <summary>
    /// 公共类
    /// </summary>
    public Common common = null!;

    /// <summary>
    /// 
    /// </summary>
    public System.Threading.Channels.Channel<TransactionRecordRes>? Channel_TransactionRecord = null;

    /// <summary>
    /// 私有构造方法
    /// </summary>
    private FactoryMatch()
    {

    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="service_base"></param>
    public void Init(ServiceBase service_base)
    {
        this.service_list = new ServiceList(service_base);
        this.common = new Common(service_base);
        this.service_depth = new MatchDepth(this.service_list);
        Grpc.Core.Server server = new Grpc.Core.Server
        {
            Services = { MatchGrpc.BindService(new ServiceGrpc()) },
            Ports = { new ServerPort("0.0.0.0", service_base.configuration.GetValue<int>("manage_port"), ServerCredentials.Insecure) }
        };
        server.Start();
    }

    /// <summary>
    /// 分配撮合对
    /// </summary>
    /// <param name="market_id"></param>
    /// <returns></returns>
    public bool InitMatch(long market_id)
    {
        Market? market = FactoryMatch.instance.service_list.service_market.GetMarketBySymbol(market_id);
        if (market != null && this.service.ContainsKey(market.market_id) == false)
        {
            MatchModel mm = new MatchModel(market);
            mm.match_core = new MatchCore(this.service_list, mm);
            mm.mq = new MQ(this.service_list, mm);
            mm.core = new MatchAssist(this.service_list, mm);
            this.service.TryAdd(market.market_id, mm);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 启动
    /// </summary>
    /// <param name="market_id"></param>
    /// <returns></returns>
    public bool Start(long market_id)
    {
        if (this.service.TryGetValue(market_id, out var model))
        {
            if (model.run == false)
            {
                ServiceClearCache(model.info);
                ServiceWarmCache(model.info);
                model.run = true;
                (string queue_name, string consume_tag) receive_match_order = model.core.ReceiveMatchOrder();
                // if (!model.mq_consumer.Contains(receive_match_order.consume_tag))
                // {
                //     model.mq_consumer.Add(receive_match_order.consume_tag);
                // }
                (string queue_name, string consume_tag) order_receive = model.mq.OrderReceive();
                // if (!model.mq_queues.Contains(order_receive.queue_name))
                // {
                //     model.mq_queues.Add(order_receive.queue_name);
                // }
                // if (!model.mq_consumer.Contains(order_receive.consume_tag))
                // {
                //     model.mq_consumer.Add(order_receive.consume_tag);
                // }
            }
            model.info.status = model.run;
            this.mutex.ReleaseMutex();
            this.service_list.service_base.logger.LogInformation(model.eventId, $"服务启动成功:{model.info.symbol}");
            return true;
        }
        this.mutex.WaitOne();
        return false;
    }

    /// <summary>
    /// 停止
    /// </summary>
    /// <param name="market_id"></param>
    /// <returns></returns>
    public bool Stop(long market_id)
    {
        if (this.service.TryGetValue(market_id, out var model))
        {
            this.service_list.service_base.logger.LogInformation($"服务准备关闭:{model.info.symbol}");
            this.mutex.WaitOne();
            if (this.service.ContainsKey(model.info.market_id))
            {
                MatchModel mm = this.service[model.info.market_id];
                mm.run = false;
                ServiceClearCache(model.info);
                this.service.TryRemove(model.info.market_id, out _);
                model.info.status = false;
                // mm.mq_helper.Close();
            }
            else
            {
                model.info.status = false;
            }
            this.mutex.ReleaseMutex();
            this.service_list.service_base.logger.LogInformation($"服务关闭成功:{model.info.symbol}");
        }
        return false;
    }

    /// <summary>
    /// 服务:清除所有缓存
    /// </summary>
    /// <param name="info">交易对基础信息</param>
    /// <returns></returns>
    private bool ServiceClearCache(Market info)
    {
        if (this.service.ContainsKey(info.market_id))
        {
            MatchModel mm = this.service[info.market_id];
            mm.mq_helper.MqDeleteConsumer();
            // foreach (var item in mm.mq_consumer)
            // {
            //     mm.mq_helper.MqDeleteConsumer(item);
            // }
            // mm.mq_consumer.Clear();
            // string queue_name = FactoryService.instance.GetMqOrderPlace(info.market);
            // if (!mm.mq_queues.Contains(queue_name))
            // {
            // IModel i_model = ServiceFactory.instance.i_commection.CreateModel();
            // mm.mq_helper.i_model.QueueDeclare(queue: queue_name, durable: true, exclusive: false, autoDelete: false, arguments: null);
            // mm.mq_queues.Add(queue_name);
            // }
            // foreach (var item in mm.mq_queues)
            // {
            //     ServiceFactory.instance.MqDeletePurge(item);
            // }
            mm.mq_helper.MqDeletePurge();
            mm.mq_helper.MqDelete();
            // mm.mq_queues.Clear();
            mm.mq.DepthChange(new List<Orders>(), new List<Deal>(), mm.match_core.CancelOrder());
        }
        //交易记录数据从DB同步到Redis 至少保存最近3个月记录
        this.stopwatch.Restart();
        long delete = this.service_list.service_deal.DeleteDeal(info.market_id, DateTimeOffset.UtcNow.AddDays(-10));
        this.service_depth.DeleteRedisDepth(info.market_id);
        this.service_list.service_kline.DeleteRedisKline(info.market_id);
        this.stopwatch.Stop();
        this.service_list.service_base.logger.LogTrace($"计算耗时:{this.stopwatch.Elapsed.ToString()};{info.symbol}:redis=>清除所有缓存");
        return true;
    }

    /// <summary>
    /// 服务:预热缓存
    /// </summary>
    /// <param name="info">交易对基础信息</param>
    /// <returns></returns>
    private bool ServiceWarmCache(Market info)
    {
        this.stopwatch.Restart();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset end = now.AddSeconds(-now.Second).AddMilliseconds(-now.Millisecond);
        this.service_list.service_deal.DealDbToRedis(info.market_id, end.AddDays(-10));
        end = end.AddMilliseconds(-1);
        this.service_list.service_kline.DBtoRedised(info.market_id, info.symbol, end);
        this.service_list.service_kline.DBtoRedising(info.market_id, info.symbol, now);
        this.service_list.service_order.PushOrderToMqRedis(info.market_id);
        this.stopwatch.Stop();
        this.service_list.service_base.logger.LogTrace($"计算耗时:{this.stopwatch.Elapsed.ToString()};{info.symbol}:redis=>预热缓存");
        return true;
    }


}