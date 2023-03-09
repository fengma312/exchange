
using Com.Models.Enum;
using Grpc.Core;
using ServiceMatchGrpc;

namespace Com.Service.Match;

/// <summary>
/// Grpc服务端
/// </summary>
public class ServiceMatchGrpc : MatchGrpc.MatchGrpcBase
{
    /// <summary>
    /// 1:一元调用:判断程序/网络是否启动
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<ActivityRes> Activity(ActivityReq request, ServerCallContext context)
    {
        ActivityRes res = new ActivityRes();
        res.Ip = FactoryMatch.instance.common.GetLocalIp();
        res.ProcessId = Environment.ProcessId;
        res.Type = ServerType.MakeMatch;
        return await Task.FromResult<ActivityRes>(res);
    }

    /// <summary>
    /// 2:一元调用:查询所分配的交易对
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<SearchSymbolRes> SearchSymbol(SearchSymbolReq request, ServerCallContext context)
    {
        SearchSymbolRes res = new SearchSymbolRes();
        List<SymbolList> symbols = new List<SymbolList>();
        foreach (var item in FactoryMatch.instance.service)
        {
            MarketType market_type = item.Value.info.market_type switch
            {
                E_MarketType.spot => MarketType.Spot,
                _ => MarketType.Spot
            };
            symbols.Add(new SymbolList()
            {
                SymbolId = item.Key,
                Symbol = item.Value.info.symbol,
                MarketType = market_type,
                Run = item.Value.run,
            });
        }
        res.SymbolLists.Add(symbols);
        return await Task.FromResult<SearchSymbolRes>(res);
    }

    /// <summary>
    /// 3:一元调用:分配撮合交易对
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<InitMatchRes> InitMatch(InitMatchReq request, ServerCallContext context)
    {
        InitMatchRes res = new InitMatchRes();
        res.Success = FactoryMatch.instance.InitMatch(request.SymbolId);
        return await Task.FromResult(res);
    }

    /// <summary>
    /// 4:一元调用:管理撮合交易对(开启,停止)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<ManageSymbolRes> ManageSymbol(ManageSymbolReq request, ServerCallContext context)
    {
        ManageSymbolRes res = new ManageSymbolRes();
        if (request.Status)
        {
            res.Success = FactoryMatch.instance.Start(request.SymbolId);
        }
        else
        {
            res.Success = FactoryMatch.instance.Stop(request.SymbolId);
        }
        return await Task.FromResult(res);
    }

    /// <summary>
    /// 5:一元调用:交易对配置更改
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<ConfigSymbolRes> ConfigSymbol(ConfigSymbolReq request, ServerCallContext context)
    {
        ConfigSymbolRes res = new ConfigSymbolRes();
        res.Success = true;
        return await Task.FromResult(res);
    }

    /// <summary>
    /// 6:服务器流:成交记录
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task TransactionRecord(TransactionRecordReq request, IServerStreamWriter<TransactionRecordRes> responseStream, ServerCallContext context)
    {
        FactoryMatch.instance.Channel_TransactionRecord = System.Threading.Channels.Channel.CreateUnbounded<TransactionRecordRes>();
        Task.Run(async () =>
        {
            await foreach (var message in FactoryMatch.instance.Channel_TransactionRecord.Reader.ReadAllAsync())
            {
                await responseStream.WriteAsync(message);
            }
        }).Start();



        long sysmbol_id = request.SymbolId;
        // if (consumerTask.IsCompleted)
        Task.Run(async () =>
        {
            await Task.Delay(3000);
            for (int i = 0; i < 10000; i++)
            {
                TransactionRecordRes res = new TransactionRecordRes();
                res.Record = sysmbol_id + 1;
                await FactoryMatch.instance.Channel_TransactionRecord.Writer.WriteAsync(res);
                await Task.Delay(100);
            }

        }).Start();

        await Task.Delay(100);
        // channel.Writer.Complete();
    }



}