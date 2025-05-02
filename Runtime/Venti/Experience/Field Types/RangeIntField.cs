using UnityEngine;
using UnityEngine.Events;
using SimpleJSON;
using System;

namespace Venti.Experience
{
    [System.Serializable]
    public class RangeIntField : BaseField
    {
        [Header("Configurations")]
        public int step;
        public int minValue;
        public int maxValue;
        public IntDisplayType display;

        [Header("Values")]
        public int @default;
        [field: SerializeField][field: ReadOnly] public int value { get; private set; }

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<int> onChange { get; private set; }
        [field: SerializeField] public UnityEvent<string, int> onChangeWithId { get; private set; }
       
        public RangeIntField()
        {
            type = FieldType.Range;
        }

        public override JSONObject GetJson()
        {
            JSONObject json = base.GetJson();
            json["default"] = @default;
            json["step"] = step;
            json["minValue"] = minValue;
            json["maxValue"] = maxValue;
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
                value = json["value"].AsInt;

            onChange.Invoke(value);
            onChangeWithId.Invoke(id, value);

            return true;
        }

        public enum IntDisplayType
        {
            Slider,
            Number
        }
    }
}