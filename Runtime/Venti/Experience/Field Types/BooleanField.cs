using SimpleJSON;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Venti.Experience
{
    [System.Serializable]
    public class BooleanField : BaseField
    {
        [Header("Configurations")]
        public BooleanDisplay display;

        [Header("Values")]
        public bool @default;
        [field: SerializeField][field: ReadOnly] public bool value { get; private set; }

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<bool> onChange { get; private set; }
        [field: SerializeField] public UnityEvent<string, bool> onChangeWithId { get; private set; }

        public BooleanField()
        {
            type = FieldType.Boolean;
        }

        public override JSONObject GetJson()
        {
            JSONObject json = base.GetJson();
            json["default"] = @default;
            json["display"] = display.ToString();

            return json;
        }

        public override bool SetFromJson(JSONObject json, bool useCache)
        {
            if (!base.SetFromJson(json, useCache))
                return false;

            if (json["value"] == null)
            {
                value = @default;
                Debug.LogWarning("value is null in JSON for " + id);
            }
            else
                value = json["value"].AsBool;

            onChange.Invoke(value);
            onChangeWithId.Invoke(id, value);

            return true;
        }

        public enum BooleanDisplay
        {
            Toggle,
            Checkbox
        }
    }
}