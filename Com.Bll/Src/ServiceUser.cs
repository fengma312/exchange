using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Com.Bll.Models;
using Com.Bll.Util;
using Com.Db;
using Com.Models.Base;
using Com.Models.Db;
using Com.Models.Enum;
using Google_Authenticator_netcore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Com.Bll;

/// <summary>
/// Service:用户
/// </summary>
public class ServiceUser
{
    /// <summary>
    /// 基础服务
    /// </summary>
    public readonly ServiceBase service_base;
    /// <summary>
    /// Service:关键字
    /// </summary>
    public readonly ServiceKey service_key;
    /// <summary>
    /// service:公共服务
    /// </summary>
    private readonly Common service_common;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="service_base">基础服务</param>
    public ServiceUser(ServiceBase service_base)
    {
        this.service_base = service_base;
        this.service_key = new ServiceKey(service_base);
        this.service_common = new Common(service_base);
    }

    /// <summary>
    /// 获取登录信息
    /// </summary>
    /// <param name="no"></param>
    /// <param name="user_id"></param>
    /// <param name="user_name"></param>
    /// <param name="app"></param>
    /// <param name="claims_principal"></param>
    /// <returns></returns>
    public (long no, long user_id, string user_name, E_App app, string public_key) GetLoginUser(System.Security.Claims.ClaimsPrincipal claims_principal)
    {
        E_App app = E_App.undefined;
        string user_name = "", public_key = "";
        long user_id = 0, no = 0;
        Claim? claim = claims_principal.Claims.FirstOrDefault(P => P.Type == "no");
        if (claim != null)
        {
            no = long.Parse(claim.Value);
        }
        claim = claims_principal.Claims.FirstOrDefault(P => P.Type == "user_id");
        if (claim != null)
        {
            user_id = long.Parse(claim.Value);
        }
        claim = claims_principal.Claims.FirstOrDefault(P => P.Type == "user_name");
        if (claim != null)
        {
            user_name = claim.Value;
        }
        claim = claims_principal.Claims.FirstOrDefault(P => P.Type == "app");
        if (claim != null)
        {
            app = (E_App)Enum.Parse(typeof(E_App), claim.Value);
        }
        claim = claims_principal.Claims.FirstOrDefault(P => P.Type == "public_key");
        if (claim != null)
        {
            public_key = claim.Value;
        }
        return (no, user_id, user_name, app, public_key);
    }

