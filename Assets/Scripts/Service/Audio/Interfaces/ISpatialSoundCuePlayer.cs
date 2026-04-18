using UnityEngine;

namespace Service.Audio.Interfaces
{
    public interface ISpatialSoundCuePlayer
    {
        void PlaySoundCueAtLocation(SoundCue cue, Vector3 position, float  pitchMultiplier = 1);
    }
}