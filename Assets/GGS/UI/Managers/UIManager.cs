using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using Zenject;

namespace GGS.UI
{
    /// <summary>
    /// UI 管理器 - 负责面板的加载、显示、隐藏和层级管理
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Roots")]
        [SerializeField] private Transform _uiRoot;
        [SerializeField] private Transform _worldUIRoot;

        [Header("Settings")]
        [SerializeField] private int _defaultSortingOrder = 1000;
        [SerializeField] private bool _createDefaultRoots = true;

        // 依赖注入
        private DiContainer _container;
        private IInstantiator _instantiator;
        private IPanelLoader _panelLoader;
        private SignalBus _signalBus;

        // 状态管理
        private readonly Dictionary<string, UIBase> _loadedPanels = new Dictionary<string, UIBase>();
        private readonly Stack<UIBase> _panelStack = new Stack<UIBase>();
        private readonly Dictionary<Type, UIBase> _singletonPanels = new Dictionary<Type, UIBase>();

        // 正在加载中的面板
        private readonly HashSet<string> _loadingPanels = new HashSet<string>();

        [Inject]
        public void Construct(
            DiContainer container,
            IInstantiator instantiator,
            IPanelLoader panelLoader,
            SignalBus signalBus)
        {
            _container = container;
            _instantiator = instantiator;
            _panelLoader = panelLoader;
            _signalBus = signalBus;

            InitializeUIRoot();
        }

        private void InitializeUIRoot()
        {
            if (_createDefaultRoots)
            {
                // 创建或获取 UI 根节点
                if (_uiRoot == null)
                {
                    GameObject uiRootGO = new GameObject("[UI Root]");
                    DontDestroyOnLoad(uiRootGO);
                    _uiRoot = uiRootGO.transform;

                    // 添加 Canvas
                    Canvas canvas = uiRootGO.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = _defaultSortingOrder;

                    // 添加 GraphicRaycaster
                    uiRootGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }

                // 创建世界空间 UI 根节点
                if (_worldUIRoot == null)
                {
                    GameObject worldUIRootGO = new GameObject("[World UI Root]");
                    worldUIRootGO.transform.SetParent(_uiRoot);
                    _worldUIRoot = worldUIRootGO.transform;
                }
            }
        }

        #region Public API

        /// <summary>
        /// 显示面板（泛型版本）
        /// </summary>
        public T ShowPanel<T>(object data = null, bool singleton = true) where T : UIBase
        {
            Type panelType = typeof(T);
            string panelName = GetPanelName(panelType);

            // 检查是否正在加载
            if (_loadingPanels.Contains(panelName))
            {
                Debug.LogWarning($"[UIManager] 面板 {panelName} 正在加载中，请勿重复调用");
                return null;
            }

            // 单例模式：检查是否已存在
            if (singleton && _singletonPanels.TryGetValue(panelType, out UIBase existingPanel))
            {
                ShowPanelInternal(existingPanel, data);
                BringToFront(existingPanel);
                return existingPanel as T;
            }

            // 检查缓存
            if (_loadedPanels.TryGetValue(panelName, out UIBase cachedPanel))
            {
                if (singleton)
                {
                    _singletonPanels[panelType] = cachedPanel;
                }
                ShowPanelInternal(cachedPanel, data);
                BringToFront(cachedPanel);
                return cachedPanel as T;
            }

            // 加载新面板
            StartCoroutine(LoadPanelCoroutine<T>(panelName, data, singleton));
            return null;
        }

        /// <summary>
        /// 显示面板（类型版本）
        /// </summary>
        public UIBase ShowPanel(Type panelType, object data = null, bool singleton = true)
        {
            if (!typeof(UIBase).IsAssignableFrom(panelType))
            {
                Debug.LogError($"[UIManager] 类型 {panelType.Name} 必须继承自 UIBase");
                return null;
            }

            string panelName = GetPanelName(panelType);

            // 检查是否正在加载
            if (_loadingPanels.Contains(panelName))
            {
                Debug.LogWarning($"[UIManager] 面板 {panelName} 正在加载中，请勿重复调用");
                return null;
            }

            // 单例模式：检查是否已存在
            if (singleton && _singletonPanels.TryGetValue(panelType, out UIBase existingPanel))
            {
                ShowPanelInternal(existingPanel, data);
                BringToFront(existingPanel);
                return existingPanel;
            }

            // 检查缓存
            if (_loadedPanels.TryGetValue(panelName, out UIBase cachedPanel))
            {
                if (singleton)
                {
                    _singletonPanels[panelType] = cachedPanel;
                }
                ShowPanelInternal(cachedPanel, data);
                BringToFront(cachedPanel);
                return cachedPanel;
            }

            // 加载新面板（使用反射调用协程）
            StartCoroutine(LoadPanelCoroutine(panelType, panelName, data, singleton));
            return null;
        }

