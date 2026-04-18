using System.Collections.Generic;
using System.Linq;
using Service.Audio.Interfaces;
using Service.Random;
using UnityEngine;

namespace Service.Audio
{
    public class TrackProvider : IMusicTrackProvider
    {
        private readonly Queue<AudioClip> _queue;
        private List<AudioClip> _playlist;

        //private RandomService RandomService = Services.GetSerivce<RandomService>();
        
        public TrackProvider()
        {
            _queue = new Queue<AudioClip>();
        }

        public void SetPlaylist(MusicPlaylist playlist)
        {
            _playlist = new List<AudioClip>(playlist.tracks);
            _queue.Clear();
            Reshuffle();
        }

        public AudioClip GetNextTrack()
        {
            EnsureQueueHasTracks();
            return _queue.Dequeue();
        }

        private void EnsureQueueHasTracks()
        {
            if (_queue.Count <= 1)
                Reshuffle();
        }

        private void Reshuffle()
        {
            Services.GetService<RandomService>().Shuffle(_playlist);

            if (_queue.Count > 0 && _playlist.Count > 1 && _playlist[0] == _queue.Last())
            {
                int swapIndex = Services.GetService<RandomService>().Range(1, _playlist.Count);
                AudioClip temp = _playlist[swapIndex];
                _playlist[swapIndex] = _playlist[0];
                _playlist[0] = temp;
            }

            foreach (AudioClip track in _playlist)
                _queue.Enqueue(track);
        }
    }
}