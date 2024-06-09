using Test;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Try
{
    public class PlayerScore : NetworkBehaviour
    {
        public FruitType team;
        public NetworkVariable<ulong> score = new(1);
        public TMP_Text textObject;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            score.OnValueChanged += OnScoreChanged;

            Debug.Log($"{team} Score on spawn: {score.Value}");
        }

        public override void OnDestroy()
        {
            score.OnValueChanged -= OnScoreChanged;
        }

        private void OnScoreChanged(ulong oldScore, ulong newScore)
        {
            Debug.Log($"Score updated from {oldScore} to {newScore} for team {team}");
            SyncScore(newScore);
        }

        [ServerRpc]
        public void AddScoreServerRpc()
        {
            Debug.Log("Server RPC: Adding score");
            score.Value += 1;
        }

        public void AddScore()
        {
            if (IsServer)
            {
                Debug.Log("Server: Adding score");
                AddScoreServerRpc();
            }
            else
            {
                Debug.Log("AddScore called on non-server instance");
            }
        }

        private void SyncScore(ulong newVal)
        {
            Debug.Log($"Syncing score: {newVal} for team {team}");
            textObject.text = newVal.ToString();
        }
    }
}