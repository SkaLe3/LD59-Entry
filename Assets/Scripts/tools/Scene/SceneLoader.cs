using System;

namespace Tools
{
    /// <summary>
    /// Wrapper tool for loading scenes and setting post-loading actions
    /// </summary>
    public static class SceneLoader
    {
        /// <summary>
        /// Asynchronously loads the scene by build index
        /// </summary>
        /// <param name="index">scene build index</param>
        public static void LoadScene(int index)
        {
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(index);
        }

        /// <summary>
        /// Asynchronously loads the scene by the build index and then performs the assigned action
        /// </summary>
        /// <param name="index">scene build index</param>
        /// <param name="afterLoadingCallback">The action to be performed after loading the scene</param>
        public static void LoadScene(int index, Action afterLoadingCallback)
        {
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(index).completed += (x) => { afterLoadingCallback?.Invoke(); };
        }

        /// <summary>
        /// Waits for the specified number of seconds and then asynchronously loads the scene at the assembly index
        /// </summary>
        /// <param name="index">scene build index</param>
        /// <param name="delayTime">delay</param>
        public static void LoadScene(int index, float delayTime)
        {
            DelayTool.NewDelay(delayTime, () =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(index);
            });
        }

        /// <summary>
        /// Waits for the specified number of seconds and then asynchronously loads the scene at the assembly index and then performs the assigned action
        /// </summary>
        /// <param name="index">scene build index</param>
        /// <param name="delayTime">delay</param>
        /// <param name="afterLoadingCallback">The action to be performed after loading the scene</param>
        public static void LoadScene(int index, float delayTime, Action afterLoadingCallback)
        {
            DelayTool.NewDelay(delayTime, () =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(index).completed += (x) => { afterLoadingCallback?.Invoke(); };
            });
        }
    }
}