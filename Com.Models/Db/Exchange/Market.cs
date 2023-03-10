using System;
using System.ComponentModel.DataAnnotations.Schema;
using Com.Db;
using Newtonsoft.Json;

namespace Com.Models.Db;

/// <summary>
/// 交易对基础信息
/// </summary>
public class Market : BaseMarket
{
    /// <summary>
    /// 基础币种id
    /// </summary>
    /// <value></value>
    public long coin_id_base { get; set; }
    /// <summary>
    /// 报价币种id
    /// </summary>
    /// <value></value>
    public long coin_id_quote { get; set; }
    /// <summary>
    /// 作市账号
    /// </summary>
    /// <value></value>
    public long market_uid { get; set; }
    /// <summary>
    /// 结算账号
    /// </summary>
    /// <value></value>
    public long settlement_uid { get; set; }
    /// <summary>
    /// 服务地址
    /// </summary>
    /// <value></value>
    public string service_url { get; set; } = null!;
}