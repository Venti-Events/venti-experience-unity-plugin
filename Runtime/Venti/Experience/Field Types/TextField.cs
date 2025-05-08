using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

namespace Venti.Experience
{
    [System.Serializable]
    public class TextField : BaseField
    {
        [Header("Configurations")]
        public TextDisplayType display; // InputField or TextArea

        [Header("Values")]
        public string @default;
        [field: SerializeField][field: ReadOnly] public string value { get; private set; }

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<string> onChange { get; private set; }   // send value
        [field: SerializeField] public UnityEvent<string, string> onChangeWithId { get; private set; } // send id and value

        public TextField()
        {
            type = FieldType.Text;
        }

        public override JSONObject GetJson()
        {
            JSONObject json = base.GetJson();
            json["default"] = @default;
            json["display"] = display.ToString();

            return json;
        }

        public override bool SetFromJson(JSONObject json)
        {
            if (!base.SetFromJson(json))
                return false;

            if (json["value"] == null)
            {
                value = @default;    
                Debug.LogWarning("value is null in JSON for " + id);
            }
            else
               value = json["value"].Value;

            onChange.Invoke(value);
            onChangeWithId.Invoke(id, value);

            return true;
        }

        public enum TextDisplayType
        {
            Input,
            TextArea
        }
    }
}