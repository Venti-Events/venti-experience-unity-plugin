using Newtonsoft.Json;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Venti.Theme
{
    public class ThemeManager : Singleton<ThemeManager>
    {
        [field: SerializeField] public Theme theme { get; private set; }

        public UnityEvent onThemeUpdate;

        // JSON File Path
        public const string themeFolderName = "cache";
        private const string configFileName = "theme-config";

        // private int pendingAssetLoads = 0;
        private List<string> pendingAssetLoadPaths = new List<string>();
        // private string queuedJson = null;

        // TODO: queuedJson with pendingAssetLoadPaths

        private void Start()
        {
            LoadFromLocalJson();
        }

        public bool LoadFromLocalJson()
        {
            string jsonStr = FileHandler.ReadFile(configFileName + ".json", themeFolderName);

            if (string.IsNullOrEmpty(jsonStr))
            {
                Debug.LogError("Local cached theme file not found");
                return false;
            }

            JSONObject json = JSON.Parse(jsonStr).AsObject;
            return LoadJson(json, false);
        }

        public bool LoadFromWebJson(string jsonStr)
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
                        FileHandler.WriteString(json.ToString(), configFileName + ".json", themeFolderName);

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