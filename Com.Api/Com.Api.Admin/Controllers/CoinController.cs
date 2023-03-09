using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using Com.Bll;
using Com.Db;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Com.Models.Db;
using Com.Models.Base;
using Com.Models.Enum;
using Com.Bll.Models;

namespace Com.Api.Admin.Controllers;

/// <summary>
/// 币种
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("[controller]")]
public class CoinController : ControllerBase
{
    /// <summary>
    /// Service:基础服务
    /// </summary>
    public readonly ServiceBase service_base;
    /// <summary>
    /// 数据库
    /// </summary>
    public DbContextEF db = null!;


    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="provider">服务驱动</param>
    /// <param name="db_factory">db上下文工厂</param>
    /// <param name="configuration">配置接口</param>
    /// <param name="environment">环境接口</param>
    /// <param name="logger">日志接口</param>
    public CoinController(IServiceProvider provider, IDbContextFactory<DbContextEF> db_factory, DbContextEF db, IConfiguration configuration, IHostEnvironment environment, ILogger<MainService> logger)
    {
        this.service_base = new ServiceBase(provider, db_factory, configuration, environment, logger);
        this.db = db;
    }

    /// <summary>
    /// 添加币种
    /// </summary>
    /// <param name="coin_name">币名称(大写)</param>
    /// <param name="full_name">币全称</param>
    /// <param name="icon">币图标</param>
    /// <returns></returns>
    [HttpPost]
    [Route("AddCoin")]
    public async Task<Res<bool>> AddCoin(string coin_name, string full_name, IFormFile icon)
    {
        Res<bool> res = new Res<bool>();
        res.code = E_Res_Code.fail;
        res.data = false;
        if (icon == null || icon.Length <= 0)
        {
            res.code = E_Res_Code.file_not_found;
            res.data = false;
            res.msg = "未找到文件";
            return res;
        }
        if (this.db.Coin.Any(P => P.coin_name == coin_name.ToUpper()))
        {
            res.code = E_Res_Code.name_repeat;
            res.data = false;
            res.msg = "币名已重复";
            return res;
        }
        if (this.db.Coin.Any(P => P.full_name == full_name))
        {
            res.code = E_Res_Code.name_repeat;
            res.data = false;
            res.msg = "全名已重复";
            return res;
        }
        Coin coin = new Coin();
        coin.coin_id = ServiceFactory.instance.worker.NextId();
        coin.coin_name = coin_name.ToUpper();
        coin.full_name = full_name;
        coin.contract = null;

        this.db.Coin.Add(coin);
        if (this.db.SaveChanges() > 0)
        {
            res.code = E_Res_Code.ok;
            res.data = true;
            res.msg = "";
            return res;
        }
        return res;
    }

    /// <summary>
    /// 获取币种列表
    /// </summary>
    /// <param name="coin_name">币名称</param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetCoin")]
    public Res<List<Coin>> GetCoin(string? coin_name)
    {
        Res<List<Coin>> res = new Res<List<Coin>>();
        res.code = E_Res_Code.ok;
        res.data = db.Coin.WhereIf(coin_name != null, P => P.coin_name == coin_name!.ToUpper()).AsNoTracking().ToList();
        return res;
    }


}
