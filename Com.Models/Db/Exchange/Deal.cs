using System;
using System.ComponentModel.DataAnnotations.Schema;
using Com.Models.Base;
using Com.Models.Enum;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Com.Models.Db;

/// <summary>
/// 成交单
/// 注:此表数据量超大,请使用数据库表分区功能
/// </summary>
public class Deal : BaseDeal
{
    /// <summary>
    /// 成交id
    /// </summary>
    /// <value></value>
    public long trade_id { get; set; }
    /// <summary>
    /// 交易对
    /// </summary>
    /// <value></value>
    public long market { get; set; }
    /// <summary>
    /// 交易模式
    /// </summary>
    /// <value></value>
    //[JsonConverter(typeof(StringEnumConverter))]

    public E_TradeModel trade_model { get; set; }
    /// <summary>
    /// 成交总额
    /// </summary>
    /// <value></value>

    public decimal total { get; set; }
    /// <summary>
    /// 买单id
    /// </summary>
    /// <value></value>
    public long bid_id { get; set; }
    /// <summary>
    /// 卖单id
    /// </summary>
    /// <value></value>
    public long ask_id { get; set; }
    /// <summary>
    /// 买单用户id
    /// </summary>
    /// <value></value>
    public long bid_uid { get; set; }
    /// <summary>
    /// 卖单用户id
    /// </summary>
    /// <value></value>
    public long ask_uid { get; set; }
    /// <summary>
    /// 买单用户名
    /// </summary>
    /// <value></value>
    public string bid_name { get; set; } = null!;
    /// <summary>
    /// 卖单用户名
    /// </summary>
    /// <value></value>
    public string ask_name { get; set; } = null!;
    /// <summary>
    /// 买单未成交额
    /// </summary>
    /// <value></value>

    public decimal bid_total_unsold { get; set; }
    /// <summary>
    /// 卖单未成交量
    /// </summary>
    /// <value></value>

    public decimal ask_amount_unsold { get; set; }
    /// <summary>
    /// 买单已成交额
    /// </summary>
    /// <value></value>

    public decimal bid_total_done { get; set; }
    /// <summary>
    /// 卖单已成交量
    /// </summary>
    /// <value></value>

    public decimal ask_amount_done { get; set; }
    /// <summary>
    /// 买单挂单手续费
    /// </summary>
    /// <value></value>

    public decimal fee_bid_maker { get; set; }
    /// <summary>
    /// 买单吃单手续费
    /// </summary>
    /// <value></value>

    public decimal fee_bid_taker { get; set; }
    /// <summary>
    /// 卖单挂单手续费
    /// </summary>
    /// <value></value>

    public decimal fee_ask_maker { get; set; }
    /// <summary>
    /// 卖单吃单手续费
    /// </summary>
    /// <value></value>

    public decimal fee_ask_taker { get; set; }
}