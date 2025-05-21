using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SimpleJSON;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Venti
{
    public class TokenManager : Singleton<TokenManager>
    {
        public string appKeyScanScene = "AppKeyScan";
        public string appKey { get; private set; }
        public string refreshToken { get; private set; }
        public string accessToken { get; private set; }

        public VentiApiRequest.Result refreshTokenResult { get; private set; }

        public const string refreshTokenUrl = "/project-license/get-access-token";

        protected override void Awake()
        {
            base.Awake();

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

        public TokenPayload GetTokenPayload()
        {
            // TODO: Use access token instead of refresh token
            if (refreshToken == null)
            {
                Debug.LogError("No refresh token found");
                return null;
            }

            // string[] tokenParts = accessToken.Split('.');
            string[] tokenParts = refreshToken.Split('.');
            if (tokenParts.Length != 3)
            {
                Debug.LogError("Invalid token");
                return null;
            }

            Debug.Log("Token parts: " + tokenParts[1]);

            string payloadJsonString = DecodeBase64(tokenParts[1]);
            if (payloadJsonString == null)
                return null;

            TokenPayload token = new TokenPayload();
            token.SetFromJson(JSON.Parse(payloadJsonString).AsObject);
            return token;
        }

        private string DecodeBase64(string base64String)
        {
            const string base64Pattern = @"^[A-Za-z0-9+/=]*$";
            if (!Regex.IsMatch(base64String, base64Pattern))
            {
                Debug.LogError("Invalid base64 string");
                return null;
            }

            if (base64String.Length % 4 != 0)
            {
                base64String = base64String.TrimEnd('=');
                base64String = base64String.PadRight(base64String.Length + 4 - base64String.Length % 4, '=');
            }

            byte[] base64Bytes = Convert.FromBase64String(base64String);
            return Encoding.UTF8.GetString(base64Bytes);
        }

        private void SetTokens(string refreshToken, string accessToken)
        {
            this.refreshToken = refreshToken;
            this.accessToken = accessToken;

            PlayerPrefs.SetString("refreshToken", refreshToken);
            PlayerPrefs.SetString("accessToken", accessToken);
            PlayerPrefs.Save();
        }
    }

    public class TokenPayload
    {
        public string projectId;    // prj_...
        public string appId;        // app
        public string tokenId;          // jti
        public string tokenCount;          // rtc
        public string issuer;          // iss
        public string issuedAt;          // iat
        public string expiresAt;          // exp
        public string liveAt;          // lva
        public string mode;          // mod

        public void SetFromJson(JSONObject json)
        {
            projectId = json["prj"].Value;
            appId = json["app"].Value;
            tokenId = json["jti"].Value;
            tokenCount = json["rtc"].Value;
            issuer = json["iss"].Value;
            issuedAt = json["iat"].Value;
            expiresAt = json["exp"].Value;
            liveAt = json["lva"].Value;
            mode = json["mod"].Value;
        }
    }
}
