using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Venti
{
    public class VentiApiRequest : UnityWebRequest
    {
        #region PUBLIC_CONSTANTS
        //public const string serverUrl = @"https://venti-server-nestjs-128798841108.us-central1.run.app";
        //public const string serverUrl = @"https://pdm18slv-8080.inc1.devtunnels.ms";
        public const string serverUrl = @"https://vent-server-nestjs-temp-128798841108.asia-south1.run.app";
        public const string apiRelativeUrl = @"/api/v1";
        public const string apiUrl = serverUrl + apiRelativeUrl;
        #endregion

        // #region PUBLIC_PROPERTIES
        // public string url
        // {
        //     get
        //     {
        //         return _unityWebRequest.url;
        //     }
        //     set
        //     {
        //         _unityWebRequest.url = apiUrl + value;
        //     }
        // }
        // public Uri uri
        // {
        //     get
        //     {
        //         return _unityWebRequest.uri;
        //     }
        //     set
        //     {
        //         _unityWebRequest.uri = new Uri(apiUrl + value);
        //     }
        // }
        // public string method
        // {
        //     get
        //     {
        //         return _unityWebRequest.method;
        //     }
        //     set
        //     {
        //         _unityWebRequest.method = value;
        //     }
        // }
        // public DownloadHandler downloadHandler
        // {
        //     get
        //     {
        //         return _unityWebRequest.downloadHandler;
        //     }
        //     set
        //     {
        //         _unityWebRequest.downloadHandler = value;
        //     }
        // }
        // public UploadHandler uploadHandler
        // {
        //     get
        //     {
        //         return _unityWebRequest.uploadHandler;
        //     }
        //     set
        //     {
        //         _unityWebRequest.uploadHandler = value;
        //     }
        // }
        // public string response
        // {
        //     get
        //     {
        //         return _unityWebRequest.downloadHandler.text;
        //     }
        // }
        // public long responseCode
        // {
        //     get
        //     {
        //         return _unityWebRequest.responseCode;
        //     }
        // }
        // #endregion

        // #region PRIVATE_PROPERTIES
        // private UnityWebRequest _unityWebRequest;
        // #endregion

        #region CONSTRUCTORS
        public VentiApiRequest()
        {
            // _unityWebRequest = new UnityWebRequest();
        }

        public VentiApiRequest(string url)
        {
            // _unityWebRequest = new UnityWebRequest(url);
            base.url = apiUrl + url;
        }

        public VentiApiRequest(string url, string method)
        {
            // _unityWebRequest = new UnityWebRequest(url, method);
            base.url = apiUrl + url;
            base.method = method;
        }

        public VentiApiRequest(string url, string method, DownloadHandler downloadHandler, UploadHandler uploadHandler)
        {
            // _unityWebRequest = new UnityWebRequest(url, method, downloadHandler, uploadHandler);
            base.url = apiUrl + url;
            base.method = method;
            base.downloadHandler = downloadHandler;
            base.uploadHandler = uploadHandler;
        }
        #endregion

        #region PUBLIC_STATIC_METHODS
        // Use these instead of UnityWebRequest.Get, Post, Put, Delete
        public static VentiApiRequest GetApi(string url)
        {
            return new VentiApiRequest(url, "GET", new DownloadHandlerBuffer(), null);
        }

        public static VentiApiRequest PostApi(string url, string postData, string contentType)
        {
            VentiApiRequest request = new VentiApiRequest(url, "POST");
            SetupPost(request, postData, contentType);
            return request;
        }

        public static VentiApiRequest PostApi(string url, WWWForm formData)
        {
            VentiApiRequest request = new VentiApiRequest(url, "POST");
            SetupPost(request, formData);
            return request;
        }

        public static VentiApiRequest PostApi(string url, Dictionary<string, string> formFields)
        {
            VentiApiRequest request = new VentiApiRequest(url, "POST");
            SetupPost(request, formFields);
            return request;
        }

        public static VentiApiRequest PutApi(string uri, byte[] bodyData, string contentType)
        {
            VentiApiRequest request = new VentiApiRequest(uri, "PUT", new DownloadHandlerBuffer(), new UploadHandlerRaw(bodyData));
            request.SetRequestHeader("Content-Type", contentType);
            return request;
        }

        public static VentiApiRequest PutApi(string uri, string bodyData, string contentType)
        {
            VentiApiRequest request = new VentiApiRequest(uri, "PUT", new DownloadHandlerBuffer(), new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyData)));
            request.SetRequestHeader("Content-Type", contentType);
            return request;
        }

        public static VentiApiRequest DeleteApi(string url)
        {
            return new VentiApiRequest(url, "DELETE");
        }

        #endregion

        #region PRIVATE_STATIC_METHODS
        // Copied from UnityWebRequest because they are private and cannot be inherited
        private static void SetupPost(VentiApiRequest request, string postData, string contentType)
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            if (string.IsNullOrEmpty(postData))
            {
                request.SetRequestHeader("Content-Type", contentType);
                return;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(postData);
            request.uploadHandler = new UploadHandlerRaw(bytes);
            request.uploadHandler.contentType = contentType;
        }

        private static void SetupPost(VentiApiRequest request, WWWForm formData)
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            if (formData == null)
            {
                return;
            }

            byte[] array = null;
            array = formData.data;
            if (array.Length == 0)
            {
                array = null;
            }

            if (array != null)
            {
                request.uploadHandler = new UploadHandlerRaw(array);
            }

            Dictionary<string, string> headers = formData.headers;
            foreach (KeyValuePair<string, string> item in headers)
            {
                request.SetRequestHeader(item.Key, item.Value);
            }
        }

        private static void SetupPost(VentiApiRequest request, Dictionary<string, string> formFields)
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            byte[] array = null;
            if (formFields != null && formFields.Count != 0)
            {
                array = SerializeSimpleForm(formFields);
            }

            if (array != null)
            {
                UploadHandler uploadHandler = new UploadHandlerRaw(array);
                uploadHandler.contentType = "application/x-www-form-urlencoded";
                request.uploadHandler = uploadHandler;
            }
        }
        #endregion

        // Use this method instead of SendWebRequest
        public UnityWebRequestAsyncOperation SendApiRequest()
        {
            // Check if url does not start with apiUrl, add it
            if (!url.StartsWith(apiUrl))
                url = apiUrl + url;
            // Debug.Log($"Sending API request to: {url}");

            return SendWebRequest();
        }

        public IEnumerator SendAuthenticatedApiRequest(bool autoRefreshToken = true)
        {
            // Check if url does not start with apiUrl, add it
            if (!url.StartsWith(apiUrl))
                url = apiUrl + url;

            if (TokenManager.Instance == null)
                throw new Exception("TokenManager is not initialized");

            // TODO: Uncomment this after EEA2025 when access-token only guards are enforced
            // SetRequestHeader("Authorization", "Bearer " + TokenManager.Instance?.accessToken);
            SetRequestHeader("Authorization", "Bearer " + TokenManager.Instance?.refreshToken);
            yield return SendWebRequest();

            if (result != Result.Success)
            {
                // Check if we got unauthorized error
                if (result == Result.ProtocolError && responseCode == 401)
                {
                    if (!autoRefreshToken)
                        yield break;

                    yield return TokenManager.Instance?.RefreshTokenCoroutine();
                    if (TokenManager.Instance?.refreshTokenResult != VentiApiRequest.Result.Success)
                        yield break;

                    yield return SendAuthenticatedApiRequest(false);
                }
            }
        }
    }
}