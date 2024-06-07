using Test;
using Unity.Netcode;
using UnityEngine;

namespace Try
{
    public class PlayerKillable : NetworkBehaviour
    {
        public bool killsEveryone;
        public FruitType team;

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.TryGetComponent(out PlayerMove pm))
            {
                if (killsEveryone || pm.fruit != team)
                {
                    if (NetworkManager.Singleton.IsServer)
                    {
                        HandleCollision(pm);
                    }
                    else
                    {
                        NotifyServerOfCollisionServerRpc(pm.NetworkObjectId);
                    }
                }
            }
        }

        private void HandleCollision(PlayerMove pm)
        {
            pm.Die();
            FindObjectOfType<SpawnRealPlayer>().OnPlayerDeath();
        }

        [ServerRpc(RequireOwnership = false)]
        private void NotifyServerOfCollisionServerRpc(ulong playerNetworkObjectId)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, 
                    out var playerObject))
            {
                if (playerObject.TryGetComponent(out PlayerMove pm))
                {
                    HandleCollision(pm);
                }
            }
        }
    }
}