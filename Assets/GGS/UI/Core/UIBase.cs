using UnityEngine;
using Zenject;

namespace GGS.UI
{
    /// <summary>
    /// UI 面板基类，提供基础的显示/隐藏功能和依赖注入支持
    /// </summary>
    public abstract class UIBase : MonoBehaviour, IUIBase
    {
        [Inject] protected UIManager UIManager;
        [Inject] protected SignalBus SignalBus;

        [Header("UI References")]
        [SerializeField] protected CanvasGroup _canvasGroup;

        public virtual bool IsVisible => gameObject != null && gameObject.activeSelf;
        public GameObject GameObject => gameObject;
        public virtual string PanelName => GetType().Name;

        /// <summary>
        /// 显示面板
        /// </summary>
        public virtual void OnShow(object data = null)
        {
            gameObject.SetActive(true);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;
            }

            OnShowInternal(data);
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public virtual void OnHide()
        {
            OnHideInternal();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// 子类可重写的显示逻辑
        /// </summary>
        protected virtual void OnShowInternal(object data) { }

        /// <summary>
        /// 子类可重写的隐藏逻辑
        /// </summary>
        protected virtual void OnHideInternal() { }

        #region Lifecycle Methods
        protected virtual void Awake()
        {
            // 如果没有手动赋值，尝试自动获取 CanvasGroup
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        protected virtual void Start() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        #endregion
    }
}
