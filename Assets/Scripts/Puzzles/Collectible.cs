using Unity.Netcode;
using UnityEngine;

namespace Puzzles
{
    [RequireComponent(typeof(Collider2D), typeof(NetworkObject))]
    public class Collectible : MonoBehaviour
    {
        public DualChoice team;
        private LevelManager _levelManager;

        private void Start()
        {
            _levelManager = FindObjectOfType<LevelManager>();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.GetComponent<DualCharacter>().CharacterType == team)
            {
                _levelManager.AddScore(team);
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}