using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Service.Game;
using UnityEngine;

namespace Service.Main
{
    public class MainService : BaseService
    {
        public override Type ServiceType => typeof(MainService);

        private GameService GameService => Services.GetService<GameService>();

        protected override async Task<bool> OnInit()
        {
            return true;
        }

        public void EntryPoint()
        {
            // Do Inits here

            // Pass game data
            GameService.StartGame();
        }
    }
}