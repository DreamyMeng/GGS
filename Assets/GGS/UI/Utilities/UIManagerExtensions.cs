using System;
using System.Collections;

namespace GGS.UI
{
    /// <summary>
    /// UIManager 扩展方法
    /// </summary>
    public static class UIManagerExtensions
    {
        /// <summary>
        /// 显示面板并等待显示完成
        /// </summary>
        public static IEnumerator ShowPanelAndWait(this UIManager manager, Type panelType, object data = null, bool singleton = true)
        {
            manager.ShowPanel(panelType, data, singleton);
            yield return null; // 等待一帧确保面板已显示

            var panel = manager.GetPanel(panelType);
            while (panel == null || !panel.IsVisible)
            {
                yield return null;
                panel = manager.GetPanel(panelType);
            }
        }

        /// <summary>
        /// 显示面板并等待显示完成（泛型版本）
        /// </summary>
        public static IEnumerator ShowPanelAndWait<T>(this UIManager manager, object data = null, bool singleton = true) where T : UIBase
        {
            manager.ShowPanel<T>(data, singleton);
            yield return null;

            var panel = manager.GetPanel<T>();
            while (panel == null || !panel.IsVisible)
            {
                yield return null;
                panel = manager.GetPanel<T>();
            }
        }

        /// <summary>
        /// 隐藏面板并等待隐藏完成
        /// </summary>
        public static IEnumerator HidePanelAndWait(this UIManager manager, Type panelType)
        {
            var panel = manager.GetPanel(panelType);
            if (panel == null)
            {
                yield break;
            }

            manager.HidePanel(panelType);

            while (panel.IsVisible)
            {
                yield return null;
            }
        }

        /// <summary>
        /// 隐藏面板并等待隐藏完成（泛型版本）
        /// </summary>
        public static IEnumerator HidePanelAndWait<T>(this UIManager manager) where T : UIBase
        {
            var panel = manager.GetPanel<T>();
            if (panel == null)
            {
                yield break;
            }

            manager.HidePanel<T>();

            while (panel.IsVisible)
            {
                yield return null;
            }
        }

        /// <summary>
        /// 显示模态对话框（带背景遮罩）
        /// </summary>
        public static void ShowModal<T>(this UIManager manager, object data = null) where T : UIBase
        {
            manager.ShowPanel<T>(data, true);

            // 可以在这里添加背景遮罩逻辑
            // 例如：显示一个半透明黑色面板
        }

        /// <summary>
        /// 切换面板（隐藏当前，显示新的）
        /// </summary>
        public static void SwitchTo<T>(this UIManager manager, object data = null) where T : UIBase
        {
            var current = manager.GetCurrentPanel();
            manager.ShowPanel<T>(data, true);

            if (current != null)
            {
                manager.HidePanel(current.GetType());
            }
        }

        /// <summary>
        /// 返回上一个面板
        /// </summary>
        public static void GoBack(this UIManager manager)
        {
            manager.HideTopPanel();
        }

        /// <summary>
        /// 检查面板是否已加载
        /// </summary>
        public static bool IsPanelLoaded(this UIManager manager, Type panelType)
        {
            return manager.GetPanel(panelType) != null;
        }

        /// <summary>
        /// 检查面板是否已加载（泛型版本）
        /// </summary>
        public static bool IsPanelLoaded<T>(this UIManager manager) where T : UIBase
        {
            return manager.GetPanel<T>() != null;
        }

        /// <summary>
        /// 检查面板是否可见
        /// </summary>
        public static bool IsPanelVisible(this UIManager manager, Type panelType)
        {
            var panel = manager.GetPanel(panelType);
            return panel != null && panel.IsVisible;
        }

        /// <summary>
        /// 检查面板是否可见（泛型版本）
        /// </summary>
        public static bool IsPanelVisible<T>(this UIManager manager) where T : UIBase
        {
            var panel = manager.GetPanel<T>();
            return panel != null && panel.IsVisible;
        }
    }
}
