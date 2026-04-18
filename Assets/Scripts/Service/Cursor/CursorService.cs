using System;
using System.Threading.Tasks;
using Service;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Services.Cursor
{
    public class CursorService : BaseService
    {
        public enum CursorType { Default, Hover, Hold }
        
        public override Type ServiceType => typeof(CursorService);

        [Header("Styles")]
        [SerializeField] private CursorStyle defaultStyle;
        [SerializeField] private CursorStyle hoverStyle;
        [SerializeField] private CursorStyle holdStyle;

        [Header("Input References")]
        [SerializeField] private InputActionReference clickAction;
        protected override async Task<bool> OnInit()
        {
            clickAction.action.started += OnClickStarted;
            clickAction.action.canceled += OnClickCanceled;
            clickAction.action.Enable();
            
            SetCursor(CursorType.Default);
            return true;
        }
        
        private void OnClickStarted(InputAction.CallbackContext context)
        {
            SetCursor(CursorType.Hold);
        }

        private void OnClickCanceled(InputAction.CallbackContext context)
        {
            SetCursor(CursorType.Default);
        }

        public void SetCursor(CursorType type)
        {
            CursorStyle styleToApply = type switch
            {
                CursorType.Default => defaultStyle,
                CursorType.Hover   => hoverStyle,
                CursorType.Hold    => holdStyle,
                _                  => defaultStyle
            };
            
            styleToApply?.Apply();
        }
        
        private void OnDestroy()
        {
            if (clickAction == null) return;
            
            clickAction.action.started -= OnClickStarted;
            clickAction.action.canceled -= OnClickCanceled;
        }
    }
}