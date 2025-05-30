using System.Collections;
using UnityEngine;
using SimpleJSON;
using PimDeWitte.UnityMainThreadDispatcher;
using Venti.Theme;
using Venti.Experience;
using UnityEngine.SceneManagement;

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

        void OnEnable()
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

            SessionManager.Instance?.FetchActiveSession();

            // Connect to server socket
            // socket = new SocketConnector(VentiApiRequest.serverUrl, TokenManager.Instance?.appKey);
            socket = new SocketConnector(VentiApiRequest.serverUrl, TokenManager.Instance?.refreshToken);
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    Debug.Log("Force fetching hashes from server.");
                    StartCoroutine(GetHashes());
                }

                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    // Debug.Log("Resetting sessions.");
                    // SessionManager.Instance?.ResetSessions();
                    Debug.Log("Ending active session.");
                    SessionManager.Instance?.EndSession();
                }

                //if (Input.GetKeyDown(KeyCode.R))
                //{
                //    Debug.Log("Reloading scene.");
                //    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                //}

                if (Input.GetKeyDown(KeyCode.T))
                {
                    // Debug.Log("Token: " + TokenManager.Instance?.refreshToken);
                    Debug.Log("Registration Tab URL: " + SessionManager.Instance.GetRegistrationTabUrl());
                    Debug.Log("Companion Tab URL: " + SessionManager.Instance.GetCompanionTabUrl());
                    // Debug.Log(SessionManager.Instance.GetCheckInAppUrl("https://google.com/?foo=bar", "1234567890"));
                }
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

        void OnDisable()
        {
            if (socket != null)
                socket.Dispose();
        }

        void OnApplicationQuit()
        {
            if (socket != null)
                socket.Dispose();
        }
    }

}