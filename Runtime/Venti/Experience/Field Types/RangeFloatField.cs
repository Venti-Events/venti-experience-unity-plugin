using UnityEngine;
using UnityEngine.Events;
using SimpleJSON;
using static UnityEditor.Progress;
using System;

namespace Venti.Experience
{
    [System.Serializable]
    public class RangeFloatField : BaseField
    {
        [Header("Configurations")]
        public float step;
        public float minValue;
        public float maxValue;
        public FloatDisplay display;

        [Header("Values")]
        public float @default;
        [field: SerializeField][field: ReadOnly] public float value { get; private set; }

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<float> onChange { get; private set; }
        [field: SerializeField] public UnityEvent<string, float> onChangeWithId { get; private set; }
       
        public RangeFloatField()
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
                value = json["value"].AsFloat;

            onChange.Invoke(value);
            onChangeWithId.Invoke(id, value);

            return true;
        }

        public enum FloatDisplay
        {
            Slider,
            Number
        }
    }
}