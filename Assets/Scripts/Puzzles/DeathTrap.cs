using UnityEngine;

namespace Puzzles
{
    [RequireComponent(typeof(Collider2D))]
    public class DeathTrap : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.TryGetComponent<DualCharacter>(out var p))
            {
                p.Die();
            }
        }
    }
}