using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Components
{
    public class AnimateButton : AnimateMoveBase, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Animation Settings")]
        [SerializeField] private float hoverYOffset = -4f;
        [SerializeField] private float pressedYOffset = -12f;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            initialPosition = rectTransform.anchoredPosition;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            AnimateToOffset(hoverYOffset);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            AnimateToOffset(0);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            AnimateToOffset(pressedYOffset);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            float target = eventData.hovered.Contains(gameObject) ? hoverYOffset : 0f;
            AnimateToOffset(target);
        }
    }
}