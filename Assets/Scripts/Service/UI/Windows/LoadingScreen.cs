using System;
using UnityEngine.PlayerLoop;

namespace Service.UI.Windows
{
    public class LoadingScreen : BaseClosableWindow
    {
        protected override Type WindowType => typeof(LoadingScreen);
        
        private UIService UIService => Services.GetService<UIService>();

        protected override void OnHide()
        {
            
        }

        protected override void OnShow()
        {
            
        }
    }
}