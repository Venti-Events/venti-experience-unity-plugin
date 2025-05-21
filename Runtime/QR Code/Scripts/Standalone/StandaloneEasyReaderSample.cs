using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using ZXing;
using System.Collections;
using System.Text;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using Venti;

public class StandaloneEasyReaderSample : MonoBehaviour
{
    [Header("Scan Result")]
    [SerializeField]
    private string appKey;

    [Header("Webcam Settings")]
    [SerializeField]
    private bool logAvailableWebcams;
    [SerializeField]
    private int selectedWebcamIndex;

    [Header("UI Elements (for manual code entry)")]
    [Tooltip("Panel containing InputField & Submit Button")]
    [SerializeField]
    private GameObject inputPanel;
    [SerializeField]
    private TMP_InputField codeInputField;
    [SerializeField]
    private Button submitButton;

    [Header("Webcam Display")]
    [Tooltip("RawImage to show webcam feed")]
    [SerializeField]
    private RawImage webcamRawImage;
    [Tooltip("RenderTexture target for webcam feed")]
    [SerializeField]
    private RenderTexture webcamRenderTexture;

    private WebCamTexture camTexture;
    private Color32[] cameraColorData;
    private int width, height;

    private IBarcodeReader barcodeReader = new BarcodeReader
    {
        AutoRotate = false,
        Options = new ZXing.Common.DecodingOptions
        {
            TryHarder = false
        }
    };

    private Result result;

    [Serializable]
    private class CodeRequest { public string otp; }
    [Serializable]
    private class RefreshTokenResponse { public string data; }

    // private const string refreshUrl = "/project-license/3192dd59-361a-4910-bd85-1f713c37f7bf/get-refresh-token";
    private const string refreshUrl = @"/project-license/get-refresh-token";

    private void Start()
    {
        appKey = PlayerPrefs.GetString("appKey", "");

        //if (WebCamTexture.devices.Length == 0)
        //{
        //    EnableManualInput();
        //    return;
        //}

        EnableManualInput();
        //DisableManualInput();
        LogWebcamDevices();
        SetupWebcamTexture();
        PlayWebcamTexture();

        if (webcamRawImage != null && webcamRenderTexture != null)
        {
            webcamRawImage.texture = webcamRenderTexture;
            webcamRawImage.GetComponent<AspectRatioFitter>().aspectRatio = (float)width / (float)height;
        }

        cameraColorData = new Color32[width * height];
    }

    private void Update()
    {
        if (camTexture != null && camTexture.isPlaying)
        {
            if (webcamRenderTexture != null)
            {
                Graphics.Blit(camTexture, webcamRenderTexture);
            }

            camTexture.GetPixels32(cameraColorData);
            result = barcodeReader.Decode(cameraColorData, width, height);
            if (result != null)
            {
                appKey = result.Text;
                Debug.Log($"Scanned: {appKey}");

                PlayerPrefs.SetString("appKey", appKey);//appkey replace
                PlayerPrefs.Save();

                DisableManualInput();
                SceneManager.LoadScene(0);
            }
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

    private void EnableManualInput()
    {
        if (inputPanel != null)
            inputPanel.SetActive(true);

        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmitCode);
    }

    private void DisableManualInput()
    {
        if (inputPanel != null)
        {
            inputPanel.SetActive(false);
        }
    }

    private void OnSubmitCode()
    {
        string code = codeInputField.text;
        if (!string.IsNullOrEmpty(code) && code.Length == 6)
        {
            submitButton.interactable = false;
            StartCoroutine(SendCodeCoroutine(code));
        }
        else
        {
            Debug.LogWarning("Please enter a valid 6-digit code.");
        }
    }

    private IEnumerator SendCodeCoroutine(string code)
    {
        var req = new CodeRequest { otp = code };
        string json = JsonUtility.ToJson(req);
        // byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (var www = VentiApiRequest.PostApi(refreshUrl, json, "application/json"))
        {
            // byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            // uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
            // uwr.downloadHandler = new DownloadHandlerBuffer();
            // uwr.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendApiRequest();

            if (www.result == VentiApiRequest.Result.Success)
            {
                Debug.Log($"Code request success: {www.downloadHandler.text}");

                var resp = JsonUtility.FromJson<RefreshTokenResponse>(www.downloadHandler.text);
                appKey = resp.data;

                Debug.Log($"Received appKey: {appKey}");

                PlayerPrefs.SetString("appKey", appKey);
                PlayerPrefs.Save();

                DisableManualInput();

                SceneManager.LoadScene(0);
            }
            else
            {
                Debug.LogError($"Code request failed: {www.error} \n {www.downloadHandler.text}");
                submitButton.interactable = true;
            }
        }
    }

    // private void OnGUI()
    // {
    //     GUI.TextField(new Rect(10, 10, 512, 25), lastResult);
    // }

    private void LogWebcamDevices()
    {
        if (!logAvailableWebcams) return;
        var devices = WebCamTexture.devices;
        for (int i = 0; i < devices.Length; i++)
            Debug.Log($"Webcam {i}: {devices[i].name}");
    }

    private void SetupWebcamTexture()
    {
        var deviceName = WebCamTexture.devices[selectedWebcamIndex].name;
        camTexture = new WebCamTexture(deviceName)
        {
            requestedWidth = Screen.width,
            requestedHeight = Screen.height
        };
    }

    private void PlayWebcamTexture()
    {
        if (camTexture == null) return;
        camTexture.Play();
        width = camTexture.width;
        height = camTexture.height;
    }

    private void OnDestroy()
    {
        if (camTexture != null)
            camTexture.Stop();
    }

}
