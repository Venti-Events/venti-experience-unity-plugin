using SimpleJSON;
using UnityEngine;

namespace Venti.Experience
{
    public class ListHeaderField : MonoBehaviour
    {
        [field: SerializeField] public BaseField[] value { get; private set; }

        public void FetchChildFields(bool searchForInactive = false)
        {
            // Fetch all child fields and populate their child fields
            value = Utils.FetchChildFields<BaseField>(gameObject, searchForInactive);
        }

        public void ClearFields()
        {
            Utils.ClearChildFields(value);
            value = null;
        }

        public JSONObject GetJson()
        {
            JSONObject json = new JSONObject();
            //for (int i = 0; i < value.Length; i++)
            //    json[i] = value[i].GetJson();

            JSONArray orderJson = new JSONArray();
            JSONObject fieldsJson = new JSONObject();

            for (int i = 0; i < value.Length; i++)
            {
                orderJson.Add(value[i].id);
                fieldsJson[value[i].id] = value[i].GetJson();
            }

            json["order"] = orderJson;
            json["fields"] = fieldsJson;

            return json;
        }
    }
}