using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.SceneManagement;
using PimDeWitte.UnityMainThreadDispatcher;
using Venti.Theme;
using Venti.Experience;

namespace Venti
{
    [RequireComponent(typeof(CacheManager))]
    [RequireComponent(typeof(UnityMainThreadDispatcher))]
    public class SettingsManager : Singleton<SettingsManager>
    {
        // public Experience.ExperienceManager experienceManager;
        // public Theme.ThemeManager themeManager;

        private SocketConnector socket;

        public string appKey;
        public const string serverUrl = "https://venti-server-nestjs-128798841108.us-central1.run.app";
        public const string getAppAndThemeHashesUrl = @"/api/v1/experience-app/get-experience-app-configuration-hash";
        public const string getAppConfigUrl = @"/api/v1/experience-app/get-experience-app-configuration";
        public const string getThemeUrl = @"/api/v1/project/get-project-theme-config";

        private string appHash;
        private string themeHash;

        void Start()
        {
            // if (PlayerPrefs.HasKey("appKey"))
            // {
            //     appKey = PlayerPrefs.GetString("appKey");
            //     Debug.Log($"Loaded saved appKey: {appKey}");
            // }
            // else
            // {
            //     SceneManager.LoadScene("QRScanScene");
            //     Debug.LogError("PlayerPrefs doesn't have appKey saved");
            // }

            appHash = PlayerPrefs.GetString("appHash", "");
            themeHash = PlayerPrefs.GetString("themeHash", "");

            // Connect to server socket
            socket = new SocketConnector(serverUrl, appKey);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.F5))
            {
                Debug.Log("Force fetching hashes from server.");
                FetchHashes();
            }
        }

        #region PUBLIC_FUNCTIONS
        // Fetch the hashes from the server
        public void FetchHashes()
        {
            StartCoroutine(GetHashes());
        }

        // Fetch app config
        public void FetchAppConfig(string hash)
        {
            if (hash != appHash)
            {
                Debug.Log("App hashes mismatch. Re-fetch");
                StartCoroutine(GetAppConfig(hash));
            }
            else
                Debug.Log("App hash is the same, no need to fetch again.");
        }

        // Fetch theme config
        public void FetchThemeConfig(string hash)
        {
            if (hash != themeHash)
            {
                Debug.Log("Theme hashes mismatch. Re-fetch");
                Debug.Log("Old hash: " +  themeHash + "\n New hash: " + hash);
                StartCoroutine(GetThemeConfig(hash));
            }
            else
                Debug.Log("Theme hash is the same, no need to fetch again.");
        }

        public void ParseHashesJson(string jsonResponse)
        {
            JSONObject json = JSON.Parse(jsonResponse).AsObject;

            if (json != null)
            {
                //string fetchedAppHash = json["data"]["settings"];
                //string fetchedThemeHash = json["data"]["theme"];
                string fetchedAppHash = json["settings"];
                string fetchedThemeHash = json["theme"];

                Debug.Log("App Hash: " + fetchedAppHash);
                Debug.Log("Theme Hash: " + fetchedThemeHash);

                FetchAppConfig(fetchedAppHash);
                FetchThemeConfig(fetchedThemeHash);
            }
        }
        #endregion

        [ContextMenu("Delete App Key")]
        private void DeletePlayerSavedAppKey()
        {
            if (PlayerPrefs.HasKey("appKey"))
            {
                PlayerPrefs.DeleteKey("appKey");
                PlayerPrefs.Save();
            }

            Debug.Log("appKey has been cleared.");

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

        private IEnumerator GetAppConfig(string hash)
        {
            Debug.Log("GetAppConfig");
            string url = serverUrl + getAppConfigUrl;
            Debug.Log("Fetching app config from: " + url + " with hash: " + hash);

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

                    bool success = ExperienceManager.Instance.LoadFromWebJson(jsonResponse);
                    if (success)
                    {
                        appHash = hash;

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
                    bool success = ThemeManager.Instance.LoadFromWebJson(jsonResponse);
                    if (success)
                    {
                        themeHash = hash;
                        //PlayerPrefs.SetString("themeHash", themeHash);
                        //PlayerPrefs.Save();
                    }
                }
            }
        }

        void OnApplicationQuit()
        {
            socket.Dispose();
        }
    }

}