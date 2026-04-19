using System.Collections.Generic;
using Core.Radio;
using TMPro;
using Tools;
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
        
        [Header("Game over")]
        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text connectionsLostText;
        
        [Header("Prefabs")]
        [SerializeField] private TowerMarkerWidget towerMarkerWidgetPrefab;
        

        private Dictionary<IRadioInterface, TowerMarkerWidget> towerMarkerWidgets = new Dictionary<IRadioInterface, TowerMarkerWidget>();

        private void Start()
        {
            HideConnectPrompt();
            HideShutdownPrompt();
            HideGameOverScreen();
        }
        
        public void SetSignalStrength(float strength)
        {
            signalIndicator.value = strength;
        }

        public void AddTowerMarker(IRadioInterface tower)
        {
            if (!towerMarkerWidgets.ContainsKey(tower))
            {
                RadioTower radioTower = tower as RadioTower;
                ETowerType towerType = radioTower.IsAvailableAsReceiver ? ETowerType.Receiver : ETowerType.Emitter;
                
                if ((towerType == ETowerType.Emitter || !tower.IsHubTower()))
                {
                    TowerMarkerWidget newMarker = Instantiate(towerMarkerWidgetPrefab, transform);
                    newMarker.SetTarget(tower.GetAntenaLocation());
                    towerMarkerWidgets.Add(tower, newMarker);
                    newMarker.SetMarkerType(towerType);
                    newMarker.SetMarkerSize(tower.IsHubTower());
                }
            }
            else
            {
                TowerMarkerWidget marker = towerMarkerWidgets[tower];
                RadioTower radioTower = tower as RadioTower;
                ETowerType towerType = radioTower.IsAvailableAsReceiver ? ETowerType.Receiver : ETowerType.Emitter;
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
                Destroy(towerMarkerWidgets[tower]);
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

        public void ShowGameOverScreen(float time, int connections)
        {
            HideConnectPrompt();
            HideShutdownPrompt();
            foreach (var widget in towerMarkerWidgets)
            {
                Destroy(widget.Value.gameObject);
            }
            towerMarkerWidgets.Clear();
            
            gameOverScreen.SetActive(true);

            connectionsLostText.text = connections.ToString();
            
            int minutes = Mathf.FloorToInt(time / 60); 
            int seconds = Mathf.FloorToInt(time % 60);
            
            timeText.text = string.Format("{0}:{1:00}", minutes, seconds);
        }

        public void HideGameOverScreen()
        {
            gameOverScreen.SetActive(false);
        }

        public void MainMenu()
        {
            Service.Services.GetService<UIService>().GetWindow<MainWindow>().HideWindow();
            HideGameOverScreen();
            
            SceneLoader.LoadScene(1, () => { Service.Services.GetService<UIService>().ShowWindow<MainMenu>();});
        }
    }
}