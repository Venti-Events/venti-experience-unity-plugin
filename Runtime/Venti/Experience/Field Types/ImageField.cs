using UnityEngine;
using UnityEngine.Events;
using SimpleJSON;
using UnityEngine.Networking;
using System.Collections;
using System.Web;

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

        public override bool SetFromJson(JSONObject json, bool useCache)
        {
            //bool firstFetch = false;
            //if(hash == null) 
            //    firstFetch = true;

            if (!base.SetFromJson(json, useCache))
                return false;

            string newValue;
            if (json["value"] == null)
            {
                newValue = @default;
                Debug.LogWarning("value is null in JSON for " + id);
            }
            else
                newValue = HttpUtility.UrlDecode(json["value"].Value);

            string oldFileName = null;
            string newFileName = null;

            // Extract file names from urls
            if (valueRaw != null)
                oldFileName = valueRaw.Substring(valueRaw.LastIndexOf('/') + 1);
            newFileName = newValue.Substring(newValue.LastIndexOf('/') + 1);
            //Debug.Log("Old value: " + valueRaw);
            //Debug.Log("Old filename: " + oldFileName);

            //Debug.Log("New value: " + newValue);
            //Debug.Log("New filename: " + newFileName);

            valueRaw = newValue;

            // Delete old file
            if (!string.IsNullOrEmpty(oldFileName))
                FileHandler.DeleteFile(oldFileName, ExperienceManager.appFolderName);

            if (FileHandler.FileExists(newFileName, ExperienceManager.appFolderName))
            {
                // If fetched image exists in cache. e.g. first run
                string filePath = FileHandler.GetFilePath(newFileName, ExperienceManager.appFolderName);
                StartCoroutine(FetchImage(filePath, newFileName, false));
                //Debug.Log("Fetching image from cache: " + filePath);
            }
            else
            {
                // Fetch new image from web and save to cache
                StartCoroutine(FetchImage(newValue, newFileName, true));
                //Debug.Log("Fetching image from web: " + newValue);
            }

            return true;
        }

        IEnumerator FetchImage(string url, string fileName, bool saveToCache)
        {
            //using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                // Request and wait for the desired page
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(webRequest.error);
                }
                else
                {
                    value = DownloadHandlerTexture.GetContent(webRequest);

                    if (saveToCache)
                    {
                        byte[] bytes = webRequest.downloadHandler.data;
                        FileHandler.WriteBytes(bytes, fileName, ExperienceManager.appFolderName);
                        //Debug.Log("Saved image to cache: " + fileName);
                    }

                    onChange.Invoke(value);
                    onChangeWithId.Invoke(id, value);
                }
            }
        }
    }
}