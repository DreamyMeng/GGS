using System.Threading.Tasks;
using UnityEngine;

namespace GGS.UI
{
    /// <summary>
    /// 面板加载器接口 - 定义面板加载和释放的标准接口
    /// </summary>
    public interface IPanelLoader
    {
        /// <summary>
        /// 异步加载面板预制体
        /// </summary>
        /// <param name="panelName">面板名称</param>
        /// <returns>面板预制体 GameObject</returns>
        Task<GameObject> LoadPanelAsync(string panelName);

        /// <summary>
        /// 释放面板资源
        /// </summary>
        /// <param name="panelName">面板名称</param>
        void ReleasePanel(string panelName);

        /// <summary>
        /// 预热面板（预先加载缓存）
        /// </summary>
        /// <param name="panelName">面板名称</param>
        Task WarmupPanel(string panelName);

        /// <summary>
        /// 释放所有缓存的资源
        /// </summary>
        void ReleaseAll();
    }
}
