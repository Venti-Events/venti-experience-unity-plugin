using Newtonsoft.Json;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Venti.Theme
{
    public class ThemeManager : Singleton<ThemeManager>
    {
        [field: SerializeField] public Theme theme { get; private set; }

        public UnityEvent onThemeUpdate;

        // JSON File Path
        // public const string themeFolderName = "cache";
        private const string configFileName = "theme-config";

        // private int pendingAssetLoads = 0;
        private string themeHash;
        private List<string> pendingAssetLoadPaths = new List<string>();
        // private string queuedJson = null;

        private void Start()
        {
            themeHash = PlayerPrefs.GetString("themeHash", "");
            
            LoadFromLocalJson();
        }

        // Fetch theme config
        public void FetchThemeConfig(string hash)
        {
            if (hash != themeHash)
            {
                Debug.Log("Theme hashes mismatch. Re-fetch");
                Debug.Log("Old hash: " +  themeHash + "\n New hash: " + hash);
                StartCoroutine(GetThemeConfig(hash));
            }
            else
                Debug.Log("Theme hash is the same, no need to fetch again.");
        }

        private IEnumerator GetThemeConfig(string hash)
        {
            string url = VentiManager.serverUrl + getThemeUrl;

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Authorization", "Bearer " + TokenManager.Instance.appKey);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error fetching theme config: " + www.error);
                }
                else
                {
                    // Parse the response and update the theme manager
                    string jsonResponse = www.downloadHandler.text;
                    bool success = ThemeManager.Instance.LoadFromWebJson(jsonResponse);
                    if (success)
                    {
                        themeHash = hash;
                        //PlayerPrefs.SetString("themeHash", themeHash);
                        //PlayerPrefs.Save();
                    }
                }
            }
        }

        private bool LoadFromLocalJson()
        {
            string jsonStr = FileHandler.ReadFile(configFileName + ".json", CacheManager.cacheFolderName);

            if (string.IsNullOrEmpty(jsonStr))
            {
                Debug.LogError("Local cached theme file not found");
                return false;
            }

            JSONObject json = JSON.Parse(jsonStr).AsObject;
            return LoadJson(json, false);
        }

        private bool LoadFromWebJson(string jsonStr)
        {
            //JSONObject json = JSON.Parse(jsonStr).AsObject;
            //if (json == null)
            //{
            //    Debug.LogError("JSON for loading experience is null");
            //    return false;
            //}

            //JSONObject configJson = json["data"]["theme"].AsObject;
            //return LoadJson(configJson.ToString(), true);

            if (string.IsNullOrEmpty(jsonStr))
            {
                Debug.LogError("JSON string for loading theme is null or empty");
                return false;
            }

            JSONObject json = JSON.Parse(jsonStr).AsObject;
            if (json == null)
            {
                Debug.LogError("JSON for loading experience is null");
                return false;
            }

            // if (pendingAssetLoads > 0)
            // {
            //     queuedJson = jsonStr;
            //     return true;
            // }
            // pendingAssetLoads = 0;
            // queuedJson = null;

            JSONObject configJson = json["data"]["theme"].AsObject;
            return LoadJson(configJson, true);
        }

        bool LoadJson(JSONObject json, bool saveJson = true)
        {
            try
            {
                if (json == null)
                    throw new Exception("JSON for loading experience is null");

                // TODO: Check whether versions match

                // Reset pending asset counter
                // pendingAssetLoads = 0;
                pendingAssetLoadPaths.Clear();

                // Update all fields from json
                if (SetFromJson(json))
                {
                    if (saveJson)
                        FileHandler.WriteString(json.ToString(), configFileName + ".json", CacheManager.cacheFolderName);

                    // If no assets need loading, invoke immediately
                    // if (pendingAssetLoads == 0)
                    if (pendingAssetLoadPaths.Count <= 0)
                        onThemeUpdate?.Invoke();
                }

                return true;
            }
            catch (JsonException e)
            {
                Debug.LogError($"Failed to load theme JSON: {e.Message}");
                return false;
            }
        }

        public void OnAssetLoadStart(string assetPath)
        {
            // pendingAssetLoads++;
            pendingAssetLoadPaths.Add(assetPath);
        }

        public void OnAssetLoadEnd(string assetPath)
        {
            // pendingAssetLoads--;
            bool removed = pendingAssetLoadPaths.Remove(assetPath);
            if (!removed)
            {
                Debug.LogError($"Asset path {assetPath} not found in pending asset load paths for ThemeManager");
                return;
            }

            // if (pendingAssetLoads <= 0)
            if (pendingAssetLoadPaths.Count == 0)
            {
                onThemeUpdate?.Invoke();

                // if (queuedJson != null)
                // {
                // LoadFromWebJson(queuedJson);
                // queuedJson = null;
                // }
            }
        }

        public bool SetFromJson(JSONObject json)
        {
            theme.SetFromJson(json);
            return true;
        }
    }
}