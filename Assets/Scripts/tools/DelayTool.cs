
using System;
using System.Collections;
using UnityEngine;

namespace Tools
{
    /// <summary>
    /// Simple tool for quick delay action based on coroutine.
    /// Do not use for regular code. Its for quick-fixing or similar.
    /// </summary>
    public class DelayTool : MonoBehaviour
    {
        /// <summary>
        /// Create new gameObject with delay component.
        /// It will destroy after action completes.
        /// </summary>
        /// <param name="time">time in seconds before action will start</param>
        /// <param name="action">action, that start invoke after delay</param>
        public static void NewDelay(float time, Action action)
        {
            DelayTool delayComponent = new GameObject("DelayTool").AddComponent<DelayTool>();
            delayComponent.StartCoroutine(delayComponent.DelayCoroutine(time, action));
        }

        private IEnumerator DelayCoroutine(float time, Action action)
        {
            yield return new WaitForSecondsRealtime(time);
            action?.Invoke();
            Destroy(gameObject);
        }
    }
}