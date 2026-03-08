# GGS UI 管理系统使用说明

## 架构概述

基于 Zenject 的 UI 管理系统，提供完整的面板生命周期管理、依赖注入、信号通信等功能。

```
Assets/UI/
├── Core/                   # 核心接口和基类
│   ├── IUIBase.cs         # UI 面板接口
│   ├── UIBase.cs          # UI 面板抽象基类
│   └── IPanelAnimation.cs # 面板动画接口
├── Managers/               # 管理器
│   └── UIManager.cs       # UI 管理器
├── Signals/                # Zenject 信号定义
│   ├── PanelShownSignal.cs
│   ├── PanelHiddenSignal.cs
│   └── PanelPreloadedSignal.cs
├── Loaders/                # 面板加载器
│   ├── IPanelLoader.cs
│   ├── ResourcesPanelLoader.cs
│   └── AddressablesPanelLoader.cs
├── Installers/             # Zenject 安装器
│   ├── UIInstaller.cs     # MonoInstaller 版本
│   └── UIInstallerSO.cs   # ScriptableObjectInstaller 版本
├── Attributes/             # 自定义特性
│   ├── UIPanelAttribute.cs
│   └── UIAutoBindAttribute.cs
├── Utilities/              # 工具类
│   ├── UIPoolManager.cs
│   ├── UIBindHelper.cs
│   └── UIManagerExtensions.cs
└── Scripts/                # 具体面板脚本
    └── UIPanelExample.cs
```

## 快速开始

### 1. 安装 Zenject

1. 通过 Unity Package Manager 安装 Zenject
2. 或从 GitHub 导入: https://github.com/svermeulen/Zenject

### 2. 设置 UIInstaller

在场景中创建一个 GameObject，添加 `UIInstaller` 组件：

```csharp
// 选择 Panel Loader 类型
- Resources: 使用 Resources.Load 加载面板
- Addressables: 使用 Addressables 加载面板（需要单独配置）

// 设置路径
- Resources Path: 默认 "UI/Panels"
```

### 3. 创建面板

```csharp
using GGS.UI;
using UnityEngine.UI;

[UIPanel(AssetPath = "UI/Panels/MyPanel", IsSingleton = true)]
public class UIMyPanel : UIBase
{
    [UIAutoBind] private Button _myButton;
    [UIAutoBind] private Text _myText;

    protected override void Awake()
    {
        base.Awake();
        UIBindHelper.AutoBind(this);      // 自动绑定 UI 组件
        UIBindHelper.BindButtonClicks(this); // 绑定按钮点击
    }

    public override void OnShow(object data = null)
    {
        base.OnShow(data);
        // 自定义显示逻辑
    }

    public override void OnHide()
    {
        base.OnHide();
        // 自定义隐藏逻辑
    }

    [UIButtonClick] // 自动绑定按钮点击
    private void OnMyButtonClick()
    {
        Debug.Log("按钮被点击");
    }
}
```

### 4. 使用 UIManager

```csharp
public class GameController : MonoBehaviour
{
    [Inject] private UIManager _uiManager;

    void Start()
    {
        // 显示面板
        _uiManager.ShowPanel<UIMyPanel>(data: "Hello");

        // 传递自定义数据
        _uiManager.ShowPanel<UIMyPanel>(new MyData { Value = 123 });

        // 隐藏当前面板
        _uiManager.HideTopPanel();

        // 隐藏指定面板
        _uiManager.HidePanel<UIMyPanel>();

        // 使用扩展方法
        _uiManager.SwitchTo<UIMyPanel>(); // 切换面板
        _uiManager.GoBack();              // 返回上一个
    }
}
```

### 5. 使用信号通信

