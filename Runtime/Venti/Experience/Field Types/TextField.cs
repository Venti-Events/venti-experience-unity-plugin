using SimpleJSON;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Venti.Experience
{
    [System.Serializable]
    public class TextField : BaseField
    {
        [Header("Configurations")]
        public TextDisplayType display; // InputField or TextArea

        [field: Header("Values")]
        //public string @default;
        [field: SerializeField][field: ReadOnly] public string value { get; private set; }

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<string> onChange { get; private set; }   // send value
        [field: SerializeField] public UnityEvent<string, string> onChangeWithId { get; private set; } // send id and value

        public TextField()
        {
            type = FieldType.text;
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
        //       value = json["value"].Value;

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
            if (!_value.IsString)
                throw new Exception("Value is not a string for field: " + _name + " (" + id + ")");

            value = _value.Value;

            onChange.Invoke(value);
            onChangeWithId.Invoke(id, value);

            return true;
        }

        public enum TextDisplayType
        {
            input,
            textArea,
            //url
        }
    }
}