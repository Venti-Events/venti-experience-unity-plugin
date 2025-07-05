using UnityEngine;
using UnityEngine.Events;
using SimpleJSON;
using System;

namespace Venti.Experience
{
    [System.Serializable]
    public class DropdownField : BaseField
    {
        [Header("Configurations")]
        public string[] options;
        public DropdownDisplay display;

        [field: Header("Values")]
        //public string @default;
        [field: SerializeField][field: ReadOnly] public int value { get; private set; }
        [field: SerializeField][field: ReadOnly] public string valueRaw { get; private set; }

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<int> onChange { get; private set; }
        [field: SerializeField] public UnityEvent<string, int> onChangeWithId { get; private set; }

        public DropdownField()
        {
            type = FieldType.select;
        }

        public override JSONObject GetJson()
        {
            JSONObject json = base.GetJson();
            //json["default"] = @default;
            json["display"] = display.ToString();

            JSONArray optionsJson = new JSONArray();
            for (int i = 0; i < options.Length; i++)
            {
                //JSONObject optionJson = new JSONObject();
                //optionJson["id"] = options[i].id;
                //optionJson["value"] = options[i].value;

                //optionsJson.Add(optionJson);
                optionsJson.Add(options[i]);
            }
            json["config"]["options"] = optionsJson;

            return json;
        }

        //public override bool SetFromJson(JSONObject json)
        //{
        //    if (!base.SetFromJson(json))
        //        return false;

        //    if (json["value"] == null)
        //    {
        //        //valueRaw = @default;
        //        Debug.LogWarning("value is null in JSON for " + id);
        //    }
        //    else
        //        valueRaw = json["value"].Value;

        //    // Find index for the value in options
        //    int index = Array.IndexOf(options, valueRaw);
        //    if (index == -1)
        //    {
        //        throw new Exception("Invalid value in JSON for " + id);
        //    }
        //    value = index;

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

            valueRaw = _value.Value;

            // Find index for the value in options
            int index = Array.IndexOf(options, valueRaw);
            if (index == -1)
            {
                throw new Exception("Invalid value in JSON for " + id);
            }
            value = index;

            onChange.Invoke(value);
            onChangeWithId.Invoke(id, value);

            return true;
        }

        public enum DropdownDisplay
        {
            dropdown,
            //radioGroup
        }

        //[Serializable]
        //public class DropdownOption
        //{
        //    public string id;
        //    public string value;
        //}
    }
}