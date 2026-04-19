using System;
using System.Collections.Generic;
using Service.Random;
using Service.UI;
using Service.UI.Windows;
using UnityEngine;

namespace Core.Radio
{
    public class NetworkManager : MonoBehaviour
    {
         [SerializeField] private List<RadioTower> _bigTowers;
         [SerializeField] private List<RadioTower> _hubTowers;

         private HUD _gameHUD;
         private RandomService _randomService;

         private bool networkConnected = false;

         public Action OnNetworkConnected;

         public int connectionsLost;

         private float timeElapsed;
         
         private void Start()
         {
             _gameHUD = Service.Services.GetService<UIService>().GetWindow<MainWindow>().gameHUD;
             _randomService = Service.Services.GetService<RandomService>();

             int emitterIndex = _randomService.Range(0, _bigTowers.Count);
             _bigTowers[emitterIndex].isSignalOrigin = true;
         }
         
         private void Update()
         {
             timeElapsed += Time.deltaTime;
             
             if (networkConnected) return;
             
             bool allTowersConnected = true;
             foreach (var tower in _bigTowers)
             {
                 _gameHUD.AddTowerMarker(tower);
                 allTowersConnected &= tower.IsAvailableAsEmitter;
             }

             foreach (var tower in _hubTowers)
             {
                 if (tower.IsAvailableAsEmitter)
                 {
                     _gameHUD.AddTowerMarker(tower);
                 }
                 else
                 {
                     _gameHUD.RemoveTowerMarker(tower);
                 }
             }

             if (allTowersConnected)
             {
                 networkConnected = true;
                 OnNetworkConnected?.Invoke();
                 EndGame();
             }
         }

         public void NotifyConnectionLost()
         {
             connectionsLost++;
         }

         private void EndGame()
         {
             _gameHUD.ShowGameOverScreen(timeElapsed, connectionsLost);
         }
        
    }
}