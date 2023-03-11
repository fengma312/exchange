
using Com.Models.Base;
using Com.Models.Db;
using Com.Models.Enum;
using Com.Service.Trade.Models;
using Grpc.Core;
using Newtonsoft.Json;
using ServiceTradeGrpc;

namespace Com.Service.Trade;

/// <summary>
/// Grpc服务端
/// </summary>
public class GrpcServiceTrade : TradeGrpc.TradeGrpcBase
{
    /// <summary>
    /// 1:一元调用:判断程序/网络是否启动
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<Google.Protobuf.WellKnownTypes.Empty> TradeActive(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context)
    {
        return await Task.FromResult(request);
    }

    /// <summary>
    /// 2:一元调用:分配撮合交易对
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<TradeLoadRes> TradeLoad(TradeLoadReq request, ServerCallContext context)
    {
        TradeLoadRes res = new TradeLoadRes()
        {
            Success = true,
        };
        List<Market> markets = FactoryTrade.instance.service_list.service_market.GetMarketById(request.MarketId.ToList());
        foreach (var item in markets)
        {
            TradeModel model = new TradeModel(item)
            {

            };
            FactoryTrade.instance.service.AddOrUpdate(item.market_id, model, (k, v) => { return model; });
        }
        return await Task.FromResult(res);
    }

    /// <summary>
    /// 3:一元调用:开启交易服务
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<TradeStartRes> TradeStart(TradeStartReq request, ServerCallContext context)
    {
        TradeStartRes res = new TradeStartRes()
        {
            Success = true,
        };
        foreach (var item in request.MarketId)
        {
            if (FactoryTrade.instance.service.TryGetValue(item, out TradeModel? model))
            {
                model.StartTrade();
            }
        }
        return await Task.FromResult(res);
    }

    /// <summary>
    /// 3:一元调用:开启交易服务
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<TradeStopRes> TradeStop(TradeStopReq request, ServerCallContext context)
    {
        TradeStopRes res = new TradeStopRes()
        {
            Success = true,
        };
        foreach (var item in request.MarketId)
        {
            if (FactoryTrade.instance.service.TryGetValue(item, out TradeModel? model))
            {
                model.StopTrade();
            }
        }
        return await Task.FromResult(res);
    }

    /// <summary>
    /// 5:一元调用:交易对配置更改
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<TradeChangeRes> TradeChange(TradeChangeReq request, ServerCallContext context)
    {
        TradeChangeRes res = new TradeChangeRes()
        {
            Success = true,
        };
        foreach (var item in request.MarketId)
        {
            if (FactoryTrade.instance.service.TryGetValue(item, out TradeModel? model))
            {
                model.ReloadConfig();
            }
        }
        return await Task.FromResult(res);
    }

    /// <summary>
    /// 6:一元调用:挂单
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<TradePlaceOrderRes> TradePlaceOrder(TradePlaceOrderReq request, ServerCallContext context)
    {
        Res<List<BaseOrdered>> response = FactoryTrade.instance.service_list.service_order.PlaceOrder(request.Symbol, request.Uid, request.UserName, request.Ip, JsonConvert.DeserializeObject<List<BaseOrder>>(request.Orders)!);
        TradePlaceOrderRes res = new TradePlaceOrderRes()
        {
            Json = JsonConvert.SerializeObject(response)
        };
        return await Task.FromResult(res);
    }

    /// <summary>
    /// 7:一元调用:取消挂单
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<TradeCancelOrderRes> TradeCancelOrder(TradeCancelOrderReq request, ServerCallContext context)
    {
        Res<bool> response = FactoryTrade.instance.service_list.service_order.CancelOrder(request.Symbol, request.Uid, request.Type, JsonConvert.DeserializeObject<List<long>>(request.Orders)!);
        TradeCancelOrderRes res = new TradeCancelOrderRes()
        {
            Json = JsonConvert.SerializeObject(response)
        };
        return await Task.FromResult(res);
    }

}