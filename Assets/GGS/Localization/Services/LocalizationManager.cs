using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GGS.Localization
{
    /// <summary>
    /// 本地化管理器 - 负责加载语言文件、提供翻译服务、切换语言
    /// </summary>
    public class LocalizationManager : MonoBehaviour, ILocalizationManager
    {
        [Header("Settings")]
        [SerializeField] private string _defaultLanguage = "en";
        [SerializeField] private string _fallbackLanguage = "en";
        [SerializeField] private bool _loadOnStart = true;

        // 当前语言数据
        private Dictionary<string, string> _currentTranslations;
        private Dictionary<string, Dictionary<string, string>> _pluralTranslations;

        // 回退语言数据
        private Dictionary<string, string> _fallbackTranslations;
        private Dictionary<string, Dictionary<string, string>> _fallbackPluralTranslations;

        private string _currentLanguage;
        private bool _isInitialized;

        // 可用语言列表
        private string[] _availableLanguages = Array.Empty<string>();

        // 通过 Zenject 注入的数据加载器
        [Inject(Optional = true)]
        private ILanguageDataLoader _dataLoader { get; set; }

        // 通过 Zenject 注入的可用语言列表
        [Inject(Optional = true)]
        private string[] _injectedAvailableLanguages { get; set; }

        public string CurrentLanguage => _currentLanguage;
        public string FallbackLanguage => _fallbackLanguage;
        public bool IsInitialized => _isInitialized;
        public string[] AvailableLanguages => _availableLanguages;

        public event Action<string> OnLanguageChanged;

        private void Awake()
        {
            // 如果没有通过 Zenject 注入数据加载器，创建默认的
            if (_dataLoader == null)
            {
                _dataLoader = new ResourcesLanguageLoader("Languages");
            }

            // 如果注入了可用语言列表，使用注入的列表
            if (_injectedAvailableLanguages != null && _injectedAvailableLanguages.Length > 0)
            {
                _availableLanguages = _injectedAvailableLanguages;
            }
        }

        private void Start()
        {
            if (_loadOnStart && !_isInitialized)
            {
                // 从 PlayerPrefs 读取上次选择的语言
                string savedLanguage = PlayerPrefs.GetString("SelectedLanguage", _defaultLanguage);
                LoadLanguage(savedLanguage);
            }
        }

        /// <summary>
        /// 加载语言
        /// </summary>
        public void LoadLanguage(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                languageCode = _defaultLanguage;
            }

            // 同步加载语言文件
            string json = _dataLoader.LoadLanguageFile(languageCode);

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"[LocalizationManager] 语言文件 {languageCode} 不存在，尝试回退语言 {_fallbackLanguage}");

                if (languageCode != _fallbackLanguage)
                {
                    json = _dataLoader.LoadLanguageFile(_fallbackLanguage);
                    if (string.IsNullOrEmpty(json))
                    {
                        Debug.LogError($"[LocalizationManager] 回退语言文件 {_fallbackLanguage} 也不存在！");
                        return;
                    }
                    languageCode = _fallbackLanguage;
                }
                else
                {
                    Debug.LogError($"[LocalizationManager] 无法加载任何语言文件");
                    return;
                }
            }

            // 解析语言数据
            ParseLanguageData(json, out _currentTranslations, out _pluralTranslations);
            _currentLanguage = languageCode;
            _isInitialized = true;

            // 如果当前语言不是回退语言，预加载回退语言
            if (languageCode != _fallbackLanguage)
            {
                LoadFallbackLanguage();
            }

            Debug.Log($"[LocalizationManager] 已加载语言: {languageCode}");
        }

        /// <summary>
        /// 设置当前语言
        /// </summary>
        public void SetLanguage(string languageCode)
        {
            if (_currentLanguage == languageCode && _isInitialized)
            {
                return;
            }

            LoadLanguage(languageCode);
            OnLanguageChanged?.Invoke(languageCode);
        }

        /// <summary>
        /// 加载回退语言
        /// </summary>
        private void LoadFallbackLanguage()
        {
            string json = _dataLoader.LoadLanguageFile(_fallbackLanguage);
            if (!string.IsNullOrEmpty(json))
            {
                ParseLanguageData(json, out _fallbackTranslations, out _fallbackPluralTranslations);
            }
        }

        /// <summary>
        /// 解析语言数据
        /// </summary>
        private void ParseLanguageData(string json, out Dictionary<string, string> translations, out Dictionary<string, Dictionary<string, string>> plurals)
        {
            translations = new Dictionary<string, string>();
            plurals = new Dictionary<string, Dictionary<string, string>>();

            try
            {
                var jsonObject = JObject.Parse(json);
                FlattenJsonObject(jsonObject, "", translations, plurals);
            }
            catch (JsonException ex)
            {
                Debug.LogError($"[LocalizationManager] 解析语言文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 递归扁平化 JSON 对象，支持点号分隔的键名
        /// </summary>
        private void FlattenJsonObject(JObject obj, string prefix, Dictionary<string, string> translations, Dictionary<string, Dictionary<string, string>> plurals)
        {
            foreach (var property in obj.Properties())
            {
                string key = string.IsNullOrEmpty(prefix) ? property.Name : prefix + "." + property.Name;
                var token = property.Value;

                if (token.Type == JTokenType.Object)
                {
                    // 检查是否是复数形式（键为 "0", "1", "2"）
                    if (IsPluralObject(token as JObject))
                    {
                        var pluralDict = new Dictionary<string, string>();
                        foreach (var p in (token as JObject).Properties())
                        {
                            pluralDict[p.Name] = p.Value.ToString();
                        }
                        plurals[key] = pluralDict;
                    }
                    else
                    {
                        // 递归处理嵌套对象
                        FlattenJsonObject(token as JObject, key, translations, plurals);
                    }
                }
                else if (token.Type == JTokenType.Array)
                {
                    // 数组转为字符串
                    translations[key] = token.ToString();
                }
                else
                {
                    // 普通值
                    translations[key] = token.ToString();
                }
            }
        }

        /// <summary>
        /// 检查是否是复数对象（包含 "0", "1", "2" 等数字键）
        /// </summary>
        private bool IsPluralObject(JObject obj)
        {
            if (obj == null) return false;

            int count = 0;
            foreach (var property in obj.Properties())
            {
                // 检查键是否是数字
                if (!int.TryParse(property.Name, out _))
                    return false;

                // 检查值是否是字符串
                if (property.Value.Type != JTokenType.String && property.Value.Type != JTokenType.Integer)
                    return false;

                count++;
                if (count > 5) // 复数对象通常只有几个键
                    return false;
            }

            return count > 0;
        }

        /// <summary>
        /// 获取翻译文本
        /// </summary>
        public string GetText(string key)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[LocalizationManager] 管理器未初始化，返回键名");
                return key;
            }

            if (_currentTranslations != null && _currentTranslations.TryGetValue(key, out string value))
            {
                return value;
            }

            // 尝试从回退语言获取
            if (_fallbackTranslations != null && _fallbackTranslations.TryGetValue(key, out value))
            {
                return value;
            }

            Debug.LogWarning($"[LocalizationManager] 缺少翻译键: {key}");
            return key;
        }

        /// <summary>
        /// 获取带参数的翻译文本
        /// </summary>
        public string GetText(string key, params object[] args)
        {
            string format = GetText(key);

            if (args == null || args.Length == 0)
            {
                return format;
            }

            try
            {
                return string.Format(format, args);
            }
            catch (FormatException ex)
            {
                Debug.LogError($"[LocalizationManager] 格式化错误 for key '{key}': {ex.Message}");
                return format;
            }
        }

        /// <summary>
        /// 获取复数形式的翻译文本
        /// </summary>
        public string GetPluralText(string key, int count, params object[] args)
        {
            if (!_isInitialized)
            {
                return key;
            }

            string pluralKey = GetPluralKey(count);

            // 尝试从当前语言获取复数形式
            if (_pluralTranslations != null && _pluralTranslations.TryGetValue(key, out var pluralDict))
            {
                if (pluralDict.TryGetValue(pluralKey, out string format))
                {
                    return FormatWithCount(format, count, args);
                }
            }

            // 尝试从回退语言获取
            if (_fallbackPluralTranslations != null && _fallbackPluralTranslations.TryGetValue(key, out pluralDict))
            {
                if (pluralDict.TryGetValue(pluralKey, out string format))
                {
                    return FormatWithCount(format, count, args);
                }
            }

            // 回退到普通翻译
            return GetText(key, args);
        }

        /// <summary>
        /// 根据数量获取复数键
        /// </summary>
        private string GetPluralKey(int count)
        {
            if (count == 0) return "0";
            if (count == 1) return "1";
            return "2"; // 其他数量
        }

        /// <summary>
        /// 格式化带数量的文本
        /// </summary>
        private string FormatWithCount(string format, int count, object[] additionalArgs)
        {
            try
            {
                if (additionalArgs == null || additionalArgs.Length == 0)
                {
                    return string.Format(format, count);
                }

                // 将 count 作为第一个参数
                var allArgs = new object[additionalArgs.Length + 1];
                allArgs[0] = count;
                Array.Copy(additionalArgs, 0, allArgs, 1, additionalArgs.Length);

                return string.Format(format, allArgs);
            }
            catch (FormatException ex)
            {
                Debug.LogError($"[LocalizationManager] 复数格式化错误: {ex.Message}");
                return format;
            }
        }

        /// <summary>
        /// 检查是否存在指定的翻译键
        /// </summary>
        public bool HasKey(string key)
        {
            if (_currentTranslations != null && _currentTranslations.ContainsKey(key))
                return true;

            if (_pluralTranslations != null && _pluralTranslations.ContainsKey(key))
                return true;

            if (_fallbackTranslations != null && _fallbackTranslations.ContainsKey(key))
                return true;

            if (_fallbackPluralTranslations != null && _fallbackPluralTranslations.ContainsKey(key))
                return true;

            return false;
        }

        /// <summary>
        /// 尝试获取翻译文本
        /// </summary>
        public bool TryGetText(string key, out string result)
        {
            result = GetText(key);
            return result != key || HasKey(key);
        }

        /// <summary>
        /// 保存当前语言选择
        /// </summary>
        public void SaveCurrentLanguage()
        {
            if (!string.IsNullOrEmpty(_currentLanguage))
            {
                PlayerPrefs.SetString("SelectedLanguage", _currentLanguage);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// 设置可用语言列表
        /// </summary>
        public void SetAvailableLanguages(string[] languages)
        {
            _availableLanguages = languages ?? Array.Empty<string>();
        }

        private void OnDestroy()
        {
            _currentTranslations?.Clear();
            _pluralTranslations?.Clear();
            _fallbackTranslations?.Clear();
            _fallbackPluralTranslations?.Clear();
        }
    }
}
