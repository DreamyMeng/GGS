namespace GGS.Data
{
    /// <summary>
    /// 可迁移版本的数据接口
    /// 实现此接口的数据类支持版本迁移功能
    /// </summary>
    public interface IVersionMigratable
    {
        /// <summary>
        /// 数据版本号
        /// </summary>
        int Version { get; set; }

        /// <summary>
        /// 迁移到指定版本
        /// </summary>
        /// <param name="targetVersion">目标版本</param>
        /// <returns>迁移后的数据实例</returns>
        IVersionMigratable MigrateTo(int targetVersion);

        /// <summary>
        /// 设置默认值
        /// 在反序列化后调用，确保缺失字段有合理默认值
        /// </summary>
        void SetDefaults();

        /// <summary>
        /// 检查是否需要迁移
        /// </summary>
        /// <returns>是否需要迁移</returns>
        bool NeedsMigration();

        /// <summary>
        /// 执行版本迁移
        /// </summary>
        void Migrate();
    }
}
