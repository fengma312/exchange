using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Com.Models.Db;

/// <summary>
/// DB上下文
/// </summary>
public class DbContextEF : DbContext
{

    /// <summary>
    /// 管理员表
    /// </summary>
    /// <value></value>
    public DbSet<Admin> Admin { get; set; } = null!;
    /// <summary>
    /// 服务列表
    /// </summary>
    /// <value></value>
    public DbSet<Cluster> Cluster { get; set; } = null!;
    /// <summary>
    /// 联盟表
    /// </summary>
    /// <value></value>
    public DbSet<Coalition> Coalition { get; set; } = null!;
    /// <summary>
    /// 币的基础信息
    /// </summary>
    /// <value></value>
    public DbSet<Coin> Coin { get; set; } = null!;
    /// <summary>
    /// 成交单
    /// </summary>
    /// <value></value>
    public DbSet<Deal> Deal { get; set; } = null!;
    /// <summary>
    /// K线
    /// </summary>
    /// <value></value>
    public DbSet<Kline> Kline { get; set; } = null!;
    /// <summary>
    /// 交易对基础信息
    /// </summary>
    /// <value></value>
    public DbSet<Market> Market { get; set; } = null!;
    /// <summary>
    /// 订单表
    /// </summary>
    /// <value></value>
    public DbSet<OrderBuy> OrderBuy { get; set; } = null!;
    /// <summary>
    /// 订单表
    /// </summary>
    /// <value></value>
    public DbSet<OrderSell> OrderSell { get; set; } = null!;
    /// <summary>
    /// 钱包流水(手续费流水)
    /// </summary>
    /// <value></value>
    public DbSet<RunningFee> RunningFee { get; set; } = null!;
    /// <summary>
    /// 钱包流水(交易流水)
    /// </summary>
    /// <value></value>
    public DbSet<RunningTrade> RunningTrade { get; set; } = null!;
    /// <summary>
    /// 用户基础信息
    /// </summary>
    /// <value></value>
    public DbSet<Users> Users { get; set; } = null!;
    /// <summary>
    /// api用户
    /// </summary>
    /// <value></value>
    public DbSet<UsersApi> UsersApi { get; set; } = null!;
    /// <summary>
    /// Vip
    /// </summary>
    /// <value></value>
    public DbSet<Vip> Vip { get; set; } = null!;
    /// <summary>
    /// 计账钱包基础信息
    /// </summary>
    /// <value></value>
    public DbSet<Wallet> Wallet { get; set; } = null!;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options"></param>
    public DbContextEF(DbContextOptions<DbContextEF> options) : base(options)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(o =>
        {
            o.HasKey(p => p.admin_id);
            o.HasIndex(P => new { P.email }).IsUnique();
            o.Property(P => P.admin_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("管理员id");
            o.Property(P => P.name).IsRequired().HasMaxLength(20).HasComment("管理员姓名");
            o.Property(P => P.email).IsRequired().HasMaxLength(50).HasComment("邮箱地址");
            o.Property(P => P.password).IsRequired().HasMaxLength(200).HasComment("密码");
            o.Property(P => P.token_key).IsRequired().HasMaxLength(100).HasComment("令牌");
            o.ToTable(nameof(Admin));
        });
        modelBuilder.Entity<Cluster>(o =>
        {
            o.HasKey(p => p.cluster_id);
            o.HasIndex(P => new { P.ip, P.port }).IsUnique();
            o.Property(P => P.cluster_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("id");
            o.Property(P => P.type).IsRequired().HasColumnType("int2").HasComment("服务类型");
            o.Property(P => P.ip).IsRequired().HasMaxLength(50).HasComment("服务地址");
            o.Property(P => P.port).IsRequired().HasColumnType("int").HasComment("端口");
            o.Property(P => P.mark).IsRequired().HasMaxLength(50).HasComment("标记0~9");
            o.Property(P => P.remark).HasMaxLength(200).HasComment("备注");
            o.ToTable(nameof(Cluster));
        });
        modelBuilder.Entity<Coalition>(o =>
        {
            o.HasKey(p => p.coalition_id);
            o.HasIndex(P => new { P.email }).IsUnique();
            o.Property(P => P.coalition_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("联盟id");
            o.Property(P => P.name).IsRequired().HasMaxLength(20).HasComment("负责人姓名");
            o.Property(P => P.email).IsRequired().HasMaxLength(50).HasComment("邮箱地址");
            o.Property(P => P.password).HasMaxLength(200).HasComment("密码");
            o.Property(P => P.token_key).IsRequired().HasMaxLength(100).HasComment("令牌");
            o.ToTable(nameof(Coalition));
        });


        modelBuilder.Entity<Coin>(o =>
        {
            o.HasKey(p => p.coin_id);
            o.HasIndex(P => new { P.coin_name }).IsUnique();
            o.Property(P => P.coin_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("币id");
            o.Property(P => P.coin_name).IsRequired().HasMaxLength(20).HasComment("币名称");
            o.Property(P => P.full_name).HasMaxLength(50).HasComment("全名");
            o.Property(P => P.icon).HasMaxLength(200).HasComment("图标地址");
            o.Property(P => P.contract).HasMaxLength(200).HasComment("合约地址");
            o.ToTable(nameof(Coin));
        });
        modelBuilder.Entity<Deal>(o =>
        {
            o.HasKey(p => p.trade_id);
            o.HasIndex(P => new { P.market, P.time });
            o.Property(P => P.trade_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("成交订单ID");
            o.Property(P => P.trade_model).IsRequired().HasColumnType("int2").HasComment("交易模式");
            o.Property(P => P.market).IsRequired().HasColumnType("bigint").HasComment("交易对");
            o.Property(P => P.symbol).HasMaxLength(20).HasComment("交易对名称");
            o.Property(P => P.price).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("成交均价");
            o.Property(P => P.amount).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("成交量");
            o.Property(P => P.total).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("成交总额");
            o.Property(P => P.trigger_side).IsRequired().HasColumnType("int2").HasComment("成交触发方向");
            o.Property(P => P.bid_id).IsRequired().HasColumnType("bigint").HasComment("买单id");
            o.Property(P => P.ask_id).IsRequired().HasColumnType("bigint").HasComment("卖单id");
            o.Property(P => P.bid_uid).IsRequired().HasColumnType("bigint").HasComment("买单用户id");
            o.Property(P => P.ask_uid).IsRequired().HasColumnType("bigint").HasComment("卖单用户id");
            o.Property(P => P.bid_name).IsRequired().HasMaxLength(50).HasComment("买用户名");
            o.Property(P => P.ask_name).IsRequired().HasMaxLength(50).HasComment("卖用户名");
            o.Property(P => P.bid_total_unsold).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("买单未成交额");
            o.Property(P => P.ask_amount_unsold).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("卖单未成交量");
            o.Property(P => P.bid_total_done).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("买单已成交额");
            o.Property(P => P.ask_amount_done).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("卖单已成交量");
            o.Property(P => P.fee_bid_maker).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("买单挂单手续费");
            o.Property(P => P.fee_bid_taker).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("买单吃单手续费");
            o.Property(P => P.fee_ask_maker).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("卖单挂单手续费");
            o.Property(P => P.fee_ask_taker).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("卖单吃单手续费");
            o.Property(P => P.time).IsRequired().HasColumnType("timestamptz").HasComment("成交时间");
            o.ToTable(nameof(Deal));
        });
        modelBuilder.Entity<Kline>(o =>
        {
            o.HasKey(p => p.id);
            o.HasIndex(P => new { P.market, P.type, P.time_start, P.time_end });
            o.HasIndex(P => new { P.market, P.type, P.time_start });
            o.Property(P => P.id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("K线ID");
            o.Property(P => P.market).IsRequired().HasColumnType("bigint").HasComment("交易对");
            o.Property(P => P.symbol).HasMaxLength(20).HasComment("交易对名称");
            o.Property(P => P.type).IsRequired().HasColumnType("int2").HasComment("K线类型");
            o.Property(P => P.amount).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("成交量");
            o.Property(P => P.count).IsRequired().HasColumnType("bigint").HasComment("成交笔数");
            o.Property(P => P.total).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("成交总额");
            o.Property(P => P.open).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("开盘价");
            o.Property(P => P.close).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("收盘价");
            o.Property(P => P.low).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("最低价");
            o.Property(P => P.high).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("最高价");
            o.Property(P => P.time_start).IsRequired().HasColumnType("timestamptz").HasComment("变更开始时间");
            o.Property(P => P.time_end).IsRequired().HasColumnType("timestamptz").HasComment("变更开始时间");
            o.Property(P => P.time).IsRequired().HasColumnType("timestamptz").HasComment("更新时间");
            o.ToTable(nameof(Kline));
        });
        modelBuilder.Entity<Market>(o =>
        {
            o.HasKey(p => p.market_id);
            o.HasIndex(P => new { P.symbol }).IsUnique();
            o.Property(P => P.market_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("交易对");
            o.Property(P => P.symbol).HasMaxLength(20).HasComment("交易对名称");
            o.Property(P => P.coin_id_base).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("基础币种id");
            o.Property(P => P.coin_name_base).IsRequired().HasMaxLength(20).HasComment("基础币种名");
            o.Property(P => P.coin_id_quote).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("报价币种id");
            o.Property(P => P.coin_name_quote).IsRequired().HasMaxLength(20).HasComment("报价币种名");
            o.Property(P => P.status).IsRequired().HasColumnType("boolean").HasComment("状态 true:正在运行,false:停止");
            o.Property(P => P.transaction).IsRequired().HasColumnType("boolean").HasComment("是否交易");
            o.Property(P => P.places_price).IsRequired().HasColumnType("int").HasComment("交易价小数位数");
            o.Property(P => P.places_amount).IsRequired().HasColumnType("int").HasComment("交易量小数位数");
            o.Property(P => P.trade_min).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("除了市价卖单外每一笔最小交易额");
            o.Property(P => P.trade_min_market_sell).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("市价卖单每一笔最小交易量");
            o.Property(P => P.market_uid).IsRequired().HasColumnType("bigint").HasComment("作市账号");
            o.Property(P => P.settlement_uid).IsRequired().HasColumnType("bigint").HasComment("结算账号");
            o.Property(P => P.service_url).IsRequired().HasMaxLength(50).HasComment("服务地址");
            o.Property(P => P.sort).IsRequired().HasColumnType("float4").HasComment("排序");
            o.Property(P => P.tag).HasMaxLength(20).HasComment("标签");
            o.ToTable(nameof(Market));
        });
        modelBuilder.Entity<OrderBuy>(o =>
        {
            o.HasKey(p => p.order_id);
            o.HasIndex(P => new { P.market_id, P.state });
            o.HasIndex(P => new { P.market_id, P.uid });
            o.HasIndex(P => new { P.create_time });
            o.Property(P => P.order_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("订单ID");
            o.Property(P => P.client_id).HasMaxLength(50).HasComment("客户自定义订单id");
            o.Property(P => P.market_id).IsRequired().HasColumnType("bigint").HasComment("交易对");
            o.Property(P => P.symbol).HasMaxLength(20).HasComment("交易对名称");
            o.Property(P => P.uid).IsRequired().HasColumnType("bigint").HasComment("用户ID");
            o.Property(P => P.user_name).IsRequired().HasMaxLength(50).HasComment("用户名");
            o.Property(P => P.side).IsRequired().HasColumnType("int2").HasComment("交易方向");
            o.Property(P => P.state).IsRequired().HasColumnType("int2").HasComment("成交状态");
            o.Property(P => P.type).IsRequired().HasColumnType("int2").HasComment("订单类型");
            o.Property(P => P.trade_model).IsRequired().HasColumnType("int2").HasComment("交易模式");
            o.Property(P => P.price).HasColumnType("decimal").HasPrecision(28, 16).HasComment("成交价");
            o.Property(P => P.amount).HasColumnType("decimal").HasPrecision(28, 16).HasComment("成交量");
            o.Property(P => P.total).HasColumnType("decimal").HasPrecision(28, 16).HasComment("成交总额");
            o.Property(P => P.deal_price).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("已成交均价");
            o.Property(P => P.deal_amount).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("已成交量");
            o.Property(P => P.deal_total).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("已成交额");
            o.Property(P => P.unsold).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("未成交额");
            o.Property(P => P.complete_thaw).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("订单完成解冻金额");
            o.Property(P => P.fee_maker).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("挂单手续费");
            o.Property(P => P.fee_taker).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("吃单手续费");
            o.Property(P => P.trigger_hanging_price).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("触发挂单价格");
            o.Property(P => P.trigger_cancel_price).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("触发撤单价格");
            o.Property(P => P.create_time).IsRequired().HasColumnType("timestamptz").HasComment("挂单时间");
            o.Property(P => P.deal_last_time).HasColumnType("timestamptz").HasComment("最后成交时间");
            o.Property(P => P.remarks).HasMaxLength(200).HasComment("备注");
            o.ToTable(nameof(OrderBuy));
        });
        modelBuilder.Entity<OrderSell>(o =>
        {
            o.HasKey(p => p.order_id);
            o.HasIndex(P => new { P.market_id, P.state });
            o.HasIndex(P => new { P.market_id, P.uid });
            o.HasIndex(P => new { P.create_time });
            o.Property(P => P.order_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("订单ID");
            o.Property(P => P.client_id).HasMaxLength(50).HasComment("客户自定义订单id");
            o.Property(P => P.market_id).IsRequired().HasColumnType("bigint").HasComment("交易对");
            o.Property(P => P.symbol).HasMaxLength(20).HasComment("交易对名称");
            o.Property(P => P.uid).IsRequired().HasColumnType("bigint").HasComment("用户ID");
            o.Property(P => P.user_name).IsRequired().HasMaxLength(50).HasComment("用户名");
            o.Property(P => P.side).IsRequired().HasColumnType("int2").HasComment("交易方向");
            o.Property(P => P.state).IsRequired().HasColumnType("int2").HasComment("成交状态");
            o.Property(P => P.type).IsRequired().HasColumnType("int2").HasComment("订单类型");
            o.Property(P => P.trade_model).IsRequired().HasColumnType("int2").HasComment("交易模式");
            o.Property(P => P.price).HasColumnType("decimal").HasPrecision(28, 16).HasComment("成交价");
            o.Property(P => P.amount).HasColumnType("decimal").HasPrecision(28, 16).HasComment("成交量");
            o.Property(P => P.total).HasColumnType("decimal").HasPrecision(28, 16).HasComment("成交总额");
            o.Property(P => P.deal_price).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("已成交均价");
            o.Property(P => P.deal_amount).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("已成交量");
            o.Property(P => P.deal_total).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("已成交额");
            o.Property(P => P.unsold).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("未成交量");
            o.Property(P => P.complete_thaw).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("订单完成解冻金额");
            o.Property(P => P.fee_maker).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("挂单手续费");
            o.Property(P => P.fee_taker).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("吃单手续费");
            o.Property(P => P.trigger_hanging_price).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("触发挂单价格");
            o.Property(P => P.trigger_cancel_price).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("触发撤单价格");
            o.Property(P => P.create_time).IsRequired().HasColumnType("timestamptz").HasComment("挂单时间");
            o.Property(P => P.deal_last_time).HasColumnType("timestamptz").HasComment("最后成交时间");
            o.Property(P => P.remarks).HasMaxLength(200).HasComment("备注");
            o.ToTable(nameof(OrderSell));
        });
        modelBuilder.Entity<RunningFee>(o =>
        {
            o.HasKey(p => p.running_id);
            o.HasIndex(P => new { P.coin_id, P.uid_from, P.uid_to, P.time });
            o.Property(P => P.running_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("id");
            o.Property(P => P.relation_id).IsRequired().HasColumnType("bigint").HasComment("关联id");
            o.Property(P => P.type).IsRequired().HasColumnType("int2").HasComment("流水类型");
            o.Property(P => P.coin_id).IsRequired().HasColumnType("bigint").HasComment("币id");
            o.Property(P => P.coin_name).HasMaxLength(20).HasComment("币名称");
            o.Property(P => P.wallet_from).IsRequired().HasColumnType("bigint").HasComment("来源 钱包id");
            o.Property(P => P.wallet_to).IsRequired().HasColumnType("bigint").HasComment("目的 钱包id");
            o.Property(P => P.wallet_type_from).IsRequired().HasColumnType("int2").HasComment("来源 钱包类型");
            o.Property(P => P.wallet_type_to).IsRequired().HasColumnType("int2").HasComment("目的 钱包类型");
            o.Property(P => P.uid_from).IsRequired().HasColumnType("bigint").HasComment("来源 用户id");
            o.Property(P => P.uid_to).IsRequired().HasColumnType("bigint").HasComment("目的 用户id");
            o.Property(P => P.user_name_from).IsRequired().HasMaxLength(50).HasComment("来源 用户名");
            o.Property(P => P.user_name_to).IsRequired().HasMaxLength(50).HasComment("目的 用户名");
            o.Property(P => P.amount).HasColumnType("decimal").HasPrecision(28, 16).HasComment("量");
            o.Property(P => P.operation_uid).IsRequired().HasColumnType("bigint").HasComment("操作人 0:系统");
            o.Property(P => P.time).IsRequired().HasColumnType("timestamptz").HasComment("时间");
            o.Property(P => P.remarks).HasMaxLength(200).HasComment("备注");
            o.ToTable(nameof(RunningFee));
        });
        modelBuilder.Entity<RunningTrade>(o =>
        {
            o.HasKey(p => p.running_id);
            o.HasIndex(P => new { P.coin_id, P.uid_from, P.uid_to, P.time });
            o.Property(P => P.running_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("id");
            o.Property(P => P.relation_id).IsRequired().HasColumnType("bigint").HasComment("关联id");
            o.Property(P => P.type).IsRequired().HasColumnType("int2").HasComment("流水类型");
            o.Property(P => P.coin_id).IsRequired().HasColumnType("bigint").HasComment("币id");
            o.Property(P => P.coin_name).HasMaxLength(20).HasComment("币名称");
            o.Property(P => P.wallet_from).IsRequired().HasColumnType("bigint").HasComment("来源 钱包id");
            o.Property(P => P.wallet_to).IsRequired().HasColumnType("bigint").HasComment("目的 钱包id");
            o.Property(P => P.wallet_type_from).IsRequired().HasColumnType("int2").HasComment("来源 钱包类型");
            o.Property(P => P.wallet_type_to).IsRequired().HasColumnType("int2").HasComment("目的 钱包类型");
            o.Property(P => P.uid_from).IsRequired().HasColumnType("bigint").HasComment("来源 用户id");
            o.Property(P => P.uid_to).IsRequired().HasColumnType("bigint").HasComment("目的 用户id");
            o.Property(P => P.user_name_from).IsRequired().HasMaxLength(50).HasComment("来源 用户名");
            o.Property(P => P.user_name_to).IsRequired().HasMaxLength(50).HasComment("目的 用户名");
            o.Property(P => P.amount).HasColumnType("decimal").HasPrecision(28, 16).HasComment("量");
            o.Property(P => P.operation_uid).IsRequired().HasColumnType("bigint").HasComment("操作人 0:系统");
            o.Property(P => P.time).IsRequired().HasColumnType("timestamptz").HasComment("时间");
            o.Property(P => P.remarks).HasMaxLength(200).HasComment("备注");
            o.ToTable(nameof(RunningTrade));
        });
        modelBuilder.Entity<Users>(o =>
        {
            o.HasKey(p => p.user_id);
            o.HasIndex(P => new { P.user_name }).IsUnique();
            o.HasIndex(P => new { P.phone }).IsUnique();
            o.HasIndex(P => new { P.email }).IsUnique();
            o.Property(P => P.user_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("用户id");
            o.Property(P => P.user_name).IsRequired().HasMaxLength(50).HasComment("用户名");
            o.Property(P => P.password).HasMaxLength(500).HasComment("用户密码");
            o.Property(P => P.email).IsRequired().HasMaxLength(500).HasComment("邮箱");
            o.Property(P => P.phone).HasMaxLength(500).HasComment("用户手机号码");
            o.Property(P => P.disabled).IsRequired().HasColumnType("boolean").HasComment("禁用");
            o.Property(P => P.transaction).IsRequired().HasColumnType("boolean").HasComment("是否交易");
            o.Property(P => P.withdrawal).IsRequired().HasColumnType("boolean").HasComment("是否提现");
            o.Property(P => P.user_type).IsRequired().HasColumnType("int2").HasComment("用户类型");
            o.Property(P => P.recommend).HasMaxLength(20).HasComment("推荐人id");
            o.Property(P => P.verify_email).IsRequired().HasColumnType("boolean").HasComment("是否验证邮箱 true:验证,false:未验证");
            o.Property(P => P.verify_phone).IsRequired().HasColumnType("boolean").HasComment("是否验证手机 true:验证,false:未验证");
            o.Property(P => P.verify_google).IsRequired().HasColumnType("boolean").HasComment("是否验证谷歌验证器 true:验证,false:未验证");
            o.Property(P => P.verify_realname).IsRequired().HasColumnType("int2").HasComment("是否验证实名认证");
            o.Property(P => P.realname_object_name).HasMaxLength(50).HasComment("实名审核文件对象名");
            o.Property(P => P.vip).IsRequired().HasColumnType("bigint").HasComment("用户等级");
            o.Property(P => P.google_key).HasMaxLength(50).HasComment("google验证码");
            o.Property(P => P.public_key).IsRequired().HasMaxLength(1000).HasComment("公钥");
            o.Property(P => P.private_key).IsRequired().HasMaxLength(3000).HasComment("私钥");
            o.ToTable(nameof(Users));
        });
        modelBuilder.Entity<UsersApi>(o =>
        {
            o.HasKey(p => p.user_api_id);
            o.HasIndex(P => new { P.user_id, P.api_key });
            o.HasIndex(P => new { P.api_key }).IsUnique();
            o.Property(P => P.user_api_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("ID");
            o.Property(P => P.name).HasMaxLength(50).HasComment("名称");
            o.Property(P => P.user_id).IsRequired().HasColumnType("bigint").HasComment("用户id");
            o.Property(P => P.api_key).HasMaxLength(50).HasComment("账户key");
            o.Property(P => P.api_secret).HasMaxLength(500).HasComment("账户密钥");
            o.Property(P => P.transaction).IsRequired().HasColumnType("boolean").HasComment("是否交易");
            o.Property(P => P.withdrawal).IsRequired().HasColumnType("boolean").HasComment("是否提现");
            o.Property(P => P.white_list_ip).HasMaxLength(200).HasComment("IP白名单");
            o.Property(P => P.create_time).IsRequired().HasColumnType("timestamptz").HasComment("创建时间");

            o.ToTable(nameof(UsersApi));
        });
        modelBuilder.Entity<Vip>(o =>
        {
            o.HasKey(p => p.vip_id);
            o.Property(P => P.vip_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("ID");
            o.Property(P => P.name).HasMaxLength(20).HasComment("等级名称");
            o.Property(P => P.volume_used).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("vip等级要求成交量总额");
            o.Property(P => P.fee_maker).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("挂单手续费");
            o.Property(P => P.fee_taker).IsRequired().HasColumnType("decimal").HasPrecision(28, 16).HasComment("吃单手续费");
            o.ToTable(nameof(Vip));
        });
        modelBuilder.Entity<Wallet>(o =>
        {
            o.HasKey(p => p.wallet_id);
            o.HasIndex(P => new { P.wallet_type, P.user_id, P.coin_id }).IsUnique();
            o.Property(P => P.wallet_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("钱包id");
            o.Property(P => P.wallet_type).IsRequired().HasColumnType("int2").HasComment("钱包类型");
            o.Property(P => P.user_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment("用户id");
            o.Property(P => P.user_name).HasMaxLength(50).HasComment("用户名");
            o.Property(P => P.coin_id).IsRequired().ValueGeneratedNever().HasColumnType("bigint").HasComment(" 币id");
            o.Property(P => P.coin_name).HasMaxLength(20).HasComment("币名称");
            o.Property(P => P.total).IsRequired().IsConcurrencyToken().HasColumnType("decimal").HasPrecision(28, 16).HasComment("总额");
            o.Property(P => P.available).IsRequired().IsConcurrencyToken().HasColumnType("decimal").HasPrecision(28, 16).HasComment("可用");
            o.Property(P => P.freeze).IsRequired().IsConcurrencyToken().HasColumnType("decimal").HasPrecision(28, 16).HasComment("冻结");
            o.Property(P => P.Version).IsRowVersion();
            o.ToTable(nameof(Wallet));
        });

        base.OnModelCreating(modelBuilder);
    }
}

