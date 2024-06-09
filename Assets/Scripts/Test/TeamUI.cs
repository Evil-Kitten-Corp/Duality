using TMPro;
using Unity.Netcode;

namespace Test
{
    public class TeamUI : NetworkBehaviour
    {
        public TMP_Text score;
        public int internalScore;
        
        [ClientRpc]
        void UpdateHealthClientRpc(float currentHealth)
        {
            if (IsServer)
                return;
            
            if (currentHealth <= 0f)
            { 
                LevelController.Instance.deathUI.SetActive(true);
            }
        }

        public void UpdateHealth(int currentHealth)
        {
            if (!IsServer)
                return;

            currentHealth = currentHealth < 0 ? 0 : currentHealth;

            if (currentHealth <= 0)
            {
                LevelController.Instance.deathUI.SetActive(true);
            }

            UpdateHealthClientRpc(currentHealth);
        }

        public void UpdateScore()
        {
            if (!IsServer)
                return;
            
            internalScore++;
            score.text = internalScore.ToString();

            UpdatePowerUpClientRpc();
        }

        [ClientRpc]
        void UpdatePowerUpClientRpc()
        {
            if (IsServer)
                return;
            
            score.text = internalScore.ToString();
        }
    }
}