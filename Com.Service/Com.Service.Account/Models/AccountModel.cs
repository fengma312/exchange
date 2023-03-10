using System;
using System.Collections.Concurrent;
using Com.Db;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using RabbitMQ.Client;
using Com.Bll;
using Com.Bll.Util;
using Com.Models.Db;

namespace Com.Service.Account.Models;

/// <summary>
/// 交易对象
/// </summary>
public class AccountModel
{    
    /// <summary>
    /// 日志事件ID
    /// </summary>
    public EventId eventId;
    /// <summary>
    /// 交易对基本信息
    /// </summary>
    /// <value></value>
    public Users users { get; set; } = null!;
       /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="users"></param>
    public AccountModel(Users users)
    {
        this.users = users;
        this.eventId = new EventId(1, users.user_name);
    
    }

}