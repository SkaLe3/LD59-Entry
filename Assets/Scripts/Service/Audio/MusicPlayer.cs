using Service.Audio.Interfaces;
using UnityEngine;

namespace Service.Audio
{
    public class MusicPlayer : IMusicPlayer
    {
        private readonly IMusicCrossfadeController _crossfadeController;
        private readonly IMusicTrackProvider _trackProvider;

        private readonly AudioSource _sourceA;
        private readonly AudioSource _sourceB;

        private AudioSource _activeSource;
        private AudioSource _nextSource;

        private float _sourceAVolumeBeforePause = 1f;
        private float _sourceBVolumeBeforePause = 1f;

        private bool _isPaused;

        public MusicPlayer(
            AudioSource musicSourceA,
            AudioSource musicSourceB,
            IMusicTrackProvider trackProvider,
            IMusicCrossfadeController crossfadeController)
        {
            _sourceA = musicSourceA;
            _sourceB = musicSourceB;
            _trackProvider = trackProvider;
            _crossfadeController = crossfadeController;

            _sourceA.gameObject.name = "Music_Source_A_Runtime";
            _sourceB.gameObject.name = "Music_Source_B_Runtime";

            _activeSource = _sourceA;
            _nextSource = _sourceB;

            _crossfadeController.OnCrossfadeStarted += HandleCrossfadeInitiation;
            _crossfadeController.OnCrossfadeCompleted += HandleCrossfadeCompleted;
        }

        public void SetMusicPlaylist(MusicPlaylist newMusicPlaylist)
        {
            _trackProvider.SetPlaylist(newMusicPlaylist);

            if (_activeSource.isPlaying)
                Skip();
        }

        public void Play()
        {
            if (TryResumePaused()) return;

            if (!_activeSource.isPlaying)
                StartPlayback();
        }

        public void Pause()
        {
            if (_isPaused) return;

            CacheSourcesVolumes();
            _isPaused = true;

            _crossfadeController.CancelCrossfade();
            _activeSource.Pause();
            _nextSource.Pause();
        }

        public void Skip()
        {
            if (_nextSource.isPlaying)
                SwapSourcesReferences();

            StopAndClear(_nextSource);
            _nextSource.clip = _trackProvider.GetNextTrack();
            _crossfadeController.CancelCrossfade();
            _crossfadeController.StartCrossfade(_activeSource, _nextSource);
        }

        public void Stop()
        {
            _crossfadeController.CancelCrossfade();
            StopAndClear(_activeSource);
            StopAndClear(_nextSource);
            _isPaused = false;
        }

        private void CacheSourcesVolumes()
        {
            _sourceAVolumeBeforePause = _sourceA.volume;
            _sourceBVolumeBeforePause = _sourceB.volume;
        }

        private void RestoreSourcesVolumes()
        {
            _sourceA.volume = _sourceAVolumeBeforePause;
            _sourceB.volume = _sourceBVolumeBeforePause;
        }

        private void StopAndClear(AudioSource src)
        {
            src.Stop();
            src.clip = null;
        }

        private void StartPlayback()
        {
            EnsureSourceHasTrack(_activeSource);

            _activeSource.volume = 1f;
            _nextSource.volume = 0f;
            _crossfadeController.Play(_activeSource);
        }

        private void EnsureSourceHasTrack(AudioSource source)
        {
            if (source.clip == null)
                source.clip = _trackProvider.GetNextTrack();
        }

        private bool TryResumePaused()
        {
            if (!_isPaused) return false;
            _isPaused = false;

            RestoreSourcesVolumes();

            _crossfadeController.Play(_activeSource);
            _nextSource.UnPause();
            return true;
        }

        private void SwapSourcesReferences()
        {
            AudioSource tempSource = _activeSource;
            _activeSource = _nextSource;
            _nextSource = tempSource;
        }

        private void HandleCrossfadeInitiation(float remainingCrossfadeDuration)
        {
            EnsureSourceHasTrack(_nextSource);
            _crossfadeController.StartCrossfade(_activeSource, _nextSource, remainingCrossfadeDuration);
        }

        private void HandleCrossfadeCompleted(AudioSource from)
        {
            StopAndClear(from);
            SwapSourcesReferences();
            StartPlayback();
        }
    }
}