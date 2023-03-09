using Com.Models.Db;
using Com.Models.Enum;

namespace Com.Bll.Models;

/// <summary>
/// 服务列表
/// </summary>
public class ServiceList
{
    /// <summary>
    /// Service:基础服务
    /// </summary>
    public readonly ServiceBase service_base;
    /// <summary>
    /// Service:总管理
    /// </summary>
    public readonly ServiceAdmin service_admin;
    /// <summary>
    /// Service:总管理
    /// </summary>
    public readonly ServiceCluster service_cluster;
    /// <summary>
    /// Service:总管理
    /// </summary>
    public readonly ServiceDeal service_deal;
    /// <summary>
    /// Service:关键字服务
    /// </summary>
    public readonly ServiceKey service_key;
    /// <summary>
    /// Service:总管理
    /// </summary>
    public readonly ServiceKline service_kline;
    /// <summary>
    /// Service:总管理
    /// </summary>
    public readonly ServiceMarket service_market;
    /// <summary>
    /// Service:总管理
    /// </summary>
    public readonly ServiceOrder service_order;
    /// <summary>
    /// Service:总管理
    /// </summary>
    public readonly ServiceUser service_user;
    /// <summary>
    /// Service:总管理
    /// </summary>
    public readonly ServiceWallet service_wallet;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="service_base">基础服务</param>
    public ServiceList(ServiceBase service_base)
    {
        this.service_base = service_base;
        this.service_admin = new ServiceAdmin(service_base);
        this.service_cluster = new ServiceCluster(service_base);
        this.service_deal = new ServiceDeal(service_base);
        this.service_key = new ServiceKey(service_base);
        this.service_kline = new ServiceKline(service_base);
        this.service_market = new ServiceMarket(service_base);
        this.service_order = new ServiceOrder(service_base);
        this.service_user = new ServiceUser(service_base);
        this.service_wallet = new ServiceWallet(service_base);
    }    

}
