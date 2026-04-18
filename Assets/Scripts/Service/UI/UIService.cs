using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Service.UI
{
    public class UIService : BaseService
    {
        public override Type ServiceType => typeof(UIService);

        [SerializeField] private List<BaseWindow> windowContainer;

        private Dictionary<Type, BaseWindow> windowsMap = new Dictionary<Type, BaseWindow>();

        protected override async Task<bool> OnInit()
        {
            InitMap();
            return true;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            GetAllWindows();
        }

        private void GetAllWindows()
        {
            windowContainer = GetAllChildrenOfType<BaseWindow>();
        }

        public List<T> GetAllChildrenOfType<T>() where T : Component
        {
            List<T> results = new List<T>();

            // Iterate over all child GameObjects
            foreach (Transform child in transform)
            {
                // Check if the child GameObject has a component of type T
                T component = child.GetComponent<T>();
                if (component != null)
                {
                    results.Add(component);
                }
            }

            return results;
        }

        private void InitMap()
        {
            foreach (BaseWindow window in windowContainer)
            {
                windowsMap.Add(window.GetType(), window);
            }
        }

        public T ShowWindow<T>() where T : BaseWindow
        {
            T window = GetWindow<T>();
            if (window != null)
            {
                SetTopOrder(window);
                window.Show();
            }

            return window;
        }
        
        
        public T HideWindow<T>() where T : BaseWindow
        {
            T window = GetWindow<T>();
            if (window != null)
            {
                SetLowestOrder(window);
                window.Hide();
            }

            return window;
        }
        
        public BaseWindow HideWindow(Type t)
        {
            BaseWindow window = GetWindow(t);
            if (window != null)
            {
                SetLowestOrder(window);
                window.Hide();
            }

            return window;
        }
        
        public T GetWindow<T>() where T : BaseWindow
        {
            if (windowsMap.ContainsKey(typeof(T)))
            {
                return (T)windowsMap[typeof(T)];
            }

            return null;
        }
        
        public BaseWindow GetWindow(Type t)
        {
            return windowsMap.ContainsKey(t) ? windowsMap[t] : null;
        }

        private void SetTopOrder(BaseWindow window)
        {
            window.Order = windowContainer.Select(baseWindow => baseWindow.Order).Prepend(-1).Max() + 1;
        }

        private void SetLowestOrder(BaseWindow window)
        {
            window.Order = -1;
        }
    }
}