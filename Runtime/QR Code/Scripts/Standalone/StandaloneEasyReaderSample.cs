using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using ZXing;
using System.Collections;
using System.Text;
using System;
using TMPro;

public class StandaloneEasyReaderSample : MonoBehaviour
{
    [Header("Scan Result")]
    [SerializeField]
    private string lastResult;

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
    private Rect screenRect;

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
    private class CodeRequest { public string code; }
    [Serializable]
    private class RefreshTokenResponse { public string appId; }

    private const string refreshUrl = "https://pdm18slv-3000.inc1.devtunnels.ms/api/v1/project-license/3192dd59-361a-4910-bd85-1f713c37f7bf/get-refresh-token";

    private void Start()
    {
        lastResult = PlayerPrefs.GetString("appId", "http://www.google.com");

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
        }

        cameraColorData = new Color32[width * height];
        screenRect = new Rect(0, 0, Screen.width, Screen.height);
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
                lastResult = result.Text;
                Debug.Log($"Scanned: {lastResult}");
                PlayerPrefs.SetString("appId", lastResult);//appkey replace
                PlayerPrefs.Save();
            }
        }
    }

    private IEnumerator SendCodeCoroutine(string code)
    {
        var req = new CodeRequest { code = code };
        string json = JsonUtility.ToJson(req);

        using (var uwr = new UnityWebRequest(refreshUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                var resp = JsonUtility.FromJson<RefreshTokenResponse>(uwr.downloadHandler.text);
                lastResult = resp.appId;
                Debug.Log($"Received appId: {lastResult}");
                PlayerPrefs.SetString("appId", lastResult);
                PlayerPrefs.Save();
                DisableManualInput();
            }
            else
            {
                Debug.LogError($"Code request failed: {uwr.error}");
                submitButton.interactable = true;
            }
        }
    }

    private void OnGUI()
    {
        GUI.TextField(new Rect(10, 10, 512, 25), lastResult);
    }

    private void OnDestroy()
    {
        if (camTexture != null)
            camTexture.Stop();
    }

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
}
