using System;
using UnityEngine;
using UnityEngine.Events;
using SimpleJSON;
using Newtonsoft.Json;

namespace Venti.Experience
{
    public class ExperienceManager : Singleton<ExperienceManager>
    {
        #region JSON_Items
        [field: SerializeField] public Metadata metaData { get; private set; }
        [field: SerializeField] public BaseField[] fields { get; private set; }
        #endregion JSON_Items

        #region Booleans
        [Tooltip("Enabling this will include inactive GameObjects in search")]
        [SerializeField] private bool searchForInactive = false; // Enabling this will include inactive GameObjects in search
        #endregion

        public UnityEvent onMetadataUpdate;

        // JSON File Path
        public const string appFolderName = "app";
        //private const string schemaFileName = "ExperienceSchema";
        private const string configFileName = "app-config";

        private void Start()
        {
            //Debug.LogWarning("Booker-T");
            ClearFields();
            FetchChildFields();

            LoadJsonFromLocal();
        }

        public void FetchChildFields()
        {
            fields = Utils.FetchChildFields<BaseField>(gameObject, searchForInactive);
        }

        public void ClearFields()
        {
            Utils.ClearChildFields(fields);
            fields = null;
        }

        public JSONObject GetJson()
        {
            string metadataJsonString = JsonConvert.SerializeObject(metaData, Formatting.Indented);
            Debug.Log(metadataJsonString);

            JSONObject metadataJson = JSON.Parse(metadataJsonString).AsObject;

            JSONArray fieldsJson = new JSONArray();
            for (int i = 0; i < fields.Length; i++)
            {
                BaseField field = fields[i];
                JSONObject fieldJson = field.GetJson();
                fieldsJson.Add(fieldJson);
            }
            Debug.Log(fieldsJson.ToString(2));

            JSONObject experienceJson = new JSONObject();
            experienceJson["metadata"] = metadataJson;
            experienceJson["fields"] = fieldsJson;

            return experienceJson;
        }

        public void ExportJson()
        {
            FetchChildFields();

            string jsonStr = GetJson().ToString();
            Debug.Log(jsonStr);

            FileHandler.WriteString(jsonStr, $"{metaData.experienceId}-schema-v{metaData.version.ToString()}.json", "Exports", true);
        }

        public string LoadJsonFromLocal()
        {
            string jsonStr = FileHandler.ReadFile(configFileName + ".json", appFolderName);
            
            if (jsonStr != null)
            {
                JSONObject json = JSON.Parse(jsonStr).AsObject;
                return LoadJson(json, false);
            }

            return null;
        }

        public string LoadJsonFromWeb(string jsonStr)
        {
            JSONObject json = JSON.Parse(jsonStr).AsObject;
            if (json == null)
            {
                Debug.LogError("JSON for loading experience is null");
                return null;
            }

            JSONObject configJson = json["data"]["configuration"].AsObject;
            return LoadJson(configJson, true);
        }

        string LoadJson(JSONObject json, bool saveJson = true)
        {
            try
            {
                //JSONObject json = JSON.Parse(jsonStr).AsObject;

                if(json == null)
                    throw new Exception("JSON for loading experience is null");

                // TODO: Check whether versions match

                // Read saved app values json
                //string savedAppJsonStr = Utils.ReadFile(valuesFileName + ".json", "app");

                // Update metadata
                if (json["metadata"] != null)
                {
                    if (json["hash"].Value != metaData.hash)
                    {
                        string metadataJsonString = json["metadata"].ToString();
                        metaData = JsonConvert.DeserializeObject<Metadata>(metadataJsonString);

                        onMetadataUpdate?.Invoke();
                    }
                }

                // Update all fields from json
                if (json["fields"] != null)
                {
                    JSONArray fetchedFields = json["fields"].AsArray;
                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (i >= fetchedFields.Count || fetchedFields[i] == null)
                            continue;

                        JSONObject fetchedField = fetchedFields[i].AsObject;
                        fields[i].SetFromJson(fetchedField, true);
                    }
                }

                if (saveJson)
                    FileHandler.WriteString(json.ToString(), configFileName + ".json", appFolderName);

                return json["hash"].Value;
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to parse experience setting JSON: " + e.Message);
                return null;
            }
        }
    }
}