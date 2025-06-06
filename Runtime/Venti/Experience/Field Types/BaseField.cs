using System;
using UnityEngine;
using SimpleJSON;

namespace Venti.Experience
{
    [Serializable]
    public abstract class BaseField : MonoBehaviour
    {
        public string id;  // Unique Identifier
        public string _name;   // Human readable name. Cannot use "name" as it is a reserved keyword
        [Multiline]
        public string description;    // Shown on hovering on info icon
        [SerializeField][ReadOnly] protected string hash;

        [field: NonSerialized] public virtual FieldType? type { get; protected set; } = null;

        private string eventCallPrefix;
        private Action<string> valueLoadStartEvent;
        private Action<string> valueLoadEndEvent;

        public void GenerateGameObjectName()
        {
            name = $"{id} ({type})";
        }

        public virtual void FetchChildFields(bool searchForInactive = false)
        {
            // Tasks to do when parent tries to fetch child fields
            GenerateGameObjectName();
        }

        public virtual void ClearFields()
        {
            // No implementation for base fields
        }

        public virtual JSONObject GetJson()
        {
            JSONObject json = new JSONObject();
            json["id"] = id;
            json["name"] = _name;
            json["description"] = description;

            if (type != null)
                json["type"] = type.ToString();
            else
                throw new Exception("Type undefined for field: " + _name + " (" + id + ")");

            return json;
        }

        public virtual bool SetFromJson(JSONObject json)
        {
            if (json == null)
                throw new Exception("JSON is null for field: " + _name + " (" + id + ")");
            if (id != json["id"])
                throw new Exception("ID mismatch for field: " + _name + " (" + id + ") - Expected: " + id + ", Found: " + json["id"]);
            //if (json["hash"] == null)
            //    throw new Exception("No hash for field: " + _name + " (" + id + ")");

            if (json["hash"] == hash)
                return false;

            hash = json["hash"];
            return true;
        }

        public void SetAsyncLoadEvents(string prefix, Action<string> onValueLoadStart, Action<string> onValueLoadEnd)
        {                
            if (prefix == null)
                eventCallPrefix = "";
            else
                eventCallPrefix = prefix;

            valueLoadStartEvent = onValueLoadStart;
            valueLoadEndEvent = onValueLoadEnd;
        }

        protected void OnAsyncValueLoadStart(string value)
        {
            valueLoadStartEvent?.Invoke(eventCallPrefix + "/" + value);
        }

        protected void OnAsyncValueLoadEnd(string value)
        {
            valueLoadEndEvent?.Invoke(eventCallPrefix + "/" + value);
        }
    }
}