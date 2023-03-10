using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Grpc.Core;
using Com.Models.Db;
using Com.Bll.Models;
using System.Diagnostics;
using System.Collections.Concurrent;
using ServiceTradeGrpc;
using Com.Service.Trade.Models;

namespace Com.Service.Trade;

/// <summary>
/// 用来管理撮合服务
/// </summary>
public class FactoryTrade
{
    /// <summary>
    /// 单例类的实例
    /// </summary>
    /// <returns></returns>
    public static readonly FactoryTrade instance = new FactoryTrade();
    /// <summary>
    /// 基础服务
    /// </summary>
    public ServiceBase service_base = null!;
    /// <summary>
    /// 服务列表
    /// </summary>
    public ServiceList service_list = null!;
    /// <summary>
    /// 互斥锁
    /// </summary>
    /// <returns></returns>
    private Mutex mutex = new Mutex(false);
    /// <summary>
    /// 交易服务集合
    /// </summary>
    /// <typeparam name="long">交易对id</typeparam>
    /// <typeparam name="TradeModel">交易对象</typeparam>
    /// <returns></returns>
    public ConcurrentDictionary<long, TradeModel> service = new ConcurrentDictionary<long, TradeModel>();
    /// <summary>
    /// 秒表
    /// </summary>
    /// <returns></returns>
    public Stopwatch stopwatch = new Stopwatch();

    /// <summary>
    /// 私有构造方法
    /// </summary>
    private FactoryTrade()
    {

    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="service_base"></param>
    public void Init(ServiceBase service_base)
    {
        this.service_base = service_base;
        this.service_list = new ServiceList(service_base);
        Cluster? cluster = this.service_list.service_cluster.GetCluster(service_base.configuration.GetValue<long>("ClusterId"));
        if (cluster == null)
        {
            return;
        }
        this.LoadConfig(cluster.mark);
        foreach (var item in service.Values)
        {
            if (item.info.status)
            {
                item.StartTrade();
            }
        }
        this.StartGrpcService(cluster.port);
    }

    /// <summary>
    /// 加载交易对配置
    /// </summary>
    /// <param name="mark">标记</param>
    private void LoadConfig(string mark)
    {
        List<Market> markets = this.service_list.service_market.GetAllMarket();
        foreach (var item in markets)
        {
            if (mark.Contains((item.market_id % 10).ToString()))
            {
                TradeModel model = new TradeModel(item)
                {

                };
                this.service.TryAdd(item.market_id, model);
            }
        }
    }

    /// <summary>
    /// 启动Grpc服务端
    /// </summary>
    private void StartGrpcService(int port)
    {
        Grpc.Core.Server server = new Grpc.Core.Server
        {
            Services = { TradeGrpc.BindService(new GrpcServiceTrade()) },
            Ports = { new ServerPort("0.0.0.0", port, ServerCredentials.Insecure) }
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
        Market? market = FactoryTrade.instance.service_list.service_market.GetMarketBySymbol(market_id);
        // if (market != null && this.service.ContainsKey(market.market_id) == false)
        // {
        //     MatchModel mm = new MatchModel(market);
        //     mm.match_core = new MatchCore(this.service_list, mm);
        //     mm.mq = new MQ(this.service_list, mm);
        //     mm.core = new MatchAssist(this.service_list, mm);
        //     this.service.TryAdd(market.market_id, mm);
        //     return true;
        // }
        return false;
    }

    /// <summary>
    /// 启动
    /// </summary>
    /// <param name="market_id"></param>
    /// <returns></returns>
    public bool Start(long market_id)
    {
        // if (this.service.TryGetValue(market_id, out var model))
        // {
        //     if (model.run == false)
        //     {
        //         ServiceClearCache(model.info);
        //         ServiceWarmCache(model.info);
        //         model.run = true;
        //         (string queue_name, string consume_tag) receive_match_order = model.core.ReceiveMatchOrder();
        //         // if (!model.mq_consumer.Contains(receive_match_order.consume_tag))
        //         // {
        //         //     model.mq_consumer.Add(receive_match_order.consume_tag);
        //         // }
        //         (string queue_name, string consume_tag) order_receive = model.mq.OrderReceive();
        //         // if (!model.mq_queues.Contains(order_receive.queue_name))
        //         // {
        //         //     model.mq_queues.Add(order_receive.queue_name);
        //         // }
        //         // if (!model.mq_consumer.Contains(order_receive.consume_tag))
        //         // {
        //         //     model.mq_consumer.Add(order_receive.consume_tag);
        //         // }
        //     }
        //     model.info.status = model.run;
        //     this.mutex.ReleaseMutex();
        //     this.service_list.service_base.logger.LogInformation(model.eventId, $"服务启动成功:{model.info.symbol}");
        //     return true;
        // }
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
        // if (this.service.TryGetValue(market_id, out var model))
        // {
        //     this.service_list.service_base.logger.LogInformation($"服务准备关闭:{model.info.symbol}");
        //     this.mutex.WaitOne();
        //     if (this.service.ContainsKey(model.info.market_id))
        //     {
        //         MatchModel mm = this.service[model.info.market_id];
        //         mm.run = false;
        //         ServiceClearCache(model.info);
        //         this.service.TryRemove(model.info.market_id, out _);
        //         model.info.status = false;
        //         // mm.mq_helper.Close();
        //     }
        //     else
        //     {
        //         model.info.status = false;
        //     }
        //     this.mutex.ReleaseMutex();
        //     this.service_list.service_base.logger.LogInformation($"服务关闭成功:{model.info.symbol}");
        // }
        return false;
    }

    /// <summary>
    /// 服务:清除所有缓存
    /// </summary>
    /// <param name="info">交易对基础信息</param>
    /// <returns></returns>
    private bool ServiceClearCache(Market info)
    {
        // if (this.service.ContainsKey(info.market_id))
        // {
        //     MatchModel mm = this.service[info.market_id];
        //     mm.mq_helper.MqDeleteConsumer();
        //     // foreach (var item in mm.mq_consumer)
        //     // {
        //     //     mm.mq_helper.MqDeleteConsumer(item);
        //     // }
        //     // mm.mq_consumer.Clear();
        //     // string queue_name = FactoryService.instance.GetMqOrderPlace(info.market);
        //     // if (!mm.mq_queues.Contains(queue_name))
        //     // {
        //     // IModel i_model = ServiceFactory.instance.i_commection.CreateModel();
        //     // mm.mq_helper.i_model.QueueDeclare(queue: queue_name, durable: true, exclusive: false, autoDelete: false, arguments: null);
        //     // mm.mq_queues.Add(queue_name);
        //     // }
        //     // foreach (var item in mm.mq_queues)
        //     // {
        //     //     ServiceFactory.instance.MqDeletePurge(item);
        //     // }
        //     mm.mq_helper.MqDeletePurge();
        //     mm.mq_helper.MqDelete();
        //     // mm.mq_queues.Clear();
        //     mm.mq.DepthChange(new List<Orders>(), new List<Deal>(), mm.match_core.CancelOrder());
        // }
        // //交易记录数据从DB同步到Redis 至少保存最近3个月记录
        // this.stopwatch.Restart();
        // long delete = this.service_list.service_deal.DeleteDeal(info.market_id, DateTimeOffset.UtcNow.AddDays(-10));
        // this.service_depth.DeleteRedisDepth(info.market_id);
        // this.service_list.service_kline.DeleteRedisKline(info.market_id);
        // this.stopwatch.Stop();
        // this.service_list.service_base.logger.LogTrace($"计算耗时:{this.stopwatch.Elapsed.ToString()};{info.symbol}:redis=>清除所有缓存");
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