#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Service.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public abstract class BaseWindow : MonoBehaviour
    {
        [SerializeField] protected Canvas canvas;

        public int Order
        {
            get => canvas.sortingOrder;
            set => canvas.sortingOrder = value;
        }
        
#if UNITY_EDITOR
        [ContextMenu("Refresh")]
        private void RefreshEditorData()
        {
            canvas = null;
            OnValidate();
        }

        private void OnValidate()
        {
            //if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(3840, 2160); //portrait Android
                scaler.matchWidthOrHeight = 0.4f;
                EditorUtility.SetDirty(gameObject);
            }
        }
#endif
        
        public void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            OnHide();
        }

        public void CloseAllSubWindows()
        {
            
        }
        
        protected abstract void OnHide();
        protected abstract void OnShow();
    }
}