using Test;
using Try;
using Unity.Netcode;
using UnityEngine;

public class WinPoint : MonoBehaviour
{
    public FruitType winTeam;
    public GameObject inputPrompt;
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        if (!NetworkManager.Singleton.IsServer) return;

        if (col.TryGetComponent(out PlayerMove pm) && pm.fruit == winTeam)
        {
            pm.Win();
            ShowInputPromptClientRpc(pm.OwnerClientId);
        }
    }
    
    private void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        if (!NetworkManager.Singleton.IsServer) return;

        if (col.TryGetComponent(out PlayerMove pm) && pm.fruit == winTeam)
        {
            pm.CancelWin();
            HideInputPromptClientRpc(pm.OwnerClientId);
        }
    }
    
    [ClientRpc]
    private void ShowInputPromptClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            inputPrompt.SetActive(true);
        }
    }
    
    [ClientRpc]
    private void HideInputPromptClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            inputPrompt.SetActive(false);
        }
    }
}
