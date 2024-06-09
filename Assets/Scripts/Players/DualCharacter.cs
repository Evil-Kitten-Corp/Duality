using System;
using System.Collections;
using Puzzles;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DualCharacter : NetworkBehaviour
{
    [Header("General Settings")] 
    public float speed;
    public float jumpForce;
    [Range(0, .3f)] [SerializeField] private float mMovementSmoothing = .05f;

    [Header("References")] 
    public DualGlobalData globals;
    public SpriteRenderer spriteRenderer;
    public Animator anim;
    public Rigidbody2D rb;

    public Action OnDeath;

    public DualChoice CharacterType { get; private set; }
    
    private LevelManager _levelManager;
    
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int Die1 = Animator.StringToHash("Die");
    private static readonly int Grounded = Animator.StringToHash("Grounded");

    private Vector3 _velocity = Vector3.zero;
    private const float LimitFallSpeed = 25f;

    public bool canDoubleJump = true;
    private int _jumpAttempts = 1;

    private bool _canJump = true;
    
    public ParticleSystem particleJumpUp; 
    public ParticleSystem particleJumpDown;
    
    private readonly NetworkVariable<bool> _isFacingRight = new(true, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private int CurrentPlayers() => _levelManager.connectedClients.Value;

    public void Start()
    {
        _levelManager = FindObjectOfType<LevelManager>();
        
        _isFacingRight.OnValueChanged += (_, newValue) =>
        {
            spriteRenderer.flipX = newValue;
        };
        
        if (canDoubleJump)
        {
            _jumpAttempts = 2;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwnedByServer)
        {
            CharacterType = DualChoice.Spectator;
            SetUpCharacter();
            _levelManager.connectedClients.Value--;
        }
        
        switch (CurrentPlayers())
        {
            case 2:
            {
                CharacterType = DualChoice.BananaBoy;
                SetUpCharacter();
                return;
            }
            case > 2:
                CharacterType = DualChoice.Spectator;
                SetUpCharacter();
                return;
        }
        
        CharacterType = DualChoice.StrawberryBoy;
        SetUpCharacter();
    }

    private void SetUpCharacter()
    {
        if (CharacterType == DualChoice.Spectator)
        {
            anim.enabled = false;
            spriteRenderer.sprite = null;
            spriteRenderer.enabled = false;
            GetComponent<Collider2D>().enabled = false;
            rb.Sleep(); 
            _levelManager.RemoveListeners(this);
            return;
        }
        
        globals.GetDualSetUp(CharacterType, out Sprite s, out RuntimeAnimatorController a, out int layer);
        anim.runtimeAnimatorController = a;
        spriteRenderer.sprite = s;
        gameObject.layer = layer;
    }

    private void Update()
    {
        if (IsLocalPlayer && CharacterType != DualChoice.Spectator)
        {
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
                particleJumpDown.Play();
                particleJumpUp.Play();
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

    public void Die()
    {
        anim.SetTrigger(Die1);
        OnDeath?.Invoke();
    }
}