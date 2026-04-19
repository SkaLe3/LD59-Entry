using UnityEngine;

namespace Core.Radio
{
    public interface IRadioInterface
    {
        public Vector3 GetAntenaLocation();

        public void NotifyConnectionEstablished(IRadioInterface from);
        public void NotifyConnectionLost();
        public bool IsHubTower();
    }
}