using System;
using UnityEngine;
using UnityEngine.Events;
using SimpleJSON;

namespace Venti.Experience
{
    [System.Serializable]
    public class DateTimeField : BaseField
    {
        [Header("Configurations")]
        public DateTimeDisplay display;

        [field: Header("Values")]
        //public string @default;
        [field: SerializeField][field: ReadOnly] public DateTime value { get; private set; }
        [field: SerializeField][field: ReadOnly] public string valueRaw { get; private set; }

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<DateTime> onChange { get; private set; }
        [field: SerializeField] public UnityEvent<string, DateTime> onChangeWithId { get; private set; }

        public DateTimeField()
        {
            type = FieldType.dateTime;
        }

        public override JSONObject GetJson()
        {
            JSONObject json = base.GetJson();
            //json["default"] = @default.ToUniversalTime();
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
        //        //valueRaw = @default;
        //        Debug.LogWarning("value is null in JSON for " + id);
        //    }
        //    else
        //        valueRaw = json["value"].Value;

        //    // Convert string to DateTime
        //    if (DateTime.TryParse(valueRaw, out DateTime dateTime))
        //    {
        //        value = dateTime;
        //    }
        //    else
        //    {
        //        throw new Exception("Invalid DateTime format in JSON for " + id);
        //    }

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

            // Convert hex string to Color
            // Convert string to DateTime
            if (DateTime.TryParse(valueRaw, out DateTime dateTime))
                value = dateTime;
            else
                throw new Exception("Invalid DateTime format in JSON for " + id);

            onChange.Invoke(value);
            onChangeWithId.Invoke(id, value);

            return true;
        }

        public enum DateTimeDisplay
        {
            dateTime,
            //date,
            //time,
        }
    }
}