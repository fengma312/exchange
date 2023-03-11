using System.IO.Compression;
using System.Text;
using Com.Bll.Models;
using Com.Models.Db;
using Google_Authenticator_netcore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Com.Bll;

/// <summary>
/// Service:公共服务
/// </summary>
public class Common
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
    /// 初始化
    /// </summary>
    /// <param name="service_base">基础服务</param>
    public Common(ServiceBase service_base)
    {
        this.service_base = service_base;
        this.service_key = new ServiceKey(service_base);
    }

    /// <summary>
    /// 生成验证码
    /// </summary>
    /// <param name="n">位数</param>
    /// <returns>验证码字符串</returns>
    public string CreateRandomCode(int n)
    {
        //产生验证码的字符集(去除I 1 l L，O 0等易混字符)
        string charSet = "2,3,4,5,6,8,9,A,B,C,D,E,F,G,H,J,K,M,N,P,R,S,U,W,X,Y";
        string[] CharArray = charSet.Split(',');
        string randomCode = "";
        int temp = -1;
        Random rand = new Random();
        for (int i = 0; i < n; i++)
        {
            if (temp != -1)
            {
                rand = new Random(i * temp * ((int)DateTime.Now.Ticks));
            }
            int t = rand.Next(CharArray.Length - 1);
            if (temp == t)
            {
                return CreateRandomCode(n);
            }
            temp = t;
            randomCode += CharArray[t];
        }
        return randomCode;
    }

    /// <summary>
    /// 压缩字符
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public byte[] Compression(string json)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        using (var compressedStream = new MemoryStream())
        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
        {
            zipStream.Write(bytes, 0, bytes.Length);
            zipStream.Close();
            bytes = compressedStream.ToArray();
            return bytes;
        }
    }

    /// <summary>
    /// 校验验证码
    /// </summary>
    /// <param name="no">编号</param>
    /// <param name="code">验证码</param>
    /// <returns></returns>
    public bool VerificationCode(string no, string code)
    {
        string? verify = ServiceFactory.instance.redis.StringGet(this.service_key.GetRedisVerificationCode(no));
        if (verify != null && verify.ToLower() == code.ToLower())
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 给用户创建google验证器
    /// </summary>
    /// <param name="issuer">签发者</param>
    /// <param name="user_id">用户id</param>
    /// <returns></returns>
    public string CreateGoogle2FA(string issuer, long user_id)
    {
        TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
        string tempKey = CreateRandomCode(40);
        using (DbContextEF db = this.service_base.db_factory.CreateDbContext())
        {
            Users? user = db.Users.FirstOrDefault(P => P.user_id == user_id);
            if (user != null && user.disabled == false && user.verify_google == false)
            {
                SetupCode setupInfo = tfa.GenerateSetupCode(issuer, user.email, tempKey, 300, 300);
                user.google_key = tempKey;
                if (db.SaveChanges() > 0)
                {
                    return setupInfo.ManualEntryKey;
                }
            }
        }
        return "";
    }

    /// <summary>
    /// 验证google验证器
    /// </summary>
    /// <param name="google_key">google key</param>
    /// <param name="_2FA">google验证码</param>
    /// <returns></returns>
    public bool Verification2FA(string google_key, string _2FA)
    {
        return new TwoFactorAuthenticator().ValidateTwoFactorPIN(google_key, _2FA);
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="email">邮箱地址</param>
    /// <param name="content">内容</param>
    /// <returns></returns>
    public bool SendEmail(string email, string content)
    {
        return true;
    }

    /// <summary>
    /// 发送手机短信
    /// </summary>
    /// <param name="phone">手机号码</param>
    /// <param name="content">内容</param>
    /// <returns></returns>
    public bool SendPhone(string phone, string content)
    {
        return true;
    }

    /// <summary>
    /// 上分布式锁
    /// </summary>
    /// <param name="key">redis键</param>
    /// <param name="value">redis值</param>
    /// <param name="timeout">超时(毫秒)</param>
    /// <param name="action">方法</param>
    public void Look(IDatabase redis, string key, string value, long timeout = 5000, Action action = null!)
    {
        if (action == null)
        {
            return;
        }
        try
        {
            if (redis.StringSet(key, value, TimeSpan.FromMilliseconds(timeout), StackExchange.Redis.When.NotExists, CommandFlags.None))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    this.service_base.logger.LogError(ex, "redis分布试锁错误(业务)");
                }
                finally
                {
                    redis.KeyDelete(key);
                }
            }
        }
        catch (Exception ex)
        {
            this.service_base.logger.LogError(ex, "redis分布试锁错误");
        }
    }

    /// <summary>
    /// 获取本地ip地址
    /// </summary>
    /// <returns></returns>
    public string? GetLocalIp()
    {
        var addressList = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList;
        var ips = addressList.Where(address => address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Select(address => address.ToString()).ToArray();
        if (ips.Length == 1)
        {
            return ips.First();
        }
        return ips.Where(address => !address.EndsWith(".1")).FirstOrDefault() ?? ips.FirstOrDefault();
    }

}