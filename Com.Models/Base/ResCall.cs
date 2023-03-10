


using Com.Models.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Com.Models.Base;

/// <summary>
/// web调用结果
/// </summary>
public class ResCall<T> : Res<T>
{
    /// <summary>
    /// 操作   
    /// </summary>
    /// <value></value>
    [JsonConverter(typeof(StringEnumConverter))]

    public E_Op op { get; set; }
    /// <summary>
    /// 交易对
    /// </summary>
    /// <value></value>
    public long market { get; set; }
}
