using System;
using Com.Models.Enum;

namespace Com.Models.Db;

/// <summary>
/// 服务列表
/// </summary>
public class Cluster
{
    /// <summary>
    /// id
    /// </summary>
    /// <value></value>
    public long cluster_id { get; set; }
    /// <summary>
    /// 服务类型
    /// </summary>
    /// <value></value>
    public E_ServiceType type { get; set; }
    /// <summary>
    /// 服务地址
    /// </summary>
    /// <value></value>
    public string ip { get; set; } = null!;
    /// <summary>
    /// 端口
    /// </summary>
    /// <value></value>
    public int port { get; set; }
    /// <summary>
    /// 标记0~9
    /// </summary>
    /// <value></value>
    public string mark { get; set; } = null!;
    /// <summary>
    /// 备注
    /// </summary>
    /// <value></value>
    public string? remark { get; set; } = null!;
}