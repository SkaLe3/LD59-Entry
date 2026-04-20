using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tools
{
    public class GameStarter : MonoBehaviour
    {
        [MenuItem("Game/Start")]
        private static void StartGameFromSpecificScene()
        {
            string scenePath = "Assets/Scenes/BootScene.unity";
            EditorSceneManager.OpenScene(scenePath);
            EditorApplication.isPlaying = true;
        }
    }
}