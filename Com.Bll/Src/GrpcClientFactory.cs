using System.Collections.Concurrent;
using Com.Bll.Models;
using Com.Models.Db;
using Com.Models.Enum;
using Grpc.Net.Client;
using ServiceAccountGrpc;

namespace Com.Bll;

/// <summary>
/// Grpc客户端:账户服务
/// </summary>
public class GrpcClientFactory
{
    /// <summary>
    /// 基础服务
    /// </summary>
    public readonly ServiceBase service_base;
    /// <summary>
    /// Service:服务列表
    /// </summary>
    public readonly ServiceCluster service_cluster;
    /// <summary>
    /// Service:服务列表
    /// </summary>
    public readonly ConcurrentDictionary<string, GrpcClientAccount> grcp_client_account = new ConcurrentDictionary<string, GrpcClientAccount>();
    /// <summary>
    /// Service:服务列表
    /// </summary>
    public readonly ConcurrentDictionary<string, GrpcClientTrade> grcp_client_trade = new ConcurrentDictionary<string, GrpcClientTrade>();
    /// <summary>
    /// Service:服务列表
    /// </summary>
    public readonly ConcurrentDictionary<string, GrpcClientMatch> grcp_client_match = new ConcurrentDictionary<string, GrpcClientMatch>();

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="service_base">基础服务</param>
    public GrpcClientFactory(ServiceBase service_base)
    {
        this.service_base = service_base;
        this.service_cluster = new ServiceCluster(service_base);
    }

    /// <summary>
    /// 初始化grpc客户端
    /// </summary>
    /// <param name="type"></param>
    public void Init(E_ServiceType type)
    {
        this.service_cluster.SaveRedis();
        List<Cluster> lists = this.service_cluster.GetAllCluster();
        foreach (var item in lists)
        {
            if (item.type != type)
            {
                continue;
            }
            string url = $"{item.ip}:{item.port}";
            switch (type)
            {
                case E_ServiceType.account:
                    grcp_client_account.TryAdd(url, new GrpcClientAccount(url));
                    break;
                case E_ServiceType.match:
                    grcp_client_match.TryAdd(url, new GrpcClientMatch(url));
                    break;
                case E_ServiceType.trade:
                    grcp_client_trade.TryAdd(url, new GrpcClientTrade(url));
                    break;
            }
        }
    }


}