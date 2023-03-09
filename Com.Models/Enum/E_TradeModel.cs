using System;

namespace Com.Models.Enum;

/// <summary>
/// 交易模式
/// </summary>
public enum E_TradeModel
{
    /// <summary>
    /// 现货
    /// </summary>
    cash = 1,
    /// <summary>
    /// 保证金
    /// </summary>
    deposit = 2,
}