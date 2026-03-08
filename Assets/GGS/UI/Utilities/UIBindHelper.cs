using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GGS.UI
{
    /// <summary>
    /// UI 自动绑定助手 - 通过反射自动绑定 UI 组件
    /// </summary>
    public static class UIBindHelper
    {
        private static readonly Dictionary<Type, Func<Transform, string, Component>> ComponentGetters = new Dictionary<Type, Func<Transform, string, Component>>
        {
            { typeof(Button), (t, p) => t.GetComponent<Button>() },
            { typeof(Text), (t, p) => t.GetComponent<Text>() },
            { typeof(Image), (t, p) => t.GetComponent<Image>() },
            { typeof(Toggle), (t, p) => t.GetComponent<Toggle>() },
            { typeof(Slider), (t, p) => t.GetComponent<Slider>() },
            { typeof(ScrollRect), (t, p) => t.GetComponent<ScrollRect>() },
            { typeof(InputField), (t, p) => t.GetComponent<InputField>() },
            { typeof(CanvasGroup), (t, p) => t.GetComponent<CanvasGroup>() },
            { typeof(RectTransform), (t, p) => t.GetComponent<RectTransform>() },
            { typeof(LayoutGroup), (t, p) => t.GetComponent<LayoutGroup>() },
            { typeof(HorizontalLayoutGroup), (t, p) => t.GetComponent<HorizontalLayoutGroup>() },
            { typeof(VerticalLayoutGroup), (t, p) => t.GetComponent<VerticalLayoutGroup>() },
            { typeof(GridLayoutGroup), (t, p) => t.GetComponent<GridLayoutGroup>() },
        };

        /// <summary>
        /// 自动绑定 MonoBehaviour 中的 UI 组件
        /// </summary>
        public static void AutoBind(MonoBehaviour behaviour)
        {
            if (behaviour == null) return;

            Transform root = behaviour.transform;
            Type type = behaviour.GetType();

            // 获取所有带有 UIAutoBind 特性的字段
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                             .Where(f => f.IsDefined(typeof(UIAutoBindAttribute), false));

            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<UIAutoBindAttribute>();
                string path = string.IsNullOrEmpty(attr.Path) ? FieldNameToGameObjectName(field.Name) : attr.Path;

                try
                {
                    Transform target = string.IsNullOrEmpty(attr.RootPath)
                        ? root.Find(path)
                        : root.Find(attr.RootPath)?.Find(path);

                    if (target == null)
                    {
                        Debug.LogWarning($"[UIBindHelper] 未找到路径: {path} (对象: {behaviour.name})");
                        continue;
                    }

                    Component component = GetComponent(target, field.FieldType);
                    if (component != null)
                    {
                        field.SetValue(behaviour, component);
                    }
                    else
                    {
                        Debug.LogWarning($"[UIBindHelper] 未找到组件类型: {field.FieldType.Name} (路径: {path})");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UIBindHelper] 绑定失败: {field.Name} - {e.Message}");
                }
            }

            // 处理 UIText 特性（绑定 Text 组件并设置初始值）
            var textFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                   .Where(f => f.IsDefined(typeof(UITextAttribute), false));

            foreach (var field in textFields)
            {
                var attr = field.GetCustomAttribute<UITextAttribute>();
                string path = string.IsNullOrEmpty(attr.Path) ? FieldNameToGameObjectName(field.Name) : attr.Path;

                try
                {
                    Transform target = string.IsNullOrEmpty(attr.RootPath)
                        ? root.Find(path)
                        : root.Find(attr.RootPath)?.Find(path);

                    if (target == null)
                    {
                        Debug.LogWarning($"[UIBindHelper] 未找到路径: {path}");
                        continue;
                    }

                    // 字段类型必须是 Text 或其子类
                    if (typeof(Text).IsAssignableFrom(field.FieldType))
                    {
                        Text textComponent = target.GetComponent<Text>();
                        if (textComponent != null)
                        {
                            field.SetValue(behaviour, textComponent);

                            // 设置初始文本值
                            if (!string.IsNullOrEmpty(attr.Text))
                            {
                                textComponent.text = attr.Text;
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[UIBindHelper] 未找到 Text 组件 (路径: {path})");
                        }
                    }
                    else if (typeof(TMPro.TMP_Text).IsAssignableFrom(field.FieldType))
                    {
                        // 支持 TextMeshPro
                        var tmpText = target.GetComponent<TMPro.TMP_Text>();
                        if (tmpText != null)
                        {
                            field.SetValue(behaviour, tmpText);

                            if (!string.IsNullOrEmpty(attr.Text))
                            {
                                tmpText.text = attr.Text;
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[UIBindHelper] 未找到 TMP_Text 组件 (路径: {path})");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[UIBindHelper] 字段类型 {field.FieldType.Name} 不是 Text 或 TMP_Text");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UIBindHelper] 文本绑定失败: {field.Name} - {e.Message}");
                }
            }

            // 处理 UIComponent 特性
            var componentFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                     .Where(f => f.IsDefined(typeof(UIComponentAttribute), false));

            foreach (var field in componentFields)
            {
                var attr = field.GetCustomAttribute<UIComponentAttribute>();
                string path = string.IsNullOrEmpty(attr.Path) ? FieldNameToGameObjectName(field.Name) : attr.Path;

                try
                {
                    Transform target = root.Find(path);
                    if (target == null)
                    {
                        Debug.LogWarning($"[UIBindHelper] 未找到路径: {path}");
                        continue;
                    }

                    Component component = target.GetComponent(attr.ComponentType);
                    if (component != null)
                    {
                        field.SetValue(behaviour, component);
                    }
                    else
                    {
                        Debug.LogWarning($"[UIBindHelper] 未找到组件类型: {attr.ComponentType.Name} (路径: {path})");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UIBindHelper] 绑定失败: {field.Name} - {e.Message}");
                }
            }
        }

        /// <summary>
        /// 查找并获取组件
        /// </summary>
        private static Component GetComponent(Transform transform, Type type)
        {
            // 先尝试从缓存的方法获取
            if (ComponentGetters.TryGetValue(type, out var getter))
            {
                return getter(transform, null);
            }

            // 使用 GetComponent 获取
            return transform.GetComponent(type);
        }

        /// <summary>
        /// 注册自定义组件获取器
        /// </summary>
        public static void RegisterComponentGetter<T>(Func<Transform, string, Component> getter) where T : Component
        {
            ComponentGetters[typeof(T)] = getter;
        }

        /// <summary>
        /// 按名称递归查找子对象
        /// </summary>
        public static Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    return child;
                }

                Transform result = FindChildRecursive(child, name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        /// <summary>
        /// 将字段名转换为 GameObject 名称
        /// 去掉下划线前缀，首字母大写
        /// 例如: _startButton -> StartButton, _text -> Text, m_playerName -> PlayerName
        /// </summary>
        private static string FieldNameToGameObjectName(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return fieldName;
            }

            // 去掉下划线前缀
            if (fieldName.StartsWith("_"))
            {
                fieldName = fieldName.Substring(1);
            }

            // 处理 m_ 前缀
            if (fieldName.StartsWith("m_") && fieldName.Length > 2 && char.IsLower(fieldName[2]))
            {
                fieldName = fieldName.Substring(2);
            }

            // 首字母大写（如果首字母是小写）
            if (fieldName.Length > 0 && char.IsLower(fieldName[0]))
            {
                fieldName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
            }

            return fieldName;
        }

        /// <summary>
        /// 将方法名转换为按钮名称
        /// 支持以下格式:
        /// - OnStartClick -> Start
        /// - OnClickStart -> Start
        /// - StartButtonOnClick -> StartButton
        /// </summary>
        private static string MethodNameToButtonName(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return methodName;
            }

            // 去掉 On 前缀
            if (methodName.StartsWith("On"))
            {
                methodName = methodName.Substring(2);
            }

            // 去掉 Click 后缀
            if (methodName.EndsWith("Click"))
            {
                methodName = methodName.Substring(0, methodName.Length - 5);
            }

            // 首字母大写
            if (methodName.Length > 0 && char.IsLower(methodName[0]))
            {
                methodName = char.ToUpper(methodName[0]) + methodName.Substring(1);
            }

            return methodName;
        }

        /// <summary>
        /// 绑定按钮点击事件
        /// </summary>
        public static void BindButtonClicks(MonoBehaviour behaviour)
        {
            if (behaviour == null) return;

            Transform root = behaviour.transform;
            Type type = behaviour.GetType();

            // 获取所有带有 UIButtonClick 特性的方法
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                              .Where(m => m.IsDefined(typeof(UIButtonClickAttribute), false));

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<UIButtonClickAttribute>();
                string buttonPath = string.IsNullOrEmpty(attr.ButtonPath) ? MethodNameToButtonName(method.Name) : attr.ButtonPath;

                Transform buttonTransform = root.Find(buttonPath);
                if (buttonTransform == null)
                {
                    Debug.LogWarning($"[UIBindHelper] 未找到按钮路径: {buttonPath}");
                    continue;
                }

                Button button = buttonTransform.GetComponent<Button>();
                if (button == null)
                {
                    Debug.LogWarning($"[UIBindHelper] 路径 {buttonPath} 上没有 Button 组件");
                    continue;
                }

                // 创建委托并绑定
                var action = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), behaviour, method);
                button.onClick.AddListener(action);
            }
        }
    }
}
