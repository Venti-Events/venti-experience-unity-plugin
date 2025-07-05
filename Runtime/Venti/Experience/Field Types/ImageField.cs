using UnityEngine;
using UnityEngine.Events;
using SimpleJSON;
using System;

namespace Venti.Experience
{
    [System.Serializable]
    public class ImageField : BaseField
    {
        //[Header("Configurations")]
        //public FileDisplay display;

        [field: Header("Values")]
        //public string @default;     // url to image
        [field: SerializeField][field: ReadOnly] public Texture2D value { get; private set; }
        [field: SerializeField][field: ReadOnly] public string valueRaw { get; private set; }   // url to image

        [field: Header("Events")]
        [field: SerializeField] public UnityEvent<Texture2D> onChange { get; private set; }    // send value
        [field: SerializeField] public UnityEvent<string, Texture2D> onChangeWithId { get; private set; }  // send id and value

        private FileDisplay display = FileDisplay.image;

        public ImageField()
        {
            type = FieldType.file;
        }

        public override JSONObject GetJson()
        {
            JSONObject json = base.GetJson();
            //json["default"] = @default;
            json["display"] = display.ToString();

            return json;
        }

        //public override bool SetFromJson(JSONObject json)
        //{
        //    if (!base.SetFromJson(json))
        //        return false;

        //    // string newValue;
        //    if (json["value"] == null)
        //    {
        //        //valueRaw = @default;
        //        Debug.LogWarning("value is null in JSON for " + id);
        //    }
        //    else
        //        valueRaw = json["value"].Value;

        //    base.OnAsyncValueLoadStart(id);
        //    CacheManager.Instance.GetAsset(valueRaw, CachedAssetType.Image, (texture) =>
        //    {
        //        if (texture != null)
        //        {
        //            value = texture as Texture2D;

        //            onChange?.Invoke(value);
        //            onChangeWithId?.Invoke(id, value);
        //        }

        //        base.OnAsyncValueLoadEnd(id);
        //    });

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

            base.OnAsyncValueLoadStart(id);
            CacheManager.Instance.GetAsset(valueRaw, CachedAssetType.Image, (texture) =>
            {
                if (texture != null)
                {
                    value = texture as Texture2D;

                    onChange?.Invoke(value);
                    onChangeWithId?.Invoke(id, value);
                }

                base.OnAsyncValueLoadEnd(id);
            });

            return true;
        }
    }

    public enum FileDisplay
    {
        image,
        //audio,
        //video,
        //pdf,
        //font,
        //other
    }
}