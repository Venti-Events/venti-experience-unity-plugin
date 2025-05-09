using SimpleJSON;
using System;
using UnityEngine;

namespace Venti.Experience
{
    [System.Serializable]
    public class ListRowField : BaseField
    {
        [field: SerializeField] public BaseField[] value { get; private set; }

        public ListRowField()
        {
            type = FieldType.ListRow;
            GenerateRandomId();
        }

        public override void FetchChildFields(bool searchForInactive = false)
        {
            base.FetchChildFields();

            GenerateRandomId();

            // Fetch all child fields and populate their child fields
            value = Utils.FetchChildFields<BaseField>(gameObject, searchForInactive);
        }

        public override void ClearFields()
        {
            Utils.ClearChildFields(value);
            value = null;
        }

        public void GenerateRandomId()
        {
            if (id == null)
                id = Utils.GenerateRandomString(8);
        }

        public void GenerateFields(BaseField[] fields)
        {
            if (fields == null)
            {
                Debug.LogWarning("Fields array is null for listRowField: " + id);
                return;
            }

            foreach (var field in fields)
            {
                BaseField newField = Instantiate(field);
                newField.transform.SetParent(this.transform);
                newField.name = newField._name + " - " + newField.id;
            }

            // Update value fields
            //value = fields;
            FetchChildFields();
        }

        public override JSONObject GetJson()
        {
            // Do not need all properties in BaseField. Make own json.
            JSONObject json = new JSONObject();
            json["id"] = id;
            json["type"] = type.ToString();

            JSONArray valueJson = new JSONArray();
            for (int i = 0; i < value.Length; i++)
                valueJson[i] = value[i].GetJson();
            json["value"] = valueJson;

            return json;
        }

        public override bool SetFromJson(JSONObject json)
        {
            // Set id so BaseField does not trigger a mismatch exception
            // since we copy id from header
            id = json["id"];

            // Check whether hash has changed or not and update it
            if (!base.SetFromJson(json))
                return false;

            if (json["value"] == null)
            {
                value = new BaseField[0];
                Debug.LogWarning("value is null in JSON for " + id);
            }

                JSONArray valueJson = json["value"].AsArray;
            if (valueJson == null)
                throw new Exception("Row has no values for " + id);
            if (valueJson.Count != value.Length)
                throw new Exception("Fields length for row do not match: " + id);

            for (int i = 0; i < value.Length ; i++)
            {
                value[i].SetFromJson(valueJson[i].AsObject);
            }

            return true;
        }
    }
}