using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Web;
using System.IO;

namespace Venti
{
    // TODO: Convert to a pure class instead of a singleton
    public class CacheManager : Singleton<CacheManager>
    {
        private Dictionary<string, CachedAsset> cache = new Dictionary<string, CachedAsset>();
        private Dictionary<string, List<Action<UnityEngine.Object>>> pendingRequests = new Dictionary<string, List<Action<UnityEngine.Object>>>();

        public const string cacheFolderName = "cache";
        public const string assetFolderName = "assets";

        // public bool GetImage(string oldUrl, string newUrl, string folderName, Action<Texture2D> callback, bool forceUpdate = false)
        // {
        //     // File hasn't changed. No need to load again.
        //     if (!forceUpdate && oldUrl == newUrl)
        //         return false;

        //     try
        //     {
        //         FileDetails fileDetails = GetFileDetailsFromUrl(oldUrl, newUrl, folderName);
        //         if (forceUpdate)
        //             fileDetails.isCached = false;
        //         StartCoroutine(FetchImage(fileDetails, callback));
        //         return true;
        //     }
        //     catch (Exception ex)
        //     {
        //         Debug.LogError(ex.Message);
        //         return false;
        //     }
        // }

        // public bool GetAudio(string oldUrl, string newUrl, string folderName, Action<AudioClip> callback, bool forceUpdate = false)
        // {
        //     // File hasn't changed. No need to load again.
        //     if (!forceUpdate && oldUrl == newUrl)
        //         return false;

        //     try
        //     {
        //         FileDetails fileDetails = GetFileDetailsFromUrl(oldUrl, newUrl, folderName);
        //         if (forceUpdate)
        //             fileDetails.isCached = false;
        //         StartCoroutine(FetchAudio(fileDetails, callback));
        //         return true;
        //     }
        //     catch (Exception ex)
        //     {
        //         Debug.LogError(ex.Message);
        //         return false;
        //     }
        // }

        // public bool GetFont(string oldUrl, string newUrl, string folderName, Action<TMP_FontAsset> callback, bool forceUpdate = false)
        // {
        //     // File hasn't changed. No need to load again.
        //     if (oldUrl == newUrl && !forceUpdate)
        //         return false;

        //     try
        //     {
        //         FileDetails fileDetails = GetFileDetailsFromUrl(oldUrl, newUrl, folderName);
        //         if (forceUpdate)
        //             fileDetails.isCached = false;
        //         StartCoroutine(FetchFont(fileDetails, callback));
        //         return true;
        //     }
        //     catch (Exception ex)
        //     {
        //         Debug.LogError(ex.Message);
        //         return false;
        //     }
        // }

        // FileDetails GetFileDetailsFromUrl(string oldUrl, string newUrl, string folderName)
        // {
        //     string oldFileName = null;
        //     string newFileName = null;

        //     // Extract file names from urls
        //     string oldUrlDecoded;
        //     if (!string.IsNullOrEmpty(oldUrl))
        //     {
        //         oldUrlDecoded = HttpUtility.UrlDecode(oldUrl);
        //         oldFileName = oldUrlDecoded.Substring(oldUrlDecoded.LastIndexOf('/') + 1);
        //     }

        //     string newUrlDecoded;
        //     if (string.IsNullOrEmpty(newUrl))
        //     {
        //         throw new Exception("Url for fetching a file from cache is null");
        //     }
        //     else
        //     {
        //         newUrlDecoded = HttpUtility.UrlDecode(newUrl);
        //         newFileName = newUrlDecoded.Substring(newUrlDecoded.LastIndexOf('/') + 1);
        //     }

        //     string assetFolderName = folderName + "/Assets";

        //     // Delete old file
        //     if (!string.IsNullOrEmpty(oldFileName))
        //         FileHandler.DeleteFile(oldFileName, assetFolderName);

        //     if (FileHandler.FileExists(newFileName, assetFolderName))
        //     {
        //         // If file to fetch exists in cache. e.g. it's a first run
        //         string filePath = FileHandler.GetFilePath(newFileName, assetFolderName);
        //         return new FileDetails
        //         {
        //             fileName = newFileName,
        //             folderName = assetFolderName,
        //             filePath = filePath,
        //             isCached = true
        //         };
        //     }

        //     // Fetch new image from web
        //     return new FileDetails
        //     {
        //         fileName = newFileName,
        //         folderName = folderName,
        //         filePath = newUrlDecoded,
        //         isCached = false
        //     };
        // }

        public bool GetAsset(string url, CachedAssetType type, Action<UnityEngine.Object> callback, bool forceUpdate = false)
        {
            try
            {
                string decodedUrl = HttpUtility.UrlDecode(url);
                string fileName = decodedUrl.Substring(decodedUrl.LastIndexOf('/') + 1);

                // Already in memory?
                if (cache.ContainsKey(decodedUrl))
                {
                    CachedAsset cachedAsset = cache[decodedUrl];
                    callback?.Invoke(cachedAsset.asset);
                    return true;
                }

                // Already loading
                if (pendingRequests.ContainsKey(decodedUrl))
                {
                    pendingRequests[decodedUrl].Add(callback);
                    return true;
                }

                // First request, create entry in pendingRequests
                pendingRequests.Add(decodedUrl, new List<Action<UnityEngine.Object>>() { callback });

                string cachedAssetsFolderName = Path.Combine(cacheFolderName, assetFolderName);

                FileDetails fileDetails = new FileDetails
                {
                    fileName = fileName,
                    folderName = cachedAssetsFolderName,
                    filePath = decodedUrl,
                    isCachePath = false
                };

                // If file exists in cache
                if (!forceUpdate && FileHandler.FileExists(fileName, cachedAssetsFolderName))
                {
                    fileDetails.filePath = FileHandler.GetFilePath(fileName, cachedAssetsFolderName);
                    fileDetails.isCachePath = true;
                }

                // Create cached asset data object without asset object
                CachedAsset cachedAssetData = new CachedAsset
                {
                    url = decodedUrl,
                    fileDetails = fileDetails,
                    type = type,
                    // asset = null
                };

                FetchAsset(cachedAssetData);

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                callback?.Invoke(null);
                return false;
            }
        }

