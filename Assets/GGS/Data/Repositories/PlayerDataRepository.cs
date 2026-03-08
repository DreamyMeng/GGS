using System;
using UnityEngine;

namespace GGS.Data
{
    /// <summary>
    /// 玩家数据示例
    /// 演示如何定义存档数据类
    /// </summary>
    [Serializable]
    public class PlayerData : SaveDataBase
    {
        /// <summary>
        /// 玩家唯一ID
        /// </summary>
        public string playerId;

        /// <summary>
        /// 玩家名称
        /// </summary>
        public string playerName;

        /// <summary>
        /// 玩家等级
        /// </summary>
        public int level = 1;

        /// <summary>
        /// 经验值
        /// </summary>
        public int experience = 0;

        /// <summary>
        /// 金币数量
        /// </summary>
        public int coins = 0;

        /// <summary>
        /// 钻石数量
        /// </summary>
        public int gems = 0;

        /// <summary>
        /// 上次登录时间（Unix 时间戳）
        /// </summary>
        public long lastLoginTime;

        /// <summary>
        /// 连续登录天数
        /// </summary>
        public int consecutiveLoginDays = 1;

        /// <summary>
        /// 当前支持的最新版本
        /// </summary>
        public override int CurrentVersion => 2;

        /// <summary>
        /// 获取最后登录时间
        /// </summary>
        public DateTime LastLoginDateTime =>
            lastLoginTime > 0
                ? DateTimeOffset.FromUnixTimeSeconds(lastLoginTime).LocalDateTime
                : DateTime.MinValue;

        /// <summary>
        /// 设置默认值
        /// </summary>
        public override void SetDefaults()
        {
            base.SetDefaults();

            if (string.IsNullOrEmpty(playerId))
            {
                playerId = System.Guid.NewGuid().ToString();
            }

            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Player_" + playerId.Substring(0, 8);
            }

            if (level < 1) level = 1;
            if (experience < 0) experience = 0;
            if (coins < 0) coins = 0;
            if (gems < 0) gems = 0;
        }

        /// <summary>
        /// 添加经验值
        /// </summary>
        public void AddExperience(int amount)
        {
            if (amount <= 0) return;

            experience += amount;

            // 简单的升级逻辑
            int expNeeded = level * 100;
            while (experience >= expNeeded)
            {
                experience -= expNeeded;
                level++;
                expNeeded = level * 100;
                Debug.Log($"[PlayerData] 升级! 当前等级: {level}");
            }

            Touch();
        }

        /// <summary>
        /// 添加金币
        /// </summary>
        public void AddCoins(int amount)
        {
            if (amount == 0) return;

            coins = Mathf.Max(0, coins + amount);
            Touch();
        }

        /// <summary>
        /// 消费金币
        /// </summary>
        /// <returns>是否成功消费</returns>
        public bool SpendCoins(int amount)
        {
            if (coins < amount)
            {
                return false;
            }

            coins -= amount;
            Touch();
            return true;
        }

        /// <summary>
        /// 添加钻石
        /// </summary>
        public void AddGems(int amount)
        {
            if (amount == 0) return;

            gems = Mathf.Max(0, gems + amount);
            Touch();
        }

        /// <summary>
        /// 消费钻石
        /// </summary>
        /// <returns>是否成功消费</returns>
        public bool SpendGems(int amount)
        {
            if (gems < amount)
            {
                return false;
            }

            gems -= amount;
            Touch();
            return true;
        }

        /// <summary>
        /// 记录登录
        /// </summary>
        public void RecordLogin()
        {
            long now = DateTimeOffset.Now.ToUnixTimeSeconds();
            long yesterday = DateTimeOffset.Now.AddDays(-1).ToUnixTimeSeconds();

            // 检查是否是连续登录（24小时内）
            if (lastLoginTime > 0 && lastLoginTime < yesterday)
            {
                // 超过24小时，重置连续天数
                consecutiveLoginDays = 1;
            }
            else if (lastLoginTime > 0)
            {
                // 24小时内，连续天数+1
                consecutiveLoginDays++;
            }

            lastLoginTime = now;
            Touch();
        }

        /// <summary>
        /// 版本迁移
        /// </summary>
        public override IVersionMigratable MigrateTo(int targetVersion)
        {
            // 从版本 1 迁移到版本 2
            if (_version == 1 && targetVersion >= 2)
            {
                // 版本 2 添加了 gems 字段，设置默认值
                gems = 100; // 赠送100钻石作为补偿
                Debug.Log("[PlayerData] 从版本 1 迁移到版本 2: 获得赠送钻石 100");
            }

            _version = targetVersion;
            return this;
        }

        /// <summary>
        /// 数据验证
        /// </summary>
        public override bool Validate()
        {
            if (level < 1 || level > 9999)
            {
                Debug.LogWarning($"[PlayerData] 无效的等级: {level}");
                return false;
            }

            if (coins < 0 || gems < 0)
            {
                Debug.LogWarning($"[PlayerData] 负货币: coins={coins}, gems={gems}");
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// 玩家数据仓库
    /// 演示如何使用 DataRepositoryBase 封装数据操作
    /// </summary>
    public class PlayerDataRepository : DataRepositoryBase<PlayerData>
    {
        private const string FileName = "player";

        public PlayerDataRepository(IDataService dataService)
            : base(dataService, FileName)
        {
            AutoSave = true; // 启用自动保存
        }

        /// <summary>
        /// 获取玩家ID
        /// </summary>
        public string GetPlayerId()
        {
            return Get()?.playerId;
        }

        /// <summary>
        /// 获取玩家名称
        /// </summary>
        public string GetPlayerName()
        {
            return Get()?.playerName;
        }

        /// <summary>
        /// 设置玩家名称
        /// </summary>
        public void SetPlayerName(string name)
        {
            Modify(data =>
            {
                data.playerName = name;
            });
        }

        /// <summary>
        /// 添加经验值
        /// </summary>
        public void AddExperience(int amount)
        {
            Modify(data =>
            {
                data.AddExperience(amount);
            });
        }

        /// <summary>
        /// 获取玩家等级
        /// </summary>
        public int GetLevel()
        {
            return Get()?.level ?? 1;
        }

        /// <summary>
        /// 获取金币数量
        /// </summary>
        public int GetCoins()
        {
            return Get()?.coins ?? 0;
        }

        /// <summary>
        /// 添加金币
        /// </summary>
        public void AddCoins(int amount)
        {
            Modify(data =>
            {
                data.AddCoins(amount);
            });
        }

        /// <summary>
        /// 尝试消费金币
        /// </summary>
        public bool TrySpendCoins(int amount)
        {
            bool success = false;
            Modify(data =>
            {
                success = data.SpendCoins(amount);
            });
            return success;
        }

        /// <summary>
        /// 获取钻石数量
        /// </summary>
        public int GetGems()
        {
            return Get()?.gems ?? 0;
        }

        /// <summary>
        /// 记录登录
        /// </summary>
        public void RecordLogin()
        {
            Modify(data =>
            {
                data.RecordLogin();
            });
        }

        /// <summary>
        /// 获取连续登录天数
        /// </summary>
        public int GetConsecutiveLoginDays()
        {
            return Get()?.consecutiveLoginDays ?? 0;
        }

        protected override void OnDataLoaded(PlayerData data)
        {
            Debug.Log($"[PlayerDataRepository] 玩家数据加载成功: {data.playerName} Lv.{data.level}");
        }

        protected override void OnDataSaved(PlayerData data)
        {
            Debug.Log($"[PlayerDataRepository] 玩家数据保存成功");
        }
    }
}
