using UnityEngine;

namespace GGS.UI
{
    /// <summary>
    /// UI 面板基础接口
    /// </summary>
    public interface IUIBase
    {
        /// <summary>
        /// 显示面板
        /// </summary>
        /// <param name="data">传递给面板的数据</param>
        void OnShow(object data = null);

        /// <summary>
        /// 隐藏面板
        /// </summary>
        void OnHide();

        /// <summary>
        /// 面板是否可见
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// 面板的 GameObject
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// 面板名称
        /// </summary>
        string PanelName { get; }
    }
}
