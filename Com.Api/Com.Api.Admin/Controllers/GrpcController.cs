using Com.Bll.Models;
using Com.Models.Db;
using Com.Models.Enum;
using ServiceMatchGrpc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Com.Api.Admin.Controllers;

/// <summary>
/// 
/// </summary>
[ApiController]
[Route("[controller]/[action]")]
public class GrpcController : ControllerBase
{
    /// <summary>
    /// 
    /// </summary>
    public readonly ServiceBase service_base;
    /// <summary>
    /// 
    /// </summary>
    public readonly ServiceList service_list;
    public string url = "http://localhost:8081";


    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="db_factory"></param>
    /// <param name="configuration"></param>
    /// <param name="environment"></param>
    /// <param name="logger"></param>
    public GrpcController(IServiceProvider provider, IDbContextFactory<DbContextEF> db_factory, IConfiguration configuration, IHostEnvironment environment, ILogger<MainService> logger)
    {
        this.service_base = new ServiceBase(provider, db_factory, configuration, environment, logger);
        this.service_list = new ServiceList(this.service_base);
    }

    /// <summary>
    /// 1:一元调用:判断程序/网络是否启动
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActivityRes> Activity(string url)
    {
        GrpcMatchingImpl impl = new GrpcMatchingImpl(url);
        return await impl.Activity();
    }

    /// <summary>
    /// 2:一元调用:查询服务所分配的交易对
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<SearchSymbolRes> SearchSymbol(string url)
    {
        GrpcMatchingImpl impl = new GrpcMatchingImpl(url);
        return await impl.SearchSymbol();
    }

    /// <summary>
    /// 3:一元调用:自动分配撮合交易对
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<List<InitMatchRes>> InitMatch()
    {
        List<InitMatchRes> response = new List<InitMatchRes>();
        using (DbContextEF db = this.service_base.db_factory.CreateDbContext())
        {
            foreach (var item in await db.Market.ToListAsync())
            {
                E_ServiceType type = item.market_type switch
                {
                    E_MarketType.spot => E_ServiceType.match,
                    _ => E_ServiceType.match
                };
                string? url = this.service_list.service_cluster.GetClusterUrl(type, item.market_id);
                if (url != null)
                {
                    GrpcMatchingImpl impl = new GrpcMatchingImpl(url);
                    response.Add(await impl.InitMatch());
                }
            }
        }
        return response;
    }

    /// <summary>
    /// 4:一元调用:管理撮合交易对(开启,停止)
    /// </summary>
    /// <param name="market_id">市场名id</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ManageSymbolRes> ManageSymbol(long market_id)
    {
        string? url = this.service_list.service_cluster.GetClusterUrl(E_ServiceType.match, market_id);
        if (url == null)
        {
            return new ManageSymbolRes()
            {
                Success = false,
            };
        }
        GrpcMatchingImpl impl = new GrpcMatchingImpl(url);
        return await impl.ManageSymbol();
    }

    /// <summary>
    /// 5:一元调用:交易对配置更改
    /// </summary>
    /// <param name="market_id">市场名id</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ConfigSymbolRes> ConfigSymbol(long market_id)
    {
        string? url = this.service_list.service_cluster.GetClusterUrl(E_ServiceType.match, market_id);
        if (url == null)
        {
            return new ConfigSymbolRes()
            {
                Success = false,
            };
        }
        GrpcMatchingImpl impl = new GrpcMatchingImpl(url);
        return await impl.ConfigSymbol();
    }

    /// <summary>
    /// 6:服务器流:成交记录
    /// </summary>
    /// <param name="url"></param>
    /// <param name="setting_id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task TransactionRecord()
    {
        GrpcMatchingImpl impl = new GrpcMatchingImpl(url);
        await impl.TransactionRecord();
    }


}
