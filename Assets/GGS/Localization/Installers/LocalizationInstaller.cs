#pragma warning disable CS0414
using UnityEngine;
using Zenject;

namespace GGS.Localization
{
    /// <summary>
    /// 本地化模块 Zenject 安装器
    /// 将此组件添加到场景中，配置本地化服务的绑定
    /// </summary>
    public class LocalizationInstaller : MonoInstaller
    {
        [Header("Localization Manager")]
        [SerializeField] private LocalizationManager _localizationManagerPrefab;

        [Header("Settings")]
        [SerializeField] private string _resourcesPath = "Languages";
        [SerializeField] private string _defaultLanguage = "en";
        [SerializeField] private string _fallbackLanguage = "en";
        [SerializeField] private bool _loadOnStart = true;

        [Header("Available Languages")]
        [SerializeField] private string[] _availableLanguages = new string[] { "en", "zh-CN" };

        [Header("Create Manager if Missing")]
        [SerializeField] private bool _createManagerIfMissing = true;

        [Header("Bind Options")]
        [SerializeField] private bool _bindLocalizationManager = true;
        [SerializeField] private bool _bindLanguageLoader = true;

        public override void InstallBindings()
        {
            // 绑定语言数据加载器
            if (_bindLanguageLoader)
            {
                Container.Bind<ILanguageDataLoader>()
                    .To<ResourcesLanguageLoader>()
                    .AsSingle()
                    .WithArguments(_resourcesPath);
            }

            // 绑定本地化管理器
            if (_bindLocalizationManager)
            {
                BindLocalizationManager();
            }

            // 声明语言切换信号（可选订阅者，防止报错）
            Container.DeclareSignal<LanguageChangedSignal>();
        }

        /// <summary>
        /// 绑定本地化管理器
        /// </summary>
        private void BindLocalizationManager()
        {
            if (_localizationManagerPrefab != null)
            {
                // 使用预制体
                Container.Bind<ILocalizationManager>()
                    .To<LocalizationManager>()
                    .FromComponentInNewPrefab(_localizationManagerPrefab)
                    .AsSingle()
                    .NonLazy();
            }
            else if (_createManagerIfMissing)
            {
                // 动态创建 GameObject
                Container.Bind<ILocalizationManager>()
                    .To<LocalizationManager>()
                    .FromNewComponentOnNewGameObject()
                    .AsSingle()
                    .NonLazy();
            }
            else
            {
                Debug.LogError("[LocalizationInstaller] LocalizationManager Prefab 未设置且禁用了自动创建");
            }

            // 绑定可用语言数组，供 LocalizationManager 使用
            Container.BindInstance(_availableLanguages).WhenInjectedInto<LocalizationManager>();
        }
    }
}
