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
         private void Start()
         {
             _gameHUD = Service.Services.GetService<UIService>().GetWindow<MainWindow>().gameHUD;
             _randomService = Service.Services.GetService<RandomService>();

             for (int i = 0; i < _bigTowers.Count; i++)
             {
                 _bigTowers[i].SetType(ETowerType.Receiver);
             }
             
             int emitterIndex = _randomService.Range(0, _hubTowers.Count);
             _bigTowers[emitterIndex].SetType(ETowerType.Emitter);
         }
         
         private void Update()
         {
             foreach (var tower in _bigTowers)
             {
                 if (tower.IsAvailableAsReceiver() || tower.IsAvailableAsEmitter())
                 {
                     _gameHUD.AddTowerMarker(tower);
                 }
             }

             foreach (var tower in _hubTowers)
             {
                 if (tower.IsAvailableAsEmitter())
                 {
                     _gameHUD.AddTowerMarker(tower);
                 }
             }
         }
         
        
    }
}