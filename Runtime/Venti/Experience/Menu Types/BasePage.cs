using System;
using UnityEngine;
using SimpleJSON;

namespace Venti.Experience
{
    [Serializable]
    public abstract class BasePage : MonoBehaviour
    {
        [field: SerializeField] public string id { get; protected set; }   // Unique Identifier
        [field: SerializeField] public string _name { get; protected set; }    // Human readable name
        [field: NonSerialized] public virtual PageType type { get; protected set; }

        private string eventCallPrefix;
        private Action<string> valueLoadStartEvent;
        private Action<string> valueLoadEndEvent;

        public void GenerateGameObjectName()
        {
            name = $"{id} screen ({type})";
        }

        public virtual void FetchChildPages(bool searchForInactive = false)
        {
            // Tasks to do when parent tries to fetch child fields
            GenerateGameObjectName();
        }

        public virtual void FetchChildFields(bool searchForInactive = false)
        {
            // Tasks to do when parent tries to fetch child fields
            GenerateGameObjectName();
        }

        public virtual void FetchChildren(bool searchForInactive = false)
        {
            FetchChildPages(searchForInactive);
            FetchChildFields(searchForInactive);
        }

        public virtual void Clear()
        {
            // No implementation for base page
        }

        public virtual JSONObject GetMenuJson()
        {
            JSONObject json = new JSONObject();
            json["id"] = id;
            json["name"] = _name;
            return json;
        }

        // Returned flattened object of all child pages or self
        public virtual JSONObject GetPagesJson()
        {
            return new JSONObject();
        }

        public virtual JSONObject GetJson()
        {
            JSONObject json = new JSONObject();
            json["menu"] = GetMenuJson();
            json["pages"] = GetPagesJson();

            return json;
        }

        public virtual bool SetFromJson(JSONObject hashes, JSONObject values)
        {
            // no implementation
            return false;
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