/*  
 * ......................我佛慈悲...................... 
 *                       _oo0oo_ 
 *                      o8888888o 
 *                      88" . "88 
 *                      (| -_- |) 
 *                      0\  =  /0 
 *                    ___/`---'\___ 
 *                  .' \\|     |// '. 
 *                 / \\|||  :  |||// \ 
 *                / _||||| -卍-|||||- \ 
 *               |   | \\\  -  /// |   | 
 *               | \_|  ''\---/''  |_/ | 
 *               \  .-\__  '-'  ___/-. / 
 *             ___'. .'  /--.--\  `. .'___ 
 *          ."" '<  `.___\_<|>_/___.' >' "". 
 *         | | :  `- \`.;`\ _ /`;.`/ - ` : | | 
 *         \  \ `_.   \_ __\ /__ _/   .-` /  / 
 *     =====`-.____`.___ \_____/___.-`___.-'===== 
 *                       `=---=' 
 *                        
 *..................佛祖开光 ,永无BUG................... 
 *  
 */




using System.Diagnostics;
using System.Text;
using Com.Bll;
using Com.Db;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using Com.Bll.Util;
using Com.Service.Match.Models;
using Com.Models.Enum;
using Com.Models.Base;
using Com.Models.Db;
using Com.Bll.Models;

namespace Com.Service.Match;

/// <summary>
/// Service:提醒后继辅助工作,如进度控制，推送等
/// </summary>
public class MatchAssist
{
    /// <summary>
    /// 服务列表
    /// </summary>
    public readonly ServiceList service_list = null!;
    /// <summary>
    /// Service:关键字
    /// </summary>
    public readonly ServiceKey service_key;
    /// <summary>
    /// 撮合服务对象
    /// </summary>
    /// <value></value>
    public MatchModel model { get; set; } = null!;

    /// <summary>
    /// 交易记录Db操作
    /// </summary>
    /// <returns></returns>
    public readonly ServiceDeal service_deal;
    /// <summary>
    /// 订单服务
    /// </summary>
    /// <returns></returns>
    public readonly ServiceOrder service_order;
    /// <summary>
    /// K线服务
    /// </summary>
    /// <returns></returns>
    public readonly ServiceKline service_kline;
    /// <summary>
    /// 钱包服务
    /// </summary>
    /// <returns></returns>
    private readonly ServiceWallet service_wallet;
    /// <summary>
    /// 交易对服务
    /// </summary>
    /// <returns></returns>
    private readonly ServiceMarket service_market;
    /// <summary>
    /// 秒表
    /// </summary>
    /// <returns></returns>
    public Stopwatch stopwatch = new Stopwatch();
    /// <summary>
    /// 临时变量
    /// </summary>
    /// <returns></returns>
    private ResWebsocker<List<BaseDeal>> res_deal = new ResWebsocker<List<BaseDeal>>();
    /// <summary>
    /// 临时变量
    /// </summary>
    /// <typeparam name="Kline?"></typeparam>
    /// <returns></returns>
    private ResWebsocker<List<ResKline>> res_kline = new ResWebsocker<List<ResKline>>();
    /// <summary>
    /// 临时变量
    /// </summary>
    /// <typeparam name="Kline?"></typeparam>
    /// <returns></returns>
    private ResWebsocker<List<Orders>> res_order = new ResWebsocker<List<Orders>>();

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="model"></param>
    public MatchAssist(ServiceList service_list, MatchModel model)
    {
        this.service_list = service_list;
        this.model = model;
        this.service_key = new ServiceKey(service_list.service_base);
        this.service_deal = new ServiceDeal(service_list.service_base);
        this.service_order = new ServiceOrder(service_list.service_base);
        this.service_kline = new ServiceKline(service_list.service_base);
        this.service_wallet = new ServiceWallet(service_list.service_base);
        this.service_market = new ServiceMarket(service_list.service_base);



        res_order.success = true;
        res_order.op = E_WebsockerOp.subscribe_event;
        res_order.channel = E_WebsockerChannel.orders;
        res_deal.success = true;
        res_deal.op = E_WebsockerOp.subscribe_event;
        res_deal.channel = E_WebsockerChannel.trades;
        res_kline.success = true;
        res_kline.op = E_WebsockerOp.subscribe_event;
        res_kline.data = new List<ResKline>();
    }

