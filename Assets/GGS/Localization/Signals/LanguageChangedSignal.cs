namespace GGS.Localization
{
    /// <summary>
    /// 语言切换信号 - 当语言改变时通过 Zenject SignalBus 广播
    /// </summary>
    public class LanguageChangedSignal
    {
        /// <summary>
        /// 新的语言代码
        /// </summary>
        public string NewLanguage { get; set; }

        /// <summary>
        /// 之前的语言代码
        /// </summary>
        public string PreviousLanguage { get; set; }

        public LanguageChangedSignal(string newLanguage, string previousLanguage = null)
        {
            NewLanguage = newLanguage;
            PreviousLanguage = previousLanguage ?? "";
        }
    }
}
