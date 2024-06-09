using Unity.Netcode;
using UnityEngine;

namespace Test
{
    public class Fruit : MonoBehaviour
    {
        public FruitType fruit;
        
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.TryGetComponent<PlayerChar>(out var p) && p != null)
            {
                if (p.mCharacterData == null)
                {
                    Debug.LogError("PlayerChar.characterData is null");
                    return;
                }

                if (p.mCharacterData.characterType == fruit)
                {
                    p.AddScoreServerRpc();
            
                    NetworkObject networkObject = GetComponent<NetworkObject>();
                    if (networkObject != null)
                    {
                        NetworkMonitor.Instance.DespawnNetworkObject(networkObject);
                    }
                    else
                    {
                        Debug.LogError("NetworkObject component is missing");
                    }
                }
            }
            else
            {
                Debug.LogError("Collider does not have a PlayerChar component or PlayerChar is null");
            }
        }

    }
}