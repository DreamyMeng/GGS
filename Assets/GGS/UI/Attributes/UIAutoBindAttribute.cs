using System;

namespace GGS.UI
{
    /// <summary>
    /// UI 自动绑定特性
    /// 将此特性添加到字段上，可以配合 UI 自动绑定系统使用
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class UIAutoBindAttribute : Attribute
    {
        /// <summary>
        /// 要绑定的 GameObject 路径
        /// 如果为空，则使用字段名称查找
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 查找的起始对象（如果为空，从当前对象开始查找）
        /// </summary>
        public string RootPath { get; set; }

        public UIAutoBindAttribute(string path = null)
        {
            Path = path;
        }

        /// <summary>
        /// 快捷构造：指定查找路径
        /// </summary>
        public static UIAutoBindAttribute From(string path)
        {
            return new UIAutoBindAttribute(path);
        }
    }

    /// <summary>
    /// 文本绑定并设置初始值特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class UITextAttribute : Attribute
    {
        /// <summary>
        /// 查找路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 初始文本值
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 查找的起始对象
        /// </summary>
        public string RootPath { get; set; }

        public UITextAttribute(string text = null, string path = null)
        {
            Text = text;
            Path = path;
        }
    }

    /// <summary>
    /// UI 组件绑定特性（指定组件类型）
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class UIComponentAttribute : Attribute
    {
        /// <summary>
        /// 组件类型
        /// </summary>
        public Type ComponentType { get; set; }

        /// <summary>
        /// 查找路径
        /// </summary>
        public string Path { get; set; }

        public UIComponentAttribute(Type componentType, string path = null)
        {
            ComponentType = componentType;
            Path = path;
        }
    }

    /// <summary>
    /// 按钮点击绑定特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UIButtonClickAttribute : Attribute
    {
        /// <summary>
        /// 按钮路径
        /// </summary>
        public string ButtonPath { get; set; }

        /// <summary>
        /// 点击声音
        /// </summary>
        public string ClickSound { get; set; }

        public UIButtonClickAttribute(string buttonPath = null)
        {
            ButtonPath = buttonPath;
        }
    }

    /// <summary>
    /// UI 分组特性 - 用于批量管理一组 UI 元素
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = true)]
    public class UIGroupAttribute : Attribute
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 分组中的排序
        /// </summary>
        public int Order { get; set; }

        public UIGroupAttribute(string groupName, int order = 0)
        {
            GroupName = groupName;
            Order = order;
        }
    }
}