```csharp
public class UIMyPanel : UIBase
{
    [Inject] private SignalBus _signalBus;

    private void OnButtonClick()
    {
        // 发送信号
        _signalBus.Fire(new PanelShownSignal
        {
            PanelName = "MyPanel",
            Data = someData
        });
    }
}

// 在其他类中订阅信号
public class MyListener : MonoBehaviour
{
    [Inject] private SignalBus _signalBus;

    [Inject]
    private void Construct()
    {
        _signalBus.Subscribe<PanelShownSignal>(OnPanelShown);
    }

    private void OnPanelShown(PanelShownSignal signal)
    {
        Debug.Log($"面板显示: {signal.PanelName}");
    }

    private void OnDestroy()
    {
        _signalBus.TryUnsubscribe<PanelShownSignal>(OnPanelShown);
    }
}
```

## 高级用法

### 面板数据传递

```csharp
// 定义数据类
public class LoginData
{
    public string Username;
    public bool RememberMe;
}

// 传递数据
_uiManager.ShowPanel<UILoginPanel>(new LoginData
{
    Username = "Player",
    RememberMe = true
});

// 在面板中接收
public override void OnShow(object data = null)
{
    if (data is LoginData loginData)
    {
        // 使用 loginData
    }
}
```

### 面板动画

实现 `IPanelAnimation` 接口：

```csharp
public class UIMyPanel : UIBase, IPanelAnimation
{
    [Inject] private PanelAnimationService _animService;

    public bool SkipAnimation { get; set; }

    public IEnumerator ShowAnimation()
    {
        if (SkipAnimation) yield break;
        yield return _animService.FadeIn(this, 0.3f);
    }

    public IEnumerator HideAnimation()
    {
        if (SkipAnimation) yield break;
        yield return _animService.FadeOut(this, 0.3f);
    }
}
```

### 对象池

```csharp
// 使用对象池
[Inject] private UIPoolManager _poolManager;

// 从池中获取
var panel = _poolManager.Get<UIMyPanel>("MyPanel", parent);

// 归还到池中
_poolManager.Return(panel);
```

### 自动绑定

使用 `[UIAutoBind]` 特性自动绑定 UI 组件：

```csharp
// 字段名与 GameObject 名称一致时，自动匹配
[UIAutoBind] private Button _startButton; // 查找 "StartButton"

// 指定查找路径
[UIAutoBind(Path = "Container/Buttons/Confirm")]
private Button _confirmButton;
```

## API 参考

### UIManager

| 方法 | 描述 |
|------|------|
| `ShowPanel<T>(data, singleton)` | 显示面板 |
| `HidePanel<T>()` | 隐藏指定面板 |
| `HideTopPanel()` | 隐藏当前最上层面板 |
| `GetPanel<T>()` | 获取面板实例 |
| `PreloadPanel<T>()` | 预加载面板 |
| `UnloadPanel<T>()` | 卸载面板 |
| `GetCurrentPanel()` | 获取当前显示的面板 |
| `ClearAllPanels()` | 清空所有面板 |

### UIManagerExtensions

| 方法 | 描述 |
|------|------|
| `ShowPanelAndWait<T>()` | 显示面板并等待完成 |
| `HidePanelAndWait<T>()` | 隐藏面板并等待完成 |
| `ShowModal<T>()` | 显示模态对话框 |
| `SwitchTo<T>()` | 切换到指定面板 |
| `GoBack()` | 返回上一个面板 |
| `IsPanelLoaded<T>()` | 检查面板是否已加载 |
| `IsPanelVisible<T>()` | 检查面板是否可见 |

## 注意事项

1. **面板预制体路径**：使用 Resources 加载时，将面板预制体放在 `Resources/UI/Panels/` 目录下
2. **Zenject 安装**：确保场景中有 `ZenjectBinding` 或 `SceneContext`
3. **Canvas 组件**：UIBase 会自动添加 CanvasGroup（如果没有）
4. **生命周期**：重写 `OnShowInternal` 和 `OnHideInternal` 而不是直接重写 `OnShow/OnHide`

## 故障排查

**面板加载失败**
- 检查预制体路径是否正确
- 确认预制体上有对应的面板组件

**依赖注入失败**
- 确保在 Zenject Container 中注册了相应类型
- 检查 `[Inject]` 标记是否正确

**信号未触发**
- 确保在 Installer 中声明了信号：`Container.DeclareSignal<YourSignal>()`
- 检查是否正确订阅/取消订阅信号
