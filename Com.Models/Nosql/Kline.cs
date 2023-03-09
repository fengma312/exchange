using System;
using Com.Models.Base;
using Com.Models.Enum;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Com.Models.Db;

/// <summary>
/// K线
/// 注:此表数据量超大,请使用数据库表分区功能
/// </summary>
public class Kline : ResKline
{
    // /// <summary>
    // /// 
    // /// </summary>
    // /// <value></value>
    // [JsonIgnore]
    // [BsonElement("_id")]
    // public ObjectId _id { get; set; }
    /// <summary>
    /// 主键
    /// </summary>
    /// <value></value>
    [JsonIgnore]
    public long id { get; set; }
    /// <summary>
    /// 交易对
    /// </summary>
    /// <value></value>
    [JsonIgnore]
    public long market { get; set; }


}