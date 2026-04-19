using System.Collections.Generic;
using Core.Radio;
using TMPro;
using UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace Service.UI.Windows
{
    public class HUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider signalIndicator;

        [SerializeField] private GameObject connectPrompt;
        [SerializeField] private TMP_Text connectPromptText;
        
        [SerializeField] private GameObject shutdownPrompt;
        [SerializeField] private TMP_Text shutdownPromptText;
        
        [Header("Prefabs")]
        [SerializeField] private TowerMarkerWidget towerMarkerWidgetPrefab;

        private Dictionary<IRadioInterface, TowerMarkerWidget> towerMarkerWidgets = new Dictionary<IRadioInterface, TowerMarkerWidget>();

        private void Start()
        {
            HideConnectPrompt();
            HideShutdownPrompt();
        }
        
        public void SetSignalStrength(float strength)
        {
            signalIndicator.value = strength;
        }

        public void AddTowerMarker(IRadioInterface tower)
        {
            if (!towerMarkerWidgets.ContainsKey(tower))
            {
                TowerMarkerWidget newMarker = Instantiate(towerMarkerWidgetPrefab, transform);
                newMarker.SetTarget(tower.GetAntenaLocation());
                towerMarkerWidgets.Add(tower, newMarker);
                ETowerType towerType = tower.GetTowerType();
                newMarker.SetMarkerType(towerType);
                newMarker.SetMarkerSize(tower.IsHubTower());
            }
            else
            {
                TowerMarkerWidget marker = towerMarkerWidgets[tower];
                ETowerType towerType = tower.GetTowerType();
                if (marker.GetTowerType() != towerType)
                {
                    marker.SetMarkerType(towerType);
                }

                if (marker.GetTowerType() == ETowerType.Receiver && tower.IsHubTower())
                {
                    RemoveTowerMarker(tower);
                    Destroy(marker);
                }
            }
        }

        public void RemoveTowerMarker(IRadioInterface tower)
        {
            if (towerMarkerWidgets.ContainsKey(tower))
            {
                towerMarkerWidgets.Remove(tower);
            }
        }

        public void ShowConnectPrompt()
        {
            connectPrompt.SetActive(true);
            connectPromptText.text = "Connect";
        }

        public void ShowDisconnectPrompt()
        {
            connectPrompt.SetActive(true);
            connectPromptText.text = "Disconnect";
        }

        public void HideConnectPrompt()
        {
            connectPrompt.SetActive(false);
        }

        public void ShowShutdownPrompt()
        {
            shutdownPrompt.SetActive(true);
        }

        public void HideShutdownPrompt()
        {
            shutdownPrompt.SetActive(false);
        }
    }
}