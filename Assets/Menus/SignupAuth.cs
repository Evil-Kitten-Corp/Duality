using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace DefaultNamespace
{
    public class SignUpAuth : MonoBehaviour
    {
        public TMP_Text error;
        public TMP_InputField username;
        public TMP_InputField password;
        public TMP_InputField confirmPassword;
        public string url;

        public IEnumerator SignUp(System.Action<bool?> callback)
        {
            if (password.text != confirmPassword.text)
            {
                error.text = "Passwords do not match";
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
                    error.text = "404 not found";
                    callback(false);
                }
                else
                {
                    if (webRequest.downloadHandler.text.Contains("error"))
                    {
                        error.text = webRequest.downloadHandler.text;
                        callback(false);
                    }
                    else
                    {
                        callback(true);
                    }
                }
            }
        }
    }
}