        public void ClearCache()
        {
            FileHandler.DeleteFolder(cacheFolderName);
        }

        // Fetch asset
        private void FetchAsset(CachedAsset cachedAssetData)
        {
            switch (cachedAssetData.type)
            {
                case CachedAssetType.Image:
                    StartCoroutine(FetchImage(cachedAssetData.fileDetails, (Texture2D texture) => OnFetchAsset(cachedAssetData, texture)));
                    break;
                case CachedAssetType.Audio:
                    StartCoroutine(FetchAudio(cachedAssetData.fileDetails, (AudioClip audioClip) => OnFetchAsset(cachedAssetData, audioClip)));
                    break;
                case CachedAssetType.Font:
                    StartCoroutine(FetchFont(cachedAssetData.fileDetails, (TMP_FontAsset font) => OnFetchAsset(cachedAssetData, font)));
                    break;
            }
        }

        private void OnFetchAsset(CachedAsset cachedAssetData, UnityEngine.Object assetObj)
        {
            if (assetObj != null)
            {
                // Add to cache
                CachedAsset cachedAsset = new CachedAsset
                {
                    url = cachedAssetData.url,
                    fileDetails = cachedAssetData.fileDetails,
                    type = cachedAssetData.type,
                    asset = assetObj
                };
             
                cache[cachedAssetData.url] = cachedAsset;
            }

            // Invoke all linked callbacks
            foreach (var callback in pendingRequests[cachedAssetData.url])
                callback?.Invoke(assetObj);

            // Remove from pendingRequests
            pendingRequests.Remove(cachedAssetData.url);
        }

        IEnumerator FetchImage(FileDetails fileDetails, Action<Texture2D> callback)
        {
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(fileDetails.filePath))
            {
                // Request and wait for the desired page
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(webRequest.error);
                    callback.Invoke(null);
                    yield break;
                }

                if (!fileDetails.isCachePath)
                {
                    byte[] bytes = webRequest.downloadHandler.data;
                    FileHandler.WriteBytes(bytes, fileDetails.fileName, fileDetails.folderName);
                    //Debug.Log("Saved image to cache: " + fileName);
                }

                Texture2D tex = DownloadHandlerTexture.GetContent(webRequest);
                callback.Invoke(tex);
            }
        }

        IEnumerator FetchAudio(FileDetails fileDetails, Action<AudioClip> callback)
        {
            string extension = fileDetails.fileName.Substring(fileDetails.fileName.LastIndexOf('.'));
            AudioType audioType = AudioType.UNKNOWN;
            if (extension == ".mp3")
                audioType = AudioType.MPEG;
            else if (extension == ".ogg")
                audioType = AudioType.OGGVORBIS;
            else if (extension == ".wav")
                audioType = AudioType.WAV;

            using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(fileDetails.filePath, audioType))
            {
                // Request and wait for the desired page
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(webRequest.error);
                    callback.Invoke(null);
                    yield break;
                }

                if (!fileDetails.isCachePath)
                {
                    byte[] bytes = webRequest.downloadHandler.data;
                    FileHandler.WriteBytes(bytes, fileDetails.fileName, fileDetails.folderName);
                }

                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
                callback.Invoke(audioClip);
            }
        }

        IEnumerator FetchFont(FileDetails fileDetails, Action<TMP_FontAsset> callback)
        {
            //Debug.Log("Fetching font from web: " + fileInfo.filePath + " to " + fileInfo.fileName + " in " + fileInfo.folderName + " folder. Save to cache: " + fileInfo.isCached + ".");
            using (UnityWebRequest webRequest = UnityWebRequest.Get(fileDetails.filePath))
            {
                // Request and wait for the desired page
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(webRequest.error);
                    callback.Invoke(null);
                    yield break;
                }

                if (!fileDetails.isCachePath)
                {
                    byte[] bytes = webRequest.downloadHandler.data;
                    FileHandler.WriteBytes(bytes, fileDetails.fileName, fileDetails.folderName);
                }

                string filePath = FileHandler.GetFilePath(fileDetails.fileName, fileDetails.folderName);
                Font font = new Font(filePath);

                if (font == null)
                {
                    Debug.LogError("Failed to load font from path: " + filePath);
                    callback.Invoke(null);
                    yield break;
                }

                TMP_FontAsset tmpFontAsset = TMP_FontAsset.CreateFontAsset(font);
                callback.Invoke(tmpFontAsset);
            }
        }

        private struct FileDetails
        {
            public string fileName;
            public string folderName;
            public string filePath;
            public bool isCachePath;   // is filePath to cache dir or web url
        }

        private class CachedAsset
        {
            public string url;
            public FileDetails fileDetails;
            public CachedAssetType type;
            public UnityEngine.Object asset;
        }
    }
}