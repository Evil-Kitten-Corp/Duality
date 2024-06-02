using System;
using System.Collections;
using Test;
using Unity.Netcode;
using UnityEngine;

namespace Try
{
    public class PlayerMove : NetworkBehaviour
    {
        [Header("General Settings")] 
        public float speed;
        public float jumpForce;
        [Range(0, .3f)] public float movementSmoothing = .05f;

        [Header("References")] 
        public Animator anim;
        public Rigidbody2D rb;
        public SpriteRenderer sr;

        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int Walking = Animator.StringToHash("Walking");
        private static readonly int Grounded = Animator.StringToHash("Grounded"); 
        private static readonly int Death = Animator.StringToHash("Death");
        private static readonly int WinBool = Animator.StringToHash("Win");
        
        private Vector3 _velocity = Vector3.zero;
        private const float LimitFallSpeed = 25f;

        public bool canDoubleJump = true;
        private int _jumpAttempts = 1;
        private bool _canJump = true;

        private readonly NetworkVariable<bool> _isFacingRight = new(true, 
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public FruitType fruit;

        private bool _canControl = true;

        public static Action<FruitType> OnWin;
        public static Action<FruitType> OnCancelWin;

        private void OnEnable()
        {
            GameControllerButton.OnButtonPress += OnUIButtonPress;
        }

        private void OnDisable()
        {
            GameControllerButton.OnButtonPress -= OnUIButtonPress;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            _isFacingRight.OnValueChanged += (_, newValue) =>
            {
                sr.flipX = newValue;
            };
        
            if (canDoubleJump)
            {
                _jumpAttempts = 2;
            }
            
            OnEnable();
        }

        private void Update()
        {
            if (IsOwner && _canControl)
            {
                CheckGrounded();

                float move = Input.GetAxis("Horizontal");

                Vector3 targetVelocity = new Vector2(move * speed, rb.velocity.y);
                
                rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, 
                    ref _velocity, movementSmoothing);

                if (Input.GetKeyDown(KeyCode.Space) && _canJump)
                {
                    rb.AddForce(new Vector2(0f, jumpForce));
                    anim.SetTrigger(Jump);
                    _jumpAttempts--;

                    if (_jumpAttempts <= 0)
                    {
                        _canJump = false;
                        StartCoroutine(JumpCooldown());
                    }
                }

                _isFacingRight.Value = move switch
                {
                    > 0 => false,
                    < 0 => true,
                    _ => _isFacingRight.Value
                };

                anim.SetBool(Walking, move != 0);
            }
            else if (IsOwner && !_canControl)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    _canControl = true;
                }
            }
        }
        
        private IEnumerator JumpCooldown()
        {
            yield return new WaitForSeconds(.6f);
            _canJump = true;
            _jumpAttempts = canDoubleJump ? 2 : 1;
        }

        private void CheckGrounded()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f);
            bool isGrounded = hit.collider != null;
            anim.SetBool(Grounded, isGrounded); 
        }

        public void Die()
        {
            rb.AddForce(new Vector2(-150, 450));
            rb.gravityScale = 0.1f;
            _canControl = false;
            anim.SetTrigger(Death);
        }
        
        private void OnUIButtonPress(GameControllerButton.ButtonType buttonType)
        {
            if (!IsOwner)
                return;
            
            Debug.Log("Calling Button Press!");

            switch (buttonType)
            {
                case GameControllerButton.ButtonType.RestartReady:
                    RestartServerRpc();
                    break;
            }
        }
        
        [ServerRpc]
        private void RestartServerRpc()
        {
            var levelControl = FindObjectOfType<SpawnRealPlayer>();
            
            if (levelControl != null)
            {
                Debug.Log("Found Level Controller!");
                StartCoroutine(levelControl.Restart(fruit));
            }
        }

        public void Win()
        {
            _canControl = false;
            anim.SetBool(Walking, false);
            anim.SetBool(WinBool, true);
            OnWin?.Invoke(fruit);
        }

        public void CancelWin()
        {
            anim.SetBool(WinBool, false);
            OnCancelWin?.Invoke(fruit);
        }
    }
}