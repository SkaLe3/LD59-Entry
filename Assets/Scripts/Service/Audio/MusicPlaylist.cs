using System.Collections.Generic;
using UnityEngine;

namespace Service.Audio
{
    [CreateAssetMenu(fileName = "NewMusicPlaylist", menuName = "Scriptable/Audio/Music Playlist")]
    public class MusicPlaylist : ScriptableObject
    {
        public List<AudioClip> tracks;
    }
}