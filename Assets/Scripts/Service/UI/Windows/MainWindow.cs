using System;
using System.Collections.Generic;
using UnityEngine;

namespace Service.UI.Windows
{
    public partial class MainWindow : BaseClosableWindow
    {
        protected override Type WindowType => typeof(MainWindow);
        
        [Header("SubViews")]
        [SerializeField] private Transform _subViewsParent;
        public List<SubWindow> subWindows;

        private bool _initialized = false;

        protected override void OnShow()
        {
            if (!_initialized)
            {
                Init();
                _initialized = true;
            }
        }

        protected override void OnHide()
        {
            
        }

        private void Init()
        {
            CloseAllSubWindows();
        }

        private void OpenSubWindow(SubWindow.SubWindowType type)
        {
            foreach (var window in subWindows)
            {
                if (window.Type == type)
                {
                    window.Show();
                }
                else
                {
                    window.Hide();
                }
            }
        }

        public void CloseAllSubWindows()
        {
            foreach (var window in subWindows)
            {
                window.Hide();
            }
        }
        
        #region Buttons functions
        
        // open subwindows
        
        #endregion
    }
}