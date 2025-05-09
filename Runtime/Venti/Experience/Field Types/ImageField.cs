using UnityEngine;
using UnityEngine.Events;
using SimpleJSON;

namespace Venti.Experience
{
    [System.Serializable]
    public class ImageField : BaseField
    {
        //[Header("Configurations")]
        [Header("Values")]
        public string @default;     // url to image
        [field: SerializeField][field: ReadOnly] public Texture2D value { get; private set; }
        [field: SerializeField][field: ReadOnly] public string valueRaw { get; private set; }   // url to image

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<Texture2D> onChange { get; private set; }    // send value
        [field: SerializeField] public UnityEvent<string, Texture2D> onChangeWithId { get; private set; }  // send id and value

        public ImageField()
        {
            type = FieldType.Image;
        }

        public override JSONObject GetJson()
        {
            JSONObject json = base.GetJson();
            json["default"] = @default;

            return json;
        }

        public override bool SetFromJson(JSONObject json)
        {
            if (!base.SetFromJson(json))
                return false;

            string newValue;
            if (json["value"] == null)
            {
                newValue = @default;
                Debug.LogWarning("value is null in JSON for " + id);
            }
            else
                newValue = json["value"].Value;

            CacheManager.Instance.GetImage(valueRaw, newValue, ExperienceManager.appFolderName, (texture) =>
            {
                valueRaw = newValue;
                value = texture;

                onChange.Invoke(value);
                onChangeWithId.Invoke(id, value);
            });

            return true;
        }
    }
}