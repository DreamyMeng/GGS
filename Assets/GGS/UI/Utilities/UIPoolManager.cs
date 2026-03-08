using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGS.UI
{
    /// <summary>
    /// UI 对象池管理器 - 用于复用面板减少 GC
    /// </summary>
    public class UIPoolManager
    {
        private readonly Dictionary<string, Stack<UIBase>> _pools = new Dictionary<string, Stack<UIBase>>();
        private readonly Dictionary<string, Transform> _poolRoots = new Dictionary<string, Transform>();
        private readonly Transform _poolContainer;

        public UIPoolManager(Transform poolContainer = null)
        {
            _poolContainer = poolContainer ?? new GameObject("[UI Pool]").transform;
            GameObject.DontDestroyOnLoad(_poolContainer.gameObject);
        }

        /// <summary>
        /// 从池中获取面板
        /// </summary>
        public T Get<T>(string panelName, Transform parent = null) where T : UIBase
        {
            if (_pools.ContainsKey(panelName) && _pools[panelName].Count > 0)
            {
                var panel = _pools[panelName].Pop();
                panel.transform.SetParent(parent);
                panel.gameObject.SetActive(true);
                return panel as T;
            }
            return null;
        }

        /// <summary>
        /// 将面板归还到池中
        /// </summary>
        public void Return(UIBase panel)
        {
            if (panel == null) return;

            string panelName = panel.PanelName;

            // 创建池的根节点
            if (!_poolRoots.ContainsKey(panelName))
            {
                var root = new GameObject($"Pool_{panelName}");
                root.transform.SetParent(_poolContainer);
                _poolRoots[panelName] = root.transform;
            }

            // 隐藏并放入池中
            panel.OnHide();
            panel.transform.SetParent(_poolRoots[panelName]);
            panel.gameObject.SetActive(false);

            if (!_pools.ContainsKey(panelName))
            {
                _pools[panelName] = new Stack<UIBase>();
            }
            _pools[panelName].Push(panel);
        }

        /// <summary>
        /// 预热对象池（预先创建指定数量的对象）
        /// </summary>
        public void Prewarm<T>(string panelName, Func<T> createFunc, int count) where T : UIBase
        {
            for (int i = 0; i < count; i++)
            {
                var panel = createFunc();
                Return(panel);
            }
        }

        /// <summary>
        /// 清空指定面板的池
        /// </summary>
        public void ClearPool(string panelName)
        {
            if (_pools.ContainsKey(panelName))
            {
                while (_pools[panelName].Count > 0)
                {
                    var panel = _pools[panelName].Pop();
                    if (panel != null)
                    {
                        GameObject.Destroy(panel.gameObject);
                    }
                }
                _pools.Remove(panelName);
            }

            if (_poolRoots.ContainsKey(panelName))
            {
                if (_poolRoots[panelName] != null)
                {
                    GameObject.Destroy(_poolRoots[panelName].gameObject);
                }
                _poolRoots.Remove(panelName);
            }
        }

        /// <summary>
        /// 清空所有池
        /// </summary>
        public void ClearAll()
        {
            foreach (var kvp in _pools)
            {
                foreach (var panel in kvp.Value)
                {
                    if (panel != null)
                    {
                        GameObject.Destroy(panel.gameObject);
                    }
                }
            }

            _pools.Clear();

            foreach (var root in _poolRoots.Values)
            {
                if (root != null)
                {
                    GameObject.Destroy(root.gameObject);
                }
            }

            _poolRoots.Clear();
        }

        /// <summary>
        /// 获取池中对象数量
        /// </summary>
        public int GetPoolCount(string panelName)
        {
            return _pools.ContainsKey(panelName) ? _pools[panelName].Count : 0;
        }
    }
}
