using System;
using System.Collections;
using Test;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Try
{
    public class SpawnRealPlayer : NetworkBehaviour
    {
        public PlayerMove strawberryPlayer;
        public PlayerMove bananaPlayer;

        public Transform strawberrySpawnPos;
        public Transform bananaSpawnPos;

        public TMP_Text strawberryScore;
        public TMP_Text bananaScore;

        private int _strawberryScore;
        private int _bananaScore;
        
        [Header("Images | UI")]
        public GameObject deathUI;
        public GameObject winningScreen;

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
                    //player.OnDeath += OnPlayerDeath;
                    player.transform.position = strawberrySpawnPos.position;
                    player.GetComponent<PlayerScore>().textObject = strawberryScore;
            
                    var prefabNetworkObject = player.GetComponent<NetworkObject>();
                    prefabNetworkObject.SpawnAsPlayerObject(connectionEventData.ClientId, true);
                    prefabNetworkObject.ChangeOwnership(connectionEventData.ClientId);
                }
                else if (NetworkManager.Singleton.ConnectedClients.Count == 2)
                {
                    Debug.Log($"Player {connectionEventData.ClientId} connected as banana boy!");

                    PlayerMove player = Instantiate(bananaPlayer, Vector3.zero, Quaternion.identity);
                    //player.OnDeath += OnPlayerDeath;
                    player.transform.position = bananaSpawnPos.position;
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
        }

        IEnumerator DeathRoutine()
        {
            yield return new WaitForSeconds(.6f); // wait for the animation to play
            Time.timeScale = 0;
            
            OnPlayerDeathClientRpc();
        }
        
        private void OnPlayerWin(FruitType team)
        {
            if (team == FruitType.Strawberry)
            {
                _sWin = true;
            }
            else if (team == FruitType.Banana)
            {
                _bWin = true;
            }

            if (_sWin && _bWin)
            {
                ShowWinningScreenClientRpc();
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
            winningScreen.SetActive(true);
        }
        
        [ClientRpc]
        private void OnPlayerDeathClientRpc()
        {
            Time.timeScale = 0;
            deathUI.SetActive(true);
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
            /*if (nextLevel != String.Empty)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(nextLevel, LoadSceneMode.Single);
            }
            else
            {
                ExitToMenu();
            }*/
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