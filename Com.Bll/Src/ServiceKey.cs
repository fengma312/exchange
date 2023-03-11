using Com.Bll.Models;
using Com.Models.Db;
using Com.Models.Enum;

namespace Com.Bll;

/// <summary>
/// Service:中间件键的关键字
/// </summary>
public class ServiceKey
{
    /// <summary>
    /// 基础服务
    /// </summary>
    public readonly ServiceBase service_base;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="service_base">基础服务</param>
    public ServiceKey(ServiceBase service_base)
    {
        this.service_base = service_base;
    }

    /// <summary>
    /// redis:获取雪花workerId
    /// </summary>
    /// <returns></returns>
    public string GetWorkerId()
    {
        return string.Format("worker_id");
    }

    /// <summary>
    /// redis(zset)键 已生成交易记录 交易时间=>deal:btc/usdt 
    /// </summary>
    /// <param name="market"></param>
    /// <returns></returns>
    public string GetRedisDeal(long market)
    {
        return string.Format("deal:{0}", market);
    }

    /// <summary>
    /// redis hast 深度行情 depth:{market}
    /// </summary>
    /// <param name="market">交易对</param>
    /// <returns></returns>
    public string GetRedisDepth(long market)
    {
        return string.Format("depth:{0}", market);
    }

    /// <summary>
    /// redis hast 数据库表缓存
    /// </summary>
    /// <returns></returns>
    public string GetRedisTable(string table_name)
    {
        return string.Format("DbTable:{0}", table_name);
    }

    /// <summary>
    /// redis hast 深度行情 depth
    /// </summary>
    /// <returns></returns>
    public string GetRedisTicker()
    {
        return string.Format("ticker");
    }

    /// <summary>
    /// redis(zset)键 已生成K线 K线开始时间=>kline:btc/usdt:main1
    /// </summary>
    /// <param name="market"></param>
    /// <returns></returns>
    public string GetRedisKline(long market, E_KlineType type)
    {
        return string.Format("kline:{0}:{1}", market, type);
    }

    /// <summary>
    /// redis(hash)键 正在生成K线 K线类型=>klineing:btc/usdt
    /// </summary>
    /// <param name="market"></param>
    /// <returns></returns>
    public string GetRedisKlineing(long market)
    {
        return string.Format("klineing:{0}", market);
    }

    /// <summary>
    /// 处理进程
    /// </summary>
    /// <returns></returns>
    public string GetRedisProcess()
    {
        return string.Format("process");
    }

    /// <summary>
    /// 验证码
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public string GetRedisVerificationCode(string id)
    {
        return string.Format("verification_code:{0}", id);
    }

    /// <summary>
    /// 登陆用户
    /// </summary>
    /// <returns></returns>
    public string GetRedisOnline(long no)
    {
        return string.Format($"online:{no}");
    }

    /// <summary>
    /// 黑名单登录用户
    /// </summary>
    /// <returns></returns>
    public string GetRedisBlacklist()
    {
        return string.Format("blacklist");
    }

    /// <summary>
    /// 用户api账户信息(注意,当修改时记得删除redis数据)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public string GetRedisApiKey()
    {
        return string.Format("api_key");
    }

    /// <summary>
    /// 用户ID全局锁
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public string GetRedisLookUserId(long user_id)
    {
        return string.Format("look:userid:{0}", user_id);
    }

    /// <summary>
    /// MQ:发送历史成交记录
    /// </summary>
    /// <param name="market"></param>
    /// <returns></returns>
    public string GetMqOrderDeal(long market)
    {
        return string.Format("deal_{0}", market);
    }

    /// <summary>
    /// MQ:挂单队列
    /// </summary>
    /// <param name="market"></param>
    /// <returns></returns>
    public string GetMqOrderPlace(long market)
    {
        return string.Format("order_place_{0}", market);
    }

    /// <summary>
    /// MQ:订阅
    /// </summary>
    /// <param name="channel">订阅频道</param>
    /// <param name="data">不需要登录:交易对id,需要登录:用户id</param>
    /// <returns></returns>
    public string GetMqSubscribe(E_WebsockerChannel channel, long data)
    {
        return string.Format("{0}_{1}", channel, data);
    }

    /// <summary>
    /// Minio:实名认证
    /// </summary>
    /// <returns></returns>
    public string GetMinioRealname()
    {
        return string.Format("realname");
    }

    /// <summary>
    /// Minio:币图标
    /// </summary>
    /// <returns></returns>
    public string GetMinioCoin()
    {
        return string.Format("coin");
    }

}