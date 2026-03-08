#if ADDRESSABLES_AVAILABLE
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GGS.UI
{
    /// <summary>
    /// Addressables 面板加载器 - 使用 Unity Addressables 系统加载面板
    /// 需要安装 Unity Addressables 包
    /// </summary>
    public class AddressablesPanelLoader : IPanelLoader
    {
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _loadedHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();
        private readonly Dictionary<string, int> _refCounts = new Dictionary<string, int>();
        private readonly string _label;

        public AddressablesPanelLoader(string label = null)
        {
            _label = label;
        }

        public async Task<GameObject> LoadPanelAsync(string panelName)
        {
            // 检查缓存
            if (_loadedHandles.TryGetValue(panelName, out AsyncOperationHandle<GameObject> cachedHandle))
            {
                _refCounts[panelName]++;
                return cachedHandle.Result;
            }

            // 构建加载地址
            string address = string.IsNullOrEmpty(_label) ? $"UI/Panels/{panelName}" : panelName;

            try
            {
                // 使用 Addressables 加载
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(address);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _loadedHandles[panelName] = handle;
                    _refCounts[panelName] = 1;
                    return handle.Result;
                }
                else
                {
                    Debug.LogError($"[AddressablesPanelLoader] 加载面板失败: {address}");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressablesPanelLoader] 加载面板异常: {address}, {e.Message}");
                return null;
            }
        }

        public void ReleasePanel(string panelName)
        {
            if (!_refCounts.ContainsKey(panelName))
            {
                Debug.LogWarning($"[AddressablesPanelLoader] 尝试释放未加载的面板: {panelName}");
                return;
            }

            _refCounts[panelName]--;

            if (_refCounts[panelName] <= 0)
            {
                // 引用计数归零，释放资源
                if (_loadedHandles.TryGetValue(panelName, out AsyncOperationHandle<GameObject> handle))
                {
                    Addressables.Release(handle);
                    _loadedHandles.Remove(panelName);
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
            foreach (var kvp in _loadedHandles)
            {
                Addressables.Release(kvp.Value);
            }

            _loadedHandles.Clear();
            _refCounts.Clear();
        }
    }
}
#else
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GGS.UI
{
    /// <summary>
    /// Addressables 面板加载器占位实现
    /// 需要安装 Unity Addressables 包并定义 ADDRESSABLES_AVAILABLE 符号
    /// </summary>
    public class AddressablesPanelLoader : IPanelLoader
    {
        public AddressablesPanelLoader(string label = null)
        {
            Debug.LogWarning("[AddressablesPanelLoader] Addressables 未启用，请安装 Addressables 包并定义 ADDRESSABLES_AVAILABLE 符号");
        }

        public Task<GameObject> LoadPanelAsync(string panelName)
        {
            Debug.LogError("[AddressablesPanelLoader] Addressables 未启用");
            return Task.FromResult<GameObject>(null);
        }

        public void ReleasePanel(string panelName)
        {
            Debug.LogWarning("[AddressablesPanelLoader] Addressables 未启用");
        }

        public Task WarmupPanel(string panelName)
        {
            return Task.CompletedTask;
        }

        public void ReleaseAll()
        {
            Debug.LogWarning("[AddressablesPanelLoader] Addressables 未启用");
        }
    }
}
#endif
