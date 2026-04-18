using Service.UI;
using Service.UI.Windows;

namespace Service.Game
{
    public static class GameSetter
    {
        private static UIService UIService => Services.GetSerivce<UIService>();

        public static void SetGame()
        {
            UIService.HideWindow<LoadingScreen>();
            UIService.ShowWindow<MainMenu>();
        }
    }
}