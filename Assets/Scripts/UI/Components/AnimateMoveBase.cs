using DG.Tweening;
using UnityEngine;

namespace UI.Components
{
    public class AnimateMoveBase : MonoBehaviour
    {
        [Header("Base Animation Settings")]
        [SerializeField] protected float duration = 0.15f;
        [SerializeField] protected Ease easeType = Ease.OutQuad;
        
        protected RectTransform rectTransform;
        protected Vector2 initialPosition;
        private Tweener _currentTween;

        protected void AnimateToOffset(float yOffset)
        {
            _currentTween?.Kill();
        
            Vector2 targetPos = initialPosition + new Vector2(-yOffset*1.2f, yOffset);
            _currentTween = rectTransform.DOAnchorPos(targetPos, duration)
                .SetEase(easeType)
                .SetUpdate(true);
        }

        private void OnDisable()
        {
            _currentTween?.Kill();
            rectTransform.anchoredPosition = initialPosition;
        }
    }
}