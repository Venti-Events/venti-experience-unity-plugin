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

        [Header("Values")]
        public string @default;
        [field: SerializeField][field: ReadOnly] public int value { get; private set; }
        [field: SerializeField][field: ReadOnly] public string valueRaw { get; private set; }

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<int> onChange { get; private set; }
        [field: SerializeField] public UnityEvent<string, int> onChangeWithId { get; private set; }

        public DropdownField()
        {
            type = FieldType.Dropdown;
        }

        public override JSONObject GetJson()
        {
            JSONObject json = base.GetJson();
            json["default"] = @default;
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
            json["options"] = optionsJson;

            return json;
        }

        public override bool SetFromJson(JSONObject json)
        {
            if (!base.SetFromJson(json))
                return false;

            if (json["value"] == null)
            {
                valueRaw = @default;
                Debug.LogWarning("value is null in JSON for " + id);
            }
            else
                valueRaw = json["value"].Value;

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
            Dropdown,
            Radio
        }

        //[Serializable]
        //public class DropdownOption
        //{
        //    public string id;
        //    public string value;
        //}
    }
}