using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Com.Bll;
using Com.Bll.Models;
using Com.Bll.Util;
using Com.Db;
using Com.Models.Base;
using Com.Models.Db;
using Com.Models.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Com.Api.Open.Controllers;

/// <summary>
/// 账户
/// </summary>
[Route("[controller]")]
[AllowAnonymous]
[ApiController]
public class AccountController : ControllerBase
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
    /// 公共类
    /// </summary>
    /// <returns></returns>
    private readonly Common common;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="provider">服务驱动</param>
    /// <param name="db_factory">db上下文工厂</param>
    /// <param name="configuration">配置接口</param>
    /// <param name="environment">环境接口</param>
    /// <param name="logger">日志接口</param>
    public AccountController(IServiceProvider provider, IDbContextFactory<DbContextEF> db_factory, IConfiguration configuration, IHostEnvironment environment, ILogger<MainService> logger)
    {

        this.service_base = new ServiceBase(provider, db_factory, configuration, environment, logger);
        this.service_list = new ServiceList(service_base);
        common = new Common(service_base);
    }

    /// <summary>
    /// 注册账号
    /// </summary>
    /// <param name="email">邮箱地址</param>
    /// <param name="password">密码</param>
    /// <param name="code">邮箱验证码</param>
    /// <param name="recommend">推荐人id</param>
    /// <returns></returns>
    [HttpPost]
    [Route("register")]
    public Res<bool> Register(string email, string password, string code, string? recommend)
    {
        return this.service_list.service_user.Register(email, password, code, recommend, Request.GetIp());
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="email">email</param>
    /// <param name="password">密码</param>
    /// <param name="app">终端</param>
    /// <returns></returns>
    [HttpPost]
    [Route("login")]
    public Res<BaseUser> Login(string email, string password, E_App app)
    {
        return this.service_list.service_user.Login(email, password, app, Request.GetIp());
    }

    /// <summary>
    /// 注册时发送Email验证码
    /// </summary>
    /// <param name="email">邮件地址</param>
    /// <returns></returns>
    [HttpPost]
    [Route("SendEmailCodeByRegister")]
    public Res<bool> SendEmailCodeByRegister(string email)
    {
        Res<bool> res = new Res<bool>();
        res.code = E_Res_Code.fail;
        res.data = false;
        email = email.Trim().ToLower();
        if (!Regex.IsMatch(email, @"^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+((\.[a-zA-Z0-9_-]{2,3}){1,2})$"))
        {
            res.code = E_Res_Code.email_irregularity;
            res.msg = "邮箱格式错误";
            return res;
        }
        string code = common.CreateRandomCode(6);
#if (DEBUG)
        code = "123456";
#endif
        string content = $"Exchange 注册验证码:{code}";
        using (DbContextEF db = this.service_base.db_factory.CreateDbContext())
        {
            if (db.Users.Any(P => P.email.ToLower() == email))
            {
                res.code = E_Res_Code.email_repeat;
                res.msg = "邮箱地址已存在";
                return res;
            }
            else
            {
                if (common.SendEmail(email, content))
                {
                    ServiceFactory.instance.redis.StringSet(this.service_list.service_key.GetRedisVerificationCode(email), code, TimeSpan.FromMinutes(10));

                    res.code = E_Res_Code.ok;
                    res.data = true;
                    return res;
                }
            }
        }
        return res;
    }

}