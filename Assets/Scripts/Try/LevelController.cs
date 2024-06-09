using System.Collections;
using Cinemachine;
using Test;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Try
{
    public class LevelController : NetworkBehaviour
    {
        public CinemachineVirtualCamera cam;
        
        public LevelSectioner[] levels;
        private int _lvCount;
        private LevelSectioner _currentLevel;
        
        public PlayerMove strawberryPlayer;
        public PlayerMove bananaPlayer;

        public TMP_Text strawberryScore;
        public TMP_Text bananaScore;

        private int _strawberryScore;
        private int _bananaScore;
        
        [Header("Images | UI")]
        public GameObject deathUI;
        public GameObject winningScreen;
        public GameObject mainMenuScreen;

        public Image[] bananaBoyRestart;
        private bool _bRest;
        public Image[] strawberryBoyRestart;
        private bool _sRest;
        
        public Image[] bananaBoyWin;
        private bool _bWin;
        public Image[] strawberryBoyWin;
        private bool _sWin;

        private void Start()
        {
            Debug.Log("Start being called.");
            _currentLevel = levels[0];
            _lvCount = 0;
            cam.Follow = _currentLevel.cameraLookAt;
            
            NetworkManager.OnConnectionEvent += OnClientSpawn;
            PlayerMove.OnWin += OnPlayerWin;
            
            PlayerMove.OnCancelWin += type =>
            {
                switch (type)
                {
                    case FruitType.Strawberry:
                        _sWin = false;
                        break;
                    case FruitType.Banana:
                        _bWin = false;
                        break;
                }
            };
        }

        public void UpdateProfile(FruitType fruit, bool win)
        {
            int winsIncrement = win ? 1 : 0;
            int lossesIncrement = win ? 0 : 1;

            var db = FindObjectOfType<DatabaseManager>();
            
            switch (SceneManagement.Instance.IsLoggedIn)
            {
                case true when fruit == FruitType.Strawberry:
                    db.UpdateUserProfile(SceneManagement.Instance.LoggedInUser, 
                        1, winsIncrement, lossesIncrement, _strawberryScore);
                    break;
                case true when fruit == FruitType.Banana:
                    db.UpdateUserProfile(SceneManagement.Instance.LoggedInUser, 
                        1, winsIncrement, lossesIncrement, _bananaScore);
                    break;
            }
        }

        private void OnClientSpawn(NetworkManager networkManager, ConnectionEventData connectionEventData)
        {
            if (connectionEventData.EventType == ConnectionEvent.ClientConnected)
            {
                Debug.Log("Checking player number...");
                Debug.Log($"There are {NetworkManager.Singleton.ConnectedClients.Count} clients.");

                if (NetworkManager.Singleton.ConnectedClients.Count == 1)
                {
                    Debug.Log($"Player {connectionEventData.ClientId} connected as strawberry boy!");

                    PlayerMove player = Instantiate(strawberryPlayer, Vector3.zero, Quaternion.identity);
                    player.transform.position = _currentLevel.strawberrySpawnPos.position;
                    player.GetComponent<PlayerScore>().textObject = strawberryScore;
            
                    var prefabNetworkObject = player.GetComponent<NetworkObject>();
                    prefabNetworkObject.SpawnAsPlayerObject(connectionEventData.ClientId, true);
                    prefabNetworkObject.ChangeOwnership(connectionEventData.ClientId);
                }
                else if (NetworkManager.Singleton.ConnectedClients.Count == 2)
                {
                    Debug.Log($"Player {connectionEventData.ClientId} connected as banana boy!");

                    PlayerMove player = Instantiate(bananaPlayer, Vector3.zero, Quaternion.identity);
                    player.transform.position = _currentLevel.bananaSpawnPos.position;
                    player.GetComponent<PlayerScore>().textObject = bananaScore;
            
                    var prefabNetworkObject = player.GetComponent<NetworkObject>();
                    prefabNetworkObject.SpawnAsPlayerObject(connectionEventData.ClientId, true);
                    prefabNetworkObject.ChangeOwnership(connectionEventData.ClientId);
                }
                else
                {
                    Debug.Log("Connected as spectator.");
                }
            }
        }

        public void OnPlayerDeath()
        {
            Debug.Log("Player died.");
            StartCoroutine(DeathRoutine());

            if (NetworkManager.Singleton.IsServer)
            {
                UpdateProfile(FruitType.Strawberry, false);
                UpdateProfile(FruitType.Banana, false);
            }
        }

        IEnumerator DeathRoutine()
        {
            yield return new WaitForSeconds(.6f); // wait for the animation to play
            Time.timeScale = 0;
            
            OnPlayerDeathClientRpc();
        }
        
        private void OnPlayerWin(FruitType team)
        {
            switch (team)
            {
                case FruitType.Strawberry:
                    _sWin = true;
                    break;
                case FruitType.Banana:
                    _bWin = true;
                    break;
            }

            if (_sWin && _bWin)
            {
                ShowWinningScreenClientRpc();
            }
            
            if (NetworkManager.Singleton.IsServer)
            {
                UpdateProfile(FruitType.Strawberry, true);
                UpdateProfile(FruitType.Banana, true);
            }
            
        }

        public void AddScore(FruitType team)
        {
            if (!IsServer)
            {
                return;
            }
            
            switch (team)
            {
                case FruitType.Strawberry:
                    _strawberryScore++;
                    strawberryScore.text = _strawberryScore.ToString();
                    break;
                case FruitType.Banana:
                    _bananaScore++;
                    bananaScore.text = _bananaScore.ToString();
                    break;
            }

            SyncScoreClientRpc(team);
        }
        
        private bool IsThereNextLevel()
        {
            int test = _lvCount;
            return test + 1 <= levels.Length;
        }

        [ClientRpc]
        private void SyncScoreClientRpc(FruitType team)
        {
            switch (team)
            {
                case FruitType.Strawberry:
                    strawberryScore.text = _strawberryScore.ToString();
                    break;
                case FruitType.Banana:
                    bananaScore.text = _bananaScore.ToString();
                    break;
            }
        }

        [ClientRpc]
        private void ShowWinningScreenClientRpc()
        {
            if (IsThereNextLevel())
            {
                winningScreen.SetActive(true);
            }
            else
            {
                mainMenuScreen.SetActive(true);
            }
        }
        
        [ClientRpc]
        private void OnPlayerDeathClientRpc()
        {
            Time.timeScale = 0;
            deathUI.SetActive(true);
        }
        
        [ClientRpc]
        private void OnPlayerReviveClientRpc()
        {
            deathUI.SetActive(false);
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
            _currentLevel.Restart(strawberryPlayer, bananaPlayer);
            OnPlayerReviveClientRpc();
        }

        private void GoToNextLevel()
        {
            Debug.Log("Now we're going to next level.");
            
            _lvCount++;
            _currentLevel = levels[_lvCount];
            
            _currentLevel.Restart(strawberryPlayer, bananaPlayer);
            cam.Follow = _currentLevel.cameraLookAt;
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

            if (NetworkManager.Singleton.ConnectedClients.Count == 1)
            {
                RestartScene();
            }
            else if (_bRest & _sRest)
            {
                Debug.Log("Both are ready.");
                RestartScene();
            }

            yield break;
        }
    }
}