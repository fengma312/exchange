using Com.Db;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using Com.Bll.Util;

using Microsoft.Extensions.Logging.Abstractions;
using Com.Models.Base;
using Com.Models.Enum;
using Com.Bll.Models;
using Com.Models.Db;

namespace Com.Bll;

/// <summary>
/// Service:K线
/// </summary>
public class ServiceKline
{

    /// <summary>
    /// DB:交易记录
    /// </summary>
    private readonly ServiceDeal service_deal;
    /// <summary>
    /// 交易对服务
    /// </summary>
    /// <returns></returns>
    private readonly ServiceMarket service_market;
    /// <summary>
    /// 基础服务
    /// </summary>
    public readonly ServiceBase service_base;
    /// <summary>
    /// Service:关键字
    /// </summary>
    public ServiceKey service_key = null!;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="service_base">基础服务</param>
    public ServiceKline(ServiceBase service_base)
    {
        this.service_base = service_base;
        this.service_deal = new ServiceDeal(service_base);
        this.service_market = new ServiceMarket(service_base);
        this.service_key = new ServiceKey(service_base);
    }

    /// <summary>
    /// 获取K线数据
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <param name="type">K线类型</param>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="skip">跳过行数</param>
    /// <param name="take">获取行数</param>
    /// <returns></returns>
    public Res<List<ResKline>?> Klines(string symbol, E_KlineType type, DateTimeOffset start, DateTimeOffset? end, long skip, long take)
    {
        Res<List<ResKline>?> res = new Res<List<ResKline>?>();
        double stop = double.PositiveInfinity;
        if (end != null)
        {
            stop = end.Value.ToUnixTimeMilliseconds();
        }
        Market? market = this.service_market.GetMarketBySymbol(symbol);
        if (market != null)
        {
            res.code = E_Res_Code.ok;
            res.data = new List<ResKline>();
            RedisValue[] rv = ServiceFactory.instance.redis.SortedSetRangeByScore(key: service_key.GetRedisKline(market.market_id, type), start: start.ToUnixTimeMilliseconds(), stop: stop, exclude: Exclude.None, skip: skip, take: take, order: StackExchange.Redis.Order.Ascending);
            foreach (var item in rv)
            {
                if (!item.HasValue)
                {
                    continue;
                }
                ResKline? resKline = JsonConvert.DeserializeObject<ResKline>(item!);
                if (resKline != null)
                {
                    res.data.Add(resKline);
                }
            }
            if (end == null || (end != null && end >= DateTimeOffset.UtcNow))
            {
                ResKline? resKline = JsonConvert.DeserializeObject<ResKline>(ServiceFactory.instance.redis.HashGet(service_key.GetRedisKlineing(market.market_id), type.ToString())!);
                if (resKline != null)
                {
                    res.data.RemoveAll(P => P.time_start == resKline.time_start);
                    res.data.Add(resKline);
                }
            }
        }
        return res;
    }

    /// <summary>
    /// 缓存预热(已确定K线)
    /// </summary>
    /// <param name="markets">交易对</param>
    /// <param name="end">结束时间</param>
    public Dictionary<E_KlineType, List<Kline>> DBtoRedised(long market, string symbol, DateTimeOffset end)
    {
        return null;
        // Dictionary<E_KlineType, List<Kline>> klines = SyncKlines(market, symbol, end);
        // DbSaveRedis(market, klines);
        // return klines;
    }

    /// <summary>
    /// 未确定K线和交易记录合并成新的K线
    /// </summary>
    /// <param name="market"></param>
    /// <param name="symbol"></param>
    /// <param name="deals"></param>
    public Dictionary<E_KlineType, Kline> DBtoRedising(long market, string symbol, List<Deal> deals)
    {
        // Dictionary<E_KlineType, Kline> klines = new Dictionary<E_KlineType, Kline>();
        // DateTimeOffset now = DateTimeOffset.UtcNow;
        // foreach (E_KlineType cycle in System.Enum.GetValues(typeof(E_KlineType)))
        // {
        //     (DateTimeOffset start, DateTimeOffset end) startend = KlineTime(cycle, now);
        //     List<Deal> deals_cycle = deals.Where(x => x.time >= startend.start && x.time <= startend.end).ToList();
        //     Kline? kline_new = DealToKline(cycle, startend.start, deals_cycle);
        //     if (kline_new == null)
        //     {
        //         continue;
        //     }
        //     RedisValue kline_old_obj = ServiceFactory.instance.constant.redis.HashGet(ServiceFactory.instance.GetRedisKlineing(market), cycle.ToString());
        //     if (kline_old_obj.HasValue)
        //     {
        //         Kline? kline_old = JsonConvert.DeserializeObject<Kline>(kline_old_obj);
        //         if (kline_old != null && kline_old.time_start == kline_new.time_start)
        //         {
        //             kline_new.id = ServiceFactory.instance.constant.worker.NextId();
        //             kline_new.amount += kline_old.amount;
        //             kline_new.count += kline_old.count;
        //             kline_new.total += kline_old.total;
        //             kline_new.open = kline_old.open;
        //             kline_new.close = kline_new.close;
        //             kline_new.low = kline_new.low > kline_old.low ? kline_old.low : kline_new.low;
        //             kline_new.high = kline_new.high < kline_old.high ? kline_old.high : kline_new.high;
        //             kline_new.time_start = kline_old.time_start;
        //         }
        //     }
        //     ServiceFactory.instance.constant.redis.HashSet(ServiceFactory.instance.GetRedisKlineing(market), cycle.ToString(), JsonConvert.SerializeObject(kline_new, new JsonConverterDecimal()));
        //     klines.Add(cycle, kline_new);
        // }
        // return klines;
        return null;
    }

