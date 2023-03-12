using System;
using System.ComponentModel.DataAnnotations.Schema;
using Com.Models.Base;
using Newtonsoft.Json;

namespace Com.Models.Db;

/// <summary>
/// Api用户
/// </summary>
public class UsersApi : BaseUsersApi
{
    /// <summary>
    /// 用户id
    /// </summary>
    /// <value></value>
    [JsonIgnore]
    public long user_id { get; set; }
    /// <summary>
    /// 账户密钥
    /// </summary>
    /// <value></value>    
    [JsonIgnore]
    public string api_secret { get; set; } = null!;
   
}