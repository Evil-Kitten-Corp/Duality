using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class SignupManager : MonoBehaviour
{
    public TMP_Text errorText;
    public TMP_InputField username;
    public TMP_InputField password;
    public TMP_InputField confirmPassword;
    public string url;
    
    public void ReturnToBoot()
    {
        SceneManagement.Instance.LoadSceneAs("Bootstrap", null);
    }

    private IEnumerator SignUp(Action<bool?> callback)
    {
        if (password.text != confirmPassword.text)
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "Passwords do not match";
            callback(false);
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("username", username.text);
        form.AddField("password", password.text);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                errorText.gameObject.SetActive(true);
                errorText.text = "404 not found";
                callback(false);
            }
            else
            {
                if (webRequest.downloadHandler.text.Contains("error"))
                {
                    errorText.gameObject.SetActive(true);
                    errorText.text = webRequest.downloadHandler.text;
                    callback(false);
                }
                else
                {
                    callback(true);
                }
            }
        }
    }

    public void ValidateRegister()
    {
        StartCoroutine(ValidateRegisterCo());
    }

    private IEnumerator ValidateRegisterCo()
    {
        bool? result = null;
        yield return StartCoroutine(SignUp((res) => result = res));

        if (result == true)
        {
            SceneManagement.Instance.LoginSuccessful(username.text);
        }
    }
}
