using Com.Bll.Models;
using Com.Models.Db;

namespace Com.Bll;

/// <summary>
/// Service:总管理
/// </summary>
public class ServiceAdmin
{
    /// <summary>
    /// 基础服务
    /// </summary>
    public readonly ServiceBase service_base;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="service_base">基础服务</param>
    public ServiceAdmin(ServiceBase service_base)
    {
        this.service_base = service_base;
    }

    public async void test(string a)
    {
        using (DbContextEF db = this.service_base.db_factory.CreateDbContext())
        {
            await db.Admin.AddAsync(new Admin()
            {
                admin_id = ServiceFactory.instance.worker.NextId(),
                name = a,
                email = ServiceFactory.instance.worker.NextId().ToString(),
                password = "c",
                token_key = "d"

            });
            int cc = await db.SaveChangesAsync();
        }
    }





}
