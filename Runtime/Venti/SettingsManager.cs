using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

namespace Venti
{
    public class SettingsManager : Singleton<SettingsManager>
    {
        public string appKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwcmoiOiJhYzYwMjNiZi01OTE2LTRkYWQtYTRlMS03YTU0NGQzMDRiNWEiLCJhcHAiOiJiNGI5NGYzOC1kNTQzLTRkMTEtYmZiOS0zYjRhYzMzODg5ZjgiLCJqdGkiOiIyYTFiNGVhYS0xYzk3LTQ1OWUtOTdmMC02N2M4ZTRkMTQ4YTYiLCJydGMiOjAsImlzcyI6InNlcnZlciIsImlhdCI6MTc0NTU1NTIzNCwiZXhwIjoxNzQ1OTUxNDAwLCJsdmEiOjE3NDUyNjAyMDAsIm1vZCI6ImRlbW8ifQ.zq8kbVGx_QPXDTT4Y-DM32BiNRYImMZng3aIm-tLxBQ";
        public string serverUrl = "https://venti-events.el.r.appspot.com/api/v1";
        public string getAppAndThemeHashesUrl = @"/experience-app/get-experience-app-configuration-hash";
        public string getAppConfigUrl = @"/experience-app/get-experience-app-configuration";
        public string getThemeUrl = @"/project/get-project-theme-config";

        public float pollInterval = 5f;

        public Experience.ExperienceManager experienceManager;
        public Theme.ThemeManager themeManager;

        private string appHash;
        private string themeHash;

        //private float timer;

        void Start()
        {
            appHash = PlayerPrefs.GetString("appHash", "");
            themeHash = PlayerPrefs.GetString("themeHash", "");

            // Fetch the hashes from the server
            //FetchHashes();
            //timer = Time.time;
        }

        private void Update()
        {
            //if (Time.time - timer > pollInterval)
            //{
            //    FetchHashes();
            //    timer = Time.time;
            //}
        }

        // Fetch the hashes from the server
        public void FetchHashes()
        {
            StartCoroutine(GetHashes());
        }

        // Fetch app config
        public void FetchAppConfig(string hash)
        {
            StartCoroutine(GetAppConfig(hash));
        }

        // Fetch theme config
        public void FetchThemeConfig(string hash)
        {
            StartCoroutine(GetThemeConfig(hash));
        }

        private IEnumerator GetHashes()
        {
            string url = serverUrl + getAppAndThemeHashesUrl;

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Authorization", "Bearer " + appKey);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error fetching hashes: " + www.error);
                }
                else
                {
                    ParseHashesJson(www.downloadHandler.text);
                }
            }
        }
        public void ParseHashesJson(string jsonResponse)
        {//
            JSONObject json = JSON.Parse(jsonResponse).AsObject;

            if (json != null)
            {
                string fetchedAppHash = json["data"]["settings"];
                string fetchedThemeHash = json["data"]["theme"];

                Debug.Log("App Hash: " + fetchedAppHash);
                Debug.Log("Theme Hash: " + fetchedThemeHash);

                if (fetchedAppHash != appHash)
                {
                    FetchAppConfig(fetchedAppHash);
                }

                if (fetchedThemeHash != themeHash)
                {
                    FetchThemeConfig(fetchedThemeHash);
                }
            }
        }
        private IEnumerator GetAppConfig(string hash)
        {
            string url = serverUrl + getAppConfigUrl;

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Authorization", "Bearer " + appKey);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error fetching app config: " + www.error);
                }
                else
                {
                    // Parse the response and update the experience manager
                    string jsonResponse = www.downloadHandler.text;
                    Debug.Log("Fetched app json: " + jsonResponse);

                    string _hash = experienceManager.LoadJsonFromWeb(jsonResponse);
                    if (_hash != null)
                    {
                        appHash = _hash;

                        PlayerPrefs.SetString("appHash", appHash);
                        PlayerPrefs.Save();
                    }
                }
            }
        }

        private IEnumerator GetThemeConfig(string hash)
        {
            string url = serverUrl + getThemeUrl;

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Authorization", "Bearer " + appKey);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error fetching theme config: " + www.error);
                }
                else
                {
                    // Parse the response and update the theme manager
                    string jsonResponse = www.downloadHandler.text;
                    Debug.Log("Fetched theme json: " + jsonResponse);
                    bool success = themeManager.LoadJsonFromWeb(jsonResponse);
                    if (success)
                    {
                        themeHash = hash;
                        //PlayerPrefs.SetString("themeHash", themeHash);
                        //PlayerPrefs.Save();
                    }
                }
            }
        }
    }

}