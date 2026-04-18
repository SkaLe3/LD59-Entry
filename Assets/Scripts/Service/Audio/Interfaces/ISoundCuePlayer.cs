namespace Service.Audio.Interfaces
{
    public interface ISoundCuePlayer
    {
        void PlaySoundCue(SoundCue so, float pitchMultiplier = 1);
    }
}