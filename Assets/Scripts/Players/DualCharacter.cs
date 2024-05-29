using Unity.Netcode;
using UnityEngine;
using Utils;

[RequireComponent(typeof(Collider2D))]
public class DualCharacter : MonoBehaviour
{
    [Header("General Settings")] 
    public float speed;
    
    [Header("References")] 
    public SpriteRenderer spriteRenderer;
    public Animator anim;

    public DualChoice CharacterType { get; private set; }
    
    private NetworkManager _networkManager;
    private DualGlobalData _globals;
    private NetworkObject _networkObject;
    
    private int CurrentPlayers => (_networkManager) ? (_networkManager.ConnectedClients.Count) : (0);
    
    private void Start()
    {
        _networkObject = GetComponent<NetworkObject>();
        _networkManager = FindObjectOfType<NetworkManager>();
        _globals = this.GetGlobalData();
        
        if (_globals == null)
        {
            Debug.LogError("Couldn't find globals in Resource folder!");
        }

        if (CurrentPlayers > 1)
        {
            DualCharacter otherPlayer = FindObjectOfType<DualCharacter>();

            if (otherPlayer != null && otherPlayer.CharacterType == DualChoice.WaterElemental)
            {
                CharacterType = DualChoice.FireElemental;
                SetUpCharacter();
                return;
            }
        }
        
        CharacterType = DualChoice.WaterElemental;
        SetUpCharacter();
    }

    private void SetUpCharacter()
    {
        _globals.GetDualSetUp(CharacterType, out Sprite s, out RuntimeAnimatorController a, out LayerMask layer);
        anim.runtimeAnimatorController = a;
        spriteRenderer.sprite = s;
        gameObject.layer = layer;
    }

    private void Update()
    {
        if (_networkObject.IsLocalPlayer)
        {
            Vector3 moveDir = Vector3.zero;
            
            moveDir.x = speed * Input.GetAxis("Horizontal");
            moveDir.y = speed * Input.GetAxis("Vertical");
            
            moveDir *= Time.deltaTime;
            transform.Translate(moveDir, Space.World);
        }
    }

    public void Die()
    {
        
    }
}