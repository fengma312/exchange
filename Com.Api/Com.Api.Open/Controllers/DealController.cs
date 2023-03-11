using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Com.Models.Enum;
using Com.Models.Base;
using Com.Bll;
using Com.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Com.Bll.Models;
using Microsoft.EntityFrameworkCore;
using Com.Models.Db;

namespace Com.Api.Open.Controllers;

/// <summary>
/// 交易接口
/// </summary>
[Route("[controller]/[action]")]
[Authorize]
[ApiController]
public class DealController : ControllerBase
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
    /// 登录信息
    /// </summary>
    private (long no, long user_id, string user_name, E_App app, string public_key) login
    {
        get
        {
            return this.service_list.service_user.GetLoginUser(User);
        }
    }
    
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="provider">服务驱动</param>
    /// <param name="db_factory">db上下文工厂</param>
    /// <param name="configuration">配置接口</param>
    /// <param name="environment">环境接口</param>
    /// <param name="logger">日志接口</param>
    public DealController(IServiceProvider provider, IDbContextFactory<DbContextEF> db_factory, IConfiguration configuration, IHostEnvironment environment, ILogger<MainService> logger)
    {
        this.service_base = new ServiceBase(provider, db_factory, configuration, environment, logger);
        this.service_list = new ServiceList(service_base);

    }

    /// <summary>
    /// 当前用户成交记录
    /// </summary>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="skip">跳过多少行</param>
    /// <param name="take">获取多少行</param>
    /// <returns></returns>
    [HttpGet]
    [ResponseCache(CacheProfileName = "cache_1")]
    public Res<List<BaseDeal>> GetDealByuid(DateTimeOffset? start = null, DateTimeOffset? end = null, int skip = 0, int take = 50)
    {
        return this.service_list.service_deal.GetDealByuid(login.user_id, skip, take, start, end);
    }



}