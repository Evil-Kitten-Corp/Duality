using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Test
{
    public class LobbyManager : NetworkBehaviour
    {
        public enum ConnectionState : byte
        {
            Disconnected,
            NotReady,
            Ready
        }

        [Serializable]
        public struct PlayerConnectionState
        {
            public ConnectionState playerState;
            public PlayerLobbyBehavior playerObject;
            public ulong clientId;
        }

        [Serializable]
        public struct CharacterContainer
        {
            public Image imageContainer;
            public Image readyState;
            public GameObject clientBadge;
            public Image playerIcon;
        }

        public static LobbyManager Instance;
        
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

        public ClientData[] charactersData;
        public CharacterContainer[] characterContainer;
        public GameObject readyBttn;
        public GameObject cancelBttn;
        public string nextScene = "Main";
        public  PlayerConnectionState[] playerConnectionStates;
        public GameObject playerPrefab;
        public bool debug;

        void Update()
        {
            if (!IsServer)
                return;

            if (!debug)
            {
                if (playerConnectionStates.Any(state => state.playerState != ConnectionState.Ready))
                {
                    return;
                }

                StartGame();
            }
            else
            {
                if (playerConnectionStates[0].playerState != ConnectionState.Ready) 
                    return;
                
                StartGame();
            }
        }

        void OnDisable()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= PlayerDisconnects;
            }
        }

        void StartGame()
        {
            NetworkManager.Singleton.SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        }

        void RemoveSelectedStates()
        {
            foreach (var t in charactersData)
            {
                t.wasLockedIn = false;
            }
        }

        void RemoveReadyStates(ulong clientId, bool disconnected)
        {
            for (int i = 0; i < playerConnectionStates.Length; i++)
            {
                if (playerConnectionStates[i].playerState == ConnectionState.Ready &&
                    playerConnectionStates[i].clientId == clientId)
                {

                    if (disconnected)
                    {
                        playerConnectionStates[i].playerState = ConnectionState.Disconnected;
                        UpdatePlayerStateClientRpc(clientId, i, ConnectionState.Disconnected);
                    }
                    else
                    {
                        playerConnectionStates[i].playerState = ConnectionState.NotReady;
                        UpdatePlayerStateClientRpc(clientId, i, ConnectionState.NotReady);
                    }
                }
            }
        }

        [ClientRpc]
        void UpdatePlayerStateClientRpc(ulong clientId, int stateIndex, ConnectionState state)
        {
            if (IsServer)
                return;

            playerConnectionStates[stateIndex].playerState = state;
            playerConnectionStates[stateIndex].clientId = clientId;
        }

        void SetNonPlayableChar(int playerId)
        {
            characterContainer[playerId].imageContainer.sprite = null;
            characterContainer[playerId].imageContainer.color = new Color(1f, 1f, 1f, 0f);
            characterContainer[playerId].playerIcon.gameObject.SetActive(false);
            characterContainer[playerId].readyState.gameObject.SetActive(false);
        }

        public bool IsReady(int playerId)
        {
            return charactersData[playerId].wasLockedIn;
        }

        public void SetCharacterUI(int playerId, int characterSelected)
        {
            characterContainer[playerId].imageContainer.sprite = charactersData[characterSelected].characterSprite;
            characterContainer[playerId].imageContainer.gameObject.SetActive(true);
        }

        public void SetPlayableChar(int playerId, int characterSelected, bool isClientOwner)
        {
            SetCharacterUI(playerId, characterSelected);
            
            characterContainer[playerId].playerIcon.gameObject.SetActive(true);
            characterContainer[playerId].clientBadge.SetActive(isClientOwner);
            characterContainer[playerId].playerIcon.sprite = charactersData[characterSelected].iconSprite;
        }

        public ConnectionState GetConnectionState(int playerId)
        {
            if (playerId != -1)
                return playerConnectionStates[playerId].playerState;

            return ConnectionState.Disconnected;
        }

        public void ServerSceneInit(ulong clientId)
        {
            GameObject go =
                NetworkMonitor.Instance.SpawnNewNetworkObjectChangeOwnershipToClient(
                    playerPrefab,
                    transform.position,
                    clientId);

            for (int i = 0; i < playerConnectionStates.Length; i++)
            {
                if (playerConnectionStates[i].playerState == ConnectionState.Disconnected)
                {
                    playerConnectionStates[i].playerState = ConnectionState.NotReady;
                    playerConnectionStates[i].playerObject = go.GetComponent<PlayerLobbyBehavior>();
                    playerConnectionStates[i].clientId = clientId;

                    break;
                }
            }

            for (int i = 0; i < playerConnectionStates.Length; i++)
            {
                if (playerConnectionStates[i].playerObject != null)
                    PlayerConnectsClientRpc(
                        playerConnectionStates[i].clientId,
                        i,
                        playerConnectionStates[i].playerState,
                        playerConnectionStates[i].playerObject.GetComponent<NetworkObject>());
            }
        }

        [ClientRpc]
        void PlayerConnectsClientRpc(ulong clientId, int stateIndex, ConnectionState state, 
            NetworkObjectReference player)
        {
            if (IsServer)
                return;

            if (state != ConnectionState.Disconnected)
            {
                playerConnectionStates[stateIndex].playerState = state;
                playerConnectionStates[stateIndex].clientId = clientId;

                if (player.TryGet(out NetworkObject playerObject))
                    playerConnectionStates[stateIndex].playerObject = 
                        playerObject.GetComponent<PlayerLobbyBehavior>();
            }
        }

        public void PlayerDisconnects(ulong clientId)
        {
            if (!NetworkMonitor.Instance.IsExtraClient(clientId))
                return;

            PlayerNotReady(clientId, isDisconected: true);

            playerConnectionStates[GetPlayerId(clientId)].playerObject.DeSpawn();

            if (clientId == 0)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        public void PlayerNotReady(ulong clientId, int characterSelected = 0, bool isDisconected = false)
        {
            int playerId = GetPlayerId(clientId);

            RemoveReadyStates(clientId, isDisconected);

            if (isDisconected)
            {
                PlayerDisconnectedClientRpc(playerId);
            }
            else
            {
                PlayerNotReadyClientRpc(clientId, playerId, characterSelected);
            }
        }

        public int GetPlayerId(ulong clientId)
        {
            for (int i = 0; i < playerConnectionStates.Length; i++)
            {
                if (playerConnectionStates[i].clientId == clientId)
                    return i;
            }

            return -1;
        }

        public void PlayerReady(ulong clientId, int playerId, int characterSelected)
        {
            if (!charactersData[characterSelected].wasLockedIn)
            {
                PlayerReadyClientRpc(clientId, playerId, characterSelected);
            }
        }

        public void SetPlayerReadyUIButtons(bool isReady, int characterSelected)
        {
            switch (isReady)
            {
                case true when !charactersData[characterSelected].wasLockedIn:
                    readyBttn.SetActive(false);
                    cancelBttn.SetActive(true);
                    break;
                case false when charactersData[characterSelected].wasLockedIn:
                    readyBttn.SetActive(true);
                    cancelBttn.SetActive(false);
                    break;
            }
        }

        public bool IsSelectedByPlayer(int playerId, int characterSelected)
        {
            return charactersData[characterSelected].playerId == playerId;
        }

        [ClientRpc]
        void PlayerReadyClientRpc(ulong clientId, int playerId, int characterSelected)
        {
            charactersData[characterSelected].wasLockedIn = true;
            charactersData[characterSelected].clientId = clientId;
            charactersData[characterSelected].playerId = playerId;
            playerConnectionStates[playerId].playerState = ConnectionState.Ready;

            characterContainer[playerId].readyState.color = Color.green;
        }

        [ClientRpc]
        void PlayerNotReadyClientRpc(ulong clientId, int playerId, int characterSelected)
        {
            charactersData[characterSelected].wasLockedIn = false;
            charactersData[characterSelected].clientId = 0UL;
            charactersData[characterSelected].playerId = -1;

            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                characterContainer[playerId].clientBadge.SetActive(true);
                characterContainer[playerId].readyState.color = Color.white;
            }
            else
            {
                characterContainer[playerId].readyState.color = Color.white;
            }
        }

        [ClientRpc]
        public void PlayerDisconnectedClientRpc(int playerId)
        {
            SetNonPlayableChar(playerId);
            RemoveSelectedStates();

            playerConnectionStates[playerId].playerState = ConnectionState.Disconnected;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                NetworkManager.Singleton.OnClientDisconnectCallback += PlayerDisconnects;
        }
    }
}