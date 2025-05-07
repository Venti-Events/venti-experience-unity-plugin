using System;
using System.Collections;
using UnityEngine;
using Venti.Experience;
using System.Web;
using UnityEngine.Networking;
using Codice.Utils;

namespace Venti
{
    public class CacheManager : Singleton<CacheManager>
    {
        // TODO: Create a list of loading files and their callbacks. If single file is being loaded by multiple variables, then combine their callbacks and load only once.

        public bool GetImage(string oldUrl, string newUrl, string folderName, Action<Texture2D> callback, bool forceUpdate = false)
        {
            // File hasn't changed. No need to load again.
            if (oldUrl == newUrl && !forceUpdate)
                return false;

            try
            {
                ResolvedPath resolvedFilePath = ResolveFilePathFromUrl(oldUrl, newUrl, folderName);
                string fileName = resolvedFilePath.fileName;
                string filePath = resolvedFilePath.filePath;
                bool saveToCache = !resolvedFilePath.isCached;

                StartCoroutine(FetchImage(filePath, fileName, folderName, saveToCache, callback));
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

        IEnumerator FetchImage(string url, string fileName, string folderName, bool saveToCache, Action<Texture2D> callback)
        {
            Debug.Log("Fetching image from web: " + url + " to " + fileName + " in " + folderName + " folder. Save to cache: " + saveToCache + ".");
            //using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                // Request and wait for the desired page
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(webRequest.error);
                }
                else
                {
                    Texture2D tex = DownloadHandlerTexture.GetContent(webRequest);

                    if (saveToCache)
                    {
                        byte[] bytes = webRequest.downloadHandler.data;
                        FileHandler.WriteBytes(bytes, fileName, folderName);
                        //Debug.Log("Saved image to cache: " + fileName);
                    }

                    callback.Invoke(tex);
                }
            }
        }

        IEnumerator FetchFont(string url, string fileName, string folderName, bool saveToCache, Action<Font> callback)
        {
            Debug.Log("Fetching image from web: " + url + " to " + fileName + " in " + folderName + " folder. Save to cache: " + saveToCache + ".");
            //using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                // Request and wait for the desired page
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(webRequest.error);
                }
                else
                {
                    //Font font = new Font(webRequest.downloadHandler.data(webRequest));

                    //if (saveToCache)
                    //{
                    //    byte[] bytes = webRequest.downloadHandler.data;
                    //    FileHandler.WriteBytes(bytes, fileName, folderName);
                    //    //Debug.Log("Saved image to cache: " + fileName);
                    //}

                    //callback.Invoke(tex);
                }
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