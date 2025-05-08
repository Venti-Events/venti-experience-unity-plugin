using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AppKeyFetcher : MonoBehaviour
{
    [Header("API Settings")]
    [Tooltip("Six‑digit OTP to send in the request body")]
    public string otp;

    //public string otp = "oPRVLU";

    [SerializeField]
    private string url = "https://pdm18slv-8080.inc1.devtunnels.ms/api/v1/project-license/get-refresh-token";

    [SerializeField]
    private string appKey;

    [Space]

    [SerializeField]
    private GameObject otpPanelContainer;

    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private Button submitButton;

    void Start()
    {
        var savedAppKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwcmoiOiI1ZjQwNjBiZi04NjdkLTQ0MDctYmQ5NC0yZTBjMWU4YmFkYmIiLCJhcHAiOiJj" +
            "MDI3YWZjYi02M2NkLTQ3NWEtYjhhMS02YzUwZWRlN2IyZmQiLCJqdGkiOiJiNzAyMzhjMi00YTdiLTRmMmMtYTU1MS03YjcyYWNkZWVmYzEiLCJydGMiOjAsImlz" +
            "cyI6InNlcnZlciIsImlhdCI6MTc0NjUxMTY5NiwiZXhwIjoxNzQ2ODM1MjAwLCJsdmEiOjE3NDU5NzEyMDAsIm1vZCI6ImRlbW8ifQ.GQ693uFd2lUAUyENO9-JUlZ" +
            "MCP3J8izj-s0XCAK5Ykc";

        PlayerPrefs.SetString("appKey", savedAppKey);
        //submitButton.onClick.AddListener(SubmitOTP);
        //StartCoroutine(RequestRefreshToken(otp));
    }

    private void SubmitOTP()
    {
        otp = inputField.text;

        Debug.Log($"OTP: {otp}");

        StartCoroutine(RequestRefreshToken(otp));

        Debug.Log($"App Key: {appKey}");
    }


    private IEnumerator RequestRefreshToken(string otpCode)
    {
        var bodyJson = JsonUtility.ToJson(new OtpPayload { otp = otpCode });
        var bodyRaw = Encoding.UTF8.GetBytes(bodyJson);

        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError($"Error fetching app key: {req.error}");
                yield break;
            }

            var txt = req.downloadHandler.text;
            Debug.Log($"Raw response: {txt}");

            try
            {
                var rsp = JsonUtility.FromJson<RefreshResponse>(txt);
                appKey = rsp.data;
                Debug.Log($"Fetched App Key: {appKey}");

                if (appKey != null)
                {
                    PlayerPrefs.SetString("appKey", appKey);
                    PlayerPrefs.Save();

                    SceneManager.LoadScene("VentiPhotoboothDemo");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JSON parse error: {e}");
            }
        }
    }

    [System.Serializable]
    private class OtpPayload
    {
        public string otp;
    }

    [System.Serializable]
    private class RefreshResponse
    {
        public string data;
    }

    private void OnDestroy()
    {
        submitButton.onClick.RemoveListener(SubmitOTP);
    }
}
