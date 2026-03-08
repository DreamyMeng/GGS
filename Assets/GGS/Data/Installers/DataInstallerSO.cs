#pragma warning disable CS0414
using UnityEngine;
using Zenject;

namespace GGS.Data
{
    /// <summary>
    /// 数据模块 ScriptableObject 安装器
    /// 可以作为 Zenject Project Context 的安装器引用
    /// </summary>
    [CreateAssetMenu(fileName = "DataInstallerSO", menuName = "GGS/Data/Installer")]
    public class DataInstallerSO : ScriptableObjectInstaller<DataInstallerSO>
    {
        [Header("Config Settings")]
        [SerializeField] private string _resourcesPath = "Json";
        [SerializeField] private bool _loadOnStart = true;

        public override void InstallBindings()
        {
            // 绑定 ConfigManager（动态创建）
            Container.Bind<ConfigManager>()
                .FromNewComponentOnNewGameObject()
                .AsSingle()
                .NonLazy();

            // 设置 ConfigManager 参数
            Container.Bind<string>().WithArguments(_resourcesPath);
        }
    }
}
