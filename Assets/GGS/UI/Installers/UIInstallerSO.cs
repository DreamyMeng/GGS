using UnityEngine;
using Zenject;

namespace GGS.UI
{
    /// <summary>
    /// UI 模块 ScriptableObject 安装器
    /// 可以在 Project Settings 中配置，适合跨场景共享配置
    /// </summary>
    [CreateAssetMenu(fileName = "UIInstallerSO", menuName = "GGS/UI/ScriptableObject Installer")]
    public class UIInstallerSO : ScriptableObjectInstaller
    {
        [Header("Panel Loader Settings")]
        [SerializeField] private PanelLoaderType _panelLoaderType = PanelLoaderType.Resources;
        [SerializeField] private string _resourcesPath = "UI/Panels";
#pragma warning disable CS0414 // 该字段已赋值，但从未使用
        [SerializeField] private string _addressablesLabel = "";
#pragma warning restore CS0414

        [Header("UI Settings")]
        [SerializeField] private int _defaultSortingOrder = 1000;
        [SerializeField] private bool _createDefaultRoots = true;

        public enum PanelLoaderType
        {
            Resources,
            Addressables
        }

        public override void InstallBindings()
        {
            // 绑定 SignalBus - 必须在其他使用 SignalBus 的绑定之前
            SignalBusInstaller.Install(Container);

            // 绑定 UIManager（动态创建）
            Container.Bind<UIManager>()
                .FromNewComponentOnNewGameObject()
                .AsSingle()
                .NonLazy();

            // 绑定配置
            Container.BindInstance(this);

            // 绑定面板加载器
            BindPanelLoader();

            // 绑定信号
            BindSignals();
        }

        private void BindPanelLoader()
        {
            switch (_panelLoaderType)
            {
                case PanelLoaderType.Resources:
                    Container.Bind<IPanelLoader>()
                        .To<ResourcesPanelLoader>()
                        .AsSingle()
                        .WithArguments(_resourcesPath);
                    break;

                case PanelLoaderType.Addressables:
#if ADDRESSABLES_AVAILABLE
                    Container.Bind<IPanelLoader>()
                        .To<AddressablesPanelLoader>()
                        .AsSingle()
                        .WithArguments(_addressablesLabel);
#else
                    Debug.LogWarning("[UIInstallerSO] Addressables 未启用，使用 Resources 加载器");
                    Container.Bind<IPanelLoader>()
                        .To<ResourcesPanelLoader>()
                        .AsSingle()
                        .WithArguments(_resourcesPath);
#endif
                    break;
            }
        }

        private void BindSignals()
        {
            // 声明 UI 相关的信号（标记为可选，避免无订阅者时的警告）
            Container.DeclareSignal<PanelShownSignal>().OptionalSubscriber();
            Container.DeclareSignal<PanelHiddenSignal>().OptionalSubscriber();
            Container.DeclareSignal<PanelPreloadedSignal>().OptionalSubscriber();
        }

        public int DefaultSortingOrder => _defaultSortingOrder;
        public bool CreateDefaultRoots => _createDefaultRoots;
    }
}