        /// <summary>
        /// 隐藏当前最上层面板
        /// </summary>
        public void HideTopPanel()
        {
            if (_panelStack.Count == 0)
            {
                Debug.LogWarning("[UIManager] 没有可隐藏的面板");
                return;
            }

            UIBase panel = _panelStack.Pop();
            HidePanelInternal(panel);

            // 显示上一个面板
            if (_panelStack.Count > 0)
            {
                UIBase previousPanel = _panelStack.Peek();
                if (previousPanel != null && !previousPanel.IsVisible)
                {
                    previousPanel.OnShow(null);
                }
            }
        }

        /// <summary>
        /// 隐藏指定面板
        /// </summary>
        public void HidePanel<T>() where T : UIBase
        {
            HidePanel(typeof(T));
        }

        /// <summary>
        /// 隐藏指定面板（类型版本）
        /// </summary>
        public void HidePanel(Type panelType)
        {
            if (_singletonPanels.TryGetValue(panelType, out UIBase panel))
            {
                // 如果在栈顶，特殊处理
                if (_panelStack.Count > 0 && _panelStack.Peek() == panel)
                {
                    HideTopPanel();
                    return;
                }

                HidePanelInternal(panel);
                RemoveFromStack(panel);
            }
            else
            {
                Debug.LogWarning($"[UIManager] 未找到类型为 {panelType.Name} 的面板");
            }
        }

        /// <summary>
        /// 获取当前显示的面板
        /// </summary>
        public UIBase GetCurrentPanel()
        {
            return _panelStack.Count > 0 ? _panelStack.Peek() : null;
        }

        /// <summary>
        /// 获取指定类型的面板实例
        /// </summary>
        public T GetPanel<T>() where T : UIBase
        {
            Type panelType = typeof(T);
            if (_singletonPanels.TryGetValue(panelType, out UIBase panel))
            {
                return panel as T;
            }

            string panelName = GetPanelName(panelType);
            if (_loadedPanels.TryGetValue(panelName, out UIBase cachedPanel))
            {
                return cachedPanel as T;
            }

            return null;
        }

        /// <summary>
        /// 获取指定类型的面板实例（非泛型版本）
        /// </summary>
        public UIBase GetPanel(Type panelType)
        {
            if (!typeof(UIBase).IsAssignableFrom(panelType))
            {
                return null;
            }

            if (_singletonPanels.TryGetValue(panelType, out UIBase panel))
            {
                return panel;
            }

            string panelName = GetPanelName(panelType);
            if (_loadedPanels.TryGetValue(panelName, out UIBase cachedPanel))
            {
                return cachedPanel;
            }

            return null;
        }

        /// <summary>
        /// 预加载面板
        /// </summary>
        public void PreloadPanel<T>() where T : UIBase
        {
            Type panelType = typeof(T);
            string panelName = GetPanelName(panelType);

            if (_loadedPanels.ContainsKey(panelName))
            {
                return; // 已加载
            }

            StartCoroutine(LoadPanelCoroutine<T>(panelName, null, false, true));
        }

        /// <summary>
        /// 卸载并释放面板
        /// </summary>
        public void UnloadPanel<T>() where T : UIBase
        {
            UnloadPanel(typeof(T));
        }

        /// <summary>
        /// 卸载并释放面板（类型版本）
        /// </summary>
        public void UnloadPanel(Type panelType)
        {
            string panelName = GetPanelName(panelType);

            if (_loadedPanels.TryGetValue(panelName, out UIBase panel))
            {
                // 从栈中移除
                RemoveFromStack(panel);

                // 从单例字典移除
                if (_singletonPanels.ContainsKey(panelType))
                {
                    _singletonPanels.Remove(panelType);
                }

                // 销毁面板
                if (panel != null)
                {
                    Destroy(panel.GameObject);
                }

                _loadedPanels.Remove(panelName);
                _panelLoader?.ReleasePanel(panelName);
            }
        }

        /// <summary>
        /// 清空所有面板
        /// </summary>
        public void ClearAllPanels()
        {
            foreach (var panel in _loadedPanels.Values)
            {
                if (panel != null)
                {
                    Destroy(panel.GameObject);
                }
            }

            _loadedPanels.Clear();
            _panelStack.Clear();
            _singletonPanels.Clear();
            _loadingPanels.Clear();
        }

        #endregion

        #region Private Methods

