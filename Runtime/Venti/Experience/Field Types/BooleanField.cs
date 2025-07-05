using System;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

namespace Venti.Experience
{
    [System.Serializable]
    public class BooleanField : BaseField
    {
        [Header("Configurations")]
        public BooleanDisplay display;

        //[Header("Values")]
        //public bool @default;
        [field: SerializeField][field: ReadOnly] public bool value { get; private set; }

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<bool> onChange { get; private set; }
        [field: SerializeField] public UnityEvent<string, bool> onChangeWithId { get; private set; }

        public BooleanField()
        {
            type = FieldType.boolean;
        }

        public override JSONObject GetJson()
        {
            JSONObject json = base.GetJson();
            //json["default"] = @default;
            json["display"] = display.ToString();

            return json;
        }

        //public override bool SetFromJson(JSONObject json)
        //{
        //    if (!base.SetFromJson(json))
        //        return false;

        //    if (json["value"] == null)
        //    {
        //        //value = @default;
        //        Debug.LogWarning("value is null in JSON for " + id);
        //    }
        //    else
        //        value = json["value"].AsBool;

        //    onChange.Invoke(value);
        //    onChangeWithId.Invoke(id, value);

        //    return true;
        //}

        public override bool SetFromJson(string[] stack, JSONObject hashes, JSONObject values)
        {
            if (!base.SetFromJson(stack, hashes, values))
                return false;

            string path = GetPath(stack);
            JSONNode _value = values[path];
            if (_value == null)
            {
                Debug.LogWarning("Value is null for field: " + _name + " (" + id + ")");
                return false;
            }
            if (!_value.IsBoolean)
            {
                throw new Exception("Value is not a boolean for field: " + _name + " (" + id + ")");
                //return false;
            }

            value = _value.AsBool;

            onChange.Invoke(value);
            onChangeWithId.Invoke(id, value);

            return true;
        }

        public enum BooleanDisplay
        {
            toggle,
            checkbox
        }
    }
}