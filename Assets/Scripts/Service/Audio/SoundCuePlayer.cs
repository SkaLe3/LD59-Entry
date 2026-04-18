using Service.Audio.Interfaces;
using Service.Random;
using UnityEngine;

namespace Service.Audio
{
    public class SoundCuePlayer : ISoundCuePlayer
    {
        private readonly AudioSource _audioSource;

        public SoundCuePlayer(AudioSource audioSource)
        {
            _audioSource = audioSource;
        }

        public void PlaySoundCue(SoundCue array, float pitchMultiplier = 1)
        {
            int index = Services.GetService<RandomService>().Range(0, array.clips.Length);
            AudioClip clip = array.clips[index];

            float originalPitch = _audioSource.pitch;
            _audioSource.pitch = originalPitch *  pitchMultiplier;
            _audioSource.PlayOneShot(clip);
            
            _audioSource.pitch = originalPitch;
        }
    }
}