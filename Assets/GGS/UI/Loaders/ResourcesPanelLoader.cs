using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GGS.UI
{
    /// <summary>
    /// Resources 面板加载器 - 使用 Unity Resources 系统加载面板
    /// </summary>
    public class ResourcesPanelLoader : IPanelLoader
    {
        private readonly Dictionary<string, GameObject> _loadedPrefabs = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, int> _refCounts = new Dictionary<string, int>();
        private readonly string _basePath = "UI/Panels";

        public ResourcesPanelLoader(string basePath = "UI/Panels")
        {
            _basePath = basePath;
        }

        public async Task<GameObject> LoadPanelAsync(string panelName)
        {
            // 检查缓存
            if (_loadedPrefabs.TryGetValue(panelName, out GameObject cachedPrefab))
            {
                _refCounts[panelName]++;
                return cachedPrefab;
            }

            // 构建加载路径
            string loadPath = $"{_basePath}/{panelName}";

            // 使用 Resources.LoadAsync 异步加载
            var loadOperation = Resources.LoadAsync<GameObject>(loadPath);

            // 转换为 Task
            var tcs = new TaskCompletionSource<GameObject>();
            loadOperation.completed += (op) =>
            {
                if (loadOperation.asset == null)
                {
                    Debug.LogError($"[ResourcesPanelLoader] 加载面板失败: {loadPath}");
                    tcs.TrySetResult(null);
                }
                else
                {
                    GameObject prefab = loadOperation.asset as GameObject;
                    _loadedPrefabs[panelName] = prefab;
                    _refCounts[panelName] = 1;
                    tcs.TrySetResult(prefab);
                }
            };

            return await tcs.Task;
        }

        public void ReleasePanel(string panelName)
        {
            if (!_refCounts.ContainsKey(panelName))
            {
                Debug.LogWarning($"[ResourcesPanelLoader] 尝试释放未加载的面板: {panelName}");
                return;
            }

            _refCounts[panelName]--;

            if (_refCounts[panelName] <= 0)
            {
                // 引用计数归零，释放资源
                if (_loadedPrefabs.TryGetValue(panelName, out GameObject prefab))
                {
                    Resources.UnloadAsset(prefab);
                    _loadedPrefabs.Remove(panelName);
                }
                _refCounts.Remove(panelName);
            }
        }

        public async Task WarmupPanel(string panelName)
        {
            await LoadPanelAsync(panelName);
        }

        public void ReleaseAll()
        {
            foreach (var prefab in _loadedPrefabs.Values)
            {
                if (prefab != null)
                {
                    Resources.UnloadAsset(prefab);
                }
            }

            _loadedPrefabs.Clear();
            _refCounts.Clear();
        }
    }
}
