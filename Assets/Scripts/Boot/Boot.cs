using System.Collections;
using System.Collections.Generic;
using Service;
using Service.Main;
using Service.UI;
using Service.UI.Windows;
using Tools;
using UnityEngine;

namespace Boot
{
    public class Boot : MonoBehaviour
    {
        [SerializeField] private BootSettings bootSetting;
        
        #region Unity lifecycle

        private void Start()
        {
            CreateServices();
        }
        
        #endregion
        
        #region Private

        private async void CreateServices()
        {
            var baseServices = new List<BaseService>();

            var serviceGameObject = new GameObject("Services");
            DontDestroyOnLoad(serviceGameObject);

            foreach (var service in bootSetting.Services)
            {
                baseServices.Add(Instantiate(service, serviceGameObject.transform));
            }

            await Service.Services.InitAppWith(baseServices);
            StartCoroutine(Loading());
        }

        private IEnumerator Loading()
        {
            Service.Services.GetService<UIService>().ShowWindow<LoadingScreen>();
            yield return new WaitForSeconds(bootSetting.BootTime);
            if (bootSetting.NextSceneIndex == 0)
            {
                Debug.Log("Next scene after boot is null, please, check the boot settings");
                yield break;
            }

            var mainService = Service.Services.GetService<MainService>();
            SceneLoader.LoadScene(bootSetting.NextSceneIndex, () => mainService.EntryPoint());
        }
        #endregion
    }
}