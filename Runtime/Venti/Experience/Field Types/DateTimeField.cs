using System;
using UnityEngine;
using UnityEngine.Events;
using SimpleJSON;

namespace Venti.Experience
{
    [System.Serializable]
    public class DateTimeField : BaseField
    {
        //[Header("Configurations")]
        //public DateTimeDisplay display;

        [Header("Values")]
        public string @default;
        [field: SerializeField][field: ReadOnly] public DateTime value { get; private set; }
        [field: SerializeField][field: ReadOnly] public string valueRaw { get; private set; }

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<DateTime> onChange { get; private set; }
        [field: SerializeField] public UnityEvent<string, DateTime> onChangeWithId { get; private set; }

        public DateTimeField()
        {
            type = FieldType.DateTime;
        }

        public override JSONObject GetJson()
        {
            JSONObject json = base.GetJson();
            //json["default"] = @default.ToUniversalTime();
            json["default"] = @default;

            //json["display"] = display.ToString();

            return json;
        }

        public override bool SetFromJson(JSONObject json, bool useCache)
        {
            if (!base.SetFromJson(json, useCache))
                return false;

            if (json["value"] == null)
            {
                valueRaw = @default;
                Debug.LogWarning("value is null in JSON for " + id);
            }
            else
                valueRaw = json["value"].Value;

            // Convert string to DateTime
            if (DateTime.TryParse(valueRaw, out DateTime dateTime))
            {
                value = dateTime;
            }
            else
            {
                throw new Exception("Invalid DateTime format in JSON for " + id);
            }

            onChange.Invoke(value);
            onChangeWithId.Invoke(id, value);

            return true;
        }

        //public enum DateTimeDisplay
        //{
        //    Date,
        //    Time,
        //    DateTime
        //}
    }
}