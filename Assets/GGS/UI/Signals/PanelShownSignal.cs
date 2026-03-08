namespace GGS.UI
{
    /// <summary>
    /// 面板显示信号 - 当面板显示时触发
    /// </summary>
    public class PanelShownSignal
    {
        /// <summary>
        /// 面板名称
        /// </summary>
        public string PanelName;

        /// <summary>
        /// 面板实例
        /// </summary>
        public UIBase Panel;

        /// <summary>
        /// 传递给面板的数据
        /// </summary>
        public object Data;
    }
}
