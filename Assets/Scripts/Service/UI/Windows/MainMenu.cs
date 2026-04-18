using System;
using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace Service.UI.Windows
{
    public class MainMenu : BaseClosableWindow
    {
        protected override Type WindowType => typeof(MainMenu);

        [SerializeField] private GameObject briefingMenu;
        
        private UIService UIService => Services.GetService<UIService>();

        protected override void OnHide()
        {
            
        }

        protected override void OnShow()
        {
            briefingMenu.SetActive(false);
        }

        public void Play()
        {
            HideWindow();

            SceneLoader.LoadScene(2, () => { UIService.ShowWindow<MainWindow>();});
        }

        public void Briefing()
        {
            briefingMenu.SetActive(!briefingMenu.activeInHierarchy);
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