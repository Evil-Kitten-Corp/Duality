using System;
using UnityEngine;

namespace Try
{
    public class LevelSectioner : MonoBehaviour
    {
        public Transform cameraLookAt;
        
        public Transform strawberrySpawnPos;
        public Transform bananaSpawnPos;
        public GameObject[] collectibles;

        public void Restart(PlayerMove strawberry, PlayerMove banana)
        {
            Time.timeScale = 1;
            
            foreach (var c in collectibles)
            {
                c.SetActive(true);
            }

            if (strawberry != null)
            {
                //strawberry.transform.position = strawberrySpawnPos.position;
                Debug.Log("Strawberry isn't null.");
                strawberry.Revive(strawberrySpawnPos.position);
            }

            if (banana != null)
            {
                //banana.transform.position = bananaSpawnPos.position;
                Debug.Log("Banana isn't null.");
                banana.Revive(bananaSpawnPos.position);
            }
        }
    }
}