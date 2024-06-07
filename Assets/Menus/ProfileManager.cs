using System.Collections;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;


namespace DefaultNamespace
{
    public class ProfileManager : MonoBehaviour
    {
        public TMP_Text matchesPlayed;
        public TMP_Text wins;
        public TMP_Text losses;
        public TMP_Text totalScore;
        public TMP_Text username;
        public string url;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => SceneManagement.Instance != null);
            FetchProfile();
        }

        public void FetchProfile()
        {
            StartCoroutine(FetchProfileCo());
        }

        private IEnumerator FetchProfileCo()
        {
            WWWForm form = new WWWForm();
            form.AddField("username", SceneManagement.Instance.LoggedInUser);

            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error fetching profile");
                }
                else
                {
                    var profileJson = webRequest.downloadHandler.text;
                    var profile = JObject.Parse(profileJson);

                    username.text = SceneManagement.Instance.LoggedInUser;
                    matchesPlayed.text = profile["matches_played"]?.ToString();
                    wins.text = profile["wins"]?.ToString();
                    losses.text = profile["losses"]?.ToString();
                    totalScore.text = profile["total_score"]?.ToString();
                }
            }
        }
    }
}
