using System.Threading.Tasks;
using UnityEngine;

namespace GGS.Localization
{
    /// <summary>
    /// Resources 语言数据加载器 - 从 Resources 文件夹加载语言文件
    /// </summary>
    public class ResourcesLanguageLoader : ILanguageDataLoader
    {
        private readonly string _resourcesPath;

        /// <summary>
        /// 资源路径 (相对于 Resources 文件夹)
        /// </summary>
        public string ResourcesPath => _resourcesPath;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="resourcesPath">Resources 文件夹下的相对路径，默认 "Languages"</param>
        public ResourcesLanguageLoader(string resourcesPath = "Languages")
        {
            _resourcesPath = resourcesPath;
        }

        /// <summary>
        /// 异步加载语言文件
        /// </summary>
        public async Task<string> LoadLanguageFileAsync(string languageCode)
        {
            string path = $"{_resourcesPath}/{languageCode}";

            var loadOperation = Resources.LoadAsync<TextAsset>(path);
            await Task.Yield();

            while (!loadOperation.isDone)
            {
                await Task.Yield();
            }

            if (loadOperation.asset == null)
            {
                Debug.LogWarning($"[ResourcesLanguageLoader] 语言文件不存在: Resources/{path}.json");
                return null;
            }

            string json = (loadOperation.asset as TextAsset).text;
            Resources.UnloadAsset(loadOperation.asset);

            return json;
        }

        /// <summary>
        /// 同步加载语言文件
        /// </summary>
        public string LoadLanguageFile(string languageCode)
        {
            string path = $"{_resourcesPath}/{languageCode}";
            TextAsset textAsset = Resources.Load<TextAsset>(path);

            if (textAsset == null)
            {
                Debug.LogWarning($"[ResourcesLanguageLoader] 语言文件不存在: Resources/{path}.json");
                return null;
            }

            string json = textAsset.text;
            Resources.UnloadAsset(textAsset);

            return json;
        }

        /// <summary>
        /// 检查语言文件是否存在
        /// </summary>
        public bool LanguageFileExists(string languageCode)
        {
            string path = $"{_resourcesPath}/{languageCode}";
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            bool exists = textAsset != null;
            if (exists) Resources.UnloadAsset(textAsset);
            return exists;
        }
    }
}
