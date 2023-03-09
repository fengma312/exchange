


using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Com.Models.Base;

/// <summary>
/// 请求操作动作
/// </summary>
public class KeyList<T, K>
{
    /// <summary>
    /// key
    /// </summary>
    /// <value></value>
    public T key { get; set; } = default(T)!;
    /// <summary>
    /// list
    /// </summary>
    /// <value></value>
    public List<K> data { get; set; } = null!;

}
