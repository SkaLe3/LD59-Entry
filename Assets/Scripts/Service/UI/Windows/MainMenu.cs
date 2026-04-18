using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Service.UI.Windows
{
    public class MainMenu : BaseClosableWindow
    {
        protected override Type WindowType => typeof(MainMenu);
        
        private UIService UIService => Services.GetSerivce<UIService>();

        protected override void OnHide()
        {
            
        }

        protected override void OnShow()
        {
            
        }

        public void Play()
        {
            HideWindow();

            UIService.ShowWindow<MainWindow>();
            // Start gameplay music
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            Debug.Log("Exit Game");
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
#else
            Application.Quit();
#endif
        }
    }
}