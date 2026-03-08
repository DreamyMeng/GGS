using UnityEngine;
using GGS.Localization;

namespace GGS.Localization.Editor
{
    /// <summary>
    /// 本地化模块简单测试
    /// 将此脚本添加到场景中的 GameObject 上运行
    /// </summary>
    public class LocalizationTest : MonoBehaviour
    {
        private ILocalizationManager _localization;

        void Start()
        {
            // 获取本地化管理器实例
            _localization = Localization.Manager;

            if (_localization == null)
            {
                Debug.LogError("[LocalizationTest] 无法获取本地化管理器，请确保场景中有 LocalizationInstaller");
                return;
            }

            // 测试基本翻译
            TestBasicTranslations();

            // 测试带参数翻译
            TestParameterizedTranslations();

            // 测试复数形式
            TestPluralTranslations();
        }

        void TestBasicTranslations()
        {
            Debug.Log("=== 基本翻译测试 ===");
            Debug.Log($"menu.title: {_localization.GetText("menu.title")}");
            Debug.Log($"system.yes: {_localization.GetText("system.yes")}");
            Debug.Log($"system.no: {_localization.GetText("system.no")}");
        }

        void TestParameterizedTranslations()
        {
            Debug.Log("=== 参数化翻译测试 ===");
            Debug.Log($"ui.welcome: {_localization.GetText("ui.welcome", "张三")}");
            Debug.Log($"ui.score: {_localization.GetText("ui.score", 100)}");
        }

        void TestPluralTranslations()
        {
            Debug.Log("=== 复数形式测试 ===");
            Debug.Log($"0 items: {_localization.GetPluralText("plural.item", 0)}");
            Debug.Log($"1 item: {_localization.GetPluralText("plural.item", 1)}");
            Debug.Log($"5 items: {_localization.GetPluralText("plural.item", 5)}");
        }

        void OnGUI()
        {
            if (_localization == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("当前语言: " + _localization.CurrentLanguage);

            if (GUILayout.Button("切换到中文"))
            {
                _localization.SetLanguage("zh-CN");
                Debug.Log("已切换到中文");
            }

            if (GUILayout.Button("Switch to English"))
            {
                _localization.SetLanguage("en");
                Debug.Log("Switched to English");
            }

            GUILayout.Space(20);
            GUILayout.Label("测试翻译:");
            GUILayout.Label(_localization.GetText("menu.title"));
            GUILayout.Label(_localization.GetText("ui.welcome", "玩家"));

            GUILayout.EndArea();
        }
    }
}
