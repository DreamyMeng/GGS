using System;
using UnityEngine;

namespace GGS.Data
{
    /// <summary>
    /// 存档数据基类
    /// 提供版本管理、默认值、数据验证等基础功能
    /// </summary>
    [Serializable]
    public abstract class SaveDataBase : IVersionMigratable
    {
        /// <summary>
        /// 数据版本号
        /// 继承类应覆盖此值并在每次数据结构变更时递增
        /// </summary>
        [SerializeField]
        protected int _version = 1;

        /// <summary>
        /// 数据创建时间（Unix 时间戳）
        /// </summary>
        [SerializeField]
        protected long _createdTime;

        /// <summary>
        /// 数据最后修改时间（Unix 时间戳）
        /// </summary>
        [SerializeField]
        protected long _lastModifiedTime;

        /// <summary>
        /// 数据版本号
        /// </summary>
        public int Version
        {
            get => _version;
            set => _version = value;
        }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreatedTime => DateTimeOffset.FromUnixTimeSeconds(_createdTime).LocalDateTime;

        /// <summary>
        /// 数据最后修改时间
        /// </summary>
        public DateTime LastModifiedTime => DateTimeOffset.FromUnixTimeSeconds(_lastModifiedTime).LocalDateTime;

        /// <summary>
        /// 当前支持的最新版本号
        /// 子类应覆盖此属性
        /// </summary>
        public virtual int CurrentVersion => 1;

        protected SaveDataBase()
        {
            long now = DateTimeOffset.Now.ToUnixTimeSeconds();
            _createdTime = now;
            _lastModifiedTime = now;
            _version = CurrentVersion;
        }

        /// <summary>
        /// 更新修改时间
        /// </summary>
        public void Touch()
        {
            _lastModifiedTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        /// <summary>
        /// 验证数据完整性
        /// </summary>
        /// <returns>是否通过验证</returns>
        public virtual bool Validate()
        {
            return true;
        }

        /// <summary>
        /// 设置默认值
        /// 在反序列化后调用，确保缺失字段有合理默认值
        /// </summary>
        public virtual void SetDefaults()
        {
            if (_createdTime == 0)
            {
                _createdTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            }

            if (_lastModifiedTime == 0)
            {
                _lastModifiedTime = _createdTime;
            }

            if (_version == 0)
            {
                _version = 1;
            }
        }

        /// <summary>
        /// 迁移到指定版本
        /// </summary>
        /// <param name="targetVersion">目标版本</param>
        /// <returns>迁移后的数据实例</returns>
        public virtual IVersionMigratable MigrateTo(int targetVersion)
        {
            if (_version >= targetVersion)
            {
                return this;
            }

            // 子类应覆盖此方法实现具体的迁移逻辑
            Debug.LogWarning($"[{GetType().Name}] 从版本 {_version} 迁移到 {targetVersion} 未实现");
            return this;
        }

        /// <summary>
        /// 检查是否需要迁移
        /// </summary>
        /// <returns>是否需要迁移</returns>
        public bool NeedsMigration()
        {
            return _version < CurrentVersion;
        }

        /// <summary>
        /// 执行版本迁移
        /// </summary>
        public void Migrate()
        {
            if (!NeedsMigration())
            {
                return;
            }

            Debug.Log($"[{GetType().Name}] 开始迁移数据从版本 {_version} 到 {CurrentVersion}");
            MigrateTo(CurrentVersion);
            _version = CurrentVersion;
            Touch();
        }
    }
}
