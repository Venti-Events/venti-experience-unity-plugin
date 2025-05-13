using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Venti.Experience
{
    [Serializable]
    public class ListField : BaseField
    {
        [Header("Configurations")]
        public int minSize;
        public int maxSize;

        [field: Header("Values")]
        [field: SerializeField] public ListHeaderField header { get; private set; }
        [field: SerializeField] public ListDefaultField @default { get; private set; }
        [field: SerializeField][field: ReadOnly] public ListRowField[] value { get; private set; }
        private GameObject rows;

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<ListRowField[]> onChange { get; private set; }   // send value
        [field: SerializeField] public UnityEvent<string, ListRowField[]> onChangeWithId { get; private set; } // send id and value

        private List<string> pendingLoadRowIds = new List<string>();

        public ListField()
        {
            type = FieldType.List;
        }

        public override void FetchChildFields(bool searchForInactive = false)
        {
            base.FetchChildFields(searchForInactive);

            // Fetch header and populate its child fields
            header = this.GetComponentInChildren<ListHeaderField>(searchForInactive);
            if (header != null)
                header.FetchChildFields(searchForInactive);

            // Fetch default and populate its child fields
            @default = this.GetComponentInChildren<ListDefaultField>(searchForInactive);
            if (@default != null)
                @default.FetchChildFields(searchForInactive);

            // Fetch all row fields and populate their child fields
            value = Utils.FetchChildFields<ListRowField>(this.gameObject, searchForInactive);
            if (value != null)
            {
                foreach (var row in value)
                    row.FetchChildFields(searchForInactive);
            }
        }

        public override void ClearFields()
        {
            if (header != null)
                header.ClearFields();
            if (@default != null)
                @default.ClearFields();
            Utils.ClearChildFields(value);

            header = null;
            @default = null;
            value = null;
        }

        public void CheckLayout()
        {
            FetchChildFields();

            if (header == null)
            {
                GameObject headerObject = new GameObject("Header");
                headerObject.transform.SetParent(this.transform);
                header = headerObject.AddComponent<ListHeaderField>();
                headerObject.transform.SetSiblingIndex(0);
            }

            if (@default == null)
            {
                GameObject defaultObject = new GameObject("Default");
                defaultObject.transform.SetParent(this.transform);
                @default = defaultObject.AddComponent<ListDefaultField>();
                defaultObject.transform.SetSiblingIndex(1);
            }
        }

        public void AddDefaultRow()
        {
            CheckLayout();

            if (header.value == null || header.value.Length == 0)
            {
                Debug.LogError("Header is empty. Cannot add default row.");
                return;
            }

            @default.AddRow(header);
        }

        public override JSONObject GetJson()
        {
            JSONObject json = base.GetJson();
            json["header"] = header.GetJson();
            json["default"] = @default.GetJson();
            return json;
        }

        public override bool SetFromJson(JSONObject json)
        {
            if (!base.SetFromJson(json))
                return false;

            if (json["value"] == null)
            {
                value = new ListRowField[0];
                Debug.LogError("List value is null in JSON for " + id);
                return false;
            }

            if (header.value == null || header.value.Length == 0)
            {
                Debug.LogError("Header is empty. Cannot add default row.");
                return false;
            }

            // Create rows object if it doesn't exist
            if (rows == null)
            {
                rows = new GameObject("Rows");
                rows.transform.SetParent(this.transform);
            }
            else
            {
                // Clear existing row gameobjects
                foreach (Transform child in rows.transform)
                    Destroy(child.gameObject);
            }

            // Clear pending row ids
            pendingLoadRowIds.Clear();
            // Inform parent that async value load has started
            base.OnAsyncValueLoadStart(id);

            // Create new rows from JSON
            JSONArray rowsJson = json["value"].AsArray;
            // Empty existing value array
            value = new ListRowField[rowsJson.Count];

            // Create new row gameobjects and populate value array
            for (int i = 0; i < rowsJson.Count; i++)
            {
                JSONObject rowJson = rowsJson[i].AsObject;

                GameObject rowObj = new GameObject($"{rowJson["id"]} (Row {i})");
                rowObj.transform.SetParent(rows.transform);
                rowObj.transform.SetSiblingIndex(i);

                ListRowField rowField = rowObj.AddComponent<ListRowField>();
                rowField.id = rowJson["id"];
                rowField.GenerateGameobjectName();
                rowField.GenerateFields(header.value);
                rowField.SetAsyncLoadEvents(OnRowLoadStart, OnRowLoadEnd);
                rowField.SetFromJson(rowJson);

                value[i] = rowField;
            }

            // There were no async values to load
            if (pendingLoadRowIds.Count == 0)
            {
                onChange?.Invoke(value);
                onChangeWithId?.Invoke(id, value);

                // Inform parent that all async value have been loaded
                base.OnAsyncValueLoadEnd(id);
            }

            return true;
        }

        private void OnRowLoadStart(string rowId)
        {
            if (!pendingLoadRowIds.Contains(rowId))
                pendingLoadRowIds.Add(rowId);
        }

        private void OnRowLoadEnd(string rowId)
        {
            bool removed = pendingLoadRowIds.Remove(rowId);
            if (!removed)
            {
                Debug.LogError($"Row id {rowId} not found in pending row load paths for listField {id}");
                return;
            }

            if (pendingLoadRowIds.Count == 0)
            {
                onChange?.Invoke(value);
                onChangeWithId?.Invoke(id, value);

                // Inform parent that all async value have been loaded
                base.OnAsyncValueLoadEnd(id);
            }
        }
    }
}