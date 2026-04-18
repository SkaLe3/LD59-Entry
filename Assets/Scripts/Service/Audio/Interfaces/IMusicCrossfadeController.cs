using System;
using UnityEngine;

namespace Service.Audio.Interfaces
{
    public interface IMusicCrossfadeController
    {
        event Action<float> OnCrossfadeStarted;
        event Action<AudioSource> OnCrossfadeCompleted;

        void Play(AudioSource source);
        void StartCrossfade(AudioSource activeSource, AudioSource nextSource, float crossfadeDuration = 1f);
        void CancelCrossfade();
    }
}