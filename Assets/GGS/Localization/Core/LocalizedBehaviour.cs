using UnityEngine;
using Zenject;

namespace GGS.Localization
{
    /// <summary>
    /// 本地化行为基类 - 所有需要本地化的 UI 组件继承此类
    /// </summary>
    public abstract class LocalizedBehaviour : MonoBehaviour
    {
        [SerializeField] protected string _key = "";
        [SerializeField] protected bool _autoUpdate = true;

        [Inject(Optional = true, Source = InjectSources.Local)]
        protected ILocalizationManager LocalizationManager { get; private set; }

        /// <summary>
        /// 翻译键
        /// </summary>
        public string Key
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    if (isActiveAndEnabled)
                    {
                        UpdateContent();
                    }
                }
            }
        }

        /// <summary>
        /// 是否自动更新
        /// </summary>
        public bool AutoUpdate
        {
            get => _autoUpdate;
            set => _autoUpdate = value;
        }

        protected virtual void Awake()
        {
            // 如果 Zenject 没有注入，尝试从 ProjectContext 获取
            if (LocalizationManager == null)
            {
                LocalizationManager = ProjectContext.Instance.Container.Resolve<ILocalizationManager>();
            }
        }

        protected virtual void OnEnable()
        {
            if (_autoUpdate && LocalizationManager != null)
            {
                LocalizationManager.OnLanguageChanged += OnLanguageChangedHandler;
                if (LocalizationManager.IsInitialized)
                {
                    UpdateContent();
                }
            }
        }

        protected virtual void OnDisable()
        {
            if (LocalizationManager != null)
            {
                LocalizationManager.OnLanguageChanged -= OnLanguageChangedHandler;
            }
        }

        /// <summary>
        /// 语言切换事件处理
        /// </summary>
        private void OnLanguageChangedHandler(string languageCode)
        {
            UpdateContent();
        }

        /// <summary>
        /// 更新内容 - 子类实现具体的更新逻辑
        /// </summary>
        public abstract void UpdateContent();

        /// <summary>
        /// 手动刷新内容
        /// </summary>
        public void Refresh()
        {
            if (isActiveAndEnabled)
            {
                UpdateContent();
            }
        }
    }
}
