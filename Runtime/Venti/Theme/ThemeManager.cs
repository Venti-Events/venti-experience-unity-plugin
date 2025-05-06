using Newtonsoft.Json;
using SimpleJSON;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Venti.Theme
{
    public class ThemeManager : MonoBehaviour
    {
        [field: SerializeField] public Theme theme { get; private set; }

        public UnityEvent onThemeUpdate;

        // JSON File Path
        public const string themeFolderName = "theme";
        private const string configFileName = "theme-config";

        private void Start()
        {
            LoadFromLocalJson();
        }

        public bool LoadFromLocalJson()
        {
            string jsonStr = FileHandler.ReadFile(configFileName + ".json", themeFolderName);

            if (string.IsNullOrEmpty(jsonStr))
            {
                //JSONObject json = JSON.Parse(jsonStr).AsObject;
                return LoadJson(jsonStr, false);
            }

            return false;
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

            if(string.IsNullOrEmpty(jsonStr))
            {
                Debug.LogError("JSON string for loading theme is null or empty");
                return false;
            }

            return LoadJson(jsonStr, true);
        }

        bool LoadJson(string jsonStr, bool saveJson = true)
        {
            try
            {
                theme = JsonConvert.DeserializeObject<ThemeResponse>(jsonStr).data.theme;
                // TODO: Download all images from cache and url
                onThemeUpdate?.Invoke();

                if (saveJson)
                    FileHandler.WriteString(jsonStr, configFileName + ".json", themeFolderName);

                return true;
            }
            catch (JsonException e)
            {
                Debug.LogError($"Failed to load theme JSON: {e.Message}");
                return false;
            }
        }

        [Serializable]
        private class ThemeResponse
        {
            public bool success;
            public ThemeData data;
            public string message;
        }

        [Serializable]
        private class ThemeData
        {
            public string id;
            public Theme theme;
        }
    }
}