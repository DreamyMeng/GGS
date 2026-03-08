using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace GGS.Data
{
    /// <summary>
    /// 本地 JSON 数据服务实现
    /// 提供基于文件的本地数据持久化功能
    /// </summary>
    public class LocalJsonDataService : IDataService
    {
        private readonly string _basePath;
        private readonly IJsonSerializer _serializer;
        private readonly IEncryptionService _encryptionService;
        private readonly bool _useEncryption;
        private readonly string _fileExtension;

        /// <summary>
        /// 创建本地 JSON 数据服务
        /// </summary>
        /// <param name="basePath">数据存储根目录</param>
        /// <param name="serializer">JSON 序列化器</param>
        /// <param name="encryptionService">加密服务（可选）</param>
        /// <param name="useEncryption">是否启用加密</param>
        /// <param name="fileExtension">文件扩展名（默认 .json）</param>
        public LocalJsonDataService(
            string basePath,
            IJsonSerializer serializer,
            IEncryptionService encryptionService = null,
            bool useEncryption = false,
            string fileExtension = ".json")
        {
            _basePath = basePath;
            _serializer = serializer;
            _encryptionService = encryptionService;
            _useEncryption = useEncryption && encryptionService != null && encryptionService.IsEnabled;
            _fileExtension = fileExtension.StartsWith(".") ? fileExtension : "." + fileExtension;

            EnsureDirectoryExists();
        }

        public async Task<T> LoadDataAsync<T>(string key) where T : class
        {
            string path = GetFilePath(key);

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string json = await reader.ReadToEndAsync();

                    // 如果启用了加密，先解密
                    if (_useEncryption)
                    {
                        json = _encryptionService.Decrypt(json);
                    }

                    return _serializer.Deserialize<T>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalJsonDataService] 加载数据失败: {key}, 错误: {ex.Message}");
                return null;
            }
        }

        public void SaveData<T>(string key, T data) where T : class
        {
            if (data == null)
            {
                Debug.LogWarning($"[LocalJsonDataService] 尝试保存 null 数据: {key}");
                return;
            }

            string path = GetFilePath(key);

            try
            {
                EnsureDirectoryExists();

                string json = _serializer.Serialize(data);

                // 如果启用了加密，加密数据
                if (_useEncryption)
                {
                    json = _encryptionService.Encrypt(json);
                }

                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalJsonDataService] 保存数据失败: {key}, 错误: {ex.Message}");
            }
        }

        public bool DeleteData(string key)
        {
            string path = GetFilePath(key);

            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalJsonDataService] 删除数据失败: {key}, 错误: {ex.Message}");
                return false;
            }
        }

        public bool HasData(string key)
        {
            return File.Exists(GetFilePath(key));
        }

        public string[] GetAllKeys()
        {
            if (!Directory.Exists(_basePath))
            {
                return Array.Empty<string>();
            }

            try
            {
                string[] files = Directory.GetFiles(_basePath, "*" + _fileExtension);
                string[] keys = new string[files.Length];

                for (int i = 0; i < files.Length; i++)
                {
                    keys[i] = Path.GetFileNameWithoutExtension(files[i]);
                }

                return keys;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalJsonDataService] 获取数据键失败: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        public void ClearAll()
        {
            if (!Directory.Exists(_basePath))
            {
                return;
            }

            try
            {
                string[] files = Directory.GetFiles(_basePath, "*" + _fileExtension);

                foreach (string file in files)
                {
                    File.Delete(file);
                }

                Debug.Log($"[LocalJsonDataService] 已清除 {files.Length} 个数据文件");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalJsonDataService] 清除数据失败: {ex.Message}");
            }
        }

        private string GetFilePath(string key)
        {
            // 确保 key 不包含非法字符
            string safeKey = string.Join("_", key.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_basePath, safeKey + _fileExtension);
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
                Debug.Log($"[LocalJsonDataService] 创建数据目录: {_basePath}");
            }
        }
    }
}
