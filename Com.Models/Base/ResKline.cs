using System;
using Com.Models.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Com.Models.Base;

/// <summary>
/// K线
/// 注:此表数据量超大,请使用数据库表分区功能
/// </summary>
public class ResKline
{
    /// <summary>
    /// 交易对名称
    /// </summary>
    /// <value></value>
    public string symbol { get; set; } = null!;
    /// <summary>
    /// K线类型
    /// </summary>
    /// <value></value>
    [JsonConverter(typeof(StringEnumConverter))]
    public E_KlineType type { get; set; }
    /// <summary>
    /// 成交量
    /// </summary>
    /// <value></value>

    public decimal amount { get; set; }
    /// <summary>
    /// 成交笔数
    /// </summary>
    /// <value></value>
    public long count { get; set; }
    /// <summary>
    /// 成交总金额
    /// </summary>
    /// <value></value>

    public decimal total { get; set; }
    /// <summary>
    /// 开盘价
    /// </summary>
    /// <value></value>

    public decimal open { get; set; }
    /// <summary>
    /// 收盘价（当K线为最晚的一根时，是最新成交价）
    /// </summary>
    /// <value></value>

    public decimal close { get; set; }
    /// <summary>
    /// 最低价
    /// </summary>
    /// <value></value>

    public decimal low { get; set; }
    /// <summary>
    /// 最高价
    /// </summary>
    /// <value></value>

    public decimal high { get; set; }
    /// <summary>
    /// K线开始时间
    /// </summary>
    /// <value></value>
    public DateTimeOffset time_start { get; set; }
    /// <summary>
    /// K线结束时间
    /// </summary>
    /// <value></value>
    public DateTimeOffset time_end { get; set; }
    /// <summary>
    /// 更新时间
    /// </summary>
    /// <value></value>
    public DateTimeOffset time { get; set; }

}