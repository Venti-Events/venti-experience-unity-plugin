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
        Dictionary<string, Action<Texture2D>[]> imageActions = new Dictionary<string, Action<Texture2D>[]>();
        Dictionary<string, Action<AudioClip>[]> audioActions = new Dictionary<string, Action<AudioClip>[]>();
        Dictionary<string, Action<TMP_FontAsset>[]> fontActions = new Dictionary<string, Action<TMP_FontAsset>[]>();

        public bool GetImage(string oldUrl, string newUrl, string folderName, Action<Texture2D> callback, bool forceUpdate = false)
        {
            // File hasn't changed. No need to load again.
            if (oldUrl == newUrl && !forceUpdate)
                return false;

            try
            {
                ResolvedPath resolvedFilePath = ResolveFilePathFromUrl(oldUrl, newUrl, folderName);
                StartCoroutine(FetchImage(resolvedFilePath.filePath, resolvedFilePath.fileName, folderName, resolvedFilePath.isCached, callback));
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
            if (oldUrl == newUrl && !forceUpdate)
                return false;

            try
            {
                ResolvedPath resolvedFilePath = ResolveFilePathFromUrl(oldUrl, newUrl, folderName);
                StartCoroutine(FetchAudio(resolvedFilePath.filePath, resolvedFilePath.fileName, folderName, resolvedFilePath.isCached, callback));
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
                ResolvedPath resolvedFilePath = ResolveFilePathFromUrl(oldUrl, newUrl, folderName);
                StartCoroutine(FetchFont(resolvedFilePath.filePath, resolvedFilePath.fileName, folderName, resolvedFilePath.isCached, callback));
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return false;
            }
        }

        ResolvedPath ResolveFilePathFromUrl(string oldUrl, string newUrl, string folderName)
        {
            string oldFileName = null;
            string newFileName = null;

            // Extract file names from urls
            string oldUrlDecoded;
            if (oldUrl != null)
            {
                oldUrlDecoded = HttpUtility.UrlDecode(oldUrl);
                oldFileName = oldUrlDecoded.Substring(oldUrlDecoded.LastIndexOf('/') + 1);
            }

            string newUrlDecoded;
            if (newUrl == null)
            {
                Debug.LogWarning("A theme item has null value");
                throw new Exception("A theme item has null value");
            }
            else
            {
                newUrlDecoded = HttpUtility.UrlDecode(newUrl);
                newFileName = newUrlDecoded.Substring(newUrlDecoded.LastIndexOf('/') + 1);
            }

            // Delete old file
            if (!string.IsNullOrEmpty(oldFileName))
                FileHandler.DeleteFile(oldFileName, folderName);

            if (FileHandler.FileExists(newFileName, folderName))
            {
                // If file to fetch exists in cache. e.g. it's a first run
                string filePath = FileHandler.GetFilePath(newFileName, folderName);
                return new ResolvedPath
                {
                    fileName = newFileName,
                    filePath = filePath,
                    isCached = true
                };
                //StartCoroutine(FetchImage(filePath, newFileName, false));
                //Debug.Log("Fetching image from cache: " + filePath);
            }

            // Fetch new image from web and save to cache
            //StartCoroutine(FetchImage(newValue, newFileName, true));
            //Debug.Log("Fetching image from web: " + newValue);

            return new ResolvedPath
            {
                fileName = newFileName,
                filePath = newUrlDecoded,
                isCached = false
            };
        }

        IEnumerator FetchImage(string url, string fileName, string folderName, bool isCached, Action<Texture2D> callback)
        {
            //Debug.Log("Fetching image from web: " + url + " to " + fileName + " in " + folderName + " folder. Save to cache: " + saveToCache + ".");
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                // Request and wait for the desired page
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(webRequest.error);
                    callback.Invoke(null);
                    yield break;
                }

                if (!isCached)
                {
                    byte[] bytes = webRequest.downloadHandler.data;
                    FileHandler.WriteBytes(bytes, fileName, folderName);
                    //Debug.Log("Saved image to cache: " + fileName);
                }

                Texture2D tex = DownloadHandlerTexture.GetContent(webRequest);
                callback.Invoke(tex);
            }
        }

        IEnumerator FetchAudio(string url, string fileName, string folderName, bool isCached, Action<AudioClip> callback)
        {
            string extension = fileName.Substring(fileName.LastIndexOf('.'));
            AudioType audioType = AudioType.UNKNOWN;
            if (extension == ".mp3")
                audioType = AudioType.MPEG;
            else if (extension == ".ogg")
                audioType = AudioType.OGGVORBIS;
            else if (extension == ".wav")
                audioType = AudioType.WAV;

            using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                // Request and wait for the desired page
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(webRequest.error);
                    callback.Invoke(null);
                    yield break;
                }

                if (!isCached)
                {
                    byte[] bytes = webRequest.downloadHandler.data;
                    FileHandler.WriteBytes(bytes, fileName, folderName);
                    //Debug.Log("Saved audio to cache: " + fileName);
                }

                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
                callback.Invoke(audioClip);
            }
        }

        // TODO: Fetch font
        IEnumerator FetchFont(string url, string fileName, string folderName, bool isCached, Action<TMP_FontAsset> callback)
        {
            Debug.Log("Fetching font from web: " + url + " to " + fileName + " in " + folderName + " folder. Save to cache: " + isCached + ".");
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                // Request and wait for the desired page
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(webRequest.error);
                    callback.Invoke(null);
                    yield break;
                }

                if (!isCached)
                {
                    byte[] bytes = webRequest.downloadHandler.data;
                    FileHandler.WriteBytes(bytes, fileName, folderName);
                    //Debug.Log("Saved font to cache: " + fileName);
                }

                string filePath = FileHandler.GetFilePath(fileName, folderName);
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

        private struct ResolvedPath
        {
            public string fileName;
            public string filePath;
            public bool isCached;
        }
    }
}