using UnityEngine;
using Zenject;

namespace GGS.UI
{
    /// <summary>
    /// UI 模块 Zenject 安装器
    /// 将此组件添加到场景中，或作为 Zenject 的 ScriptableObjectInstaller
    /// </summary>
    public class UIInstaller : MonoInstaller
    {
        [Header("UI Manager")]
        [SerializeField] private UIManager _uiManagerPrefab;

        [Header("Panel Loader Settings")]
        [SerializeField] private PanelLoaderType _panelLoaderType = PanelLoaderType.Resources;
        [SerializeField] private string _resourcesPath = "UI/Panels";
#pragma warning disable CS0414 // 该字段已赋值，但从未使用
        [SerializeField] private string _addressablesLabel = "";
#pragma warning restore CS0414

        [Header("Create Manager if Missing")]
        [SerializeField] private bool _createManagerIfMissing = true;

        public enum PanelLoaderType
        {
            Resources,
            Addressables
        }

        public override void InstallBindings()
        {
            // 绑定 SignalBus - 必须在其他使用 SignalBus 的绑定之前
            SignalBusInstaller.Install(Container);

            // 绑定 UIManager
            BindUIManager();

            // 绑定面板加载器
            BindPanelLoader();

            // 绑定信号
            BindSignals();
        }

        private void BindUIManager()
        {
            if (_uiManagerPrefab != null)
            {
                Container.Bind<UIManager>()
                    .FromComponentInNewPrefab(_uiManagerPrefab)
                    .AsSingle()
                    .NonLazy();
            }
            else if (_createManagerIfMissing)
            {
                // 动态创建 UIManager GameObject
                Container.Bind<UIManager>()
                    .FromNewComponentOnNewGameObject()
                    .AsSingle()
                    .NonLazy();

                // 设置名称
                Container.BindInstance(new GameObject { name = "[UI Manager]" });
            }
            else
            {
                Debug.LogError("[UIInstaller] UIManager Prefab 未设置且禁用了自动创建");
            }
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
                    Debug.LogWarning("[UIInstaller] Addressables 未启用，使用 Resources 加载器");
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
    }
}
