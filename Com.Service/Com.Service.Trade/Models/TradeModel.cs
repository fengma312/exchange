using System;
using System.Collections.Concurrent;
using Com.Db;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using RabbitMQ.Client;
using Com.Bll;
using Com.Bll.Util;
using Com.Models.Db;

namespace Com.Service.Trade.Models;

/// <summary>
/// 交易对象
/// </summary>
public class TradeModel
{
    /// <summary>
    /// 是否运行
    /// </summary>
    /// <value></value>
    public bool run { get; set; }
    /// <summary>
    /// 日志事件ID
    /// </summary>
    public EventId eventId;
    /// <summary>
    /// 交易对基本信息
    /// </summary>
    /// <value></value>
    public Market info { get; set; } = null!;
    /// <summary>
    /// 撮合服务
    /// </summary>
    /// <value></value>
    // public MatchCore match_core { get; set; } = null!;
    /// <summary>
    /// 消息队列服务
    /// </summary>
    /// <value></value>
    // public MQ mq { get; set; } = null!;
    /// <summary>
    /// 核心服务
    /// </summary>
    /// <value></value>
    // public MatchAssist core { get; set; } = null!;
    /// <summary>
    /// 秒表
    /// </summary>
    /// <returns></returns>
    public Stopwatch stopwatch = new Stopwatch();
    /// <summary>
    /// mq
    /// </summary>
    public readonly HelperMq mq_helper = null!;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="info"></param>
    public TradeModel(Market info)
    {
        this.info = info;
        this.eventId = new EventId(1, info.symbol);
        this.mq_helper = new HelperMq(ServiceFactory.instance.connection_factory.CreateConnection());
    }

    /// <summary>
    /// 启动交易对
    /// </summary>
    /// <param name="market_id"></param>
    public void StartTrade(long market_id)
    {

    }

    /// <summary>
    /// 停止交易对
    /// </summary>
    /// <param name="market_id"></param>
    public void StopTrade(long market_id)
    {

    }
}