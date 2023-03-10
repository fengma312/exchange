using System.Collections.Concurrent;
using Grpc.Net.Client;
using ServiceAccountGrpc;

namespace Com.Bll;

/// <summary>
/// Grpc客户端:账户服务
/// </summary>
public class ServiceGrpcClient
{
    ConcurrentDictionary<string, object> grcp_client_account = new ConcurrentDictionary<string, object>();
}