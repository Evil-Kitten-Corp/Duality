using System;
using TMPro;
using UnityEngine;

public class SignupManager : MonoBehaviour
{
    public TMP_Text errorText;
    
    public void ReturnToBoot()
    {
        SceneManagement.Instance.LoadSceneAs("Bootstrap", null);
    }

    public void ValidateRegister()
    {
        if ("" == "")
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "Username already exists.";
        }
        else
        {
            SceneManagement.Instance.LoginSuccessful();
        }
    }
}
