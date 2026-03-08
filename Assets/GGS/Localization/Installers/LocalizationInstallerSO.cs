#pragma warning disable CS0414
using UnityEngine;
using Zenject;

namespace GGS.Localization
{
    /// <summary>
    /// 本地化模块 ScriptableObject 安装器
    /// 用于在 ProjectContext 或其他场景独立的位置绑定本地化服务
    /// </summary>
    [CreateAssetMenu(fileName = "LocalizationInstallerSO", menuName = "GGS/Localization/Installer SO")]
    public class LocalizationInstallerSO : ScriptableObjectInstaller
    {
        [Header("Settings")]
        [SerializeField] private string _resourcesPath = "Languages";
        [SerializeField] private string _defaultLanguage = "en";
        [SerializeField] private string _fallbackLanguage = "en";
        [SerializeField] private bool _loadOnStart = true;

        [Header("Available Languages")]
        [SerializeField] private string[] _availableLanguages = new string[] { "en", "zh-CN" };

        [Header("Bind Options")]
        [SerializeField] private bool _bindLanguageLoader = true;
        [SerializeField] private bool _bindLocalizationManager = true;

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
                Container.Bind<ILocalizationManager>()
                    .To<LocalizationManager>()
                    .FromNewComponentOnNewGameObject()
                    .AsSingle()
                    .NonLazy();
            }

            // 声明语言切换信号（可选订阅者，防止报错）
            Container.DeclareSignal<LanguageChangedSignal>();
        }

        /// <summary>
        /// 获取可用语言列表
        /// </summary>
        public string[] GetAvailableLanguages()
        {
            return _availableLanguages ??= new string[] { "en" };
        }

        /// <summary>
        /// 获取默认语言
        /// </summary>
        public string GetDefaultLanguage()
        {
            return _defaultLanguage;
        }
    }
}
