using UnityEngine;

namespace Puzzles
{
    [RequireComponent(typeof(Collider2D))]
    public class Step : MonoBehaviour
    {
        public StateSwitcher interactable;
        private Animator _anim;
        private static readonly int StepBool = Animator.StringToHash("Step");

        public void Start() 
        {
            _anim = GetComponent<Animator>();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            interactable.Switch();
            _anim.SetBool(StepBool, true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            interactable.Switch();
            _anim.SetBool(StepBool, false); 
        }
    }
}