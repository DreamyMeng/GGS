using System.Collections;

namespace GGS.UI
{
    /// <summary>
    /// 面板动画接口
    /// 实现此接口可自定义面板的显示/隐藏动画
    /// </summary>
    public interface IPanelAnimation
    {
        /// <summary>
        /// 显示动画协程
        /// </summary>
        /// <returns></returns>
        IEnumerator ShowAnimation();

        /// <summary>
        /// 隐藏动画协程
        /// </summary>
        /// <returns></returns>
        IEnumerator HideAnimation();

        /// <summary>
        /// 是否跳过动画（用于调试或快速切换）
        /// </summary>
        bool SkipAnimation { get; set; }
    }
}
