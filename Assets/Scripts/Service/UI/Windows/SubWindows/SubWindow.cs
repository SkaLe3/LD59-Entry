using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Service.UI.Windows
{
    public abstract class SubWindow : MonoBehaviour
    {
        public enum SubWindowType
        {
            
        }
        
        public abstract SubWindowType Type { get; }
        public Type parentType;
        
        protected BaseWindow MainWindow => Services.GetService<UIService>().GetWindow(parentType);

        public void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        protected virtual void OnShow()
        {
            
        }

        public void CloseByParent()
        {
            Services.GetService<UIService>().GetWindow(parentType).CloseAllSubWindows();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            OnHide();
        }

        protected virtual void OnHide()
        {
            
        }
    }
}