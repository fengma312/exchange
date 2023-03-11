using Com.Models.Db;
using Grpc.Net.Client;
using Newtonsoft.Json;
using ServiceAccountGrpc;

namespace Com.Bll;

/// <summary>
/// Grpc客户端:账户服务
/// </summary>
public class GrpcClientAccount
{
    /// <summary>
    /// 服务端地址
    /// </summary>
    public readonly string url;
    /// <summary>
    /// grpc管道
    /// </summary>
    public readonly GrpcChannel channel;
    /// <summary>
    /// grpc客户端对象
    /// </summary>
    public readonly AccountGrpc.AccountGrpcClient client;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="url"></param>
    public GrpcClientAccount(string url)
    {
        this.url = url;
        this.channel = GrpcChannel.ForAddress(this.url);
        this.client = new AccountGrpc.AccountGrpcClient(channel);
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

    /// <summary>
    /// 2:加载用户信息
    /// </summary>
    /// <param name="users_id"></param>
    /// <returns></returns>
    public async Task LoadUser(List<long> users_id)
    {
        LoadUserReq req = new LoadUserReq();
        req.UserId.AddRange(users_id);
        await client.LoadUserAsync(req);
    }

    /// <summary>
    /// 2:加载用户信息
    /// </summary>
    /// <param name="users_id"></param>
    /// <returns></returns>
    public async Task<List<Users>?> GetUser(List<long> users_id)
    {
        GetUserReq req = new GetUserReq();
        req.UserId.AddRange(users_id);
        GetUserRes res = await client.GetUserAsync(req);
        return JsonConvert.DeserializeObject<List<Users>>(res.Users);
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