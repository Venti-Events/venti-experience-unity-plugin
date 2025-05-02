using Newtonsoft.Json;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

namespace Venti.Theme
{
    public class ThemeManager : MonoBehaviour
    {
        [field: SerializeField] public Theme theme { get; private set; }

        public UnityEvent onThemetaUpdate;

        // JSON File Path
        public const string themeFolderName = "theme";
        private const string configFileName = "theme-config";

        private void Start()
        {
            LoadJsonFromLocal();
        }

        public bool LoadJsonFromLocal()
        {
            string jsonStr = FileHandler.ReadFile(configFileName + ".json", themeFolderName);

            if (jsonStr != null)
            {
                //JSONObject json = JSON.Parse(jsonStr).AsObject;
                return LoadJson(jsonStr, false);
            }

            return false;
        }

        public bool LoadJsonFromWeb(string jsonStr)
        {
            JSONObject json = JSON.Parse(jsonStr).AsObject;
            if (json == null)
            {
                Debug.LogError("JSON for loading experience is null");
                return false;
            }

            JSONObject configJson = json["data"]["theme"].AsObject;
            return LoadJson(configJson.ToString(), true);
        }

        bool LoadJson(string jsonStr, bool saveJson = true)
        {
            try
            {
                theme = JsonConvert.DeserializeObject<Theme>(jsonStr);
                // TODO: Download all images from cache and url
                onThemetaUpdate?.Invoke();

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
    }
}