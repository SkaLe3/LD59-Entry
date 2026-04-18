using System;
using System.Threading.Tasks;
using Service.Audio.Interfaces;
using Service.Random;
using UnityEngine;

namespace Service.Audio
{
    public class AudioService : BaseService
    {
        public override Type ServiceType => typeof(AudioService);
        
        [Header("Audio Mixer Groups")]
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup musicMixerGroup;
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup gameplayCueMixerGroup;
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup uiCueMixerGroup;
        
        [Header("Registry")]
        [SerializeField] private SoundCueRegistry soundCueRegistry;
        
        [Header("Spatial Audio Settings")] 
        [SerializeField] private SpatialAudioSettings SpatialAudiosettings;
        
        private IMusicPlayer _musicPlayer;
        private ISoundCuePlayer _gameplayCuePlayer;
        private ISoundCuePlayer _uiCuePlayer;
        
        private ISpatialSoundCuePlayer _spatialCuePlayer;
        
        protected override Task<bool> OnInit()
        {
            Initialize();
            return Task.FromResult(true);
        }

        private void Initialize()
        {
            AudioSource musicSourceA = CreateAudioSource("Music_Source_A", musicMixerGroup);
            AudioSource musicSourceB = CreateAudioSource("Music_Source_B", musicMixerGroup);
            AudioSource gameplayCueSource = CreateAudioSource("GameplayCue_Source", gameplayCueMixerGroup);
            AudioSource uiCueSource = CreateAudioSource("UICue_Source", uiCueMixerGroup);

            IMusicCrossfadeController crossfadeController = new CrossfadeController();
            IMusicTrackProvider trackProvider = new TrackProvider();

            _musicPlayer = new MusicPlayer(
                musicSourceA,
                musicSourceB,
                trackProvider,
                crossfadeController
            );

            _gameplayCuePlayer = new SoundCuePlayer(gameplayCueSource);
            _uiCuePlayer = new SoundCuePlayer(uiCueSource);

            _spatialCuePlayer =
                new SpatialSoundCuePlayer(transform, gameplayCueMixerGroup, SpatialAudiosettings);
        }
        
        private AudioSource CreateAudioSource(string sourceName, UnityEngine.Audio.AudioMixerGroup mixerGroup)
        {
            GameObject sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = mixerGroup;
            return source;
        }
        
        public void SetMusicPlaylist(MusicPlaylist newMusicPlaylist)
        {
            _musicPlayer.SetMusicPlaylist(newMusicPlaylist);
        }

        public void PlaySound(string key, float pitchMultiplier = 1f, SoundCueGroup group = SoundCueGroup.Gameplay)
        {
            SoundCue cue = soundCueRegistry.GetSoundCue(key);
            
            ISoundCuePlayer player = group == SoundCueGroup.UI ? _uiCuePlayer : _gameplayCuePlayer;
            player.PlaySoundCue(cue, pitchMultiplier);
        }
        
        public void PlaySoundAtLocation(string key, Vector3 position, float pitchMultiplier = 1f, SoundCueGroup group = SoundCueGroup.Gameplay)
        {
            SoundCue cue = soundCueRegistry.GetSoundCue(key);
            _spatialCuePlayer.PlaySoundCueAtLocation(cue, position, pitchMultiplier);
        }

        public void PlayMusic()
        {
            _musicPlayer.Play();
        }

        public void PauseMusic()
        {
            _musicPlayer.Pause();
        }

        public void StopMusic()
        {
            _musicPlayer.Stop();
        }

        public void SkipMusic()
        {
            _musicPlayer.Skip();
        }
    }
}