    /// <summary>
    /// 缓存预热(未确定K线)
    /// </summary>
    /// <param name="market">交易对</param>
    public void DBtoRedising(long market, string symbol, DateTimeOffset now)
    {
        foreach (E_KlineType cycle in System.Enum.GetValues(typeof(E_KlineType)))
        {
            (DateTimeOffset start, DateTimeOffset end) startend = KlineTime(cycle, now);
            Kline? kline_new = this.service_deal.GetKlinesByDeal(market, cycle, startend.start, startend.end);
            if (kline_new == null)
            {
                Deal? last_deal = service_deal.GetRedisLastDeal(market);
                if (last_deal == null)
                {
                    ServiceFactory.instance.redis.HashDelete(this.service_key.GetRedisKlineing(market), cycle.ToString());
                }
                else
                {
                    kline_new = new Kline()
                    {
                        market = market,
                        symbol = symbol,
                        amount = 0,
                        count = 0,
                        total = 0,
                        open = last_deal.price,
                        close = last_deal.price,
                        low = last_deal.price,
                        high = last_deal.price,
                        type = cycle,
                        time_start = startend.start,
                        time_end = now,
                        time = now,
                    };
                    ServiceFactory.instance.redis.HashSet(this.service_key.GetRedisKlineing(market), cycle.ToString(), JsonConvert.SerializeObject(kline_new, new JsonConverterDecimal()));
                }
            }
            else
            {
                ServiceFactory.instance.redis.HashSet(this.service_key.GetRedisKlineing(market), cycle.ToString(), JsonConvert.SerializeObject(kline_new, new JsonConverterDecimal()));
            }
        }
    }

    /// <summary>
    /// 计算K线开始时间和结束时间
    /// </summary>
    /// <param name="cycle"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public (DateTimeOffset start, DateTimeOffset end) KlineTime(E_KlineType cycle, DateTimeOffset time)
    {
        DateTimeOffset start = time;
        DateTimeOffset end = time;
        switch (cycle)
        {
            case E_KlineType.min1:
                start = time.AddSeconds(-time.Second).AddMilliseconds(-time.Millisecond);
                end = start.AddMinutes(1).AddMilliseconds(-1);
                break;
            case E_KlineType.min5:
                start = ServiceFactory.instance.system_init.AddMinutes((int)(time - ServiceFactory.instance.system_init).TotalMinutes / 5 * 5);
                end = start.AddMinutes(5).AddMilliseconds(-1);
                break;
            case E_KlineType.min15:
                start = ServiceFactory.instance.system_init.AddMinutes((int)(time - ServiceFactory.instance.system_init).TotalMinutes / 15 * 15);
                end = start.AddMinutes(15).AddMilliseconds(-1);
                break;
            case E_KlineType.min30:
                start = ServiceFactory.instance.system_init.AddMinutes((int)(time - ServiceFactory.instance.system_init).TotalMinutes / 30 * 30);
                end = start.AddMinutes(30).AddMilliseconds(-1);
                break;
            case E_KlineType.hour1:
                start = time.AddMinutes(-time.Minute).AddSeconds(-time.Second).AddMilliseconds(-time.Millisecond);
                end = start.AddHours(1).AddMilliseconds(-1);
                break;
            case E_KlineType.hour6:
                start = ServiceFactory.instance.system_init.AddHours((int)(time - ServiceFactory.instance.system_init).TotalHours / 6 * 6);
                end = start.AddHours(6).AddMilliseconds(-1);
                break;
            case E_KlineType.hour12:
                start = ServiceFactory.instance.system_init.AddHours((int)(time - ServiceFactory.instance.system_init).TotalHours / 12 * 12);
                end = start.AddHours(12).AddMilliseconds(-1);
                break;
            case E_KlineType.day1:
                start = time.AddHours(-time.Hour).AddMinutes(-time.Minute).AddSeconds(-time.Second).AddMilliseconds(-time.Millisecond);
                end = start.AddDays(1).AddMilliseconds(-1);
                break;
            case E_KlineType.week1:
                start = ServiceFactory.instance.system_init.AddDays((int)(time - ServiceFactory.instance.system_init).TotalDays / 7 * 7);
                end = start.AddDays(7).AddMilliseconds(-1);
                break;
            case E_KlineType.month1:
                start = time.AddDays(-time.Day).AddHours(-time.Hour).AddMinutes(-time.Minute).AddSeconds(-time.Second).AddMilliseconds(-time.Millisecond);
                end = start.AddMonths(1).AddMilliseconds(-1);
                break;
            default:
                break;
        }
        return (start, end);
    }

    /// <summary>
    /// 删除redis kline数据
    /// </summary>
    /// <param name="market">交易对</param>
    public void DeleteRedisKline(long market)
    {
        ServiceFactory.instance.redis.KeyDelete(this.service_key.GetRedisKlineing(market));
        foreach (E_KlineType cycle in System.Enum.GetValues(typeof(E_KlineType)))
        {
            ServiceFactory.instance.redis.KeyDelete(this.service_key.GetRedisKline(market, cycle));
        }
    }

}