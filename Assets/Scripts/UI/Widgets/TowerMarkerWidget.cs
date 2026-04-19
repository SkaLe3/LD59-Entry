using System;
using Core.Radio;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Widgets
{
    public class TowerMarkerWidget : MonoBehaviour
    {
        [SerializeField] private GameObject tileObject;
        [SerializeField] private Image iconImage;
        [SerializeField] private Sprite emitterSprite;
		[SerializeField] private Sprite receiverSprite;
        [SerializeField] private float edgeMargin = 200f;
        
        private ETowerType _towerType;

        private Camera _cam;
        private RectTransform canvasRect;
        private Vector3 targetWorldLocation;
        private RectTransform markerTransform;
        
        private void Awake()
        {
            
        }

        private void Start()
        {
            _cam = FindAnyObjectByType<Camera>();
            canvasRect = transform.parent.GetComponent<RectTransform>();
            markerTransform = GetComponent<RectTransform>();
        }

        public void SetTarget(Vector3 target)
        {
            targetWorldLocation = target;
        }
        
        public void SetMarkerType(ETowerType towerType)
        {
            _towerType = towerType;
            if (_towerType == ETowerType.Emitter)
            {
                iconImage.sprite = emitterSprite;
            }
            else if (_towerType == ETowerType.Receiver)
            {
                iconImage.sprite = receiverSprite;
            }
        }

        public void SetMarkerSize(bool isHub)
        {
            transform.localScale = isHub ? new Vector3(0.8f, 0.8f, 0.8f) : Vector3.one;
        }

        public ETowerType GetTowerType()
        {
            return _towerType;
        }
        
        private void Update()
        {
            if ( _cam == null)
                return;

            Vector3 viewportPos = _cam.WorldToViewportPoint(targetWorldLocation);

            // Check if target is in front of camera
            bool isBehind = viewportPos.z < 0f;

            // Check if inside screen bounds
            bool isInside =
                viewportPos.x >= 0f && viewportPos.x <= 1f &&
                viewportPos.y >= 0f && viewportPos.y <= 1f;

            // If target is visible → hide marker
            if (isInside)
            {

                tileObject.SetActive(false);
                return;
            }

            tileObject.SetActive(true);
            // If behind camera, flip direction
            if (isBehind)
            {
                //viewportPos.x = 1f - viewportPos.x;
                //viewportPos.y = 1f - viewportPos.y;
            }

            // Clamp to screen edges with margin
            float minX = edgeMargin / canvasRect.rect.width;
            float maxX = 1f - minX;

            float minY = edgeMargin / canvasRect.rect.height;
            float maxY = 1f - minY;

            viewportPos.x = Mathf.Clamp(viewportPos.x, minX, maxX);
            viewportPos.y = Mathf.Clamp(viewportPos.y, minY, maxY);

            // Convert viewport → local UI position
            Vector2 screenPos = _cam.ViewportToScreenPoint(viewportPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                null,
                out Vector2 localPos
            );

            markerTransform.localPosition = localPos;
            
            //Vector2 dir = (viewportPos - new Vector3(0.5f, 0.5f)).normalized;
            //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            //markerTransform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}