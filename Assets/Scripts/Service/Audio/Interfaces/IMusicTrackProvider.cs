using UnityEngine;

namespace Service.Audio.Interfaces
{
    public interface IMusicTrackProvider
    {
        void SetPlaylist(MusicPlaylist playlist);
        AudioClip GetNextTrack();
    }
}