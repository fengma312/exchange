using System.Collections.Concurrent;
using Com.Bll.Models;
using Com.Models.Db;
using Com.Models.Enum;
using Newtonsoft.Json;
using StackExchange.Redis;

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
    /// <summary>
    /// Service:关键字
    /// </summary>
    public readonly ServiceKey service_key;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="service_base">基础服务</param>
    public ServiceCluster(ServiceBase service_base)
    {
        this.service_base = service_base;
        this.service_key = new ServiceKey(service_base);
    }


    /// <summary>
    /// 保存到redis
    /// </summary>
    public void SaveRedis()
    {
        using DbContextEF db = this.service_base.db_factory.CreateDbContext();
        foreach (var item in db.Cluster.ToList())
        {
            ServiceFactory.instance.redis.HashSet(this.service_key.GetRedisTable(nameof(Cluster)), item.cluster_id, JsonConvert.SerializeObject(item));
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
        long remain = id % 10;
        HashEntry[] he = ServiceFactory.instance.redis.HashGetAll(this.service_key.GetRedisTable(nameof(Cluster)));
        foreach (var item in he)
        {
            if (item.Value.IsNullOrEmpty)
            {
                continue;
            }
            Cluster? cluster = JsonConvert.DeserializeObject<Cluster>(item.Value!);
            if (cluster != null && cluster.type == type && cluster.mark.Contains(remain.ToString()))
            {
                return cluster.url;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取所有数据
    /// </summary>
    /// <returns></returns>
    public List<Cluster> GetAllCluster()
    {
        List<Cluster> lists = new List<Cluster>();
        HashEntry[] he = ServiceFactory.instance.redis.HashGetAll(this.service_key.GetRedisTable(nameof(Cluster)));
        foreach (var item in he)
        {
            if (item.Value.IsNullOrEmpty)
            {
                continue;
            }
            Cluster? cluster = JsonConvert.DeserializeObject<Cluster>(item.Value!);
            if (cluster != null)
            {
                lists.Add(cluster);
            }
        }
        return lists;
    }



}
