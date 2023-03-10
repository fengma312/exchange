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
public class ServiceGrpcClient
{
    /// <summary>
    /// 基础服务
    /// </summary>
    public readonly ServiceBase service_base;
    /// <summary>
    /// Service:服务列表
    /// </summary>
    public readonly ServiceCluster service_cluster;

    public readonly ConcurrentDictionary<string, GrpcClientAccount> grcp_client_account = new ConcurrentDictionary<string, GrpcClientAccount>();
    public readonly ConcurrentDictionary<string, GrpcClientTrade> grcp_client_trade = new ConcurrentDictionary<string, GrpcClientTrade>();
    public readonly ConcurrentDictionary<string, GrpcClientMatch> grcp_client_match = new ConcurrentDictionary<string, GrpcClientMatch>();

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="service_base">基础服务</param>
    public ServiceGrpcClient(ServiceBase service_base)
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
        List<Cluster> lists = this.service_cluster.GetAllCluster();
        foreach (var item in lists)
        {
            if (item.type != type)
            {
                continue;
            }
            switch (type)
            {
                case E_ServiceType.account:
                    grcp_client_account.TryAdd(item.url, new GrpcClientAccount(item.url));
                    break;
                case E_ServiceType.match:
                    grcp_client_match.TryAdd(item.url, new GrpcClientMatch(item.url));
                    break;
                case E_ServiceType.trade:
                    grcp_client_trade.TryAdd(item.url, new GrpcClientTrade(item.url));
                    break;
            }
        }
    }


}