using Service.Audio;
using Service.UI;
using Service.UI.Windows;

namespace Service.Game
{
    public static class GameSetter
    {
        private static UIService UIService => Services.GetService<UIService>();


        public static void SetGame()
        {
            UIService.HideWindow<LoadingScreen>();
            UIService.HideWindow<MainWindow>();
            UIService.ShowWindow<MainMenu>();
            
        }
    }
}