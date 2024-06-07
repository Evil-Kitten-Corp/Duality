using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace DefaultNamespace
{
    public class LoginAuth : MonoBehaviour
    {
        public TMP_Text error;
        public TMP_InputField username;
        public TMP_InputField password;
        public string url;
        
        private WWWForm _form;

        public IEnumerator Authenticate(System.Action<bool?> callback)
        {
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
                        error.text = "Invalid username or password";
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