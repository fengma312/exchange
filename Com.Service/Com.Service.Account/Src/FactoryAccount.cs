using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Grpc.Core;
using Com.Models.Db;
using Com.Bll.Models;
using System.Diagnostics;
using System.Collections.Concurrent;
using ServiceAccountGrpc;
using Com.Service.Account.Models;

namespace Com.Service.Account;

/// <summary>
/// 用来管理撮合服务
/// </summary>
public class FactoryAccount
{
    /// <summary>
    /// 单例类的实例
    /// </summary>
    /// <returns></returns>
    public static readonly FactoryAccount instance = new FactoryAccount();
    /// <summary>
    /// 基础服务
    /// </summary>
    public ServiceBase service_base = null!;
    /// <summary>
    /// 服务列表
    /// </summary>
    public ServiceList service_list = null!;  
    /// <summary>
    /// 交易服务集合
    /// </summary>
    /// <typeparam name="long">交易对id</typeparam>
    /// <typeparam name="TradeModel">交易对象</typeparam>
    /// <returns></returns>
    public ConcurrentDictionary<long, AccountModel> service = new ConcurrentDictionary<long, AccountModel>();
    
    /// <summary>
    /// 私有构造方法
    /// </summary>
    private FactoryAccount()
    {
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="service_base"></param>
    public void Init(ServiceBase service_base)
    {
        this.service_base = service_base;
        this.service_list = new ServiceList(service_base);
        this.StartGrpcService();
    }

    /// <summary>
    /// 启动Grpc服务端
    /// </summary>
    private void StartGrpcService()
    {
        Cluster? cluster = this.service_list.service_cluster.GetCluster(service_base.configuration.GetValue<long>("ClusterId"));
        if (cluster == null)
        {
            return;
        }
        Grpc.Core.Server server = new Grpc.Core.Server
        {
            Services = { AccountGrpc.BindService(new GrpcServiceAccount()) },
            Ports = { new ServerPort("0.0.0.0", cluster.port, ServerCredentials.Insecure) }
        };
        server.Start();
    }

    /// <summary>
    /// 加载用户信息
    /// </summary>
    public void LoadConfig(List<long> user_id)
    {
        List<long> id = user_id.Distinct().Except(this.service.Keys).ToList();
        List<Users> users = this.service_list.service_user.GetUser(id);
        foreach (var item in users)
        {
            this.service.TryAdd(item.user_id, new AccountModel(item));
        }
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <param name="user_id"></param>
    /// <returns></returns>
    public List<Users> GetUser(List<long> user_id)
    {
        List<Users> users = new List<Users>();
        foreach (var item in user_id.Distinct())
        {
            if (this.service.TryGetValue(item, out var value))
            {
                users.Add(value.users);
            }
        }
        return users;
    }



}