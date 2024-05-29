using System;
using Unity.Netcode;
using UnityEngine;

namespace Puzzles
{
    public class LevelManager : MonoBehaviour
    {
        public Level level;
        public DualCharacter playerPrefab;
        private int playerPrefabIndex;

        private void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        
        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Player {clientId} connected, prefab index = {playerPrefabIndex}!");
            
            //spawn outside of view
            DualCharacter player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            player.transform.position = GetSpawn(player.CharacterType).position;
            
            var prefabNetworkObject = player.GetComponent<NetworkObject>();
            prefabNetworkObject.SpawnAsPlayerObject(clientId, true);
            prefabNetworkObject.ChangeOwnership(clientId);
            playerPrefabIndex = (playerPrefabIndex + 1) % 1;
        }
        
        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Player {clientId} disconnected!");
        }

        public Transform GetSpawn(DualChoice dual)
        {
            switch (dual)
            {
                case DualChoice.FireElemental:
                    return level.fireSpawnPoint;
                case DualChoice.WaterElemental:
                    return level.waterSpawnPoint;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dual), dual, null);
            }
        }
    }
}