using UnityEngine;

namespace Test
{
    [RequireComponent(typeof(Collider2D))]
    public class FruitTerritory : MonoBehaviour
    {
        public FruitType type;
        
        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.TryGetComponent<PlayerChar>(out var p) && p.mCharacterData != null && 
                p.mCharacterData.characterType != type)
            {
                p.Kill();
            }
        }
    }
}