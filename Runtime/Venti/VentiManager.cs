using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.SceneManagement;
using PimDeWitte.UnityMainThreadDispatcher;
using Venti.Theme;
using Venti.Experience;
using Venti.Token;
using System;
using System.Text;
using System.Net.Http;

namespace Venti
{
    [RequireComponent(typeof(CacheManager))]
    [RequireComponent(typeof(UnityMainThreadDispatcher))]
    public class VentiManager : Singleton<VentiManager>
    {
        // public Experience.ExperienceManager experienceManager;
        // public Theme.ThemeManager themeManager;

        private SocketConnector socket;

        public const string serverUrl = "https://venti-server-nestjs-128798841108.us-central1.run.app";
        public const string getAppAndThemeHashesUrl = @"/api/v1/experience-app/get-experience-app-configuration-hash";
        public const string getThemeUrl = @"/api/v1/project/get-project-theme-config";


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




            // Connect to server socket
            socket = new SocketConnector(serverUrl, TokenManager.Instance.appKey);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
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

        public void ParseHashesJson(string jsonStr)
        {
            JSONObject json = JSON.Parse(jsonStr).AsObject;

            if (json != null)
            {
                //string fetchedAppHash = json["data"]["settings"];
                //string fetchedThemeHash = json["data"]["theme"];
                string fetchedAppHash = json["settings"];
                string fetchedThemeHash = json["theme"];

                Debug.Log("App Hash: " + fetchedAppHash);
                Debug.Log("Theme Hash: " + fetchedThemeHash);

                ExperienceManager.Instance?.FetchAppConfig(fetchedAppHash);
                ThemeManager.Instance?.FetchThemeConfig(fetchedThemeHash);
            }
        }

        // Make an authenticated web request to Venti server
        public void VentiWebRequest(string url, WebRequestMethod method, WWWForm form, Action<string> callback)
        {
            StartCoroutine(VentiWebRequestCoroutine(url, method, form, callback));
        }

        private IEnumerator VentiWebRequestCoroutine(string url, WebRequestMethod method, WWWForm form, Action<string> callback)
        {
            UnityWebRequest www;

            switch (method)
            {
                case HttpMethod.Get:
                    www = UnityWebRequest.Get(url);
                    break;
                case HttpMethod.Post:
                    www = UnityWebRequest.Post(url, form);
                    break;
                case HttpMethod.Put:
                    www = UnityWebRequest.Put(url, form);
                    break;
                case HttpMethod.Delete:
                    www = UnityWebRequest.Delete(url);
                    break;
                default:
                    throw new ArgumentException("Invalid HTTP method");
            }

            using (UnityWebRequest www = new UnityWebRequest(url, method))
            {
                www.SetRequestHeader("Authorization", "Bearer " + TokenManager.Instance.appKey);
                www.SetRequestHeader("Content-Type", "application/json");
                // www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error fetching hashes: " + www.error);
                }
                else
                {
                    callback(www.downloadHandler.text);
                }
            }
        }

        #endregion

        #region PRIVATE_FUNCTIONS
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
        #endregion
        void OnApplicationQuit()
        {
            socket.Dispose();
        }
    }

}