using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Com.Models.Enum;
using Com.Models.Base;
using Com.Bll;
using Com.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Com.Bll.Models;
using Microsoft.EntityFrameworkCore;
using Com.Models.Db;

namespace Com.Api.Open.Controllers;

/// <summary>
/// 行情数据
/// </summary>
[Route("[controller]/[action]")]
[ApiController]
[AllowAnonymous]
public class MarketController : ControllerBase
{
    /// <summary>
    /// Service:基础服务
    /// </summary>
    public readonly ServiceBase service_base;
    /// <summary>
    /// 
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
    public MarketController(IServiceProvider provider, IDbContextFactory<DbContextEF> db_factory, IConfiguration configuration, IHostEnvironment environment, ILogger<MainService> logger)
    {
        this.service_base = new ServiceBase(provider, db_factory, configuration, environment, logger);
        this.service_list = new ServiceList(service_base);
    }

    /// <summary>
    /// 获取交易对基本信息
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <returns></returns>
    [HttpPost]
    public Res<List<BaseMarket>> Market(List<string> symbol)
    {
        return this.service_list.service_market.Market(symbol);
    }

    /// <summary>
    /// 获取聚合行情
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <returns></returns>
    [HttpPost]
    public Res<List<ResTicker>> Ticker(List<string> symbol)
    {
        return this.service_list.service_deal.Ticker(symbol);
    }

    /// <summary>
    /// 获取深度行情
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <param name="sz">深度档数,只支持10,50,200</param>
    /// <returns></returns>
    [HttpGet]
    public Res<ResDepth?> Depth(string symbol, int sz = 50)
    {
        return this.service_list.service_market.Depth(symbol, sz);
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
    [HttpGet]
    public Res<List<ResKline>?> Klines(string symbol, E_KlineType type, DateTimeOffset start, DateTimeOffset? end, long skip, long take)
    {
        return this.service_list.service_kline.Klines(symbol, type, start, end, skip, take);
    }

    /// <summary>
    /// 获取历史成交记录
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="skip">跳过行数</param>
    /// <param name="take">获取行数</param>
    /// <returns></returns>
    [HttpGet]
    public Res<List<BaseDeal>> Deals(string symbol, DateTimeOffset start, DateTimeOffset? end, long skip, long take)
    {
        return this.service_list.service_deal.Deals(symbol, start, end, skip, take);
    }

}