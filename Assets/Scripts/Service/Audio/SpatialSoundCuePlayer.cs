using System.Collections.Generic;
using Service.Audio.Interfaces;
using Service.Random;
using UnityEngine;

namespace Service.Audio
{
    public class SpatialSoundCuePlayer : ISpatialSoundCuePlayer
    {
        private readonly Transform _parentTransform;
        private readonly UnityEngine.Audio.AudioMixerGroup _mixerGroup;
        private readonly SpatialAudioSettings _settings;
        
        private readonly Queue<SpatialAudioSourceWrapper> _availablePool;
        private readonly List<SpatialAudioSourceWrapper> _activeWrappers;
        
        private RandomService RandomService = Services.GetSerivce<RandomService>();
        
        private const int INITIAL_POOL_SIZE = 10;
        private const int MAX_POOL_SIZE = 50;

        public SpatialSoundCuePlayer(
            Transform parentTransform,
            UnityEngine.Audio.AudioMixerGroup mixerGroup,
            SpatialAudioSettings settings)
        {
            _parentTransform = parentTransform;
            _mixerGroup = mixerGroup;
            _settings = settings;
            
            _availablePool = new Queue<SpatialAudioSourceWrapper>(INITIAL_POOL_SIZE);
            _activeWrappers = new List<SpatialAudioSourceWrapper>(INITIAL_POOL_SIZE);
            
            InitializePool();
        }

        public void PlaySoundCueAtLocation(SoundCue cue, Vector3 position, float pitchMultiplier = 1f)
        {
            if (cue == null || cue.clips == null || cue.clips.Length == 0)
            {
                Debug.LogWarning("SpatialSoundCuePlayer: Invalid sound cue provided.");
                return;
            }

            int index = RandomService.Range(0, cue.clips.Length);
            AudioClip clip = cue.clips[index];

            if (clip == null)
            {
                Debug.LogWarning("SpatialSoundCuePlayer: Null audio clip in sound cue.");
                return;
            }

            SpatialAudioSourceWrapper wrapper = GetOrCreateWrapper();
            wrapper.PlayAtLocation(clip, position, pitchMultiplier);
            _activeWrappers.Add(wrapper);
        }

        private void InitializePool()
        {
            for (int i = 0; i < INITIAL_POOL_SIZE; i++)
            {
                SpatialAudioSourceWrapper wrapper = CreateNewWrapper();
                _availablePool.Enqueue(wrapper);
            }
        }

        private SpatialAudioSourceWrapper GetOrCreateWrapper()
        {
            CleanupFinishedWrappers();

            if (_availablePool.Count > 0)
            {
                return _availablePool.Dequeue();
            }

            if (_activeWrappers.Count < MAX_POOL_SIZE)
            {
                return CreateNewWrapper();
            }

            // Pool is full, reuse oldest active wrapper
            Debug.LogWarning("SpatialSoundCuePlayer: Max pool size reached. Reusing oldest source.");
            SpatialAudioSourceWrapper oldest = _activeWrappers[0];
            _activeWrappers.RemoveAt(0);
            oldest.Stop();
            return oldest;
        }

        private void CleanupFinishedWrappers()
        {
            for (int i = _activeWrappers.Count - 1; i >= 0; i--)
            {
                if (!_activeWrappers[i].IsPlaying)
                {
                    SpatialAudioSourceWrapper wrapper = _activeWrappers[i];
                    _activeWrappers.RemoveAt(i);
                    _availablePool.Enqueue(wrapper);
                }
            }
        }

        private SpatialAudioSourceWrapper CreateNewWrapper()
        {
            GameObject sourceObject = new GameObject($"SpatialAudioSource_{_availablePool.Count + _activeWrappers.Count}");
            sourceObject.transform.SetParent(_parentTransform);
            
            AudioSource audioSource = sourceObject.AddComponent<AudioSource>();
            ConfigureAudioSource(audioSource);
            
            return new SpatialAudioSourceWrapper(audioSource);
        }

        private void ConfigureAudioSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.outputAudioMixerGroup = _mixerGroup;
            
            // Spatial audio settings
            source.spatialBlend = _settings != null ? _settings.spatialBlend : 1f;
            source.dopplerLevel = _settings != null ? _settings.dopplerLevel : 1f;
            source.spread = _settings != null ? _settings.spread : 0f;
            source.minDistance = _settings != null ? _settings.minDistance : 1f;
            source.maxDistance = _settings != null ? _settings.maxDistance : 500f;
            source.rolloffMode = _settings != null ? _settings.rolloffMode : AudioRolloffMode.Logarithmic;
        }

        private class SpatialAudioSourceWrapper
        {
            private readonly AudioSource _audioSource;
            
            public bool IsPlaying => _audioSource.isPlaying;

            public SpatialAudioSourceWrapper(AudioSource audioSource)
            {
                _audioSource = audioSource;
            }

            public void PlayAtLocation(AudioClip clip, Vector3 position, float pitchMultiplier = 1f)
            {
                _audioSource.transform.position = position;
                _audioSource.clip = clip;
                _audioSource.pitch = pitchMultiplier;
                _audioSource.Play();
            }

            public void Stop()
            {
                _audioSource.Stop();
                _audioSource.clip = null;
                _audioSource.pitch = 1f;
            }
        }
    }
}