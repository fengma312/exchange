namespace Com.Models.Enum;

/// <summary>
/// 服务类型 1=资产服务 2=公共市场服务 3=主流币服务 4=项目方币服务 5=合约服务 6=Api后台管理接口 7=对冲服务
/// </summary>
public enum E_ServiceType
{
    /// <summary>
    /// 撮合服务
    /// </summary>
    match = 1,
    /// <summary>
    /// 交易服务
    /// </summary>
    trade = 2,
    /// <summary>
    /// 管理员api服务
    /// </summary>
    admin = 3,
    /// <summary>
    /// 公开api服务
    /// </summary>
    openapi = 4

}