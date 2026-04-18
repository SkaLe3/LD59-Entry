using System;
using System.Threading.Tasks;

namespace Service.Game
{
    public class GameService : BaseService
    {
        public override Type ServiceType => typeof(GameService);

        protected override async Task<bool> OnInit()
        {
            return true;
        }

        public void StartGame()
        {
            GameSetter.SetGame();
        }

        public void ExitGame()
        {
            
        }
        
    }
}