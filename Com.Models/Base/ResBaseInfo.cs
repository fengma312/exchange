
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Com.Models.Base;

/// <summary>
/// 基本信息
/// </summary>
public class ResBaseInfo
{
    /// <summary>
    /// 网站:名称
    /// </summary>
    /// <value></value>
    public string website_name { get; set; } = null!;
    /// <summary>
    /// 网站:icon
    /// </summary>
    /// <value></value>
    public string website_icon { get; set; } = null!;
    /// <summary>
    ///  网站:系统时间
    /// </summary>
    /// <value></value>
    // [JsonConverter(typeof(JsonConverterDateTimeOffset))]
    public DateTimeOffset website_time { get; set; }
    /// <summary>
    /// 时区
    /// </summary>
    /// <value></value>
    public int time_zone { get; set; }
    /// <summary>
    ///  网站:文件服务器地址
    /// </summary>
    /// <value></value>
    public string website_serivcefile { get; set; } = null!;

}