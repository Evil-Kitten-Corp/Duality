using System.Collections;
using DefaultNamespace;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : NetworkBehaviour
{
    public static SceneManagement Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    
    public bool IsLoggedIn { get; private set; }
    public string LoggedInUser { get; private set; }

    public void Register()
    {
        SceneManager.LoadScene("Signup");
    }

    public void TryLogin(LoginAuth auth)
    {
        if (auth.Authenticate())
        {
            LoginSuccessful(auth.username.text);
        }
        else
        {
            auth.error.gameObject.SetActive(true);
        }
    }

    public void LoginSuccessful(string user)
    {
        LoggedInUser = user;
        IsLoggedIn = true;
        GoToMainMenu();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnSceneLoaded(string mode)
    {
        Debug.Log("On Scene Loaded callback.");
        StartCoroutine(StartNetworkSession(mode));
    }

    private IEnumerator StartNetworkSession(string mode)
    {
        Debug.Log("Starting network session...");
        yield return new WaitForSeconds(1f);
        Debug.Log("Finished waiting.");

        switch (mode)
        {
            case "client":
                NetworkManager.Singleton.StartClient();
                break;
            
            case "host": 
                NetworkManager.Singleton.StartHost();
                break;
            
            case "server":
                NetworkManager.Singleton.StartServer();
                break;
            
            default:
                Debug.LogError("Invalid mode specified.");
                break;
        }
    }
    
    public void LoadSceneAs(string scene, string mode) 
    {
        SceneManager.LoadSceneAsync(scene).completed += _ => OnSceneLoaded(mode);
    }
}
