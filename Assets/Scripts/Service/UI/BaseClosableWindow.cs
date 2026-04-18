using System;
using UnityEngine.PlayerLoop;

namespace Service.UI
{
    public abstract class BaseClosableWindow : BaseWindow
    {
        protected abstract Type WindowType { get; }
        protected UIService UIService => Services.GetSerivce<UIService>();
        
        public void HideWindow()
        {
            Services.GetSerivce<UIService>().HideWindow(WindowType);
        }
    }
}