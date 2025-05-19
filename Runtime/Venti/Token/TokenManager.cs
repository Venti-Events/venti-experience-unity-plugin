using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Venti.Token
{
    public class TokenManager : Singleton<TokenManager>
    {
        public string appKey { get; private set; }
        public string refreshToken { get; private set; }
        public string accessToken { get; private set; }

        public VentiApiRequest.Result refreshTokenResult { get; private set; }

        public const string refreshTokenUrl = "/auth/refresh-token";

        void Start()
        {
            if (PlayerPrefs.HasKey("appKey"))
            {
                appKey = PlayerPrefs.GetString("appKey");
                Debug.Log($"Loaded saved appKey: {appKey}");
            }
            else
            {
                // SceneManager.LoadScene("QRScanScene");
                Debug.LogError("PlayerPrefs doesn't have appKey saved");
            }
        }

        public void RefreshToken()
        {
            // TODO: Refresh token
        }

        public IEnumerator RefreshTokenCoroutine()
        {
            using (VentiApiRequest request = VentiApiRequest.Post(refreshTokenUrl, new Dictionary<string, string> { { "appKey", appKey } }))
            {
                yield return request.SendAuthenticatedApiRequest(false);

                refreshTokenResult = request.result;

                if (refreshTokenResult != VentiApiRequest.Result.Success)
                {
                    if (refreshTokenResult == VentiApiRequest.Result.ProtocolError && request.responseCode == 401)
                    {
                        Debug.LogError("App-Key is invalid");

                        // Go to App-Key QR Scan Scene
                        SceneManager.LoadScene("QRScanScene");
                        yield break;
                    }

                    Debug.LogError("Failed to refresh token");
                    yield break;
                }

                TokenResponse response = JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text);
                SetTokens(response.refreshToken, response.accessToken);
            }
        }

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

        private void SetTokens(string refreshToken, string accessToken)
        {
            this.refreshToken = refreshToken;
            this.accessToken = accessToken;

            PlayerPrefs.SetString("refreshToken", refreshToken);
            PlayerPrefs.SetString("accessToken", accessToken);
            PlayerPrefs.Save();
        }

        private class TokenResponse
        {
            public string refreshToken;
            public string accessToken;
        }
    }
}
