using System;
using Service.Audio;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Core.Settings
{
    public class VolumeSettings : MonoBehaviour
    {
        [SerializeField] private AudioMixer myMixer;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        
        
        private void Start()
        {
            SetMusicVolume();
            SetSFXVolume();
        }

        public void SetMusicVolume()
        {
            float volume = musicSlider.value;
            float volumeInDb = Mathf.Log10(Mathf.Pow(volume, 3)) * 20;
            if (volume <= 0.0001f) volumeInDb = -80f;
            myMixer.SetFloat("VolumeMusic", volumeInDb);

        }

        public void SetSFXVolume()
        {
            float volume = sfxSlider.value;
            float volumeInDb = Mathf.Log10(volume) * 20;
            if (volume <= 0.0001f) volumeInDb = -80f;
            myMixer.SetFloat("VolumeSFX", volumeInDb);
        }
    }
}