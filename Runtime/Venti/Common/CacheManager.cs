using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Venti.Experience;
using System.Web;
using UnityEngine.Networking;
using TMPro;

namespace Venti
{
    public class CacheManager : Singleton<CacheManager>
    {
        // TODO: Create a list of loading files and their callbacks. If single file is being loaded by multiple variables, then combine their callbacks and load only once.
        Dictionary<string, CachedAsset<Texture2D>> imageAssets = new Dictionary<string, CachedAsset<Texture2D>>();
        Dictionary<string, CachedAsset<AudioClip>> audioActions = new Dictionary<string, CachedAsset<AudioClip>>();
        Dictionary<string, CachedAsset<TMP_FontAsset>> fontActions = new Dictionary<string, CachedAsset<TMP_FontAsset>>();

        public bool GetImage(string oldUrl, string newUrl, string folderName, Action<Texture2D> callback, bool forceUpdate = false)
        {
            // File hasn't changed. No need to load again.
            if (!forceUpdate && oldUrl == newUrl)
                return false;

            try
            {
                FileDetails fileDetails = GetFileDetailsFromUrl(oldUrl, newUrl, folderName);
                if (forceUpdate)
                    fileDetails.isCached = false;
                StartCoroutine(FetchImage(fileDetails, callback));
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return false;
            }
        }

        public bool GetAudio(string oldUrl, string newUrl, string folderName, Action<AudioClip> callback, bool forceUpdate = false)
        {
            // File hasn't changed. No need to load again.
            if (!forceUpdate && oldUrl == newUrl)
                return false;

            try
            {
                FileDetails fileDetails = GetFileDetailsFromUrl(oldUrl, newUrl, folderName);
                if (forceUpdate)
                    fileDetails.isCached = false;
                StartCoroutine(FetchAudio(fileDetails, callback));
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return false;
            }
        }

        public bool GetFont(string oldUrl, string newUrl, string folderName, Action<TMP_FontAsset> callback, bool forceUpdate = false)
        {
            // File hasn't changed. No need to load again.
            if (oldUrl == newUrl && !forceUpdate)
                return false;

            try
            {
                FileDetails fileDetails = GetFileDetailsFromUrl(oldUrl, newUrl, folderName);
                if (forceUpdate)
                    fileDetails.isCached = false;
                StartCoroutine(FetchFont(fileDetails, callback));
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return false;
            }
        }

        FileDetails GetFileDetailsFromUrl(string oldUrl, string newUrl, string folderName)
        {
            string oldFileName = null;
            string newFileName = null;

            // Extract file names from urls
            string oldUrlDecoded;
            if (!string.IsNullOrEmpty(oldUrl))
            {
                oldUrlDecoded = HttpUtility.UrlDecode(oldUrl);
                oldFileName = oldUrlDecoded.Substring(oldUrlDecoded.LastIndexOf('/') + 1);
            }

            string newUrlDecoded;
            if (string.IsNullOrEmpty(newUrl))
            {
                throw new Exception("Url for fetching a file from cache is null");
            }
            else
            {
                newUrlDecoded = HttpUtility.UrlDecode(newUrl);
                newFileName = newUrlDecoded.Substring(newUrlDecoded.LastIndexOf('/') + 1);
            }

            string assetFolderName = folderName + "/Assets";

            // Delete old file
            if (!string.IsNullOrEmpty(oldFileName))
                FileHandler.DeleteFile(oldFileName, assetFolderName);

            if (FileHandler.FileExists(newFileName, assetFolderName))
            {
                // If file to fetch exists in cache. e.g. it's a first run
                string filePath = FileHandler.GetFilePath(newFileName, assetFolderName);
                return new FileDetails
                {
                    fileName = newFileName,
                    folderName = assetFolderName,
                    filePath = filePath,
                    isCached = true
                };
            }

            // Fetch new image from web
            return new FileDetails
            {
                fileName = newFileName,
                folderName = folderName,
                filePath = newUrlDecoded,
                isCached = false
            };
        }

        IEnumerator FetchImage(FileDetails fileDetails, Action<Texture2D> callback)
        {
            //Debug.Log("Fetching image from web: " + url + " to " + fileName + " in " + folderName + " folder. Is in cache: " + isCached + ".");
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

                if (!fileDetails.isCached)
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

                if (!fileDetails.isCached)
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

                if (!fileDetails.isCached)
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
            public bool isCached;
        }

        private class CachedAsset<T>
        {
            public T asset;
            public string url;
            public FileDetails fileDetails;
            public bool isLoading = false;
            public List<Action<T>> Callbacks = new List<Action<T>>();
        }
    }
}