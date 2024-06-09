using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Test
{
    public class LevelController : NetworkBehaviour
    {
        public TeamUI bananaScore;
        public TeamUI strawberryScore;
        public GameObject deathUI;
        public string nextLevel;

        public static Action<ulong> OnPlayerDefeated;

        [FormerlySerializedAs("m_charactersData")] [SerializeField] private ClientData[] backupCharacterData;
        [SerializeField] public Transform strawberryStartingPosition;
        [SerializeField] public Transform bananaStartingPosition;

        private List<ulong> _connectedClients = new();
        private List<PlayerChar> _playerCharacters = new();
        
        [Header("Images | UI")]
        public Image[] bananaBoyRestart;
        private bool _bRest;
        public Image[] strawberryBoyRestart;
        private bool _sRest;
        
        public Image[] bananaBoyWin;
        private bool _bWin;
        public Image[] strawberryBoyWin;
        private bool _sWin;

        public static LevelController Instance;

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

        private void OnEnable()
        {
            OnPlayerDefeated += PlayerDeath;
            
            if (!IsServer)
                return;
            
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        }
        
        private void OnDisable()
        {
            OnPlayerDefeated -= PlayerDeath;
            
            if (!IsServer)
                return;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
            }
        }

        public void PlayerDeath(ulong clientId)
        {
            ActivateDeathUIClientRpc(clientId);
        }

        private void OnClientDisconnect(ulong clientId)
        {
            foreach (var player in _playerCharacters
                         .Where(player => player != null)
                         .Where(player => player.characterData.clientId == clientId))
            {
                player.Kill();
            }
        }

        [ClientRpc]
        private void ActivateDeathUIClientRpc(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                Time.timeScale = 0;
                deathUI.SetActive(true);
            }
        }

        [ClientRpc]
        private void SetPlayerUIClientRpc(string playerFruit)
        {
            GameObject playerCharacter = GameObject.Find(playerFruit);

            if (playerCharacter == null)
            {
                Debug.LogError($"Player with name {playerFruit} not found");
                return;
            }

            PlayerChar playerController = playerCharacter.GetComponent<PlayerChar>();
            if (playerController == null)
            {
                Debug.LogError($"PlayerChar component not found on {playerFruit}");
                return;
            }

            if (playerController.characterData == null)
            {
                Debug.LogError("PlayerChar.characterData is null");
                return;
            }

            switch (playerController.characterData.characterType)
            {
                case FruitType.Strawberry:
                    playerController.playerUI = strawberryScore;
                    break;
                case FruitType.Banana:
                    playerController.playerUI = bananaScore;
                    break;
                default:
                    Debug.LogError($"Unknown character type: {playerController.characterData.characterType}");
                    throw new ArgumentOutOfRangeException();
            }
        }


        private IEnumerator HostShutdown()
        {
            ShutdownClientRpc();
            yield return new WaitForSeconds(0.5f);
            Shutdown();
        }

        private void Shutdown()
        {
            NetworkManager.Singleton.Shutdown();
        }

        [ClientRpc]
        private void ShutdownClientRpc()
        {
            if (IsServer)
                return;

            Shutdown();
        }

        public void ExitToMenu()
        {
            if (IsServer)
            {
                StartCoroutine(HostShutdown());
            }
            else
            {
                NetworkManager.Singleton.Shutdown();
            }
        }
        
        public void ServerSceneInit(ulong clientId)
        {
            Debug.Log($"I'm being called with this id: {clientId}");
             
            _connectedClients.Add(clientId);
            
            if (_connectedClients.Count < NetworkManager.Singleton.ConnectedClients.Count)
                return;

            foreach (var client in _connectedClients)
            {
                foreach (ClientData data in backupCharacterData)
                {
                    if (data.wasLockedIn && data.clientId == client)
                    {
                        Transform startingPos = data.characterType switch
                        {
                            FruitType.Banana => bananaStartingPosition,
                            FruitType.Strawberry => strawberryStartingPosition,
                            _ => null
                        };

                        if (startingPos == null) continue;
                        
                        GameObject playerSpaceship =
                            NetworkMonitor.Instance.SpawnNewNetworkObjectAsPlayerObject(
                                data.playerPrefab,
                                startingPos.position,
                                data.clientId,
                                true);

                        PlayerChar playerShipController =
                            playerSpaceship.GetComponent<PlayerChar>();
                        playerShipController.characterData = data;

                        _playerCharacters.Add(playerShipController);
                        SetPlayerUIClientRpc(playerSpaceship.name);
                    }
                }
            }
        }

        [ClientRpc]
        private void SetUIButtonsClientRpc(FruitType characterDataCharacterType, bool win)
        {
            if (win)
            {
                switch (characterDataCharacterType)
                {
                    case FruitType.Strawberry:
                        
                        foreach (var i in strawberryBoyWin)
                        {
                            i.color = Color.white;
                        }
                        break;
                    case FruitType.Banana:
                        
                        foreach (var i in bananaBoyWin)
                        {
                            i.color = Color.white;
                        }
                        break;
                }
            }
            else
            {
                switch (characterDataCharacterType)
                {
                    case FruitType.Strawberry:
                        
                        foreach (var i in strawberryBoyRestart)
                        {
                            i.color = Color.white;
                        }
                        break;
                    case FruitType.Banana:
                        
                        foreach (var i in bananaBoyRestart)
                        {
                            i.color = Color.white;
                        }
                        break;
                }
            }
        }

        private void RestartScene()
        {
            Debug.Log("I've been called to restart.");
            NetworkManager.Singleton.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }

        private void GoToNextLevel()
        {
            if (nextLevel != String.Empty)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(nextLevel, LoadSceneMode.Single);
            }
            else
            {
                ExitToMenu();
            }
        }
        
        public IEnumerator Restart(FruitType characterDataCharacterType)
        {
            switch (characterDataCharacterType)
            {
                case FruitType.Banana:
                    _bRest = true;
                    break;
                case FruitType.Strawberry:
                    _sRest = true;
                    break;
            }
            
            SetUIButtonsClientRpc(characterDataCharacterType, false);

            if (_bRest & _sRest)
            {
                Debug.Log("Both are ready.");
                RestartScene();
            }

            yield break;
        }

        public IEnumerator Win(FruitType characterDataCharacterType)
        {
            switch (characterDataCharacterType)
            {
                case FruitType.Banana:
                    _bWin = true;
                    break;
                case FruitType.Strawberry:
                    _sWin = true;
                    break;
            }
            
            SetUIButtonsClientRpc(characterDataCharacterType, true);

            if (_bWin & _sWin)
            {
                yield return new WaitForSeconds(1f);
                GoToNextLevel();
            }
        }
    }
}