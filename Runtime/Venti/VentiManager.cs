using System.Collections;
using UnityEngine;
using SimpleJSON;
using PimDeWitte.UnityMainThreadDispatcher;
using Venti.Theme;
using Venti.Experience;
using Venti.Token;

namespace Venti
{
    [RequireComponent(typeof(CacheManager))]
    [RequireComponent(typeof(TokenManager))]
    [RequireComponent(typeof(SessionManager))]
    [RequireComponent(typeof(UnityMainThreadDispatcher))]
    public class VentiManager : Singleton<VentiManager>
    {
        private SocketConnector socket;
        public const string getAppAndThemeHashesUrl = @"/experience-app/get-experience-app-configuration-hash";

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
            socket = new SocketConnector(VentiApiRequest.serverUrl, TokenManager.Instance?.appKey);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5)
                && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                Debug.Log("Force fetching hashes from server.");
                StartCoroutine(GetHashes());
            }
        }

        #region PUBLIC_FUNCTIONS
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
        #endregion

        #region PRIVATE_FUNCTIONS
        private IEnumerator GetHashes()
        {
            using (VentiApiRequest www = VentiApiRequest.PostApi(getAppAndThemeHashesUrl, new WWWForm()))
            {
                yield return www.SendAuthenticatedApiRequest();

                if (www.result != VentiApiRequest.Result.Success)
                    Debug.LogError("Error fetching hashes: " + www.error);
                else
                    ParseHashesJson(www.downloadHandler.text);
            }
        }
        #endregion

        void OnApplicationQuit()
        {
            if (socket != null)
                socket.Dispose();
        }
    }

}