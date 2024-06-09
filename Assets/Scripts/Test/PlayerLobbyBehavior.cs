using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Test
{
    public class PlayerLobbyBehavior : NetworkBehaviour
    {
        private const int KNoCharacterSelectedValue = -1;

        public NetworkVariable<int> charSelected = new(KNoCharacterSelectedValue);
        public NetworkVariable<int> playerId = new(KNoCharacterSelectedValue);

        private void Start()
        {
            if (IsServer)
            {
                playerId.Value = LobbyManager.Instance.GetPlayerId(OwnerClientId);
            }
            else if (!IsOwner && HasCharacterSelected())
            {
                LobbyManager.Instance.SetPlayableChar(
                    playerId.Value,
                    charSelected.Value,
                    IsOwner);
            }

            gameObject.name = $"Player{playerId.Value + 1}";
        }

        private bool HasCharacterSelected()
        {
            return playerId.Value != KNoCharacterSelectedValue;
        }

        private void OnEnable()
        {
            playerId.OnValueChanged += OnPlayerIdSet;
            charSelected.OnValueChanged += OnCharacterChanged;
            ConfirmationButton.OnButtonPress += OnUIButtonPress;
        }

        private void OnDisable()
        {
            playerId.OnValueChanged -= OnPlayerIdSet;
            charSelected.OnValueChanged -= OnCharacterChanged;
            ConfirmationButton.OnButtonPress -= OnUIButtonPress;
        }

        private void OnPlayerIdSet(int oldValue, int newValue)
        {
            LobbyManager.Instance.SetPlayableChar(newValue, newValue, IsOwner);

            if (IsServer)
                charSelected.Value = newValue;
        }

        private void OnCharacterChanged(int oldValue, int newValue)
        {
            if (!IsOwner && HasCharacterSelected())
                LobbyManager.Instance.SetCharacterUI(playerId.Value, newValue);
        }

        private void Update()
        {
            if (IsOwner && LobbyManager.Instance.GetConnectionState(playerId.Value) != LobbyManager.ConnectionState.Ready)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    ChangeCharacterSelection(-1);
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    ChangeCharacterSelection(1);
                }
            }

            if (IsOwner)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (!LobbyManager.Instance.IsReady(charSelected.Value))
                    {
                        LobbyManager.Instance.SetPlayerReadyUIButtons(
                            true,
                            charSelected.Value);

                        ReadyServerRpc();
                    }
                    else
                    {
                        if (LobbyManager.Instance.IsSelectedByPlayer(
                                playerId.Value, charSelected.Value))
                        {
                            LobbyManager.Instance.SetPlayerReadyUIButtons(
                                false,
                                charSelected.Value);

                            NotReadyServerRpc();
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (playerId.Value == 0)
                    {
                        StartCoroutine(HostShutdown());
                    }
                    else
                    {
                        Shutdown();
                    }
                }
            }
        }

        IEnumerator HostShutdown()
        {
            ShutdownClientRpc();
            yield return new WaitForSeconds(0.5f);
            Shutdown();
        }

        void Shutdown()
        {
            NetworkManager.Singleton.Shutdown();
        }

        [ClientRpc]
        void ShutdownClientRpc()
        {
            if (IsServer)
                return;

            Shutdown();
        }

        private void ChangeCharacterSelection(int value)
        {
            int charTemp = charSelected.Value;
            charTemp += value;
            
            if (charTemp >= LobbyManager.Instance.charactersData.Length)
                charTemp = 0;
            else if (charTemp < 0)
                charTemp = LobbyManager.Instance.charactersData.Length - 1;

            if (IsOwner)
            {
                ChangeCharacterSelectionServerRpc(charTemp);
                LobbyManager.Instance.SetPlayableChar(playerId.Value, charTemp, IsOwner);
            }
        }

        [ServerRpc]
        private void ChangeCharacterSelectionServerRpc(int newValue)
        {
            charSelected.Value = newValue;
        }

        [ServerRpc]
        private void ReadyServerRpc()
        {
            LobbyManager.Instance.PlayerReady(OwnerClientId, playerId.Value, charSelected.Value);
        }

        [ServerRpc]
        private void NotReadyServerRpc()
        {
            LobbyManager.Instance.PlayerNotReady(OwnerClientId, charSelected.Value);
        }

        private void OnUIButtonPress(ButtonActions buttonAction)
        {
            if (!IsOwner)
                return;

            switch (buttonAction)
            {
                case ButtonActions.LobbyReady:
                    LobbyManager.Instance.SetPlayerReadyUIButtons(true, charSelected.Value);
                    ReadyServerRpc();
                    break;

                case ButtonActions.LobbyNotReady:
                    LobbyManager.Instance.SetPlayerReadyUIButtons(false, charSelected.Value);
                    NotReadyServerRpc();
                    break;
            }
        }

        public void DeSpawn()
        {
            NetworkMonitor.Instance.DespawnNetworkObject(NetworkObject);
        }
    }
}