using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Puzzles
{
    public class LevelManager : NetworkBehaviour
    {
        public Level level;
        

        public TMP_Text bananaScore;
        public TMP_Text strawberryScore;
        public TMP_Text connectedClientsDebug;
        
        public DualCharacter playerPrefab;
        
        private int _playerPrefabIndex;
        
        private readonly NetworkVariable<int> _bananaScore = new();
        private readonly NetworkVariable<int> _strawberryScore = new();
        [HideInInspector] public NetworkVariable<int> connectedClients = new();

        private void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            
            
            connectedClients.OnValueChanged += OnValueChanged;
            _bananaScore.OnValueChanged += delegate { OnScoreChange(DualChoice.BananaBoy); };
            _strawberryScore.OnValueChanged += delegate { OnScoreChange(DualChoice.StrawberryBoy); };
        }

        private void OnValueChanged(int previousValue, int newValue)
        {
            connectedClientsDebug.text = newValue.ToString();
        }

        private void OnScoreChange(DualChoice type)
        {
            switch (type)
            {
                case DualChoice.BananaBoy:
                    bananaScore.text = _bananaScore.Value.ToString();
                    break;
                case DualChoice.StrawberryBoy:
                    strawberryScore.text = _strawberryScore.Value.ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void LoadNextLevelForBothClients()
        {
            NetworkManager.Singleton.SceneManager.LoadScene(level.nextLevelName, LoadSceneMode.Single);
        }

        private void RestartLevelForBothClients()
        {
            NetworkManager.Singleton.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Player {clientId} connected, prefab index = {_playerPrefabIndex}!");

            DualCharacter player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            player.OnDeath += OnPlayerDeath;
            player.transform.position = GetSpawn(player.CharacterType).position;
            
            var prefabNetworkObject = player.GetComponent<NetworkObject>();
            prefabNetworkObject.SpawnAsPlayerObject(clientId, true);
            prefabNetworkObject.ChangeOwnership(clientId);
            _playerPrefabIndex = (_playerPrefabIndex + 1) % 1;
            
            connectedClients.Value++;
        }

        private void OnPlayerDeath()
        {
            if (connectedClients.Value > 1)
            {
                Time.timeScale = 0;
            }
            else
            {
                RestartLevelForBothClients();
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Player {clientId} disconnected!");
            connectedClients.Value--;
        }

        public Transform GetSpawn(DualChoice dual)
        {
            switch (dual)
            {
                case DualChoice.BananaBoy:
                    return level.fireSpawnPoint;
                case DualChoice.StrawberryBoy:
                    return level.waterSpawnPoint;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dual), dual, null);
            }
        }
        
        public void AddScore(DualChoice team)
        {
            switch (team)
            {
                case DualChoice.BananaBoy:
                    _bananaScore.Value++;
                    break;
                case DualChoice.StrawberryBoy:
                    _strawberryScore.Value++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(team), team, null);
            }
        }

        public void RemoveListeners(DualCharacter dualCharacter)
        {
            dualCharacter.OnDeath -= OnPlayerDeath;
        }
    }
}