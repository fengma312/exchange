using Grpc.Net.Client;
using Grpc.Core;
using ServiceMatchGrpc;

namespace Com.Bll;

/// <summary>
/// Grpc客户端:撮合服务
/// </summary>
public class GrpcClientMatch
{
    /// <summary>
    /// 
    /// </summary>
    public string url = "";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    public GrpcClientMatch(string url)
    {
        this.url = url;
    }

    /// <summary>
    /// 1:一元调用:判断程序/网络是否启动
    /// </summary>
    /// <returns></returns>
    public async Task<ActivityRes> Activity()
    {
        GrpcChannel channel = GrpcChannel.ForAddress(this.url);
        var client = new MatchGrpc.MatchGrpcClient(channel);
        ActivityReq req = new ActivityReq();
        ActivityRes res = await client.ActivityAsync(req);
        await channel.ShutdownAsync();
        return res;
    }

    /// <summary>
    /// 2:一元调用:查询所分配的交易对
    /// </summary>
    /// <returns></returns>
    public async Task<SearchSymbolRes> SearchSymbol()
    {
        GrpcChannel channel = GrpcChannel.ForAddress(this.url);
        var client = new MatchGrpc.MatchGrpcClient(channel);
        SearchSymbolReq req = new SearchSymbolReq();
        SearchSymbolRes res = await client.SearchSymbolAsync(req);
        foreach (var item in res.SymbolLists)
        {

        }
        await channel.ShutdownAsync();
        return res;
    }

    /// <summary>
    /// 3:一元调用:分配撮合交易对
    /// </summary>
    /// <returns></returns>
    public async Task<InitMatchRes> InitMatch()
    {
        GrpcChannel channel = GrpcChannel.ForAddress(this.url);
        var client = new MatchGrpc.MatchGrpcClient(channel);
        InitMatchReq req = new InitMatchReq();
        InitMatchRes res = await client.InitMatchAsync(req);

        await channel.ShutdownAsync();
        return res;
    }

    /// <summary>
    /// 4:一元调用:管理撮合交易对(开启,停止)
    /// </summary>
    /// <returns></returns>
    public async Task<ManageSymbolRes> ManageSymbol()
    {
        GrpcChannel channel = GrpcChannel.ForAddress(this.url);
        var client = new MatchGrpc.MatchGrpcClient(channel);
        ManageSymbolReq req = new ManageSymbolReq();
        ManageSymbolRes res = await client.ManageSymbolAsync(req);

        await channel.ShutdownAsync();
        return res;
    }

    /// <summary>
    /// 5:一元调用:交易对配置更改
    /// </summary>
    /// <returns></returns>
    public async Task<ConfigSymbolRes> ConfigSymbol()
    {
        GrpcChannel channel = GrpcChannel.ForAddress(this.url);
        var client = new MatchGrpc.MatchGrpcClient(channel);
        ConfigSymbolReq req = new ConfigSymbolReq();
        ConfigSymbolRes res = await client.ConfigSymbolAsync(req);

        await channel.ShutdownAsync();
        return res;
    }

    /// <summary>
    /// 6:服务器流:成交记录
    /// </summary>
    /// <returns></returns>
    public async Task TransactionRecord()
    {
        GrpcChannel channel = GrpcChannel.ForAddress(this.url);
        var client = new MatchGrpc.MatchGrpcClient(channel);
        TransactionRecordReq req = new TransactionRecordReq();
        req.SymbolId = 333;
        var res = client.TransactionRecord(req);
        await foreach (var response in res.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine("Greeting: " + response.Record);
        }
        // await channel.ShutdownAsync();
    }



}
