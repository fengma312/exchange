using System.Collections.Concurrent;
using Com.Bll.Models;
using Com.Models.Db;
using Com.Models.Enum;

namespace Com.Bll;

/// <summary>
/// Service:服务列表
/// </summary>
public class ServiceCluster
{
    /// <summary>
    /// 基础服务
    /// </summary>
    public readonly ServiceBase service_base;

    private readonly ConcurrentDictionary<long, Cluster> cluster = new ConcurrentDictionary<long, Cluster>();

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="service_base">基础服务</param>
    public ServiceCluster(ServiceBase service_base)
    {
        this.service_base = service_base;
        using (DbContextEF db = this.service_base.db_factory.CreateDbContext())
        {
            foreach (var item in db.Cluster.ToList())
            {
                this.cluster.TryAdd(item.cluster_id, item);
            }
        }
    }

    /// <summary>
    /// 获取服务地址
    /// </summary>
    /// <param name="type">服务类型</param>
    /// <param name="id">主键</param>
    /// <returns></returns>
    public string? GetClusterUrl(E_ServiceType type, long id)
    {
        long mark = id % 10;
        foreach (var item in this.cluster.Values)
        {
            if (item.type == type && item.mark.Contains(mark.ToString()))
            {
                return item.url;
            }
        }
        return null;
    }



}
