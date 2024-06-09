using UnityEngine;

namespace Test
{
    [CreateAssetMenu(menuName = "SO/Client Data", fileName = "ClientData", order = 0)]
    public class ClientData : ScriptableObject 
    {
        [Header("Data")]
        public Sprite characterSprite;
        public Sprite iconSprite;
        public FruitType characterType; 
        public GameObject playerPrefab;
        
        [Header("Client Info")]
        public ulong clientId;
        public int playerId;
        public bool wasLockedIn;

        [Header("Score")] 
        public int totalScore;

        void OnEnable()
        {
            EmptyData();
        }

        public void EmptyData()
        {
            wasLockedIn = false;
            clientId = 0;
            playerId = -1;
            totalScore = 0;
        }
    }
}