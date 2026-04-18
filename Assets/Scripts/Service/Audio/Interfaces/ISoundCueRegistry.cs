namespace Service.Audio.Interfaces
{
    public interface ISoundCueRegistry
    {
        SoundCue GetSoundCue(string key);
    }
}