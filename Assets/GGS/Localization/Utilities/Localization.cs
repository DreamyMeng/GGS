using UnityEngine;

namespace GGS.Localization
{
    /// <summary>
    /// 本地化工具类 - 提供静态方法快速访问本地化服务
    /// </summary>
    public static class Localization
    {
        private static ILocalizationManager _manager;

        /// <summary>
        /// 获取本地化管理器实例
        /// </summary>
        public static ILocalizationManager Manager
        {
            get
            {
                if (_manager == null)
                {
                    // 尝试从 Zenject ProjectContext 获取
                    if (Zenject.ProjectContext.Instance != null)
                    {
                        _manager = Zenject.ProjectContext.Instance.Container.TryResolve<ILocalizationManager>();
                    }

                    // 如果仍然为空，尝试从场景中查找
                    if (_manager == null)
                    {
                        _manager = Object.FindFirstObjectByType<LocalizationManager>() as ILocalizationManager;
                    }

                    if (_manager == null)
                    {
                        Debug.LogWarning("[Localization] 未找到 ILocalizationManager 实例");
                    }
                }
                return _manager;
            }
            set => _manager = value;
        }

        /// <summary>
        /// 获取翻译文本
        /// </summary>
        public static string GetText(string key)
        {
            return Manager?.GetText(key) ?? key;
        }

        /// <summary>
        /// 获取带参数的翻译文本
        /// </summary>
        public static string GetText(string key, params object[] args)
        {
            return Manager?.GetText(key, args) ?? key;
        }

        /// <summary>
        /// 获取复数形式的翻译文本
        /// </summary>
        public static string GetPluralText(string key, int count, params object[] args)
        {
            return Manager?.GetPluralText(key, count, args) ?? key;
        }

        /// <summary>
        /// 检查是否存在指定的翻译键
        /// </summary>
        public static bool HasKey(string key)
        {
            return Manager != null && Manager.HasKey(key);
        }

        /// <summary>
        /// 尝试获取翻译文本
        /// </summary>
        public static bool TryGetText(string key, out string result)
        {
            if (Manager != null)
            {
                return Manager.TryGetText(key, out result);
            }
            result = key;
            return false;
        }

        /// <summary>
        /// 设置当前语言
        /// </summary>
        public static void SetLanguage(string languageCode)
        {
            Manager?.SetLanguage(languageCode);
        }

        /// <summary>
        /// 当前语言代码
        /// </summary>
        public static string CurrentLanguage => Manager?.CurrentLanguage;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool IsInitialized => Manager?.IsInitialized ?? false;
    }
}
