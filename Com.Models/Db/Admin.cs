using System;

namespace Com.Models.Db;

/// <summary>
/// 管理员表
/// </summary>
public class Admin
{
    /// <summary>
    /// 管理员id
    /// </summary>
    /// <value></value>
    public long admin_id { get; set; }
    /// <summary>
    /// 负责人姓名
    /// </summary>
    /// <value></value>
    public string name { get; set; } = null!;
    /// <summary>
    /// 邮箱地址
    /// </summary>
    /// <value></value>
    public string email { get; set; } = null!;
    /// <summary>
    /// 密码
    /// </summary>
    /// <value></value>
    public string password { get; set; } = null!;
    /// <summary>
    /// token
    /// </summary>
    /// <value></value>
    public string token_key { get; set; } = null!;
}