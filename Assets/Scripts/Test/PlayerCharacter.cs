using Unity.Netcode;
using UnityEngine;

namespace Test
{
    public enum SpriteType
    {
        Red,
        Blue, 
        Yellow
    }

    public class PlayerCharacter : NetworkBehaviour
    {
        public SpriteRenderer spriteRenderer;

        public Sprite redSprite;
        public Sprite blueSprite;
        public Sprite yellowSprite;

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                SetPlayerColorRpc(NetworkManager.Singleton.LocalClientId);
            }
        }
        
        [Rpc(SendTo.Server)]
        private void SetPlayerColorRpc(ulong clientId)
        {
            int clientCount = ClientManager.Instance.clientCount.Value;

            SpriteType chosenSpriteType = clientCount == 0 ? SpriteType.Red : clientCount == 1 ? SpriteType.Blue : SpriteType.Yellow;

            SetPlayerColorRpc(chosenSpriteType);
        }

        [Rpc(SendTo.Everyone)]
        private void SetPlayerColorRpc(SpriteType chosenSpriteType)
        {
            switch (chosenSpriteType)
            {
                case SpriteType.Red:
                    spriteRenderer.sprite = redSprite;
                    break;
                case SpriteType.Blue:
                    spriteRenderer.sprite = blueSprite;
                    break;
                case SpriteType.Yellow:
                    spriteRenderer.sprite = yellowSprite;
                    break;
            }
        }

        private void Update()
        {
            if (IsOwner)
            {
                float move = Input.GetAxis("Horizontal");
                float moveUp = Input.GetAxis("Vertical");

                transform.Translate(Vector3.right * (move * 10 * Time.deltaTime), Space.World);
                transform.Translate(Vector3.up * (moveUp * 10 * Time.deltaTime), Space.World);
            }
        }
    }
}