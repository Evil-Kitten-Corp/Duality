using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Test
{
    public class MainMenu : MonoBehaviour
    {
        public ClientData[] charData;
        public string nextScene;

        private IEnumerator Start()
        {
            ClearAllCharacterData();
            yield return new WaitUntil(() => NetworkManager.Singleton.SceneManager != null);
            NetworkMonitor.Instance.Init();
        }

        public void OnClickHost()
        {
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        }

        public void OnClickJoin()
        {
            NetworkManager.Singleton.StartClient();
        }

        public void OnClickQuit()
        {
            Application.Quit();
        }

        private void ClearAllCharacterData()
        {
            foreach (var data in charData)
            {
                data.EmptyData();
            }
        }
    }
}