        private IEnumerator LoadPanelCoroutine<T>(string panelName, object data, bool singleton, bool preload = false) where T : UIBase
        {
            Type panelType = typeof(T);
            _loadingPanels.Add(panelName);

            // 使用加载器加载预制体
            var loadTask = _panelLoader.LoadPanelAsync(panelName);
            yield return new WaitUntil(() => loadTask.IsCompleted);

            if (loadTask.IsFaulted || loadTask.Result == null)
            {
                Debug.LogError($"[UIManager] 加载面板失败: {panelName}");
                _loadingPanels.Remove(panelName);
                yield break;
            }

            GameObject prefab = loadTask.Result;

            // 使用 Zenject 实例化，确保依赖注入
            GameObject panelGO = _container.InstantiatePrefab(prefab, _uiRoot);
            T panel = panelGO.GetComponent<T>();

            if (panel == null)
            {
                Debug.LogError($"[UIManager] 预制体 {panelName} 上没有找到 {typeof(T).Name} 组件");
                Destroy(panelGO);
                _loadingPanels.Remove(panelName);
                yield break;
            }

            // 存储
            _loadedPanels[panelName] = panel;
            if (singleton)
            {
                _singletonPanels[panelType] = panel;
            }

            _loadingPanels.Remove(panelName);

            // 如果不是预加载，则显示
            if (!preload)
            {
                ShowPanelInternal(panel, data);
                BringToFront(panel);
            }
            else
            {
                panel.gameObject.SetActive(false);
            }
        }

        private IEnumerator LoadPanelCoroutine(Type panelType, string panelName, object data, bool singleton, bool preload = false)
        {
            _loadingPanels.Add(panelName);

            // 使用加载器加载预制体
            var loadTask = _panelLoader.LoadPanelAsync(panelName);
            yield return new WaitUntil(() => loadTask.IsCompleted);

            if (loadTask.IsFaulted || loadTask.Result == null)
            {
                Debug.LogError($"[UIManager] 加载面板失败: {panelName}");
                _loadingPanels.Remove(panelName);
                yield break;
            }

            GameObject prefab = loadTask.Result;

            // 使用 Zenject 实例化
            GameObject panelGO = _container.InstantiatePrefab(prefab, _uiRoot);
            UIBase panel = panelGO.GetComponent(panelType) as UIBase;

            if (panel == null)
            {
                Debug.LogError($"[UIManager] 预制体 {panelName} 上没有找到 {panelType.Name} 组件");
                Destroy(panelGO);
                _loadingPanels.Remove(panelName);
                yield break;
            }

            // 存储
            _loadedPanels[panelName] = panel;
            if (singleton)
            {
                _singletonPanels[panelType] = panel;
            }

            _loadingPanels.Remove(panelName);

            // 如果不是预加载，则显示
            if (!preload)
            {
                ShowPanelInternal(panel, data);
                BringToFront(panel);
            }
            else
            {
                panel.gameObject.SetActive(false);
            }
        }

        private void ShowPanelInternal(UIBase panel, object data)
        {
            if (panel == null) return;

            panel.OnShow(data);

            // 发送信号
            _signalBus?.Fire(new PanelShownSignal
            {
                PanelName = panel.PanelName,
                Panel = panel,
                Data = data
            });
        }

        private void HidePanelInternal(UIBase panel)
        {
            if (panel == null) return;

            panel.OnHide();

            // 发送信号
            _signalBus?.Fire(new PanelHiddenSignal
            {
                PanelName = panel.PanelName,
                Panel = panel
            });
        }

        private void BringToFront(UIBase panel)
        {
            if (panel == null) return;

            // 从栈中移除（如果存在）
            RemoveFromStack(panel);

            // 压入栈顶
            _panelStack.Push(panel);

            // 调整渲染顺序
            AdjustSortingOrder(panel);
        }

        private void RemoveFromStack(UIBase panel)
        {
            if (panel == null) return;

            // 使用 LINQ 重建栈（移除指定面板）
            var newStack = new Stack<UIBase>(_panelStack.Where(p => p != panel));
            _panelStack.Clear();
            foreach (var p in newStack)
            {
                _panelStack.Push(p);
            }
        }

        private void AdjustSortingOrder(UIBase panel)
        {
            if (panel == null) return;

            Canvas canvas = panel.GetComponent<Canvas>();
            if (canvas != null)
            {
                int baseOrder = _defaultSortingOrder;
                if (_uiRoot != null)
                {
                    Canvas rootCanvas = _uiRoot.GetComponent<Canvas>();
                    if (rootCanvas != null)
                    {
                        baseOrder = rootCanvas.sortingOrder;
                    }
                }
                canvas.sortingOrder = baseOrder + _panelStack.Count;
            }
        }

        private string GetPanelName(Type panelType)
        {
            // 检查是否有 UIPanel 特性
            var attr = panelType.GetCustomAttributes(typeof(UIPanelAttribute), false)
                              .FirstOrDefault() as UIPanelAttribute;

            if (attr != null && !string.IsNullOrEmpty(attr.AssetPath))
            {
                // 从路径中提取文件名
                return System.IO.Path.GetFileNameWithoutExtension(attr.AssetPath);
            }

            return panelType.Name;
        }

        #endregion

        #region Debug

        /// <summary>
        /// 打印当前面板栈状态
        /// </summary>
        public void DebugPrintStack()
        {
            Debug.Log($"[UIManager] 当前面板栈 (Count: {_panelStack.Count}):");
            int index = 0;
            foreach (var panel in _panelStack.Reverse())
            {
                Debug.Log($"  [{index++}] {panel.PanelName} - Visible: {panel.IsVisible}");
            }
        }

        #endregion
    }
}