    /// <summary>
    /// 生成token
    /// </summary>
    /// <param name="no">登录唯一码</param>
    /// <param name="user">用户信息</param>
    /// <param name="app">终端类型</param>
    /// <returns>jwt</returns>
    public string GenerateToken(long no, Users user, E_App app)
    {
        var claims = new[]
            {
                new Claim("no",no.ToString()),
                new Claim("user_id",user.user_id.ToString()),
                new Claim("user_name",user.user_name),
                new Claim("app", app.ToString()),
                // new Claim("name",user.user_name),
                // new Claim("iis",this.service_base.configuration["Jwt:Issuer"]),
                // new Claim("aud",this.service_base.configuration["Jwt:Audience"]),
                // new Claim("sub","aaa"),
            };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.service_base.configuration["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: this.service_base.configuration["Jwt:Issuer"],// 签发者
            audience: this.service_base.configuration["Jwt:Audience"],// 接收者
            expires: DateTimeOffset.UtcNow.AddMinutes(double.Parse(this.service_base.configuration["Jwt:Expires"]!)).LocalDateTime,// 过期时间
            claims: claims,
            signingCredentials: creds);// 令牌
        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="email">Email</param>
    /// <param name="password">密码</param>
    /// <param name="code">邮箱验证码</param>
    /// <param name="recommend">推荐人id</param>
    /// <param name="ip">ip地址</param>
    public Res<bool> Register(string email, string password, string code, string? recommend, string ip)
    {
        Res<bool> res = new Res<bool>();
        res.code = E_Res_Code.fail;
        res.data = false;
        if (!Regex.IsMatch(email, @"^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+((\.[a-zA-Z0-9_-]{2,3}){1,2})$"))
        {
            res.code = E_Res_Code.email_irregularity;
            res.msg = "邮箱格式错误";
            return res;
        }
        if (!Regex.IsMatch(password, @"
                            (?=.*[0-9])                     #必须包含数字
                            (?=.*[a-zA-Z])                  #必须包含小写或大写字母
                            (?=([\x21-\x7e]+)[^a-zA-Z0-9])  #必须包含特殊符号
                            .{6,20}                         #至少6个字符,最多20个字符
                            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace))
        {
            res.code = E_Res_Code.password_irregularity;
            res.msg = "密码必须包含数字、小写字母或大写字母、特殊符号,长度6-20位";
            return res;
        }
        if (!service_common.VerificationCode(email, code))
        {
            res.code = E_Res_Code.verification_error;
            res.msg = "验证码错误";
            return res;
        }
        (string public_key, string private_key) key_res = Encryption.GetRsaKey();
        using (DbContextEF db = this.service_base.db_factory.CreateDbContext())
        {
            if (db.Users.Any(P => P.email == email))
            {
                res.code = E_Res_Code.email_repeat;
                res.msg = "邮箱已重复";
                return res;
            }
            Vip? vip0 = db.Vip.SingleOrDefault(P => P.name == "vip0");
            string user_name = ServiceFactory.instance.random.NextInt64(10_001_000, 99_999_999).ToString();
            while (db.Users.Any(P => P.user_name == user_name))
            {
                user_name = ServiceFactory.instance.random.NextInt64(10_001_000, 99_999_999).ToString();
            }
            Users settlement_btc_usdt = new Users()
            {
                user_id = ServiceFactory.instance.worker.NextId(),
                user_name = user_name,
                password = Encryption.SHA256Encrypt(password),
                email = email,
                phone = null,
                verify_email = true,
                verify_phone = false,
                verify_google = false,
                verify_realname = E_Verify.verify_not,
                realname_object_name = null,
                disabled = false,
                transaction = true,
                withdrawal = false,
                user_type = E_UserType.general,
                recommend = recommend,
                vip = vip0?.vip_id ?? 0,
                google_key = null,
                public_key = key_res.public_key,
                private_key = key_res.private_key,
            };
            db.Users.Add(settlement_btc_usdt);
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
    /// 登录
    /// </summary>
    /// <param name="email">账号</param>
    /// <param name="password">密码</param>
    /// <param name="app">登录终端</param>
    /// <param name="ip">登录ip</param>
    /// <returns></returns>
    public async Task<Res<BaseUser>> Login(string email, string password, E_App app, string ip)
    {
        Res<BaseUser> res = new Res<BaseUser>();
        res.code = E_Res_Code.fail;
        using (DbContextEF db = this.service_base.db_factory.CreateDbContext())
        {
            var user = db.Users.FirstOrDefault(P => P.disabled == false && (P.phone == email || P.email == email) && P.password == Encryption.SHA256Encrypt(password));
            if (user == null)
            {

                res.code = E_Res_Code.name_password_error;
                res.msg = "账户或密码错误,登陆失败";
                return res;
            }
            ServiceFactory.instance.redis.KeyDelete(this.service_key.GetRedisVerificationCode(email));
            long no = ServiceFactory.instance.worker.NextId();
            ServiceFactory.instance.redis.HashDelete(this.service_key.GetRedisBlacklist(), $"{user.user_id}_{app}_*");
            ServiceFactory.instance.redis.StringSet(this.service_key.GetRedisOnline(no), $"{user.user_id}_{user.user_name}_{app}", new TimeSpan(0, int.Parse(this.service_base.configuration["Jwt:Expires"]!), 0));
            res.code = E_Res_Code.ok;
            res.data = user;
            res.data.token = GenerateToken(no, user, app);
            string? url = ServiceFactory.instance.service_grpc_client.service_cluster.GetClusterUrl(E_ServiceType.account, user.user_id);
            if (!string.IsNullOrWhiteSpace(url) && ServiceFactory.instance.service_grpc_client.grcp_client_account.TryGetValue(url, out var client))
            {
                await client.LoadUser(new List<long>() { user.user_id });
            }
            return res;
        }
    }

    /// <summary>
    /// 登出
    /// </summary>
    /// <param name="no">登录编号</param>
    /// <param name="uid">用户id</param>
    /// <param name="app">终端</param>
    /// <returns></returns>
    public Res<bool> Logout(long no, long uid, E_App app)
    {
        Res<bool> res = new Res<bool>();
        ServiceFactory.instance.redis.HashSet(this.service_key.GetRedisBlacklist(), $"{uid}_{app}_{no}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        ServiceFactory.instance.redis.StringGetDelete(this.service_key.GetRedisOnline(no));

        res.code = E_Res_Code.ok;
        res.data = true;
        return res;
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
    public List<Users> GetUser(long? uid, string? user_name, string? email, string? phone, int skip = 0, int take = 50)
    {
        //using (var scope = ServiceFactory.instance.constant.provider.CreateScope())
        {
            using (DbContextEF db = this.service_base.db_factory.CreateDbContext())
            {
                return db.Users.AsNoTracking().WhereIf(uid != null, P => P.user_id == uid).WhereIf(!string.IsNullOrWhiteSpace(user_name), P => P.user_name.Contains(user_name!)).WhereIf(!string.IsNullOrWhiteSpace(email), P => P.email == email).WhereIf(!string.IsNullOrWhiteSpace(phone), P => P.phone == phone).Skip(skip).Take(take).ToList();
            }
        }
    }

    /// <summary>
    /// 获取用户
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public Users? GetUser(long uid)
    {
        //using (var scope = ServiceFactory.instance.constant.provider.CreateScope())
        {
            using (DbContextEF db = this.service_base.db_factory.CreateDbContext())
            {
                return db.Users.AsNoTracking().SingleOrDefault(P => P.user_id == uid);
            }
        }
    }

    /// <summary>
    /// 获取用户
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public List<Users> GetUser(List<long> uid)
    {
        //using (var scope = ServiceFactory.instance.constant.provider.CreateScope())
        {
            using (DbContextEF db = this.service_base.db_factory.CreateDbContext())
            {
                return db.Users.AsNoTracking().Where(P => uid.Contains(P.user_id)).ToList();
            }
        }
    }

    /// <summary>
    /// 获取vip
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Vip? GetVip(long id)
    {
        //using (var scope = ServiceFactory.instance.constant.provider.CreateScope())
        {
            using (DbContextEF db = this.service_base.db_factory.CreateDbContext())
            {
                return db.Vip.AsNoTracking().SingleOrDefault(P => P.vip_id == id);
            }
        }
    }

    /// <summary>
    /// 获取user api用户
    /// </summary>
    /// <param name="api_key"></param>
    /// <returns></returns>
    public UsersApi? GetApi(string api_key)
    {
        //using (var scope = ServiceFactory.instance.constant.provider.CreateScope())
        {
            using (DbContextEF db = this.service_base.db_factory.CreateDbContext())
            {
                return db.UsersApi.AsNoTracking().SingleOrDefault(P => P.api_key == api_key);
            }
        }
    }

    /// <summary>
    /// 判断Api账户是否可以交易
    /// </summary>
    /// <param name="api_key"></param>
    /// <returns></returns>
    public (bool, Users?, UsersApi?) ApiUserTransaction(string api_key)
    {
        UsersApi? api = GetApi(api_key);
        if (api == null || !api.transaction)
        {
            return (false, null, null);
        }
        Users? users = GetUser(api.user_id);
        if (users == null || users.disabled || !users.transaction)
        {
            return (false, null, null);
        }
        return (true, users, api);
    }

    /// <summary>
    /// 判断Api账户是否可以取款
    /// </summary>
    /// <param name="api_key"></param>
    /// <returns></returns>
    public (bool, Users?, UsersApi?) ApiUserWithdraw(string api_key)
    {
        UsersApi? api = GetApi(api_key);
        if (api == null || !api.withdrawal)
        {
            return (false, null, null);
        }
        Users? users = GetUser(api.user_id);
        if (users == null || users.disabled || !users.withdrawal)
        {
            return (false, null, null);
        }
        return (true, users, api);
    }

    /// <summary>
    /// 判断账户是否可以交易
    /// </summary>
    /// <param name="uid">用户id</param>
    /// <returns></returns>
    public (bool, Users?) UserTransaction(long uid)
    {
        Users? users = GetUser(uid);
        if (users == null || users.disabled || !users.transaction)
        {
            return (false, null);
        }
        return (true, users);
    }

    /// <summary>
    /// 判断账户是否可以取款
    /// </summary>
    /// <param name="uid">用户id</param>
    /// <returns></returns>
    public (bool, Users?) UserWithdraw(long uid)
    {
        Users? users = GetUser(uid);
        if (users == null || users.disabled || !users.disabled)
        {
            return (false, null);
        }
        return (true, users);
    }

}