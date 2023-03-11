using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


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
using StackExchange.Redis;

namespace Com.Api.Admin.Controllers;

/// <summary>
/// 用户接口
/// </summary>
[Route("[controller]/[action]")]
[Authorize]
[ApiController]
public class UserController : ControllerBase
{

    /// <summary>
    /// Service:基础服务
    /// </summary>
    public readonly ServiceBase service_base;
    public readonly ServiceList service_list;

    /// <summary>
    /// db
    /// </summary>
    private readonly DbContextEF db;


    /// <summary>
    /// 登录信息
    /// </summary>
    /// <value></value>
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
    public UserController(IServiceProvider provider, IDbContextFactory<DbContextEF> db_factory, DbContextEF db, IConfiguration configuration, IHostEnvironment environment, ILogger<MainService> logger)
    {
        this.service_base = new ServiceBase(provider, db_factory, configuration, environment, logger);
        service_list = new ServiceList(service_base);
        this.db = db;
    }


    /// <summary>
    /// 获取用户
    /// </summary>
    /// <param name="uid">用户id</param>
    /// <param name="user_name">用户名</param>
    /// <param name="email">邮箱地址</param>
    /// <param name="phone">手机号</param>
    /// <param name="skip">跳过多少行</param>
    /// <param name="take">提取多少行</param>
    /// <returns></returns>
    [HttpGet]
    public Res<List<Users>> GetUser(long? uid, string? user_name, string? email, string? phone, int skip = 0, int take = 50)
    {
        Res<List<Users>> res = new Res<List<Users>>();
        res.code = E_Res_Code.ok;
        res.data = service_list.service_user.GetUser(uid, user_name, email, phone, skip, take);
        return res;
    }

    /// <summary>
    /// 需要实名认证用户
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public Res<List<BaseUser>> ApplyRealname()
    {
        Res<List<BaseUser>> res = new Res<List<BaseUser>>();
        res.code = E_Res_Code.ok;
        res.data = db.Users.AsNoTracking().Where(P => P.verify_realname == E_Verify.verify_no).ToList().ConvertAll(P => (BaseUser)P);
        return res;
    }

    /// <summary>
    /// 实名认证用户
    /// </summary>
    /// <param name="uid">用户id</param>
    /// <param name="verify">验证方式</param>
    /// <returns></returns>
    [HttpPost]
    public Res<bool> VerifyRealname(long uid, E_Verify verify)
    {
        Res<bool> res = new Res<bool>();
        Users? users = db.Users.FirstOrDefault(P => P.user_id == uid && P.verify_realname == E_Verify.verify_no);
        if (users == null)
        {
            res.code = E_Res_Code.fail;
            res.msg = "用户不存在或已实名认证";
            res.data = false;
            return res;
        }
        users.verify_realname = verify;
        db.SaveChanges();
        res.code = E_Res_Code.ok;
        res.data = true;
        return res;
    }

}