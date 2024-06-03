using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class MainMenuOptions : MonoBehaviour
{
    public GameObject profileButton;
    public GameObject clientWarning;
    public GameObject loading;
    public string nextScene;
    public string profileScene;
    
    private readonly bool _showProfile = SceneManagement.Instance.IsLoggedIn;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => SceneManagement.Instance != null);
        
        if (_showProfile)
        {
            profileButton.gameObject.SetActive(true);
        }
    }

    public void StartHost()
    {
        SceneManagement.Instance.LoadSceneAs(nextScene, "host");
    }

    public void StartServer()
    {
        SceneManagement.Instance.LoadSceneAs(nextScene, "server");
    }

    public void StartClient()
    {
        StartCoroutine(TryStartClient());
    }

    private IEnumerator TryStartClient()
    {
        // Starting the client
        loading.SetActive(true);
        NetworkManager.Singleton.StartClient();

        // Waiting a short time to see if the client successfully connects
        float timeout = 5f;
        float timer = 0f;
        bool isConnected = false;

        while (timer < timeout)
        {
            if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsConnectedClient)
            {
                isConnected = true;
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (isConnected)
        {
            // Successfully connected to a server, yay
            SceneManagement.Instance.LoadSceneAs(nextScene, "client");
        }
        else
        {
            // Failed to connect to a server
            clientWarning.SetActive(true);
            loading.SetActive(false);
            NetworkManager.Singleton.Shutdown();
        }
    }

    public void Profile()
    {
        SceneManagement.Instance.LoadSceneAs(profileScene, null);
    }
    
    public void Quit() 
    { 
        Application.Quit();
    }
}
