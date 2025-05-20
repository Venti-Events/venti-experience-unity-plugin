using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Venti.Token
{
    public class TokenManager : Singleton<TokenManager>
    {
        public string appKeyScanScene = "AppKeyScan";
        public string appKey { get; private set; }
        public string refreshToken { get; private set; }
        public string accessToken { get; private set; }

        public VentiApiRequest.Result refreshTokenResult { get; private set; }

        public const string refreshTokenUrl = "/project-license/get-access-token";

        void Start()
        {
            if (PlayerPrefs.HasKey("appKey"))
            {
                appKey = PlayerPrefs.GetString("appKey");
                Debug.Log($"Loaded saved appKey: {appKey}");
            }
            else
            {
                Debug.LogError("PlayerPrefs doesn't have appKey saved");
                SceneManager.LoadScene(appKeyScanScene);
                return;
            }

            if (PlayerPrefs.HasKey("refreshToken"))
            {
                refreshToken = PlayerPrefs.GetString("refreshToken");
                Debug.Log($"Loaded saved refreshToken: {refreshToken}");
            }
            else
            {
                Debug.LogError("PlayerPrefs doesn't have refreshToken saved");
                RefreshToken();
                return;
            }

            if (PlayerPrefs.HasKey("accessToken"))
            {
                accessToken = PlayerPrefs.GetString("accessToken");
                Debug.Log($"Loaded saved accessToken: {accessToken}");
            }
            else
            {
                Debug.LogError("PlayerPrefs doesn't have accessToken saved");
                RefreshToken();
                return;
            }
        }

        [ContextMenu("Delete App Key")]
        private void DeleteSavedAppKey()
        {
            if (PlayerPrefs.HasKey("appKey"))
            {
                PlayerPrefs.DeleteKey("appKey");
                PlayerPrefs.Save();
            }

            Debug.Log("appKey has been deleted");
        }

        public void RefreshToken()
        {
            StartCoroutine(RefreshTokenCoroutine());
        }

        public IEnumerator RefreshTokenCoroutine()
        {
            using (VentiApiRequest request = VentiApiRequest.PostApi(refreshTokenUrl,
                    new Dictionary<string, string> { { "token", appKey } }))
            {
                yield return request.SendAuthenticatedApiRequest(false);

                refreshTokenResult = request.result;

                if (refreshTokenResult != VentiApiRequest.Result.Success)
                {
                    if (refreshTokenResult == VentiApiRequest.Result.ProtocolError && request.responseCode == 401)
                    {
                        Debug.LogError("App-Key is invalid");

                        // Go to App-Key QR Scan Scene
                        SceneManager.LoadScene(appKeyScanScene);
                        yield break;
                    }

                    Debug.LogError("Failed to refresh token");
                    yield break;
                }

                JSONObject response = JSON.Parse(request.downloadHandler.text).AsObject;
                if (response["data"] != null)
                {
                    JSONObject data = response["data"].AsObject;
                    SetTokens(data["newRefreshToken"].Value, data["accessToken"].Value);
                }
            }
        }

        private void SetTokens(string refreshToken, string accessToken)
        {
            this.refreshToken = refreshToken;
            this.accessToken = accessToken;

            PlayerPrefs.SetString("refreshToken", refreshToken);
            PlayerPrefs.SetString("accessToken", accessToken);
            PlayerPrefs.Save();
        }

        // private class TokenResponse
        // {
        //     public string newRefreshToken;
        //     public string accessToken;
        // }
    }
}
