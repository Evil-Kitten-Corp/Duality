using System.Collections;
using DefaultNamespace;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LoginContext
{
    Login,
    Signup
}

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
    
    public LoginContext? LoginMode { get; private set; }
    public bool IsLoggedIn { get; private set; }
    public string LoggedInUser { get; private set; }

    private void Start()
    {
        LoginMode = null;
    }

    public void Register()
    {
        LoginMode = LoginContext.Signup;
        SceneManager.LoadScene("Signup");
    }

    public void TryLogin(LoginAuth auth)
    {
        LoginMode = LoginContext.Login;
        StartCoroutine(LoginCo(auth));
    }

    private IEnumerator LoginCo(LoginAuth auth)
    {
        bool? result = null;
        yield return StartCoroutine(auth.Authenticate((res) => result = res));

        if (result == true)
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

    public void RestartActiveScene()
    {
        Debug.Log("Restarting scene.");
        ReloadScene("host");
        
        /*SceneManager.LoadSceneAsync("Loading").completed += _ =>
        {
            Debug.Log("Intermediate loading screen loaded.");
            
            LoadingScreen.OnLoad += delegate
            {
                Debug.Log("Loading next scene...");
            };
        };*/
                
        return;
        
        if (NetworkManager.Singleton.IsHost)
        {
            //DespawnLocalPlayer();
            ShutdownNetworkManager();
            ReloadScene("host");
            return;
        }

        if (NetworkManager.Singleton.IsServer)
        {
            ShutdownNetworkManager();
            ReloadScene("server");
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            //DespawnLocalPlayer();
            ShutdownNetworkManager();
            ReloadScene("client");
        }
    }
    
    private void DespawnLocalPlayer()
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(NetworkManager.Singleton.LocalClientId, 
                out var playerObject))
        {
            if (playerObject != null)
            {
                playerObject.Despawn();
                Debug.Log("Local player despawned.");
            }
            else
            {
                Debug.LogError("Local player object not found.");
            }
        }
        else 
        {
            Debug.LogError("Failed to retrieve local player object.");
        }
    }

    private void ShutdownNetworkManager()
    {
        NetworkManager.Singleton.Shutdown();
        Debug.Log("NetworkManager shut down.");
    }
    
    private void ReloadScene(string mode)
    {
        string activeScene = SceneManager.GetActiveScene().name;
        
        Debug.Log($"Loading intermediate loading screen for mode: {mode}");
        
        LoadingScreen.OnLoad += delegate
        {
            SceneManager.LoadScene(activeScene); 
            StartCoroutine(RestartNetworkSession(mode));
        };

        SceneManager.LoadScene("Loading");
        
        /*SceneManager.LoadSceneAsync("Loading").completed += _ =>
        {
            Debug.Log("Intermediate loading screen loaded.");
            LoadingScreen.OnLoad += delegate
            {
                Debug.Log("Loading next scene...");
                SceneManager.LoadSceneAsync(activeScene).completed += operation =>
                {
                    Debug.Log("Loading finished.");
                    OnSceneLoaded(operation, mode);
                };
            };
        };*/
    }
    
    private void OnSceneLoaded(AsyncOperation asyncOperation, string mode)
    {
        Debug.Log("On Scene Loaded callback.");
        StartCoroutine(RestartNetworkSession(mode));
    }

    private IEnumerator RestartNetworkSession(string mode)
    {
        Debug.Log("Restarting network session...");
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

    /*public void NetworkLoadScene(string name, string mode)
    {
        NetworkManager.Singleton.Shutdown();
        
        NetworkManager.Singleton.SceneManager.LoadScene(name, LoadSceneMode.Single);

        switch (mode)
        {
            case "client":
                break;
            
            case "host": 
                break;
            
            case "server": 
                break;
        }
    }*/

    public void LoadSceneAs(string scene, string mode) 
    {
        SceneManager.LoadSceneAsync(scene).completed += operation => OnSceneLoaded(operation, mode);
    }
}
