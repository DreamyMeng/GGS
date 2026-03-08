using TMPro;
using UnityEngine;

namespace GGS.Localization
{
    /// <summary>
    /// TextMeshPro 本地化组件 - 自动根据语言切换更新文本
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTMP : LocalizedBehaviour
    {
        [SerializeField] private TMP_Text _textComponent;
        [SerializeField] private string[] _formatArgs;

        private TMP_Text TextComponent => _textComponent != null ? _textComponent : (_textComponent = GetComponent<TMP_Text>());

        /// <summary>
        /// 格式化参数（用于静态参数）
        /// </summary>
        public string[] FormatArgs
        {
            get => _formatArgs;
            set
            {
                _formatArgs = value;
                UpdateContent();
            }
        }

        private void Reset()
        {
            _textComponent = GetComponent<TMP_Text>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // 确保引用正确
            if (_textComponent == null)
            {
                _textComponent = GetComponent<TMP_Text>();
            }
        }

        /// <summary>
        /// 更新文本内容
        /// </summary>
        public override void UpdateContent()
        {
            if (TextComponent == null || LocalizationManager == null)
                return;

            if (string.IsNullOrEmpty(_key))
            {
                Debug.LogWarning("[LocalizedTMP] 翻译键为空");
                return;
            }

            string translated = GetTranslatedText();
            TextComponent.text = translated;
        }

        /// <summary>
        /// 获取翻译后的文本
        /// </summary>
        private string GetTranslatedText()
        {
            if (_formatArgs == null || _formatArgs.Length == 0)
            {
                return LocalizationManager.GetText(_key);
            }

            return LocalizationManager.GetText(_key, _formatArgs);
        }

        /// <summary>
        /// 动态设置翻译键
        /// </summary>
        public void SetKey(string key)
        {
            Key = key;
        }

        /// <summary>
        /// 动态设置翻译键和参数
        /// </summary>
        public void SetKey(string key, params object[] args)
        {
            Key = key;
            _formatArgs = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                _formatArgs[i] = args[i]?.ToString() ?? "";
            }
        }

        /// <summary>
        /// 动态设置翻译键和参数（使用字符串数组）
        /// </summary>
        public void SetKeyWithStringArgs(string key, string[] args)
        {
            Key = key;
            _formatArgs = args;
        }
    }
}
