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
/// 钱包接口
/// </summary>
[Route("[controller]/[action]")]
[Authorize]
[ApiController]
public class WalletController : ControllerBase
{

    /// <summary>
    /// db
    /// </summary>
    private readonly DbContextEF db;
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
    /// Service:基础服务
    /// </summary>
    public readonly ServiceBase service_base;
    /// <summary>
    /// Service:基础服务
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
    public WalletController(IServiceProvider provider, IDbContextFactory<DbContextEF> db_factory, IConfiguration configuration, IHostEnvironment environment, ILogger<MainService> logger)
    {
        this.service_base = new ServiceBase(provider, db_factory, configuration, environment, logger);
        service_list = new ServiceList(service_base);
        using DbContextEF db = db_factory.CreateDbContext();
        this.db = db;
    }

    /// <summary>
    /// 划转(同一账户同一币种不同钱包类型划转资金)
    /// </summary>
    /// <param name="coin_id">币id</param>
    /// <param name="from">支付钱包类型</param>
    /// <param name="to">接收钱包类型</param>
    /// <param name="amount">金额</param>
    /// <returns></returns>
    [HttpPost]
    public Res<bool> Transfer(long coin_id, E_WalletType from, E_WalletType to, decimal amount)
    {
        return service_list.service_wallet.Transfer(login.user_id, coin_id, from, to, amount);
    }

    /// <summary>
    /// 获取用户所有钱包
    /// </summary>
    /// <param name="wallet_type">钱包类型</param>
    /// <param name="coin_name">币名</param>
    /// <returns></returns>
    [HttpGet]
    public Res<List<Wallet>?> GetWallet(E_WalletType wallet_type, string? coin_name)
    {
        Res<List<Wallet>?> res = new Res<List<Wallet>?>();
        res.code = E_Res_Code.fail;
        var linq = from coin in db.Coin
                   join wallet in db.Wallet.Where(P => P.user_id == this.login.user_id && P.wallet_type == wallet_type).WhereIf(!string.IsNullOrWhiteSpace(coin_name), P => P.coin_name.Contains(coin_name!))
                   on coin.coin_id equals wallet.coin_id into temp
                   from bb in temp.DefaultIfEmpty()
                   select new Wallet
                   {
                       wallet_id = bb == null ? ServiceFactory.instance.worker.NextId() : bb.user_id,
                       wallet_type = wallet_type,
                       user_id = this.login.user_id,
                       user_name = this.login.user_name,
                       coin_id = coin.coin_id,
                       coin_name = coin.coin_name,
                       total = bb == null ? 0 : bb.total,
                       available = bb == null ? 0 : bb.available,
                       freeze = bb == null ? 0 : bb.freeze,
                   };
        res.data = linq.ToList();
        return res;
    }

    /// <summary>
    /// 获取流水(手续费)
    /// </summary>
    /// <param name="user_name">用户名</param>
    /// <param name="relation_id">关联id</param>
    /// <param name="coin_name">币名</param>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="skip">跳过行数</param>
    /// <param name="take">提取行数</param>
    /// <returns></returns>
    [HttpGet]
    public Res<List<BaseRunning>> GetRunningFee(string? user_name, long? relation_id, string? coin_name, DateTimeOffset? start, DateTimeOffset? end, int skip, int take)
    {
        Res<List<BaseRunning>> res = new Res<List<BaseRunning>>();
        res.code = E_Res_Code.ok;
        res.data = db.RunningFee.AsNoTracking().Where(P => P.type == E_RunningType.fee).WhereIf(relation_id != null, P => P.relation_id == relation_id).WhereIf(user_name != null, P => P.user_name_from == user_name || P.user_name_to == user_name).WhereIf(coin_name != null, P => P.coin_name == coin_name).WhereIf(start != null, P => P.time >= start).WhereIf(end != null, P => P.time <= end).Skip(skip).Take(take).ToList().ConvertAll(P => (BaseRunning)P);
        return res;
    }

    /// <summary>
    /// 获取流水(币币交易)
    /// </summary>
    /// <param name="user_name">用户名</param>
    /// <param name="relation_id">关联id</param>
    /// <param name="coin_name">币名</param>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="skip">跳过行数</param>
    /// <param name="take">提取行数</param>
    /// <returns></returns>
    [HttpGet]
    public Res<List<BaseRunning>> GetRunningTrade(string? user_name, long? relation_id, string? coin_name, DateTimeOffset? start, DateTimeOffset? end, int skip, int take)
    {
        Res<List<BaseRunning>> res = new Res<List<BaseRunning>>();
        res.code = E_Res_Code.ok;
        res.data = db.RunningTrade.AsNoTracking().Where(P => P.type == E_RunningType.trade).WhereIf(relation_id != null, P => P.relation_id == relation_id).WhereIf(user_name != null, P => P.user_name_from == user_name || P.user_name_to == user_name).WhereIf(coin_name != null, P => P.coin_name == coin_name).WhereIf(start != null, P => P.time >= start).WhereIf(end != null, P => P.time <= end).Skip(skip).Take(take).ToList().ConvertAll(P => (BaseRunning)P);
        return res;
    }

}