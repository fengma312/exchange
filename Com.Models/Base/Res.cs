using System;
using Com.Models.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Com.Models.Base;

/// <summary>
/// 响应操作动作
/// </summary>
public class Res<T>
{
    /// <summary>
    /// 返回编号
    /// </summary>
    /// <value></value>
    [JsonConverter(typeof(StringEnumConverter))]
    public E_Res_Code code { get; set; } = E_Res_Code.ok;
    /// <summary>
    /// 响应消息
    /// </summary>
    /// <value></value>
    public string? msg { get; set; } = null;
    /// <summary>
    /// 数据
    /// </summary>
    /// <value></value>
    public T data { get; set; } = default!;
}