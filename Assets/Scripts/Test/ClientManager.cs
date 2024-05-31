using Unity.Netcode;
using UnityEngine;

namespace Test
{
    public class ClientManager : NetworkBehaviour
    {
        public static ClientManager Instance;

        public NetworkVariable<int> clientCount = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client connected: {clientId}");
            clientCount.Value++;
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Client disconnected: {clientId}");
            clientCount.Value--;
        }
    }
}