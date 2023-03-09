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

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="service_base">基础服务</param>
    public ServiceCluster(ServiceBase service_base)
    {
        this.service_base = service_base;
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
        using (DbContextEF db = this.service_base.db_factory.CreateDbContext())
        {
            return db.Cluster.FirstOrDefault(P => P.type == type && P.mark.Contains(mark.ToString()))?.url;
        }
    }



}
