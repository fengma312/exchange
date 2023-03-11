using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


using Com.Bll;
using Com.Bll.Models;
using Com.Db;
using Com.Models.Base;
using Com.Models.Db;
using Com.Models.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Com.Api.Admin.Controllers;

/// <summary>
/// 订单接口
/// </summary>
[Route("[controller]/[action]")]
// [Authorize]
[ApiController]
public class OrderController : ControllerBase
{
    /// <summary>
    /// Service:基础服务
    /// </summary>
    public readonly ServiceBase service_base;
    /// <summary>
    /// 服务列表
    /// </summary>

    public readonly ServiceList service_list;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="provider">服务驱动</param>
    /// <param name="db_factory">db上下文工厂</param>
    /// <param name="configuration">配置接口</param>
    /// <param name="environment">环境接口</param>
    /// <param name="logger">日志接口</param>
    public OrderController(IServiceProvider provider, IDbContextFactory<DbContextEF> db_factory, IConfiguration configuration, IHostEnvironment environment, ILogger<MainService> logger)
    {
        this.service_base = new ServiceBase(provider, db_factory, configuration, environment, logger);
        service_list = new ServiceList(service_base);
    }

    /// <summary>
    /// 订单查询
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <param name="user_name">用户名</param>
    /// <param name="state">订单状态</param>
    /// <param name="order_id">订单id</param>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="skip">跳过多少行</param>
    /// <param name="take">提取多少行</param>
    /// <returns></returns>
    [HttpGet]
    [ResponseCache(CacheProfileName = "cache_1")]
    public Res<List<Orders>> GetOrder(string symbol, string? user_name = null, E_OrderState? state = null, long? order_id = null, DateTimeOffset? start = null, DateTimeOffset? end = null, int skip = 0, int take = 50)
    {
        return this.service_list.service_order.GetOrder(symbol: symbol, user_name: user_name, state: state, order_id: order_id, start: start, end: end, skip: skip, take: take);
    }

    /// <summary>
    /// 按交易对全部撤单
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<Res<bool>> OrderCancelBySymbol(string symbol)
    {
        Res<bool> result = new Res<bool>();
        Market? info = this.service_list.service_market.GetMarketBySymbol(symbol);
        if (info != null)
        {
            string? url = ServiceFactory.instance.service_grpc_client.service_cluster.GetClusterUrl(E_ServiceType.trade, info.market_id);
            if (!string.IsNullOrWhiteSpace(url) && ServiceFactory.instance.service_grpc_client.grcp_client_trade.TryGetValue(url, out var client))
            {
                return await client.TradeCancelOrder(symbol, 0, 1, new List<long>());
            }
        }
        return result;
    }

}