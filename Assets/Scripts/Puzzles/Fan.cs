using UnityEngine;

namespace Puzzles
{
    [RequireComponent(typeof(Collider2D))]
    public class Fan : StateSwitcher
    {
        [SerializeField] private bool active;
    
        public override void Switch()
        {
            active = !active;
        }
        
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (active)
            {
                collision.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 0.5f, ForceMode2D.Impulse);
            }
        }
    }
}