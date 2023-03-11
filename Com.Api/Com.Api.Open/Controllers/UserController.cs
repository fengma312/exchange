using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Com.Models.Enum;
using Com.Models.Base;
using Com.Bll;
using Com.Bll.Util;
using Com.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Com.Bll.Models;
using Com.Models.Db;

namespace Com.Api.Open.Controllers;

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
    /// <summary>
    /// 
    /// </summary>
    public readonly ServiceList service_list;
    /// <summary>
    /// 数据库
    /// </summary>
    public DbContextEF db = null!;
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
    public UserController(IServiceProvider provider, IDbContextFactory<DbContextEF> db_factory, IConfiguration configuration, IHostEnvironment environment, ILogger<MainService> logger)
    {
        using (DbContextEF db = db_factory.CreateDbContext())
        {
            this.db = db;
        }
        this.service_base = new ServiceBase(provider, db_factory, configuration, environment, logger);
        this.service_list = new ServiceList(service_base);
        common = new Common(service_base);
    }

    /// <summary>
    /// 登出
    /// </summary>   
    /// <returns></returns>
    [HttpPost]
    [Route("logout")]
    public Res<bool> Logout()
    {
        return this.service_list.service_user.Logout(this.login.no, this.login.user_id, this.login.app);
    }

    /// <summary>
    /// 申请手机验证
    /// </summary>
    /// <param name="phone">手机号</param>
    /// <returns></returns>
    [HttpPost]
    [Route("ApplyPhone")]
    public Res<bool> ApplyPhone(string phone)
    {
        Res<bool> res = new Res<bool>();
        res.code = E_Res_Code.fail;
        res.data = false;
        Users? user = db.Users.AsNoTracking().SingleOrDefault(P => P.user_id == this.login.user_id);
        if (user == null || user.disabled == true || user.verify_phone == true)
        {

            res.code = E_Res_Code.apply_fail;
            res.data = false;
            res.msg = "用户被禁用或已经验证过";
            return res;
        }
        else
        {
            string code = common.CreateRandomCode(6);
#if (DEBUG)
            code = "123456";
#endif
            string content = $"Exchange 手机验证码:{code}";
            if (common.SendPhone(phone, content))
            {
                ServiceFactory.instance.redis.StringSet(this.service_list.service_key.GetRedisVerificationCode(this.login.user_id + phone.Trim()), code, TimeSpan.FromMinutes(10));

                res.code = E_Res_Code.ok;
                res.data = true;
                return res;
            }
        }
        return res;
    }

    /// <summary>
    /// 验证手机申请
    /// </summary>
    /// <param name="phone">手机号</param>
    /// <param name="code">短信验证码</param>
    /// <returns></returns>
    [HttpPost]
    [Route("VerifyPhone")]
    public Res<bool> VerifyPhone(string phone, string code)
    {
        Res<bool> res = new Res<bool>();
        res.code = E_Res_Code.fail;
        res.data = false;
        RedisValue rv = ServiceFactory.instance.redis.StringGet(this.service_list.service_key.GetRedisVerificationCode(this.login.user_id + phone.Trim()));
        if (rv.HasValue)
        {
            if (rv.ToString().ToUpper() == code.Trim().ToUpper())
            {
                Users? user = db.Users.SingleOrDefault(P => P.user_id == this.login.user_id);
                if (user == null || user.disabled == true || user.verify_phone == true)
                {

                    res.code = E_Res_Code.apply_fail;
                    res.data = false;
                    res.msg = "用户被禁用或已经验证过";
                    return res;
                }
                else
                {
                    user.verify_phone = true;
                    user.phone = phone.Trim();
                    if (db.SaveChanges() > 0)
                    {

                        res.code = E_Res_Code.ok;
                        res.data = true;
                        return res;
                    }
                }
            }
            else
            {

                res.code = E_Res_Code.verification_error;
                res.data = false;
                res.msg = "验证码错误";
                return res;
            }
        }
        return res;
    }

    /// <summary>
    /// 申请Google验证
    /// </summary>   
    /// <returns></returns>
    [HttpPost]
    [Route("ApplyGoogle")]
    public Res<string?> ApplyGoogle()
    {
        Res<string?> res = new Res<string?>();
        res.code = E_Res_Code.fail;
        res.data = common.CreateGoogle2FA(this.service_base.configuration["Jwt:Issuer"], this.login.user_id);
        if (string.IsNullOrWhiteSpace(res.data))
        {
            res.code = E_Res_Code.verification_disable;
            res.msg = "用户被禁用或已申请过验证";
            return res;
        }
        else
        {
            res.code = E_Res_Code.ok;
            return res;
        }
    }

    /// <summary>
    /// 验证google申请
    /// </summary>
    /// <param name="code">google验证码</param>
    /// <returns></returns>
    [HttpPost]
    [Route("VerifyGoogle")]
    public Res<bool> VerifyGoogle(string code)
    {
        Res<bool> res = new Res<bool>();
        res.code = E_Res_Code.fail;
        Users? user = db.Users.SingleOrDefault(P => P.user_id == this.login.user_id);
        if (user == null || user.disabled == true || user.verify_google == true || string.IsNullOrWhiteSpace(user.google_key))
        {
            res.code = E_Res_Code.apply_fail;
            res.data = false;
            res.msg = "用户被禁用或已经验证过";
            return res;
        }
        else
        {
            res.data = common.Verification2FA(user.google_key, code);
            if (res.data == false)
            {
                res.code = E_Res_Code.verification_error;
                res.msg = "验证码错误";
                return res;
            }
            else
            {
                user.verify_google = true;
                db.Users.Update(user);
                if (db.SaveChanges() > 0)
                {
                    res.code = E_Res_Code.ok;
                    return res;
                }
            }
        }
        return res;
    }

    /// <summary>
    /// 验证实名认证
    /// </summary>
    /// <param name="files">实名凭证</param>
    /// <returns></returns>
    [HttpPost]
    [Route("ApplyRealname")]
    public async Task<Res<bool>> ApplyRealname(IFormFile files)
    {
        Res<bool> res = new Res<bool>();
        res.code = E_Res_Code.fail;
        res.data = false;
        if (files == null || files.Length <= 0)
        {
            res.code = E_Res_Code.file_not_found;
            res.data = false;
            res.msg = "未找到文件";
            return res;
        }
        // ServiceMinio service_minio = new ServiceMinio(config, logger);
        // string object_name = ServiceFactory.instance.worker.NextId().ToString() + Path.GetExtension(files.FileName);
        // await service_minio.UploadFile(files.OpenReadStream(), FactoryService.instance.GetMinioRealname(), object_name, files.ContentType);
        Users? users = db.Users.SingleOrDefault(P => P.user_id == this.login.user_id);
        if (users == null || users.disabled == true || users.verify_realname == E_Verify.verify_ok)
        {
            res.code = E_Res_Code.apply_fail;
            res.data = false;
            res.msg = "用户被禁用或已经验证过";
            return res;
        }
        else
        {
            // users.realname_object_name = object_name;
            users.verify_realname = E_Verify.verify_apply;
            db.Users.Update(users);
            if (db.SaveChanges() > 0)
            {

                res.code = E_Res_Code.ok;
                res.data = true;
                return res;
            }
        }
        return res;
    }

    /// <summary>
    /// 申请api用户
    /// </summary>
    /// <param name="code">google验证码</param>
    /// <param name="name">名称</param>
    /// <param name="transaction">是否可交易</param>
    /// <param name="withdrawal">是否可提现</param>
    /// <param name="ip">白名单,多个使用;进行隔离</param>
    /// <returns></returns>
    [HttpPost]
    [Route("ApplyApiUser")]
    public Res<KeyValuePair<string, string>> ApplyApiUser(string code, string? name, bool transaction, bool withdrawal, string ip)
    {
        Res<KeyValuePair<string, string>> res = new Res<KeyValuePair<string, string>>();
        res.code = E_Res_Code.fail;
        res.data = new KeyValuePair<string, string>();
        UsersApi api = new UsersApi()
        {
            user_api_id = ServiceFactory.instance.worker.NextId(),
            name = name,
            user_id = this.login.user_id,
            api_key = Encryption.SHA256Encrypt(ServiceFactory.instance.worker.NextId().ToString()),
            api_secret = Encryption.SHA256Encrypt(ServiceFactory.instance.worker.NextId().ToString()) + Encryption.SHA256Encrypt(ServiceFactory.instance.worker.NextId().ToString()),
            transaction = transaction,
            withdrawal = withdrawal,
            white_list_ip = ip,
            create_time = DateTimeOffset.UtcNow,
        };
        db.UsersApi.Add(api);
        if (db.SaveChanges() > 0)
        {
            res.code = E_Res_Code.ok;
            res.data = new KeyValuePair<string, string>(api.api_key, api.api_secret);
            return res;
        }
        return res;
    }

    /// <summary>
    /// 修改api用户信息
    /// </summary>
    /// <param name="code">google验证码</param>
    /// <param name="id">数据id</param>
    /// <param name="name">名称</param>
    /// <param name="transaction">是否可交易</param>
    /// <param name="withdrawal">是否可提现</param>
    /// <param name="ip">白名单,多个使用;进行隔离</param>
    /// <returns></returns>
    [HttpPost]
    [Route("UpdateApiUser")]
    public Res<bool> UpdateApiUser(string code, long id, string? name, bool transaction, bool withdrawal, string ip)
    {
        Res<bool> res = new Res<bool>();
        res.code = E_Res_Code.fail;
        UsersApi? api = db.UsersApi.AsNoTracking().SingleOrDefault(P => P.user_api_id == id);
        if (api != null)
        {
            api.name = name;
            api.transaction = transaction;
            api.withdrawal = withdrawal;
            api.white_list_ip = ip;
            db.UsersApi.Update(api);
            if (db.SaveChanges() > 0)
            {
                res.code = E_Res_Code.ok;
                res.data = true;
                return res;
            }
        }
        return res;
    }

    /// <summary>
    /// 获取api用户
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("GetApiUser")]
    public Res<List<BaseUsersApi>> GetApiUser()
    {
        Res<List<BaseUsersApi>> res = new Res<List<BaseUsersApi>>();
        res.code = E_Res_Code.ok;
        res.data = db.UsersApi.AsNoTracking().Where(P => P.user_id == this.login.user_id).ToList().ConvertAll(P => (BaseUsersApi)P);
        return res;
    }

}