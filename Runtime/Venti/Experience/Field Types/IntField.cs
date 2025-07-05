using UnityEngine;
using UnityEngine.Events;
using SimpleJSON;
using System;

namespace Venti.Experience
{
    [System.Serializable]
    public class IntField : BaseField
    {
        [Header("Configurations")]
        public int step;
        public int minValue;
        public int maxValue;
        public NumberDisplay display;

        [field: Header("Values")]
        //public int @default;
        [field: SerializeField][field: ReadOnly] public int value { get; private set; }

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<int> onChange { get; private set; }
        [field: SerializeField] public UnityEvent<string, int> onChangeWithId { get; private set; }
       
        public IntField()
        {
            type = FieldType.number;
        }

        public override JSONObject GetJson()
        {
            JSONObject json = base.GetJson();
            //json["default"] = @default;
            json["display"] = display.ToString();

            json["config"]["step"] = step;
            json["config"]["minValue"] = minValue;
            json["config"]["maxValue"] = maxValue;

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
        //        value = json["value"].AsInt;

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
            if (!_value.IsNumber)
                throw new Exception("Value is not an number for field: " + _name + " (" + id + ")");

            value = _value.AsInt;

            onChange.Invoke(value);
            onChangeWithId.Invoke(id, value);

            return true;
        }
    }

    public enum NumberDisplay
    {
        input,
        slider,
        //stepper
    }
}