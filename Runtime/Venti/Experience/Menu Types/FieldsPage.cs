using System;
using UnityEngine;
using SimpleJSON;

namespace Venti.Experience
{
    public class FieldsPage : BasePage
    {
        //[field: SerializeField] public override PageType type { get; protected set; }
        [field: SerializeField] public FieldsPageType _type { get; protected set; }

        [field: SerializeField] public BaseField[] fields { get; protected set; }

        public FieldsPage()
        {
            if (_type == FieldsPageType.imageFields)
                type = PageType.imageFields;
            else
                type = PageType.fields;
        }

        public override void FetchChildFields(bool searchForInactive = false)
        {
            fields = Utils.FetchChildFields<BaseField>(gameObject, searchForInactive);

            //foreach (var field in fields)
            //    field.SetAsyncLoadEvents(appHash, OnFieldLoadStart, OnFieldLoadEnd);
        }

        public override void Clear()
        {
            Utils.ClearChildFields(fields);
            fields = null;
        }

        public override JSONObject GetPagesJson()
        {
            JSONObject json = new JSONObject();

            JSONObject pageJson = new JSONObject();
            JSONArray orderJson = new JSONArray();
            JSONObject fieldsJson = new JSONObject();

            for (int i = 0; i < fields.Length; i++)
            {
                orderJson.Add(fields[i].id);
                fieldsJson[fields[i].id] = fields[i].GetJson();
            }

            pageJson["type"] = type.ToString();
            pageJson["order"] = orderJson;
            pageJson["fields"] = fieldsJson;

            json[id] = pageJson;

            return json;
        }

        public override bool SetFromJson(JSONObject hashes, JSONObject values)
        {
            bool success = true;

            string[] stack = new string[] { id, "fields"};

            foreach (var field in fields)
            {
                try
                {
                    if (!field.SetFromJson(stack, hashes, values))
                        success = false;
                }
                catch (Exception e)
                {
                    Debug.LogError("Unable to set value for field(" + id + "." + field.id + "): " + e.Message);
                }
            }

            return success;
        }

        public enum FieldsPageType
        {
            fields,
            imageFields,
        }
    }
}