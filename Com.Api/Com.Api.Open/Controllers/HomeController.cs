using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Com.Models.Enum;
using Com.Models.Base;
using Com.Bll;
using Com.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Com.Bll.Models;
using Microsoft.EntityFrameworkCore;
using Com.Models.Db;

namespace Com.Api.Open.Controllers;

/// <summary>
/// 基础接口
/// </summary>
[Route("[controller]/[action]")]
[ApiController]
public class HomeController : ControllerBase
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
    public HomeController(IServiceProvider provider, IDbContextFactory<DbContextEF> db_factory, IConfiguration configuration, IHostEnvironment environment, ILogger<MainService> logger)
    {

        this.service_base = new ServiceBase(provider, db_factory, configuration, environment, logger);
        this.service_list = new ServiceList(service_base);

    }

    /// <summary>
    /// 获取基本信息
    /// </summary>
    /// <param name="site">站点</param>
    /// <returns></returns>
    [HttpGet]
    // [ResponseCache(CacheProfileName = "cache_3")]
    public Res<ResBaseInfo> GetBaseInfo(int site = 1)
    {
        Res<ResBaseInfo> res = new Res<ResBaseInfo>();
        res.code = E_Res_Code.ok;
        res.data = new ResBaseInfo()
        {
            website_name = "模拟交易",
            website_icon = "https://freeware.iconfactory.com/assets/engb/preview.png",
            website_time = DateTimeOffset.UtcNow,
            time_zone = HttpContext.Session.GetInt32("time_zone") ?? 0,
            website_serivcefile = this.service_base.configuration["minio:endpoint"],
        };
        return res;
    }

    /// <summary>
    /// 设置会话时区
    /// </summary>
    /// <param name="time_zone">时区,8:东八区</param>
    /// <returns></returns>
    [HttpPost]
    public Res<bool> SetTimeZone(int time_zone)
    {
        Res<bool> res = new Res<bool>();
        res.code = E_Res_Code.ok;
        HttpContext.Session.SetInt32("time_zone", time_zone);
        return res;
    }

}