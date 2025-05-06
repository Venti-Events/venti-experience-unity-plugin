using UnityEngine;
using UnityEngine.Events;
using SimpleJSON;
using System;

namespace Venti.Experience
{
    [System.Serializable]
    public class ColorField : BaseField
    {
        //[Header("Configurations")]
        [Header("Values")]
        public Color @default;
        [field: SerializeField][field: ReadOnly] public Color value { get; private set; }
        [field: SerializeField][field: ReadOnly] public string valueRaw { get; private set; }

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<Color> onChange { get; private set; }
        [field: SerializeField] public UnityEvent<string, Color> onChangeWithId { get; private set; }

        public ColorField()
        {
            type = FieldType.Color;
        }

        public override JSONObject GetJson()
        {
            JSONObject json = base.GetJson();
            json["default"] = "#" + ColorUtility.ToHtmlStringRGBA(@default);

            return json;
        }

        public override bool SetFromJson(JSONObject json, bool useCache)
        {
            if (!base.SetFromJson(json, useCache))
                return false;

            if (json["value"] == null)
            {
                valueRaw = "#" + ColorUtility.ToHtmlStringRGBA(@default);
                Debug.LogWarning("value is null in JSON for " + id);
            }
            else
                valueRaw = json["value"].Value;
                
            // Convert hex string to Color
            if (UnityEngine.ColorUtility.TryParseHtmlString(valueRaw, out Color color))
                value = color;
            else
                throw new Exception("Invalid color format in JSON for " + id);

            onChange.Invoke(value);
            onChangeWithId.Invoke(id, value);

            return true;
        }
    }
}