using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Venti.Experience
{
    [System.Serializable]
    public class ListRowField : BaseField
    {
        [field: SerializeField] public BaseField[] value { get; private set; }

        [field: SerializeField] public UnityEvent<BaseField[]> onChange { get; private set; }   // send value
        [field: SerializeField] public UnityEvent<string, BaseField[]> onChangeWithId { get; private set; } // send id and value

        private List<string> pendingLoadFieldIds = new List<string>();

        public ListRowField()
        {
            type = FieldType.ListRow;
            GenerateRandomId();
        }

        public override void FetchChildFields(bool searchForInactive = false)
        {
            base.FetchChildFields();

            GenerateRandomId();

            // Fetch all child fields and populate their child fields
            value = Utils.FetchChildFields<BaseField>(gameObject, searchForInactive);

            // Setup async value load events
            foreach (var field in value)
                field.SetAsyncLoadEvents(hash, OnFieldLoadStart, OnFieldLoadEnd);
        }

        public override void ClearFields()
        {
            Utils.ClearChildFields(value);
            value = null;
        }

        public void GenerateRandomId()
        {
            if (id == null)
                id = Utils.GenerateRandomString(8);
        }

        public void GenerateFields(BaseField[] fields)
        {
            if (fields == null)
            {
                Debug.LogWarning("Fields array is null for listRowField: " + id);
                return;
            }

            foreach (var field in fields)
            {
                BaseField newField = Instantiate(field);
                newField.transform.SetParent(this.transform);
                newField.name = newField._name + " - " + newField.id;
            }

            // Update value fields
            //value = fields;
            FetchChildFields();
        }

        public override JSONObject GetJson()
        {
            // Do not need all properties in BaseField. Make own json.
            JSONObject json = new JSONObject();
            json["id"] = id;
            json["type"] = type.ToString();

            JSONArray valueJson = new JSONArray();
            for (int i = 0; i < value.Length; i++)
                valueJson[i] = value[i].GetJson();
            json["value"] = valueJson;

            return json;
        }

        public override bool SetFromJson(JSONObject json)
        {
            // Set id so BaseField does not trigger a mismatch exception
            // since we copy id from header
            id = json["id"];

            // Check whether hash has changed or not and update it
            if (!base.SetFromJson(json))
                return false;

            if (json["value"] == null)
            {
                value = new BaseField[0];
                Debug.LogWarning("Row value is null in JSON for " + id);
                return false;
            }

            JSONArray valueJson = json["value"].AsArray;
            if (valueJson == null)
                throw new Exception("Row has no values for " + id);
            if (valueJson.Count != value.Length)
                throw new Exception("Fields length for row do not match: " + id);

            // Clear pending field
            pendingLoadFieldIds.Clear();
            // Inform parent that async value load has started
            base.OnAsyncValueLoadStart(id);

            for (int i = 0; i < value.Length; i++)
                value[i].SetFromJson(valueJson[i].AsObject);

            // There were no async values to load
            if (pendingLoadFieldIds.Count == 0)
            {
                onChange?.Invoke(value);
                onChangeWithId?.Invoke(id, value);

                // Inform parent that all async value have been loaded
                base.OnAsyncValueLoadEnd(id);
            }

            return true;
        }

        private void OnFieldLoadStart(string fieldId)
        {
            if (!pendingLoadFieldIds.Contains(fieldId))
                pendingLoadFieldIds.Add(fieldId);
        }

        private void OnFieldLoadEnd(string fieldId)
        {
            bool removed = pendingLoadFieldIds.Remove(fieldId);
            if (!removed)
            {
                Debug.LogError($"Field id {fieldId} not found in pending field load paths for listRowField {id}");
                return;
            }

            if (pendingLoadFieldIds.Count == 0)
            {
                onChange?.Invoke(value);
                onChangeWithId?.Invoke(id, value);

                // Inform parent that all async value have been loaded
                base.OnAsyncValueLoadEnd(id);
            }
        }
    }
}