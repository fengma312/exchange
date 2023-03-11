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
/// 订单接口
/// </summary>
[Route("[controller]/[action]")]
[Authorize]
[ApiController]
public class OrderController : ControllerBase
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
    public OrderController(IServiceProvider provider, IDbContextFactory<DbContextEF> db_factory, IConfiguration configuration, IHostEnvironment environment, ILogger<MainService> logger)
    {
        this.service_base = new ServiceBase(provider, db_factory, configuration, environment, logger);
        this.service_list = new ServiceList(service_base);
    }

    /// <summary>
    /// 挂单
    /// </summary>
    /// <param name="symbol">交易对名称</param>
    /// <param name="trade_model">交易模式,现货(cash)</param>
    /// <param name="type">订单类型</param>
    /// <param name="side">交易方向</param>
    /// <param name="price">挂单价:限价单(有效),其它无效</param>
    /// <param name="amount">挂单量:限价单/市场卖价(有效),其它无效</param>
    /// <param name="total">挂单额:市价买单(有效),其它无效</param>
    /// <param name="trigger_hanging_price">触发挂单价格</param>
    /// <param name="trigger_cancel_price">触发撤单价格</param>
    /// <param name="client_id">客户自定义订单id</param>
    /// <returns></returns>
    [HttpPost]
    [Route("OrderPlace")]
    public Res<List<BaseOrdered>> OrderPlace(string symbol, E_TradeModel trade_model, E_OrderType type, E_OrderSide side, decimal? price, decimal? amount, decimal? total, decimal? trigger_hanging_price, decimal? trigger_cancel_price, string? client_id)
    {
        Res<List<BaseOrdered>> result = new Res<List<BaseOrdered>>();
        Users? users = this.service_list.service_user.GetUser(login.user_id);
        if (users == null)
        {
            result.code = E_Res_Code.user_not_found;
            result.msg = "未找到该用户";
            return result;
        }
        if (users.disabled || !users.transaction)
        {
            result.code = E_Res_Code.permission_no;
            result.msg = "用户禁止交易";
            return result;
        }
        List<BaseOrder> orders = new List<BaseOrder>()
        {
            new BaseOrder()
            {
                symbol = symbol,
                trade_model = trade_model,
                type = type,
                side = side,
                price = price,
                amount = amount,
                total = total,
                trigger_hanging_price = trigger_hanging_price??0,
                trigger_cancel_price = trigger_cancel_price??0,
                client_id = client_id
            }
        };
        return this.service_list.service_order.PlaceOrder(symbol, users.user_id, users.user_name, Request.GetIp(), orders);
    }

    /// <summary>
    /// 批量挂单
    /// </summary>
    /// <param name="data">挂单数据</param>
    /// <returns></returns>
    [HttpPost]
    [Route("OrderPlaces")]
    public Res<List<BaseOrdered>> OrderPlaces(CallOrder data)
    {
        Res<List<BaseOrdered>> result = new Res<List<BaseOrdered>>();
        Users? users = this.service_list.service_user.GetUser(login.user_id);
        if (users == null)
        {
            result.code = E_Res_Code.user_not_found;
            result.msg = "未找到该用户";
            return result;
        }
        if (users.disabled || !users.transaction)
        {
            result.code = E_Res_Code.permission_no;
            result.msg = "用户禁止交易";
            return result;
        }
        return this.service_list.service_order.PlaceOrder(data.symbol, login.user_id, login.user_name, Request.GetIp(), data.orders);
    }

    /// <summary>
    /// 按交易对,用户撤单
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <returns></returns>
    [HttpPost]
    [Route("OrderCancelByUserId")]
    public Res<bool> OrderCancelByUserId(string symbol)
    {
        Res<bool> result = new Res<bool>();
        Users? users = this.service_list.service_user.GetUser(login.user_id);
        if (users == null)
        {
            result.code = E_Res_Code.user_not_found;
            result.msg = "未找到该用户";
            return result;
        }
        if (users.disabled || !users.transaction)
        {
            result.code = E_Res_Code.permission_no;
            result.msg = "用户禁止交易";
            return result;
        }
        return this.service_list.service_order.CancelOrder(symbol, users.user_id, 2, new List<long>());
    }

    /// <summary>
    ///  按交易对,订单id撤单
    /// </summary>
    /// <param name="model">订单id key:symbol,data:订单id数组</param>
    /// <returns></returns>
    [HttpPost]
    [Route("OrderCancelByOrderid")]
    public Res<bool> OrderCancelByOrderid(KeyList<string, long> model)
    {
        Res<bool> result = new Res<bool>();
        Users? users = this.service_list.service_user.GetUser(login.user_id);
        if (users == null)
        {
            result.code = E_Res_Code.user_not_found;
            result.msg = "未找到该用户";
            return result;
        }
        if (users.disabled || !users.transaction)
        {
            result.code = E_Res_Code.permission_no;
            result.msg = "用户禁止交易";
            return result;
        }
        return this.service_list.service_order.CancelOrder(model.key, users.user_id, 3, model.data);

    }

    // /// <summary>
    // ///  按用户,交易对,用户自定义id撤单
    // /// </summary>
    // /// <param name="model">订单id</param>
    // /// <returns></returns>
    // [HttpPost]
    // [Route("OrderCancelByClientId")]
    // public Res<bool> OrderCancelByClientId(CallOrderCancel model)
    // {
    //     Res<bool> result = new Res<bool>();
    //     Users? users =this.service_list.service_user.GetUser(login.user_id);
    //     if (users == null)
    //     {
    //         result.code = E_Res_Code.user_not_found;
    //         result.message = "未找到该用户";
    //         return result;
    //     }
    //     if (users.disabled || !users.transaction)
    //     {
    //         result.code = E_Res_Code.no_permission;
    //         result.message = "用户禁止交易";
    //         return result;
    //     }
    //     return this.service_order.CancelOrder(model.symbol, users.user_id, 4, model.data);
    // }

    /// <summary>
    /// 当前用户正在委托挂单
    /// </summary>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="skip">跳过多少行</param>
    /// <param name="take">获取多少行</param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetOrderHanging")]
    [ResponseCache(CacheProfileName = "cache_0")]
    public Res<List<BaseOrdered>> GetOrderHanging(DateTimeOffset? start, DateTimeOffset? end, int skip = 0, int take = 50)
    {
        return this.service_list.service_order.GetOrder(uid: login.user_id, state: new List<E_OrderState>() { E_OrderState.unsold, E_OrderState.partial }, start: start, end: end, skip: skip, take: take);
    }

    /// <summary>
    /// 当前用户历史委托挂单
    /// </summary>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="skip">跳过多少行</param>
    /// <param name="take">获取多少行</param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetOrderHistory")]
    [ResponseCache(CacheProfileName = "cache_1")]
    public Res<List<BaseOrdered>> GetOrderHistory(DateTimeOffset? start, DateTimeOffset? end, int skip = 0, int take = 50)
    {
        return this.service_list.service_order.GetOrder(uid: login.user_id, state: new List<E_OrderState>() { E_OrderState.completed, E_OrderState.cancel }, start: start, end: end, skip: skip, take: take);
    }

    // /// <summary>
    // /// 按订单id查询
    // /// </summary>
    // /// <param name="model">key:symbol,data:订单id数组</param>
    // /// <returns></returns>
    // [HttpPost]
    // [Route("GetOrderById")]
    // [ResponseCache(CacheProfileName = "cache_0")]
    // public Res<List<BaseOrdered>> GetOrderById(KeyList<string, long> model)
    // {
    //     return this.service_order.GetOrder(symbol: model.key, uid: login.user_id, ids: model.data);
    // }

    /// <summary>
    /// 按订单状态查询
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <param name="state">订单状态</param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="skip">跳过多少行</param>
    /// <param name="take">获取多少行</param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetOrderByState")]
    [ResponseCache(CacheProfileName = "cache_0")]
    public Res<List<BaseOrdered>> GetOrderByState(string symbol, E_OrderState state, DateTimeOffset start, DateTimeOffset end, int skip = 0, int take = 50)
    {
        return this.service_list.service_order.GetOrder(symbol: symbol, uid: login.user_id, state: new List<E_OrderState>() { state }, start: start, end: end, skip: skip, take: take);
    }

    /// <summary>
    /// 订单时间查询
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="skip">跳过多少行</param>
    /// <param name="take">获取多少行</param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetOrderByDate")]
    [ResponseCache(CacheProfileName = "cache_2")]
    public Res<List<BaseOrdered>> GetOrderByDate(string symbol, DateTimeOffset start, DateTimeOffset end, int skip = 0, int take = 50)
    {
        return this.service_list.service_order.GetOrder(symbol: symbol, uid: login.user_id, start: start, end: end, skip: skip, take: take);
    }

}