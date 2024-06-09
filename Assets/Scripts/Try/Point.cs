using Test;
using Unity.Netcode;
using UnityEngine;

namespace Try
{
    public class Point : NetworkBehaviour
    {
        public FruitType team;
        
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.CompareTag("Player")) return;
            if (!NetworkManager.Singleton.IsServer) return;

            if (col.TryGetComponent(out PlayerScore ps) && ps.team == team)
            {
                FindObjectOfType<LevelController>().AddScore(team);
                gameObject.SetActive(false);
            }
            else if (col.TryGetComponent(out PlayerMove pm) && pm.fruit == team)
            {
                FindObjectOfType<LevelController>().AddScore(team);
                gameObject.SetActive(false);
            }
        }
    }
}