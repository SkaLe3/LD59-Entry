using UnityEngine;

namespace Core.Radio
{
    public interface IRadioInterface
    {
        public Vector3 GetAntenaLocation();
        public void OrientAntetaTo(Vector3 direction);

        public void NotifySignalStrength(float strength);
        public void NotifyConnectionAquired(IRadioInterface from);
        public void NotifyConnectionLost();

        public bool IsAvailableAsReceiver();
        public bool IsAvailableAsEmitter();
        public ETowerType GetTowerType();

        public bool IsHubTower();
        
        public void SetType(ETowerType towerType);
    }
}