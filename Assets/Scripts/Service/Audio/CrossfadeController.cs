using System;
using DG.Tweening;
using Service.Audio.Interfaces;
using UnityEngine;

namespace Service.Audio
{
    public class CrossfadeController : IMusicCrossfadeController
    {
        private Tween _fadeTween;
        private Tween _scheduleTween;
        private const float CROSSFADE_DURATION = 2f;

        public event Action<float> OnCrossfadeStarted;
        public event Action<AudioSource> OnCrossfadeCompleted;

        public void Play(AudioSource source)
        {
            source.UnPause();
            if (!source.isPlaying)
                source.Play();

            ScheduleCrossfadeIfNeeded(source);
        }

        public void StartCrossfade(AudioSource activeSource, AudioSource nextSource, float crossfadeDuration = CROSSFADE_DURATION)
        {
            nextSource.Play();
            CrossFade(activeSource, nextSource, crossfadeDuration);
        }

        public void CancelCrossfade()
        {
            _fadeTween?.Kill();
            _scheduleTween?.Kill();
        }

        private void ScheduleCrossfadeIfNeeded(AudioSource source)
        {
            float remainingTime = source.clip.length - source.time;
            if (remainingTime <= CROSSFADE_DURATION)
            {
                OnCrossfadeStarted?.Invoke(Mathf.Min(CROSSFADE_DURATION, remainingTime));
                return;
            }

            _scheduleTween = DOVirtual.DelayedCall(
                remainingTime - CROSSFADE_DURATION,
                () => OnCrossfadeStarted?.Invoke(CROSSFADE_DURATION));
        }

        private void CrossFade(AudioSource from, AudioSource to, float duration)
        {
            _fadeTween = DOTween.Sequence()
                .Join(from.DOFade(0f, duration))
                .Join(to.DOFade(1f, duration))
                .OnComplete(() => OnCrossfadeCompleted?.Invoke(from));
        }
    }
}