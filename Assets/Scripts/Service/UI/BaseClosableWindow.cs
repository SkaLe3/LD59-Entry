using System;
using UnityEngine.PlayerLoop;

namespace Service.UI
{
    public abstract class BaseClosableWindow : BaseWindow
    {
        protected abstract Type WindowType { get; }
        protected UIService UIService => Services.GetService<UIService>();
        
        public void HideWindow()
        {
            Services.GetService<UIService>().HideWindow(WindowType);
        }
    }
}