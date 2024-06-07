using System.Collections;
using TMPro;
using UnityEngine;

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
        
        private DatabaseManager _dbManager;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => SceneManagement.Instance != null);
            _dbManager = FindObjectOfType<DatabaseManager>();
            FetchProfile();
        }
        
        public void FetchProfile()
        {
            UserProfile? profile = _dbManager.GetUserProfile(SceneManagement.Instance.LoggedInUser);
            
            if (profile != null)
            {
                username.text = profile.Value.Username;
                matchesPlayed.text = profile.Value.MatchesPlayed.ToString();
                wins.text = profile.Value.Wins.ToString();
                losses.text = profile.Value.Losses.ToString();
                totalScore.text = profile.Value.TotalScore.ToString();
            }
            else
            {
                Debug.LogError("Profile loading failed");
            }
        }
        
        public void ReturnToMain()
        {
            SceneManagement.Instance.GoToMainMenu();
        }

        /*public void FetchProfile()
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
        }*/
    }
}
