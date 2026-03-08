# GGS 数据管理模块

基于 Zenject 的数据管理解决方案，分为两个系统：

1. **存档系统**：玩家进度数据（等级、金币等），支持保存/加载、加密、版本迁移
2. **配置系统**：游戏静态数据（角色表、技能表等），支持 ScriptableObject 和 JSON 加载

## 目录结构

```
Assets/GGS/Data/
├── Core/                          # 核心接口
│   ├── IDataService.cs            # 存档数据服务接口
│   ├── IConfigDataService.cs      # 配置数据服务接口
│   ├── IJsonSerializer.cs         # JSON 序列化器接口
│   └── IEncryptionService.cs      # 加密服务接口
├── Services/                      # 服务实现
│   ├── LocalJsonDataService.cs    # 本地存档服务
│   ├── ConfigDataService.cs       # 配置数据服务
│   ├── JsonUtilitySerializer.cs   # Unity JsonUtility 实现
│   ├── NewtonsoftJsonSerializer.cs # Newtonsoft.Json 实现
│   └── AesEncryptionService.cs    # AES 加密服务
├── Models/                        # 数据模型
│   ├── SaveDataBase.cs            # 存档基类
│   ├── IVersionMigratable.cs      # 版本迁移接口
│   └── CharacterConfig.cs         # 配置数据示例（角色、技能等）
├── Repositories/                  # 仓库实现
│   ├── DataRepositoryBase.cs      # 存档仓库基类
│   ├── PlayerDataRepository.cs    # 玩家存档示例
│   ├── ConfigRepositoryBase.cs    # 配置仓库基类（在 ConfigDataService.cs）
│   └── ConfigListRepositoryBase.cs # 配置列表仓库基类
└── Installers/                    # Zenject 安装器
    ├── DataInstaller.cs           # MonoInstaller
    └── DataInstallerSO.cs         # ScriptableObjectInstaller
```

## 快速开始

### 1. 安装配置

在场景中添加 `DataInstaller` 组件，或在 Zenject Project Context 中引用 `DataInstallerSO`。

```csharp
// DataInstaller 配置选项
_serializerType = JsonSerializerType.JsonUtility; // JSON 序列化器类型
_enableEncryption = false;                         // 是否启用加密
_customSavePath = "";                              // 自定义保存路径
_bindPlayerRepository = true;                      // 是否绑定玩家数据仓库
```

### 2. 定义数据类

继承 `SaveDataBase` 定义你的存档数据类：

```csharp
[Serializable]
public class GameSettings : SaveDataBase
{
    public string playerName;
    public int level;
    public float volume;

    public override int CurrentVersion => 1;

    public override void SetDefaults()
    {
        base.SetDefaults();
        if (string.IsNullOrEmpty(playerName))
            playerName = "Player";
        if (volume == 0)
            volume = 0.5f;
    }
}
```

### 3. 创建数据仓库

继承 `DataRepositoryBase<T>` 封装数据操作：

```csharp
public class GameSettingsRepository : DataRepositoryBase<GameSettings>
{
    public GameSettingsRepository(IDataService dataService)
        : base(dataService, "settings") { }

    public string GetPlayerName() => Get()?.playerName;
    public void SetPlayerName(string name) => Modify(d => d.playerName = name);
}
```

### 4. 在 Installer 中绑定仓库

```csharp
public class DataInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // ... 绑定服务

        // 绑定仓库
        Container.Bind<GameSettingsRepository>().AsSingle();
    }
}
```

### 5. 使用仓库

```csharp
public class GameManager : MonoBehaviour
{
    private GameSettingsRepository _settingsRepo;

    [Inject]
    public void Construct(GameSettingsRepository settingsRepo)
    {
        _settingsRepo = settingsRepo;
    }

    private async void Start()
    {
        // 加载数据
        await _settingsRepo.LoadAsync();

        // 读取数据
        string playerName = _settingsRepo.GetPlayerName();

        // 修改数据（自动保存）
        _settingsRepo.SetPlayerName("NewName");

        // 或手动控制保存
        _settingsRepo.AutoSave = false;
        _settingsRepo.Modify(d => d.level = 10);
        _settingsRepo.Save();
    }
}
```

## 功能特性

### 数据服务 (IDataService)

```csharp
Task<T> LoadDataAsync<T>(string key)   // 异步加载
void SaveData<T>(string key, T data)   // 保存数据
bool DeleteData(string key)             // 删除数据
bool HasData(string key)                // 检查存在
string[] GetAllKeys()                   // 获取所有键
void ClearAll()                         // 清空所有
```

### 加密存储

启用 AES 加密保护敏感数据：

```csharp
// 在 DataInstaller 中启用
_enableEncryption = true;
_encryptionKey = "Your32ByteEncryptionKey!!";
_encryptionIv = "16ByteInitVector";
```

### 版本迁移

支持存档格式的版本升级：

```csharp
[Serializable]
public class PlayerData : SaveDataBase
{
    public override int CurrentVersion => 2;

    public override IVersionMigratable MigrateTo(int targetVersion)
    {
        if (_version == 1 && targetVersion >= 2)
        {
            // 从版本 1 迁移到版本 2
            gems = 100; // 添加新字段
        }
        _version = targetVersion;
        return this;
    }
}
```

