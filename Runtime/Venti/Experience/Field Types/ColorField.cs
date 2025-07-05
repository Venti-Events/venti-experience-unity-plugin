using System;
using UnityEngine;
using UnityEngine.Events;
using SimpleJSON;

namespace Venti.Experience
{
    [System.Serializable]
    public class ColorField : BaseField
    {
        //[Header("Configurations")]
        //public Color @default;
        [field: Header("Values")]
        [field: SerializeField][field: ReadOnly] public Color value { get; private set; }
        [field: SerializeField][field: ReadOnly] public string valueRaw { get; private set; }

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<Color> onChange { get; private set; }
        [field: SerializeField] public UnityEvent<string, Color> onChangeWithId { get; private set; }

        public ColorField()
        {
            type = FieldType.color;
        }

        public override JSONObject GetJson()
        {
            JSONObject json = base.GetJson();
            //json["default"] = "#" + ColorUtility.ToHtmlStringRGBA(@default);

            return json;
        }

        //public override bool SetFromJson(JSONObject json)
        //{
        //    if (!base.SetFromJson(json))
        //        return false;

        //    if (json["value"] == null)
        //    {
        //        //valueRaw = "#" + ColorUtility.ToHtmlStringRGBA(@default);
        //        Debug.LogWarning("value is null in JSON for " + id);
        //    }
        //    else
        //        valueRaw = json["value"].Value;

        //    // Convert hex string to Color
        //    if (UnityEngine.ColorUtility.TryParseHtmlString(valueRaw, out Color color))
        //        value = color;
        //    else
        //        throw new Exception("Invalid color format in JSON for " + id);

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
            if (ColorUtility.TryParseHtmlString(valueRaw, out Color color))
                value = color;
            else
                throw new Exception("Invalid color format in JSON for " + id);

            onChange.Invoke(value);
            onChangeWithId.Invoke(id, value);

            return true;
        }
    }
}