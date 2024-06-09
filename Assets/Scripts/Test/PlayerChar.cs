using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Test
{
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class PlayerChar : NetworkBehaviour
    {
        public NetworkVariable<int> lives = new(1, NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Owner);
        
        [SerializeField] public ClientData mCharacterData;
        [SerializeField] SpriteRenderer mShipRenderer;
        
        [Header("Runtime set")] 
        public TeamUI playerUI;
        public ClientData characterData;
        
        bool _mIsPlayerDefeated;
        
        [Header("General Settings")] 
        public float speed;
        public float jumpForce;
        [Range(0, .3f)] public float mMovementSmoothing = .05f;

        [Header("References")] 
        public Animator anim;
        public Rigidbody2D rb;

        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int Walking = Animator.StringToHash("Walking");
        private static readonly int Grounded = Animator.StringToHash("Grounded");

        private Vector3 _velocity = Vector3.zero;
        private const float LimitFallSpeed = 25f;

        public bool canDoubleJump = true;
        private int _jumpAttempts = 1;
        private bool _canJump = true;

        private readonly NetworkVariable<bool> _isFacingRight = new(true, 
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private void OnEnable()
        {
            ConfirmationButton.OnButtonPress += OnUIButtonPress;
        }

        private void OnDisable()
        {
            ConfirmationButton.OnButtonPress -= OnUIButtonPress;
        }

        private void Start()
        {
            _isFacingRight.OnValueChanged += (_, newValue) =>
            {
                mShipRenderer.flipX = newValue;
            };
        
            if (canDoubleJump)
            {
                _jumpAttempts = 2;
            }
        }

        void Update()
        {
            if (IsOwner)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (IsServer)
                    {
                        StartCoroutine(HostShutdown());
                    }
                    else
                    {
                        Shutdown();
                    }
                }
                
                CheckGrounded();
            
                if (rb.velocity.y < -LimitFallSpeed)
                    rb.velocity = new Vector2(rb.velocity.x, -LimitFallSpeed);

                float move = Input.GetAxis("Horizontal");

                Vector3 targetVelocity = new Vector2(move * speed, rb.velocity.y);
                rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, 
                    ref _velocity, mMovementSmoothing);

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

        IEnumerator HostShutdown()
        {
            ShutdownClientRpc();
            yield return new WaitForSeconds(0.5f);
            Shutdown();
        }

        void Shutdown()
        {
            NetworkManager.Singleton.Shutdown();
        }

        [ClientRpc]
        void ShutdownClientRpc()
        {
            if (IsServer)
                return;

            Shutdown();
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddScoreServerRpc()
        {
            if (characterData == null)
            {
                Debug.Log("Character data is null, logging score into fallback.");
                mCharacterData.totalScore++;
            }
            else
            {
                characterData.totalScore++;
            }

            playerUI.UpdateScore();
        }

        public void Kill()
        {
            if (_mIsPlayerDefeated)
                return;

            lives.Value -= 1;
            playerUI.UpdateHealth(lives.Value);
            
            if (lives.Value <= 0)
            {
                _mIsPlayerDefeated = true;
                LevelController.Instance.PlayerDeath(characterData.clientId);
            }
        }
        
        private void OnUIButtonPress(ButtonActions buttonAction)
        {
            if (!IsOwner)
                return;

            switch (buttonAction)
            {
                case ButtonActions.RestartReady:
                    RestartServerRpc();
                    break;

                case ButtonActions.WinReady:
                    WinServerRpc();
                    break;
            }
        }
        
        [ServerRpc]
        private void RestartServerRpc()
        {
            StartCoroutine(LevelController.Instance.Restart(characterData.characterType));
        }

        [ServerRpc]
        private void WinServerRpc()
        {
            StartCoroutine(LevelController.Instance.Win(characterData.characterType));
        }
    }
}