using System;
using System.Threading.Tasks;
using Service.Audio;

namespace Service.Game
{
    public class GameService : BaseService
    {
        public override Type ServiceType => typeof(GameService);
        public MusicPlaylist musicPlaylist;
        
        private static AudioService AudioService => Services.GetService<AudioService>();

        protected override async Task<bool> OnInit()
        {
            return true;
        }

        public void StartGame()
        {
            GameSetter.SetGame();
            AudioService.SetMusicPlaylist(musicPlaylist);
            AudioService.PlayMusic();
        }

        public void ExitGame()
        {
            
        }
        
    }
}