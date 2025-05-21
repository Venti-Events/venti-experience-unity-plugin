using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Venti;

public class AppKeyFetcher : MonoBehaviour
{
    [Header("API Settings")]
    [Tooltip("6-character code to fetch app-key token")]
    public string otp;

    //public string otp = "oPRVLU";

    [SerializeField]
    private string url = @"/project-license/get-refresh-token";

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
        if (submitButton != null)
        submitButton.onClick.AddListener(SubmitOTP);
        //StartCoroutine(RequestRefreshToken(otp));
    }

    private void SubmitOTP()
    {
        otp = inputField.text;
        Debug.Log($"OTP: {otp}");

        StartCoroutine(RequestRefreshToken(otp));
    }


    private IEnumerator RequestRefreshToken(string otpCode)
    {
        var bodyJson = JsonUtility.ToJson(new OtpPayload { otp = otpCode });
        // var bodyRaw = Encoding.UTF8.GetBytes(bodyJson);

        using (var req = VentiApiRequest.PostApi(url, bodyJson, "application/json"))
        {
            yield return req.SendApiRequest();

            if(req.result != VentiApiRequest.Result.Success)
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

                    // Load the main scene
                    SceneManager.LoadScene(0);
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
