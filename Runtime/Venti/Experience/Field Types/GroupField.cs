using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SimpleJSON;

namespace Venti.Experience
{
    [System.Serializable]
    public class GroupField : BaseField
    {
        [field: Header("Values")]
        [field: SerializeField][field: ReadOnly] public BaseField[] value { get; private set; }
        private GameObject rows;

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<BaseField[]> onChange { get; private set; }   // send value
        [field: SerializeField] public UnityEvent<string, BaseField[]> onChangeWithId { get; private set; } // send id and value

        private List<string> pendingLoadFieldIds = new List<string>();

        public GroupField()
        {
            type = FieldType.group;
        }

        public override void FetchChildFields(bool searchForInactive = false)
        {
            base.FetchChildFields(searchForInactive);

            // Fetch all row fields and populate their child fields
            value = Utils.FetchChildFields<BaseField>(this.gameObject, searchForInactive);

            // Setup async value load events
            foreach (var field in value)
                field.SetAsyncLoadEvents(hash, OnFieldLoadStart, OnFieldLoadEnd);
        }

        public override void Clear()
        {
            Utils.ClearChildFields(value);
            value = null;
        }

        public override JSONObject GetJson()
        {
            JSONObject json = base.GetJson();
            //for (int i = 0; i < value.Length; i++)
            //    json[i] = value[i].GetJson();

            JSONArray orderJson = new JSONArray();
            JSONObject fieldsJson = new JSONObject();

            for (int i = 0; i < value.Length; i++)
            {
                orderJson.Add(value[i].id);
                fieldsJson[value[i].id] = value[i].GetJson();
            }

            json["order"] = orderJson;
            json["fields"] = fieldsJson;

            return json;
        }

        public override bool SetFromJson(string[] stack, JSONObject hashes, JSONObject values)
        {
            //if (!base.SetFromJson(stack, hashes, values))
            //    return false;

            string[] childStack = Utils.JoinArrays(stack, new string[] { id, "fields"});
            
            // Clear pending field
            pendingLoadFieldIds.Clear();
            // Inform parent that async value load has started
            base.OnAsyncValueLoadStart(id);

            foreach (var field in value)
                field.SetFromJson(childStack, hashes, values);

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
                Debug.LogError($"Field id {fieldId} not found in pending field load paths for group {id}");
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