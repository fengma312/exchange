


using Com.Models.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Com.Models.Base;

/// <summary>
/// 请求操作动作
/// </summary>
public class ReqCall<T> : Req<T>
{
    /// <summary>
    /// 操作   
    /// </summary>
    /// <value></value>
    //[JsonConverter(typeof(StringEnumConverter))]
    public E_Op op { get; set; }
    /// <summary>
    /// 交易对
    /// </summary>
    /// <value></value>
    public long market { get; set; }

}
