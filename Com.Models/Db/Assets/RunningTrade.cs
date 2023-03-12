using System;
using System.ComponentModel.DataAnnotations.Schema;

using Com.Models.Enum;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Com.Models.Db;

/// <summary>
/// 计账钱包流水(交易流水)
/// 注:此表数据量超大,请使用数据库表分区功能
/// </summary>
public class RunningTrade : Running
{

}