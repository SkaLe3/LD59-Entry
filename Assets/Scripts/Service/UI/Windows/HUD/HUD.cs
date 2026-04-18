using UnityEngine;
using UnityEngine.UI;

namespace Service.UI.Windows
{
    public class HUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider signalIndicator;

        public void SetSignalStrength(float strength)
        {
            signalIndicator.value = strength;
        }
    }
}