namespace Service.Audio.Interfaces
{
    public interface IMusicPlayer
    {
        void SetMusicPlaylist(MusicPlaylist newMusicPlaylist);
        void Play();
        void Pause();
        void Stop();
        void Skip();
    }
}