using System;
using Service.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Components
{
    [RequireComponent(typeof(Button))]
    public class ButtonClickSound : MonoBehaviour, IPointerEnterHandler
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }
        
        private void OnEnable()
        {
            _button.onClick.AddListener(PlayClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(PlayClick);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button.interactable)
            {
                PlayHover();
            }
        }

        private void PlayClick()
        {
            Service.Services.GetService<AudioService>().PlaySound("uiclick");
        }

        private void PlayHover()
        {
            Service.Services.GetService<AudioService>().PlaySound("uihover");
        }
    }
}