using Com.Models.Base;
using Grpc.Net.Client;
using Newtonsoft.Json;
using ServiceTradeGrpc;

namespace Com.Bll;

/// <summary>
/// Grpc客户端:交易服务
/// </summary>
public class GrpcClientTrade
{
    /// <summary>
    /// 服务端地址
    /// </summary>
    public readonly string url = "";
    /// <summary>
    /// grpc管道
    /// </summary>
    public readonly GrpcChannel channel;
    /// <summary>
    /// grpc客户端对象
    /// </summary>
    public readonly TradeGrpc.TradeGrpcClient client;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="url"></param>
    public GrpcClientTrade(string url)
    {
        this.url = url;
        this.channel = GrpcChannel.ForAddress(this.url);
        this.client = new TradeGrpc.TradeGrpcClient(channel);
    }

    /// <summary>
    /// 1:一元调用:判断程序/网络是否启动
    /// </summary>
    /// <returns></returns>
    public async Task<bool> TradeActive()
    {
        try
        {
            Google.Protobuf.WellKnownTypes.Empty res = await client.TradeActiveAsync(new Google.Protobuf.WellKnownTypes.Empty());
        }
        catch
        {

            return false;
        }
        return true;
    }

    /// <summary>
    /// 2:加载用户信息
    /// </summary>
    /// <param name="market_id"></param>
    /// <returns></returns>
    public async Task<bool> TradeLoad(List<long> market_id)
    {
        TradeLoadReq req = new TradeLoadReq();
        req.MarketId.AddRange(market_id);
        TradeLoadRes res = await client.TradeLoadAsync(req);
        return res.Success;
    }

    /// <summary>
    /// 3:一元调用:开启交易服务
    /// </summary>
    /// <param name="market_id"></param>
    /// <returns></returns>
    public async Task<bool> TradeStart(List<long> market_id)
    {
        TradeStartReq req = new TradeStartReq();
        req.MarketId.AddRange(market_id);
        TradeStartRes res = await client.TradeStartAsync(req);
        return res.Success;
    }

    /// <summary>
    /// 4:一元调用:停止交易服务
    /// </summary>
    /// <param name="market_id"></param>
    /// <returns></returns>
    public async Task<bool> TradeStop(List<long> market_id)
    {
        TradeStopReq req = new TradeStopReq();
        req.MarketId.AddRange(market_id);
        TradeStopRes res = await client.TradeStopAsync(req);
        return res.Success;
    }

    /// <summary>
    /// 5:一元调用:交易对配置更改
    /// </summary>
    /// <param name="market_id"></param>
    /// <returns></returns>
    public async Task<bool> TradeChange(List<long> market_id)
    {
        TradeChangeReq req = new TradeChangeReq();
        req.MarketId.AddRange(market_id);
        TradeChangeRes res = await client.TradeChangeAsync(req);
        return res.Success;
    }

    /// <summary>
    /// 6:一元调用:挂单
    /// </summary>
    /// <param name="market_id"></param>
    /// <returns></returns>
    public async Task<Res<List<BaseOrdered>>> TradePlaceOrder(string symbol, long uid, string user_name, string ip, List<BaseOrder> orders)
    {
        TradePlaceOrderReq req = new TradePlaceOrderReq()
        {
            Symbol = symbol,
            Uid = uid,
            UserName = user_name,
            Ip = ip,
            Orders = JsonConvert.SerializeObject(orders)
        };
        TradePlaceOrderRes res = await client.TradePlaceOrderAsync(req);
        return JsonConvert.DeserializeObject<Res<List<BaseOrdered>>>(res.Json)!;
    }

    /// <summary>
    /// 关闭
    /// </summary>
    /// <returns></returns>
    public async Task Closeconnect()
    {
        await channel.ShutdownAsync();
    }

}