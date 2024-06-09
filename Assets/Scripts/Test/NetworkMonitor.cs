using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Test
{
    public class NetworkMonitor : NetworkBehaviour
    {
        public static NetworkMonitor Instance;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        
        public int maxConnections;
        public ClientData[] charData;

        IEnumerator Start()
        {
            yield return new WaitUntil(() => NetworkManager.Singleton != null);
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        public void Init()
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        }

        private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode mode)
        {
            if (!NetworkManager.Singleton.IsServer) 
                return;

            if (!CanClientConnect(clientId)) 
                return;

            switch (sceneName)
            {
                case "Lobby":
                    LobbyManager.Instance.ServerSceneInit(clientId);
                    break;
                case "Main":
                    LevelController.Instance.ServerSceneInit(clientId);
                    break;
            }
        }
        
        public bool IsExtraClient(ulong clientId)
        {
            return CanConnect(clientId);
        }

        private bool CanClientConnect(ulong clientId)
        {
            if (!IsServer)
                return false;

            bool canConnect = CanConnect(clientId);
            if (!canConnect)
            {
                RemoveClient(clientId);
            }

            return canConnect;
        }

        private bool CanConnect(ulong clientId)
        {
            if (SceneManager.GetActiveScene().name == "Lobby")
            {
                int playersConnected = NetworkManager.Singleton.ConnectedClientsList.Count;

                if (playersConnected > maxConnections)
                {
                    return false;
                }
                
                return true;
            }

            return ChoseCharacter(clientId);
        }

        
        private void RemoveClient(ulong clientId)
        {
            
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            };

            ShutdownClientRpc();
        }

        private bool ChoseCharacter(ulong clientId)
        {
            return charData.Any(data => data.clientId == clientId);
        }

        [ClientRpc]
        private void ShutdownClientRpc()
        {        
            Shutdown();
        }

        private void Shutdown()
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        public GameObject SpawnNewNetworkObject(GameObject prefab, bool destroyWithScene = true)
        {
            GameObject newGameObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            NetworkObject newGameObjectNetworkObject = newGameObject.GetComponent<NetworkObject>();
            newGameObjectNetworkObject.Spawn(destroyWithScene);

            return newGameObject;
        }

        public GameObject SpawnNewNetworkObject(GameObject prefab, Vector3 position, bool destroyWithScene = true)
        {
            GameObject newGameObject = Object.Instantiate(prefab, position, Quaternion.identity);
            NetworkObject newGameObjectNetworkObject = newGameObject.GetComponent<NetworkObject>();
            newGameObjectNetworkObject.Spawn(destroyWithScene);

            return newGameObject;
        }

        public GameObject SpawnNewNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation, 
            bool destroyWithScene = true)
        {
            GameObject newGameObject = Instantiate(prefab, position, rotation);
            NetworkObject newGameObjectNetworkObject = newGameObject.GetComponent<NetworkObject>();
            newGameObjectNetworkObject.Spawn(destroyWithScene);

            return newGameObject;
        }

        public GameObject SpawnNewNetworkObjectAsPlayerObject(GameObject prefab, Vector3 position,
            ulong newClientOwnerId, bool destroyWithScene = true)
        {
            GameObject newGameObject = Instantiate(prefab, position, Quaternion.identity);
            NetworkObject newGameObjectNetworkObject = newGameObject.GetComponent<NetworkObject>();
            newGameObjectNetworkObject.SpawnAsPlayerObject(newClientOwnerId, destroyWithScene);

            return newGameObject;
        }

        public GameObject SpawnNewNetworkObjectChangeOwnershipToClient(GameObject prefab, Vector3 position,
            ulong newClientOwnerId, bool destroyWithScene = true)
        {
            GameObject newGameObject = Instantiate(prefab, position, Quaternion.identity);
            NetworkObject newGameObjectNetworkObject = newGameObject.GetComponent<NetworkObject>();
            newGameObjectNetworkObject.SpawnWithOwnership(newClientOwnerId, destroyWithScene);

            return newGameObject;
        }
        
        public void DespawnNetworkObject(NetworkObject networkObject)
        {
            if (networkObject != null && networkObject.IsSpawned) 
                networkObject.Despawn();
        }
    }
}