    /// <summary>
    /// 接收撮合传过来的成交订单
    /// </summary>
    /// <param name="queue_name">列队名称</param>
    /// <param name="consume_tag">消费器标签</param>
    /// <returns>queue_name:队列名,consume_tag消费者标签</returns>
    public (string queue_name, string consume_tag) ReceiveMatchOrder()
    {
        return this.model.mq_helper.MqWorker(this.service_key.GetMqOrderDeal(this.model.info.market_id), (b) =>
        {
            string json = Encoding.UTF8.GetString(b);
            (long no, List<Orders> orders, List<Deal> deals, List<Orders> cancels) deals = JsonConvert.DeserializeObject<(long no, List<Orders> orders, List<Deal> deals, List<Orders> cancels)>(json);
            this.stopwatch.Restart();
            RedisValue rv = ServiceFactory.instance.redis.HashGet(this.service_key.GetRedisProcess(), deals.no);
            if (!rv.HasValue)
            {
                return true;
            }
            Processing? process = JsonConvert.DeserializeObject<Processing>(rv!);
            if (process == null || process.match == false)
            {
                ServiceFactory.instance.redis.HashDelete(this.service_key.GetRedisProcess(), deals.no);
                return true;
            }
            ReceiveDealOrder(process, deals.orders, deals.deals, deals.cancels);
            this.stopwatch.Stop();
            this.service_list.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{this.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:撮合后续处理总时间(成交记录数:{deals.deals.Count},成交订单数:{deals.orders.Count},撤单数:{deals.cancels.Count}),处理结果:{JsonConvert.SerializeObject(process)}");
            if (process.match && process.asset && process.running_fee && process.deal && process.order && process.order_cancel && process.order_complete_thaw_buy && process.order_complete_thaw_sell && process.push_order && process.push_order_cancel && process.push_kline && process.push_deal && process.push_ticker)
            {
                ServiceFactory.instance.redis.HashDelete(this.service_key.GetRedisProcess(), process.no);
                return true;
            }
            else
            {
                ServiceFactory.instance.redis.HashSet(this.service_key.GetRedisProcess(), process.no, JsonConvert.SerializeObject(process));
                return false;
            }
        });
    }

    /// <summary>
    /// 接收到成交订单
    /// </summary>
    /// <param name="process">处理进度</param>
    /// <param name="orders">成交的订单</param>
    /// <param name="deals">成交记录</param>
    /// <param name="cancels">撤单订单</param>
    private void ReceiveDealOrder(Processing process, List<Orders> orders, List<Deal> deals, List<Orders> cancels)
    {
        this.service_list.service_base.logger.LogInformation($"线程Id({model.info.symbol}):{Thread.CurrentThread.ManagedThreadId.ToString()}");
        if (deals.Count > 0)
        {
            Process_Asset(process, orders, deals);
            Process_Deal(process, deals);
            Process_Order(process, orders);
            Process_Kline(process, deals);
            Process_Deal_Push(process, deals);
            Process_Ticker_Push(process);
        }
        else
        {
            process.asset = true;
            process.running_fee = true;
            process.running_trade = true;
            process.deal = true;
            process.order = true;
            process.order_complete_thaw_buy = true;
            process.order_complete_thaw_sell = true;
            process.push_order = true;
            process.push_kline = true;
            process.push_deal = true;
            process.push_ticker = true;
        }
        if (cancels.Count > 0)
        {
            Process_Cancel(process, cancels);
            Process_Cancel_Push(process, cancels);
        }
        else
        {
            process.order_cancel = true;
            process.push_order_cancel = true;
        }
    }

    /// <summary>
    /// 处理进程 资产
    /// </summary>
    /// <param name="process">处理进程</param>
    /// <param name="orders">影响订单</param>
    /// <param name="deals">成交记录</param>
    private void Process_Asset(Processing process, List<Orders> orders, List<Deal> deals)
    {
        if (process.asset == false)
        {
            this.model.stopwatch.Restart();
            (bool result, List<RunningFee> running_fee, List<RunningTrade> running_trade) result = service_wallet.Transaction(this.model.info, deals);
            process.asset = result.result;
            this.model.stopwatch.Stop();
            this.service_list.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{this.model.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:DB=>成交记录{deals.Count}条,实际资产转移(结果)");
            if (result.result)
            {
                if (process.order == false && orders.Count > 0)
                {
                    this.model.stopwatch.Restart();
                    process.order = service_order.UpdateOrder(orders);
                    this.model.stopwatch.Stop();
                    this.service_list.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{this.model.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:DB=>更新{orders.Count}条订单记录");
                }
                else
                {
                    process.order = true;
                }
                if (process.running_fee == false && result.running_fee.Count > 0)
                {
                    this.model.stopwatch.Restart();
                    process.running_fee = service_wallet.AddRunningFee(result.running_fee);
                    this.model.stopwatch.Stop();
                    this.service_list.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{this.model.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:DB=>添加资金流水(手续费){result.running_fee.Count}条");
                }
                else
                {
                    process.running_fee = true;
                }
                if (process.running_trade == false && result.running_trade.Count > 0)
                {
                    this.model.stopwatch.Restart();
                    process.running_trade = service_wallet.AddRunningTrade(result.running_trade);
                    this.model.stopwatch.Stop();
                    this.service_list.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{this.model.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:DB=>添加资金流水(交易){result.running_trade.Count}条");
                }
                else
                {
                    process.running_trade = true;
                }
            }
        }
    }

    /// <summary>
    /// 处理进程 交易记录
    /// </summary>
    /// <param name="process">处理进程</param>
    /// <param name="deals">deals</param>
    private void Process_Deal(Processing process, List<Deal> deals)
    {
        if (process.deal == false)
        {
            this.model.stopwatch.Restart();
            process.deal = service_deal.AddDeal(deals);
            this.model.stopwatch.Stop();
            this.service_list.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{this.model.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:DB=>成交记录:{deals.Count}");
        }
    }

    /// <summary>
    /// 处理进程 订单
    /// </summary>
    /// <param name="process">处理进程</param>
    /// <param name="orders">影响订单</param>
    private void Process_Order(Processing process, List<Orders> orders)
    {
        if (orders.Count > 0)
        {
            E_WalletType wallet_type = E_WalletType.main;
            if (this.model.info.market_type == E_MarketType.spot)
            {
                wallet_type = E_WalletType.spot;
            }
            if (process.order_complete_thaw_buy == false)
            {
                this.model.stopwatch.Restart();
                List<Orders> order_buy = orders.Where(P => P.state == E_OrderState.completed && P.unsold > 0 && P.side == E_OrderSide.buy).ToList();
                var order_buy_uid = from o in order_buy
                                    group o by o.uid into g
                                    select new { uid = g.Key, unsold = g.Sum(P => P.unsold), order = g.ToList() };
                foreach (var item in order_buy_uid)
                {
                    if (service_wallet.FreezeChange(wallet_type, item.uid, this.model.info.coin_id_quote, -item.unsold))
                    {
                        item.order.ForEach(P => { P.complete_thaw = P.unsold; P.unsold = 0; });
                    }
                }
                process.order_complete_thaw_buy = service_order.UpdateOrder(order_buy);
            }
            if (process.order_complete_thaw_sell == false)
            {
                List<Orders> order_sell = orders.Where(P => P.state == E_OrderState.completed && P.unsold > 0 && P.side == E_OrderSide.sell).ToList();
                var order_sell_uid = from o in order_sell
                                     group o by o.uid into g
                                     select new { uid = g.Key, unsold = g.Sum(P => P.unsold), order = g.ToList() };
                foreach (var item in order_sell_uid)
                {
                    if (service_wallet.FreezeChange(wallet_type, item.uid, this.model.info.coin_id_base, -item.unsold))
                    {
                        item.order.ForEach(P => { P.complete_thaw = P.unsold; P.unsold = 0; });
                    }
                }
                process.order_complete_thaw_sell = service_order.UpdateOrder(order_sell);
                this.model.stopwatch.Stop();
                this.service_list.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{this.model.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:DB=>更新订单记录(完成解冻多余资金)");
            }
            if (process.push_order == false)
            {
                this.model.stopwatch.Restart();
                var uid_order = orders.GroupBy(P => P.uid).ToList();
                process.push_order = true;
                foreach (var item in uid_order)
                {
                    res_order.data = item.ToList();
                    process.push_order = process.push_order && ServiceFactory.instance.mq_helper.MqPublish(this.service_key.GetMqSubscribe(E_WebsockerChannel.orders, item.Key), JsonConvert.SerializeObject(res_order, new JsonConverterDecimal()));
                }
                this.model.stopwatch.Stop();
                this.service_list.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{this.model.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:Mq=>推送订单更新");
            }
        }
        else
        {
            process.order_complete_thaw_buy = true;
            process.order_complete_thaw_sell = true;
            process.push_order = true;
        }
    }

    /// <summary>
    /// 处理进程 K线
    /// </summary>
    /// <param name="process">处理进程</param>
    /// <param name="deals">deals</param>
    private void Process_Kline(Processing process, List<Deal> deals)
    {
        Dictionary<E_KlineType, DateTimeOffset> last_kline = new Dictionary<E_KlineType, DateTimeOffset>();
        if (process.push_kline == false)
        {
            this.model.stopwatch.Restart();
            Dictionary<E_KlineType, List<Kline>> klines = this.service_kline.DBtoRedised(this.model.info.market_id, this.model.info.symbol, deals.Max(P => P.time));
            Dictionary<E_KlineType, Kline> klineing = this.service_kline.DBtoRedising(this.model.info.market_id, this.model.info.symbol, deals);
            this.model.stopwatch.Stop();
            this.service_list.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{this.model.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:DB,redis=>同步K线记录");
            this.model.stopwatch.Restart();
            foreach (E_KlineType cycle in System.Enum.GetValues(typeof(E_KlineType)))
            {
                List<ResKline> push_kline = new List<ResKline>();
                if (klines.ContainsKey(cycle))
                {
                    push_kline = klines[cycle].ConvertAll(P => (ResKline)P);
                }
                if (klineing.ContainsKey(cycle))
                {
                    push_kline.RemoveAll(P => P.time_start == klineing[cycle].time_start);
                    push_kline.Add(klineing[cycle]);
                }
                if (push_kline.Count > 0)
                {
                    res_kline.channel = (E_WebsockerChannel)Enum.Parse(typeof(E_WebsockerChannel), cycle.ToString());
                    res_kline.data = push_kline;
                    ServiceFactory.instance.mq_helper.MqPublish(this.service_key.GetMqSubscribe(res_kline.channel, this.model.info.market_id), JsonConvert.SerializeObject(res_kline, new JsonConverterDecimal()));
                }
            }
            process.push_kline = true;
            this.model.stopwatch.Stop();
            this.service_list.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{this.model.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:Mq=>推送K线记录");
        }
    }

    /// <summary>
    /// 处理进程 成交记录
    /// </summary>
    /// <param name="process">处理进程</param>
    /// <param name="deals">deals</param>
    private void Process_Deal_Push(Processing process, List<Deal> deals)
    {
        if (process.push_deal == false)
        {
            this.model.stopwatch.Restart();
            SortedSetEntry[] entries = new SortedSetEntry[deals.Count()];
            res_deal.data = new List<BaseDeal>();
            for (int i = 0; i < deals.Count(); i++)
            {
                BaseDeal resdeal = service_deal.Convert(deals[i]);
                entries[i] = new SortedSetEntry(JsonConvert.SerializeObject(resdeal), resdeal.time.ToUnixTimeMilliseconds());
                res_deal.data.Add(resdeal);
            }
            ServiceFactory.instance.redis.SortedSetAdd(this.service_key.GetRedisDeal(this.model.info.market_id), entries);
            process.push_deal = ServiceFactory.instance.mq_helper.MqPublish(this.service_key.GetMqSubscribe(E_WebsockerChannel.trades, this.model.info.market_id), JsonConvert.SerializeObject(res_deal, new JsonConverterDecimal()));
            this.model.stopwatch.Stop();
            this.service_list.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{this.model.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:Mq,Redis=>推送交易记录");
        }
    }

    /// <summary>
    /// 处理进程 聚合行情
    /// </summary>
    /// <param name="process">处理进程</param>
    private void Process_Ticker_Push(Processing process)
    {
        if (process.push_ticker == false)
        {
            this.model.stopwatch.Restart();
            ResTicker? ticker = service_deal.Get24HoursTicker(this.model.info.market_id);
            process.push_ticker = service_deal.PushTicker(ticker);
            this.model.stopwatch.Stop();
            this.service_list.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{this.model.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:Mq,Redis=>推送聚合行情");
        }
    }

    /// <summary>
    /// 处理进程 订单取消
    /// </summary>
    /// <param name="process">处理进程</param>
    /// <param name="cancels">取消订单</param>
    private void Process_Cancel(Processing process, List<Orders> cancels)
    {
        if (process.order_cancel == false)
        {
            this.model.stopwatch.Restart();
            E_WalletType wallet_type = E_WalletType.main;
            if (this.model.info.market_type == E_MarketType.spot)
            {
                wallet_type = E_WalletType.spot;
            }
            List<Orders> order_buy = cancels.Where(P => P.state == E_OrderState.cancel && P.unsold > 0 && P.side == E_OrderSide.buy).ToList();
            var order_buy_uid = from o in order_buy
                                group o by o.uid into g
                                select new { uid = g.Key, unsold = g.Sum(P => P.unsold), order = g.ToList() };
            foreach (var item in order_buy_uid)
            {
                if (service_wallet.FreezeChange(wallet_type, item.uid, this.model.info.coin_id_quote, -item.unsold))
                {
                    item.order.ForEach(P => { P.complete_thaw = P.unsold; P.unsold = 0; });
                }
            }
            List<Orders> order_sell = cancels.Where(P => P.state == E_OrderState.cancel && P.unsold > 0 && P.side == E_OrderSide.sell).ToList();
            var order_sell_uid = from o in order_sell
                                 group o by o.uid into g
                                 select new { uid = g.Key, unsold = g.Sum(P => P.unsold), order = g.ToList() };
            foreach (var item in order_sell_uid)
            {
                if (service_wallet.FreezeChange(wallet_type, item.uid, this.model.info.coin_id_base, -item.unsold))
                {
                    item.order.ForEach(P => { P.complete_thaw = P.unsold; P.unsold = 0; });
                }
            }
            process.order_cancel = service_order.UpdateOrder(cancels);
            this.model.stopwatch.Stop();
            this.service_list.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{this.model.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:DB=>撤单{cancels.Count}条订单记录");
        }
    }

    /// <summary>
    /// 处理进程 订单取消推送
    /// </summary>
    /// <param name="process">处理进程</param>
    /// <param name="cancels">取消订单</param>
    private void Process_Cancel_Push(Processing process, List<Orders> cancels)
    {
        if (process.push_order_cancel == false)
        {
            var uid_order = cancels.GroupBy(P => P.uid).ToList();
            process.push_order_cancel = true;
            foreach (var item in uid_order)
            {
                res_order.data = item.ToList();
                process.push_order_cancel = process.push_order_cancel && ServiceFactory.instance.mq_helper.MqPublish(this.service_key.GetMqSubscribe(E_WebsockerChannel.orders, item.Key), JsonConvert.SerializeObject(item.ToList(), new JsonConverterDecimal()));
            }
            this.model.stopwatch.Stop();
            this.service_list.service_base.logger.LogTrace(this.model.eventId, $"计算耗时:{this.model.stopwatch.Elapsed.ToString()};{this.model.eventId.Name}:Mq=>推送撤单订单");
        }
    }

}
