
using Com.Models.Enum;
using Com.Service.Account.Models;
using Grpc.Core;
using ServiceAccountGrpc;

namespace Com.Service.Account;

/// <summary>
/// Grpc服务端
/// </summary>
public class ServiceAccountGrpc : AccountGrpc.AccountGrpcBase
{
    /// <summary>
    /// 1:一元调用:判断程序/网络是否启动
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<Google.Protobuf.WellKnownTypes.Empty> Activity(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context)
    {
        return await Task.FromResult(request);
    }

    /// <summary>
    /// 2:加载用户信息
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<Google.Protobuf.WellKnownTypes.Empty> LoadUser(LoadUserReq request, ServerCallContext context)
    {
        FactoryAccount.instance.LoadConfig(request.UserId.ToList());
        return await Task.FromResult(new Google.Protobuf.WellKnownTypes.Empty());
    }

    /// <summary>
    /// 3:获取用户信息
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<GetUserRes> GetUser(GetUserReq request, ServerCallContext context)
    {       
        GetUserRes res = new GetUserRes();
        foreach (var item in FactoryAccount.instance.GetUser(request.UserId.ToList()))
        {
            res.Users.Add(new UserInfo()
            {
                UserId = item.user_id,
                UserName = item.user_name,
            });
        }
        return await Task.FromResult(res);
    }

}