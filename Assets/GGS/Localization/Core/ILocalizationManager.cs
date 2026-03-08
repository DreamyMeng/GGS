using System;

namespace GGS.Localization
{
    /// <summary>
    /// 本地化管理器接口 - 负责多语言翻译的管理和获取
    /// </summary>
    public interface ILocalizationManager
    {
        /// <summary>
        /// 当前语言代码 (如 "en", "zh-CN")
        /// </summary>
        string CurrentLanguage { get; }

        /// <summary>
        /// 回退语言代码 (当当前语言缺少翻译时使用)
        /// </summary>
        string FallbackLanguage { get; }

        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 可用语言列表
        /// </summary>
        string[] AvailableLanguages { get; }

        /// <summary>
        /// 语言切换事件 - 当语言改变时触发
        /// </summary>
        event Action<string> OnLanguageChanged;

        /// <summary>
        /// 设置当前语言
        /// </summary>
        /// <param name="languageCode">语言代码 (如 "en", "zh-CN")</param>
        void SetLanguage(string languageCode);

        /// <summary>
        /// 获取翻译文本
        /// </summary>
        /// <param name="key">翻译键</param>
        /// <returns>翻译后的文本，如果找不到则返回键名</returns>
        string GetText(string key);

        /// <summary>
        /// 获取带参数的翻译文本
        /// </summary>
        /// <param name="key">翻译键</param>
        /// <param name="args">格式化参数</param>
        /// <returns>格式化后的翻译文本</returns>
        string GetText(string key, params object[] args);

        /// <summary>
        /// 获取复数形式的翻译文本
        /// </summary>
        /// <param name="key">翻译键</param>
        /// <param name="count">数量（用于选择复数形式）</param>
        /// <param name="args">额外的格式化参数</param>
        /// <returns>格式化后的翻译文本</returns>
        string GetPluralText(string key, int count, params object[] args);

        /// <summary>
        /// 检查是否存在指定的翻译键
        /// </summary>
        /// <param name="key">翻译键</param>
        /// <returns>是否存在</returns>
        bool HasKey(string key);

        /// <summary>
        /// 尝试获取翻译文本
        /// </summary>
        /// <param name="key">翻译键</param>
        /// <param name="result">翻译结果</param>
        /// <returns>是否获取成功</returns>
        bool TryGetText(string key, out string result);
    }
}
