using System;
using System.Collections.Generic;
using System.Text;
using Com.Bll;
using Com.Db;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Snowflake;
using Com.Bll.Models;
using Com.Models.Db;
using Com.Models.Enum;
using Com.Models.Base;
using Com.Service.Matchmaking.Models;

namespace Com.Service.Match;

/// <summary>
/// RabbitMQ 接收数据和发送数据
/// </summary>
public class MQ
{
    /// <summary>
    /// 服务列表
    /// </summary>
    public readonly ServiceList service_list = null!;
    /// <summary>
    /// 
    /// </summary>
    public readonly MatchDepth service_depth = null!;
    /// <summary>
    /// 撮合服务对象
    /// </summary>
    private MatchModel model;
    /// <summary>
    /// 上次深度行情
    /// </summary>
    (List<OrderBook> bid, List<OrderBook> ask) orderbook_old;
    /// <summary>
    /// 临时变量
    /// </summary>
    /// <typeparam name="Orders"></typeparam>
    /// <returns></returns>
    private List<Orders> orders = new List<Orders>();
    /// <summary>
    /// 临时变量
    /// </summary>
    /// <typeparam name="MatchDeal"></typeparam>
    /// <returns></returns>
    private List<Deal> deal = new List<Deal>();
    /// <summary>
    /// 临时变量
    /// </summary>
    /// <typeparam name="MatchOrder"></typeparam>
    /// <returns></returns>
    private List<Orders> cancel = new List<Orders>();

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="model">撮合核心</param>
    public MQ(ServiceList service_list, MatchModel model)
    {
        this.service_list = service_list;
        this.model = model;
    }

    /// <summary>
    /// 接收订单列队
    /// </summary>
    /// <returns>队列标识</returns>
    public (string queue_name, string consume_tag) OrderReceive()
    {
        return this.model.mq_helper.MqReceive(this.service_list.service_key.GetMqOrderPlace(this.model.info.market_id), (e) =>
        {
            string json = Encoding.UTF8.GetString(e);
            ReqCall<object>? reqCall = JsonConvert.DeserializeObject<ReqCall<object>>(json);
            if (reqCall != null)
            {
                if (reqCall.op == E_Op.place)
                {
                    ReqCall<List<Orders>>? req = JsonConvert.DeserializeObject<ReqCall<List<Orders>>>(json);
                    if (req != null && req.op == E_Op.place && req.data != null && req.data.Count > 0)
                    {
                        orders.Clear();
                        deal.Clear();
                        cancel.Clear();
                        ServiceFactory.instance.stopwatch.Restart();
                        foreach (Orders item in req.data)
                        {
                            (List<Orders> orders, List<Deal> deals, List<Orders> cancels) match = this.model.match_core.MatchByArchives(item, 10);
                            if (match.orders.Count == 0 && match.deals.Count == 0 && match.cancels.Count == 0)
                            {
                                continue;
                            }
                            deal.AddRange(match.deals);
                            orders.RemoveAll(P => match.orders.Select(P => P.order_id).Contains(P.order_id));
                            orders.AddRange(match.orders);
                            cancel.AddRange(match.cancels);
                        }
                        ServiceFactory.instance.stopwatch.Stop();
                        this.service_list.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{ServiceFactory.instance.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:撮合订单{req.data.Count}条");
                        DepthChange(orders, deal, cancel);
                    };
                }
                else
                {
                    orders.Clear();
                    deal.Clear();
                    cancel.Clear();
                    ReqCall<(long uid, List<long> order_id)>? req = JsonConvert.DeserializeObject<ReqCall<(long, List<long>)>>(json);
                    if (req != null)
                    {
                        if (req.op == E_Op.cancel_by_id)
                        {
                            cancel.AddRange(this.model.match_core.CancelOrder(req.data.uid, req.data.order_id));
                        }
                        else if (req.op == E_Op.cancel_by_uid)
                        {
                            cancel.AddRange(this.model.match_core.CancelOrder(req.data.uid));
                        }
                        else if (req.op == E_Op.cancel_by_clientid)
                        {
                            cancel.AddRange(this.model.match_core.CancelOrder(req.data.uid, req.data.order_id));
                        }
                        else if (req.op == E_Op.cancel_by_all)
                        {
                            cancel.AddRange(this.model.match_core.CancelOrder());
                        }
                        if (cancel.Count > 0)
                        {
                            DepthChange(orders, new List<Deal>(), cancel);
                        }
                    }
                }
            }
            return true;
        });
    }

    /// <summary>
    /// 深度变更
    /// </summary>
    public void DepthChange(List<Orders> orders, List<Deal> deal, List<Orders> cancel)
    {
        if (orders.Count() > 0 || deal.Count() > 0 || cancel.Count() > 0)
        {
            Processing process = new Processing() { no = ServiceFactory.instance.worker.NextId(), match = true };
            ServiceFactory.instance.redis.HashSet(this.service_list.service_key.GetRedisProcess(), process.no, JsonConvert.SerializeObject(process));
            this.model.mq_helper.MqTask(this.service_list.service_key.GetMqOrderDeal(this.model.info.market_id), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((process.no, orders, deal, cancel))));
        }
        if (deal.Count() > 0 || cancel.Count > 0)
        {
            ServiceFactory.instance.stopwatch.Restart();
            (List<OrderBook> bid, List<OrderBook> ask) orderbook = this.model.match_core.GetOrderBook();
            Dictionary<E_WebsockerChannel, ResDepth> depths = this.service_depth.ConvertDepth(this.model.info.market_id, this.model.info.symbol, orderbook);
            this.service_depth.Push(this.model.info.market_id, depths, true);
            (List<(int index, OrderBook orderbook)> bid, List<(int index, OrderBook orderbook)> ask) diff = this.service_depth.DiffOrderBook(this.orderbook_old, orderbook);
            Dictionary<E_WebsockerChannel, ResDepth> depths_diff = this.service_depth.ConvertDepth(this.model.info.market_id, this.model.info.symbol, diff);
            foreach (var item in depths_diff)
            {
                if (item.Key == E_WebsockerChannel.books10_inc)
                {
                    if (depths.ContainsKey(E_WebsockerChannel.books10))
                    {
                        item.Value.total_bid = depths[E_WebsockerChannel.books10].total_bid;
                        item.Value.total_ask = depths[E_WebsockerChannel.books10].total_ask;
                    }
                }
                else if (item.Key == E_WebsockerChannel.books50_inc)
                {
                    if (depths.ContainsKey(E_WebsockerChannel.books50))
                    {
                        item.Value.total_bid = depths[E_WebsockerChannel.books50].total_bid;
                        item.Value.total_ask = depths[E_WebsockerChannel.books50].total_ask;
                    }
                }
                else if (item.Key == E_WebsockerChannel.books200_inc)
                {
                    if (depths.ContainsKey(E_WebsockerChannel.books200))
                    {
                        item.Value.total_bid = depths[E_WebsockerChannel.books200].total_bid;
                        item.Value.total_ask = depths[E_WebsockerChannel.books200].total_ask;
                    }
                }
            }
            this.service_depth.Push(this.model.info.market_id, depths_diff, false);
            this.orderbook_old = orderbook;
            ServiceFactory.instance.stopwatch.Stop();
            ServiceFactory.instance.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{ServiceFactory.instance.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:推送深度行情");
        }
    }

}