### 数据验证

```csharp
public override bool Validate()
{
    if (level < 1 || level > 9999) return false;
    if (coins < 0) return false;
    return true;
}
```

## 配置数据系统

用于加载游戏静态数据（角色表、技能表、物品表等）。支持两种格式：

### 方式一：ScriptableObject 配置（推荐）

适合需要在编辑器中配置的数据，支持预览和引用。

#### 1. 创建配置类

```csharp
[CreateAssetMenu(fileName = "Character", menuName = "GGS/Config/Character")]
public class CharacterConfig : ScriptableObject
{
    public int id;
    public string characterName;
    public int maxHp = 100;
    public int attack = 10;
    public Sprite icon;
    public GameObject prefab;
}
```

#### 2. 创建资源

在 Unity 中右键 → GGS/Config/Character 创建配置资源，放置到 `Resources/Config/Characters/` 目录。

#### 3. 使用配置管理器

```csharp
public class CharacterManager : MonoBehaviour
{
    private GameConfigManager _configManager;

    [Inject]
    public void Construct(GameConfigManager configManager)
    {
        _configManager = configManager;
    }

    void Start()
    {
        // 加载所有角色配置
        var characters = _configManager.LoadAllCharacters();

        // 获取指定ID的角色
        CharacterConfig character = _configManager.GetCharacter(1001);
    }
}
```

### 方式二：JSON 配置表

适合从外部文件导入的数据，方便策划修改。

#### 1. 定义数据结构

```csharp
[Serializable]
public class CharacterTable
{
    public CharacterData[] characters;
}

[Serializable]
public class CharacterData
{
    public int id;
    public string name;
    public int maxHp;
    public int attack;
}
```

#### 2. 创建 JSON 文件

在 `Assets/StreamingAssets/Config/` 下创建 `CharacterTable.json`：

```json
{
    "characters": [
        {"id": 1001, "name": "战士", "maxHp": 100, "attack": 10},
        {"id": 1002, "name": "法师", "maxHp": 60, "attack": 25}
    ]
}
```

#### 3. 加载 JSON 配置

```csharp
// 从 StreamingAssets 加载
CharacterTable table = _configService.LoadJsonConfig<CharacterTable>("CharacterTable.json");

// 或从 Resources 加载（需创建 TextAsset）
CharacterTable table = _configService.LoadJsonConfig<CharacterTable>("CharacterTable");
```

### 自定义配置仓库

继承仓库基类封装特定配置的访问：

```csharp
public class CharacterConfigRepository : ConfigListRepositoryBase<CharacterConfig>
{
    public CharacterConfigRepository(IConfigDataService configService)
        : base(configService, "Config/Characters") { }

    public CharacterConfig GetById(int id)
    {
        return Find(c => c.id == id);
    }

    public List<CharacterConfig> GetByName(string name)
    {
        return FindAll(c => c.characterName.Contains(name));
    }
}
```

## 存档系统 vs 配置系统

| 特性 | 存档系统 (IDataService) | 配置系统 (IConfigDataService) |
|------|------------------------|------------------------------|
| 用途 | 玩家进度数据 | 游戏静态数据 |
| 存储位置 | persistentDataPath | Resources / StreamingAssets |
| 可写性 | 可读写 | 只读 |
| 数据类型 | 任意 class | ScriptableObject 或 JSON |
| 示例 | 等级、金币、存档 | 角色表、技能表、道具表 |

## 示例：玩家数据仓库

```csharp
// 依赖注入
private PlayerDataRepository _playerRepo;

[Inject]
public void Construct(PlayerDataRepository playerRepo)
{
    _playerRepo = playerRepo;
}

// 使用示例
await _playerRepo.LoadAsync();

int level = _playerRepo.GetLevel();
_playerRepo.AddExperience(100);
bool success = _playerRepo.TrySpendCoins(50);
```

## 存储路径

默认路径：`Application.persistentDataPath + "/Saves"`

不同平台的实际路径：
- Windows: `%USERPROFILE%/AppData/LocalLow/CompanyName/ProductName/Saves`
- macOS: `~/Library/Application Support/CompanyName/ProductName/Saves`
- Android: `/storage/emulated/0/Android/data/com.company.productname/files/Saves`
- iOS: `/var/mobile/Containers/Data/Application/[GUID]/Documents/Saves`

## 注意事项

1. **加密安全性**：客户端加密只能防君子，重要数据需服务端校验
2. **线程安全**：数据操作建议在主线程执行
3. **JSON 序列化器选择**：
   - JsonUtility：轻量级，但功能有限
   - Newtonsoft：功能强大，但包体积较大
4. **自动保存**：默认启用，可通过 `AutoSave` 属性控制

## 扩展

### 自定义数据服务

```csharp
public class CloudDataService : IDataService
{
    public async Task<T> LoadDataAsync<T>(string key) where T : class
    {
        // 从云端加载
    }
    // ... 实现其他接口
}
```

### 自定义序列化器

```csharp
public class BinarySerializer : IJsonSerializer
{
    public string Serialize(object obj) { /* ... */ }
    public T Deserialize<T>(string json) { /* ... */ }
}
```
