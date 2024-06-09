using Unity.Netcode;
using UnityEngine;

namespace Try
{
    public class LevelSectioner : NetworkBehaviour
    {
        public Transform cameraLookAt;
        
        public Transform strawberrySpawnPos;
        public Transform bananaSpawnPos;
        public GameObject[] collectibles;

        public void Restart(PlayerMove strawberry, PlayerMove banana)
        {
            Time.timeScale = 1;
            
            foreach (var c in collectibles)
            {
                c.SetActive(true);
            }

            if (strawberry != null)
            {
                var networkObject = strawberry.GetComponent<NetworkObject>();
                Debug.Log($"Strawberry NetworkObject is {(networkObject.IsSpawned ? "spawned" : "not spawned")} " +
                          $"and {(networkObject.IsOwner ? "owned" : "not owned")} by this client.");

                if (!networkObject.IsSpawned)
                {
                    networkObject.Spawn();
                }

                strawberry.Revive(strawberrySpawnPos.position);
            }

            if (banana != null)
            {
                var networkObject = banana.GetComponent<NetworkObject>();
                Debug.Log($"Banana NetworkObject is {(networkObject.IsSpawned ? "spawned" : "not spawned")} " +
                          $"and {(networkObject.IsOwner ? "owned" : "not owned")} by this client.");
                
                if (!networkObject.IsSpawned)
                {
                    networkObject.Spawn();
                }
                
                banana.Revive(bananaSpawnPos.position);
            }
        }

        public void vStart(PlayerMove strawberry, PlayerMove banana)
        {
            if (strawberry != null)
            {
                var networkObject = strawberry.GetComponent<NetworkObject>();
                Debug.Log($"Strawberry NetworkObject is {(networkObject.IsSpawned ? "spawned" : "not spawned")} " +
                          $"and {(networkObject.IsOwner ? "owned" : "not owned")} by this client.");

                if (!networkObject.IsSpawned)
                {
                    networkObject.Spawn();
                }

                strawberry.Teleport(strawberrySpawnPos.position);
            }

            if (banana != null)
            {
                var networkObject = banana.GetComponent<NetworkObject>();
                Debug.Log($"Banana NetworkObject is {(networkObject.IsSpawned ? "spawned" : "not spawned")} " +
                          $"and {(networkObject.IsOwner ? "owned" : "not owned")} by this client.");
                
                if (!networkObject.IsSpawned)
                {
                    networkObject.Spawn();
                }
                
                banana.Teleport(bananaSpawnPos.position);
            }
        }
    }
}