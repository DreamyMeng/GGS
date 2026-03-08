using System;

namespace GGS.UI
{
    /// <summary>
    /// UI 面板特性 - 用于标记面板类并提供元数据
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UIPanelAttribute : Attribute
    {
        /// <summary>
        /// Addressables 或 Resources 中的资源路径
        /// </summary>
        public string AssetPath { get; set; }

        /// <summary>
        /// 是否为单例面板（默认 true）
        /// 单例面板在同一时间只会存在一个实例
        /// </summary>
        public bool IsSingleton { get; set; } = true;

        /// <summary>
        /// 面板层级（用于排序）
        /// </summary>
        public int Layer { get; set; } = 0;

        /// <summary>
        /// 面板分组（用于批量管理）
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 是否在场景启动时预加载
        /// </summary>
        public bool PreloadOnStart { get; set; } = false;

        public UIPanelAttribute(string assetPath = null)
        {
            AssetPath = assetPath;
        }

        /// <summary>
        /// 快捷构造：仅指定路径
        /// </summary>
        public static UIPanelAttribute Path(string path)
        {
            return new UIPanelAttribute(path);
        }

        /// <summary>
        /// 快捷构造：指定路径和是否单例
        /// </summary>
        public static UIPanelAttribute Configure(string path, bool singleton = true, int layer = 0)
        {
            return new UIPanelAttribute(path)
            {
                IsSingleton = singleton,
                Layer = layer
            };
        }
    }
}
