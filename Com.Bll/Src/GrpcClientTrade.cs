using Grpc.Net.Client;
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
    public async Task<bool> Activity()
    {
        try
        {
            Google.Protobuf.WellKnownTypes.Empty res = await client.ActivityAsync(new Google.Protobuf.WellKnownTypes.Empty());
        }
        catch
        {

            return false;
        }
        return true;
    }

    // /// <summary>
    // /// 2:加载用户信息
    // /// </summary>
    // /// <param name="users_id"></param>
    // /// <returns></returns>
    // public async Task LoadUser(List<long> users_id)
    // {
    //     LoadUserReq req = new LoadUserReq();
    //     req.UserId.AddRange(users_id);
    //     await client.LoadUserAsync(req);
    // }

    // /// <summary>
    // /// 2:加载用户信息
    // /// </summary>
    // /// <param name="users_id"></param>
    // /// <returns></returns>
    // public async Task<GetUserRes> GetUser(List<long> users_id)
    // {
    //     GetUserReq req = new GetUserReq();
    //     req.UserId.AddRange(users_id);
    //     return await client.GetUserAsync(req);
    // }

    /// <summary>
    /// 关闭
    /// </summary>
    /// <returns></returns>
    public async Task Closeconnect()
    {
        await channel.ShutdownAsync();
    }

}