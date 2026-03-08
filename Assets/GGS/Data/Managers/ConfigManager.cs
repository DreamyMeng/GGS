using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Newtonsoft.Json;

namespace GGS.Data
{
    /// <summary>
    /// 配置表管理器 - 负责加载和管理游戏配置数据
    /// 只使用 Resources 加载，使用 Newtonsoft.Json 反序列化
    /// </summary>
    public class ConfigManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string _resourcesPath = "Json";
        [SerializeField] private bool _loadOnStart = true;

        // 缓存已加载的配置数据
        private readonly Dictionary<string, object> _configCache = new Dictionary<string, object>();
        private readonly Dictionary<Type, object> _typedCache = new Dictionary<Type, object>();

        // 是否已初始化
        private bool _isInitialized;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 资源路径
        /// </summary>
        public string ResourcesPath => _resourcesPath;

        /// <summary>
        /// 初始化（由 Zenject 注入后调用）
        /// </summary>
        [Inject]
        public void Construct()
        {
            Initialize();
        }

        private void Start()
        {
            if (_loadOnStart && !_isInitialized)
            {
                Initialize();
            }
        }

        /// <summary>
        /// 初始化配置管理器
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            _isInitialized = true;
            Debug.Log($"[ConfigManager] 初始化完成，资源路径: Resources/{_resourcesPath}");
        }

        /// <summary>
        /// 加载配置数据（使用泛型）
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="fileName">文件名（不含扩展名）</param>
        /// <returns>配置数据实例</returns>
        public T LoadConfig<T>(string fileName) where T : class
        {
            // 检查缓存
            string cacheKey = $"{typeof(T).Name}:{fileName}";
            if (_configCache.TryGetValue(cacheKey, out var cached))
            {
                return cached as T;
            }

            // 从 Resources 加载
            string path = $"{_resourcesPath}/{fileName}";
            TextAsset textAsset = Resources.Load<TextAsset>(path);

            if (textAsset == null)
            {
                Debug.LogError($"[ConfigManager] 未找到配置文件: Resources/{path}.json");
                return null;
            }

            try
            {
                // 使用 Newtonsoft.Json 反序列化
                T config = JsonConvert.DeserializeObject<T>(textAsset.text);

                if (config != null)
                {
                    // 缓存结果
                    _configCache[cacheKey] = config;
                    Debug.Log($"[ConfigManager] 加载配置成功: {fileName} ({typeof(T).Name})");
                }

                return config;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ConfigManager] 反序列化失败: {fileName}, 错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 加载配置列表（JSON 数组格式）
        /// </summary>
        /// <typeparam name="T">配置项类型</typeparam>
        /// <param name="fileName">文件名（不含扩展名）</param>
        /// <returns>配置列表</returns>
        public List<T> LoadConfigList<T>(string fileName) where T : class
        {
            // 检查缓存
            string cacheKey = $"List:{typeof(T).Name}:{fileName}";
            if (_configCache.TryGetValue(cacheKey, out var cached))
            {
                return cached as List<T>;
            }

            // 从 Resources 加载
            string path = $"{_resourcesPath}/{fileName}";
            TextAsset textAsset = Resources.Load<TextAsset>(path);

            if (textAsset == null)
            {
                Debug.LogError($"[ConfigManager] 未找到配置文件: Resources/{path}.json");
                return new List<T>();
            }

            try
            {
                // 使用 Newtonsoft.Json 反序列化数组
                List<T> config = JsonConvert.DeserializeObject<List<T>>(textAsset.text);

                if (config != null)
                {
                    // 缓存结果
                    _configCache[cacheKey] = config;
                    Debug.Log($"[ConfigManager] 加载配置列表成功: {fileName} ({config.Count} 项)");
                }

                return config ?? new List<T>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ConfigManager] 反序列化列表失败: {fileName}, 错误: {ex.Message}");
                return new List<T>();
            }
        }

        /// <summary>
        /// 加载并缓存配置（按类型索引）
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="fileName">文件名</param>
        /// <returns>配置数据实例</returns>
        public T Get<T>(string fileName) where T : class, new()
        {
            Type type = typeof(T);

            // 检查类型缓存
            if (_typedCache.TryGetValue(type, out var cached))
            {
                return cached as T;
            }

            // 加载并缓存
            T config = LoadConfig<T>(fileName);
            if (config != null)
            {
                _typedCache[type] = config;
            }

            return config;
        }

        /// <summary>
        /// 加载并缓存配置列表（按类型索引）
        /// </summary>
        /// <typeparam name="T">配置项类型</typeparam>
        /// <param name="fileName">文件名</param>
        /// <returns>配置列表</returns>
        public List<T> GetList<T>(string fileName) where T : class
        {
            Type type = typeof(List<T>);

            // 检查类型缓存
            if (_typedCache.TryGetValue(type, out var cached))
            {
                return cached as List<T>;
            }

            // 加载并缓存
            List<T> config = LoadConfigList<T>(fileName);
            _typedCache[type] = config;

            return config;
        }

        /// <summary>
        /// 预加载配置文件
        /// </summary>
        /// <param name="fileNames">要预加载的文件名列表</param>
        public void Preload(params string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                TextAsset textAsset = Resources.Load<TextAsset>($"{_resourcesPath}/{fileName}");
                if (textAsset != null)
                {
                    Debug.Log($"[ConfigManager] 预加载: {fileName}");
                }
            }
        }

        /// <summary>
        /// 清除指定配置的缓存
        /// </summary>
        /// <param name="fileName">文件名</param>
        public void ClearCache(string fileName)
        {
            foreach (var key in _configCache.Keys.Where(k => k.Contains(fileName)).ToList())
            {
                _configCache.Remove(key);
            }
        }

        /// <summary>
        /// 清除类型缓存
        /// </summary>
        public void ClearTypedCache<T>()
        {
            _typedCache.Remove(typeof(T));
            _typedCache.Remove(typeof(List<T>));
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void ClearAllCache()
        {
            _configCache.Clear();
            _typedCache.Clear();
            Debug.Log("[ConfigManager] 已清除所有配置缓存");
        }

        /// <summary>
        /// 检查配置文件是否存在
        /// </summary>
        /// <param name="fileName">文件名（不含扩展名）</param>
        /// <returns>文件是否存在</returns>
        public bool Exists(string fileName)
        {
            TextAsset textAsset = Resources.Load<TextAsset>($"{_resourcesPath}/{fileName}");
            bool exists = textAsset != null;
            if (exists) Resources.UnloadAsset(textAsset);
            return exists;
        }

        /// <summary>
        /// 获取所有已加载的配置键名
        /// </summary>
        public IEnumerable<string> GetLoadedKeys()
        {
            return _configCache.Keys;
        }

        /// <summary>
        /// 应用生命周期结束时清理
        /// </summary>
        private void OnDestroy()
        {
            ClearAllCache();
        }
    }
}
