using System;
using UnityEngine;
using UnityEngine.Events;
using SimpleJSON;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;

namespace Venti.Experience
{
    public class ExperienceManager : Singleton<ExperienceManager>
    {
        [field: SerializeField] public Metadata metaData { get; private set; }
        [field: SerializeField] public BaseField[] fields { get; private set; }

        // Settings
        [Tooltip("Enabling this will include inactive GameObjects in search")]
        [SerializeField] private bool searchForInactive = false;

        // Events
        public UnityEvent onMetadataUpdate;
        public UnityEvent onFieldsUpdate;

        // Path Constants
        // public const string appFolderName = "cache";
        private const string configFileName = "app-config";
        private const string getAppConfigUrl = @"/experience-app/get-experience-app-configuration";

        // Private Variables
        private string appHash;
        private List<string> pendingAssetLoadPaths = new List<string>();

        // TODO: queuedJson with pendingAssetLoadPaths

        #region Unity_Methods
        private void Start()
        {
            ClearFields();
            FetchChildFields();

            LoadFromLocalJson();
        }
        #endregion

        #region Public_Methods
        // Fetch app config
        public void FetchAppConfig(string hash)
        {
            if (hash != appHash)
            {
                Debug.Log("App hashes mismatch. Re-fetch");
                StartCoroutine(GetAppConfig());
            }
            else
                Debug.Log("App hash is the same, no need to fetch again.");
        }

        public void FetchChildFields()
        {
            fields = Utils.FetchChildFields<BaseField>(gameObject, searchForInactive);

            foreach (var field in fields)
                field.SetAsyncLoadEvents(appHash, OnFieldLoadStart, OnFieldLoadEnd);
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

        public void SaveJson()
        {
            FetchChildFields();

            string jsonStr = GetJson().ToString();
            Debug.Log(jsonStr);

            FileHandler.WriteString(jsonStr, $"{metaData.experienceId}-schema-v{metaData.version.ToString()}.json", "Exports", true);
        }
        #endregion

        #region Private_Methods
        private bool LoadFromLocalJson()
        {
            string jsonStr = FileHandler.ReadFile(configFileName + ".json", CacheManager.cacheFolderName);

            if (jsonStr != null)
            {
                JSONObject json = JSON.Parse(jsonStr).AsObject;
                return LoadJson(json, false);
            }

            return false;
        }

        private bool LoadFromWebJson(string jsonStr)
        {
            Debug.Log("Experience JSON: " + jsonStr);

            JSONObject json = JSON.Parse(jsonStr).AsObject;
            if (json == null)
            {
                Debug.LogError("JSON for loading experience is null");
                return false;
            }

            JSONObject configJson = json["data"]["configuration"].AsObject;
            return LoadJson(configJson, true);
        }

        private IEnumerator GetAppConfig()
        {
            // Debug.Log("Fetching app config from: " + url + " with hash: " + hash);
            using (VentiApiRequest www = VentiApiRequest.GetApi(getAppConfigUrl))
            {
                yield return www.SendAuthenticatedApiRequest();

                if (www.result != VentiApiRequest.Result.Success)
                {
                    Debug.LogError("Error fetching app config: " + www.error);
                }
                else
                {
                    // Parse the response and update the experience manager
                    string jsonResponse = www.downloadHandler.text;
                    Debug.Log("Fetched app json: " + jsonResponse);

                    bool success = LoadFromWebJson(jsonResponse);
                    // if (success)
                    // {
                    //     appHash = hash;
                    // }
                }
            }
        }

        private bool LoadJson(JSONObject json, bool saveJson = true)
        {
            try
            {
                bool success = true;

                if (json == null)
                    throw new Exception("JSON for loading experience is null");

                pendingAssetLoadPaths.Clear();

                // TODO: Check whether versions match

                // Update metadata
                if (json["metadata"] != null)
                {
                    if (json["metadata"]["hash"].Value != metaData.hash)
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

                        // Catch any errors but continue updating other fields
                        try
                        {
                            JSONObject fetchedField = fetchedFields[i].AsObject;
                            fields[i].SetFromJson(fetchedField);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Unable to parse field JSON: " + e.Message);
                            success = false;
                        }
                    }
                }

                if (success)    // so manual refresh works
                    appHash = json["hash"].ToString();

                if (saveJson)
                    FileHandler.WriteString(json.ToString(), configFileName + ".json", CacheManager.cacheFolderName);

                // if (success && pendingAssetLoadPaths.Count == 0)
                if (pendingAssetLoadPaths.Count == 0)
                    onFieldsUpdate?.Invoke();

                return success;
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to parse experience setting JSON: " + e.Message);
                return false;
            }
        }
        #endregion

        private void OnFieldLoadStart(string fieldId)
        {
            pendingAssetLoadPaths.Add(fieldId);
        }

        private void OnFieldLoadEnd(string fieldId)
        {
            bool removed = pendingAssetLoadPaths.Remove(fieldId);
            if (!removed)
            {
                Debug.LogError($"Asset path {appHash}/{fieldId} not found in pending field load paths for experienceManager");
                return;
            }

            if (pendingAssetLoadPaths.Count <= 0)
                onFieldsUpdate?.Invoke();
        }
    }
}