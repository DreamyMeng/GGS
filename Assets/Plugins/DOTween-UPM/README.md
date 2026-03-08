# DOTween (Unity UPM Package)

A Unity C# animation/tween engine. HOTween v2
Developed by Daniele Giardini - http://www.demigiant.com

## UPM Installation

### Install via Git URL
1. In Unity, go to `Window > Package Manager`
2. Click the `+` icon and select `Add package from git URL...`
3. Enter: `https://github.com/[your-username]/dotween-upm.git`

### Install via Local Folder
1. Copy this `DOTween-UPM` folder to your project's `Packages` folder
2. Unity will automatically detect and import the package

## Documentation

Check the official docs on DOTween's website: http://dotween.demigiant.com

## Included Modules

- **DOTweenModuleAudio**: Audio tweening operations
- **DOTweenModulePhysics**: Physics tweening operations
- **DOTweenModulePhysics2D**: Physics2D tweening operations
- **DOTweenModuleSprite**: Sprite tweening operations
- **DOTweenModuleUI**: UI (uGUI) tweening operations
- **DOTweenModuleUIToolkit**: UI Toolkit tweening operations
- **DOTweenModuleUtils**: Additional utility tweens
- **DOTweenModuleUnityVersion**: Unity version compatibility layer
- **DOTweenModuleEPOOutline**: EPO Outline compatibility

## Quick Start

```csharp
using DG.Tweening;

// Simple tween
transform.DOMove(new Vector3(2, 3, 4), 1f);

// Sequence
Sequence sequence = DOTween.Sequence();
sequence.Append(transform.DOMoveX(1, 1));
sequence.Append(transform.DORotate(new Vector3(0, 180, 0), 1));
sequence.SetLoops(-1, LoopType.Yoyo);
```

## License

See [LICENSE.md](LICENSE.md) for details.
