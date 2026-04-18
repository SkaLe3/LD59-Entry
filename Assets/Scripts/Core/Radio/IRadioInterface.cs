using UnityEngine;

namespace Core.Radio
{
    public interface IRadioInterface
    {
        public Vector3 GetAntenaLocation();
        public void OrientAntetaTo(Vector3 direction);

        public void NotifySignalStrength(float strength);
        public void NotifyConnectionAquired();
        public void NotifyConnectionLost();
    }
}