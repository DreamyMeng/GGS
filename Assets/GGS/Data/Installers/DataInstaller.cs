#pragma warning disable CS0414
using UnityEngine;
using Zenject;
using System.IO;

namespace GGS.Data
{
    /// <summary>
    /// 数据模块 Zenject 安装器
    /// 将此组件添加到场景中，配置数据服务的绑定
    /// </summary>
    public class DataInstaller : MonoInstaller
    {
        [Header("Config Manager")]
        [SerializeField] private ConfigManager _configManagerPrefab;

        [Header("Config Settings")]
        [SerializeField] private string _resourcesPath = "Json";
        [SerializeField] private bool _loadOnStart = true;

        [Header("Save Data Settings")]
        [SerializeField] private string _savePath = "Save";
        [SerializeField] private bool _enableEncryption = false;
        [SerializeField] private string _encryptionKey = "";
        [SerializeField] private string _encryptionIv = "";

        [Header("Create Manager if Missing")]
        [SerializeField] private bool _createManagerIfMissing = true;

        [Header("Bind Services")]
        [SerializeField] private bool _bindConfigManager = true;
        [SerializeField] private bool _bindDataService = true;
        [SerializeField] private bool _bindPlayerRepository = true;

        public override void InstallBindings()
        {
            // 绑定配置管理器
            if (_bindConfigManager)
            {
                BindConfigManager();
            }

            // 绑定存档服务
            if (_bindDataService)
            {
                BindSaveServices();
            }

            // 绑定玩家数据仓库
            if (_bindPlayerRepository)
            {
                BindPlayerRepository();
            }
        }

        /// <summary>
        /// 绑定配置管理器
        /// </summary>
        private void BindConfigManager()
        {
            if (_configManagerPrefab != null)
            {
                Container.Bind<ConfigManager>()
                    .FromComponentInNewPrefab(_configManagerPrefab)
                    .AsSingle()
                    .NonLazy();
            }
            else if (_createManagerIfMissing)
            {
                Container.Bind<ConfigManager>()
                    .FromNewComponentOnNewGameObject()
                    .AsSingle()
                    .NonLazy();
            }
            else
            {
                Debug.LogError("[DataInstaller] ConfigManager Prefab 未设置且禁用了自动创建");
            }
        }

        /// <summary>
        /// 绑定存档服务
        /// </summary>
        private void BindSaveServices()
        {
            // 绑定 JSON 序列化器
            Container.Bind<IJsonSerializer>()
                .To<NewtonsoftJsonSerializer>()
                .AsSingle();

            // 绑定加密服务（如果启用）
            bool useEncryption = _enableEncryption && !string.IsNullOrEmpty(_encryptionKey);
            if (useEncryption)
            {
                Container.Bind<IEncryptionService>()
                    .To<AesEncryptionService>()
                    .AsSingle()
                    .WithArguments(_encryptionKey, _encryptionIv, true);
            }
            else
            {
                // 绑定一个禁用的加密服务
                Container.Bind<IEncryptionService>()
                    .To<AesEncryptionService>()
                    .AsSingle()
                    .WithArguments("", "", false);
            }

            // 绑定数据服务 - 使用工厂方法动态创建
            string fullPath = Path.Combine(Application.persistentDataPath, _savePath);
            Container.Bind<IDataService>()
                .FromMethod(context =>
                {
                    var serializer = context.Container.Resolve<IJsonSerializer>();
                    var encryption = context.Container.Resolve<IEncryptionService>();
                    return new LocalJsonDataService(fullPath, serializer, encryption, useEncryption, ".json");
                })
                .AsSingle();
        }

        /// <summary>
        /// 绑定玩家数据仓库
        /// </summary>
        private void BindPlayerRepository()
        {
            Container.Bind<PlayerDataRepository>()
                .AsSingle();
        }
    }
}
