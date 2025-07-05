using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace Venti.Experience
{
    public class NestedPage : BasePage
    {
        //[field: SerializeField] public override string id { get; protected set; }
        //[field: SerializeField] public override string _name { get; protected set; }
        [field: SerializeField] public BasePage[] subPages { get; protected set; }

        public NestedPage()
        {
            type = PageType.pageParent;
        }

        public override void FetchChildPages(bool searchForInactive = false)
        {
            subPages = Utils.FetchChildPages(gameObject, searchForInactive);

            //foreach (var field in fields)
            //    field.SetAsyncLoadEvents(appHash, OnFieldLoadStart, OnFieldLoadEnd);
        }

        public override void Clear()
        {
            Utils.ClearChildPages(subPages);
            subPages = null;
        }

        public override JSONObject GetMenuJson()
        {
            JSONObject json = new JSONObject();
            json["id"] = id;
            json["name"] = _name;

            JSONArray subMenu = new JSONArray();
            for (int i = 0; i < subPages.Length; i++)
                subMenu.Add(subPages[i].GetMenuJson());
            json["subMenu"] = subMenu;

            return json;
        }

        public override JSONObject GetPagesJson()
        {
            JSONObject json = new JSONObject();

            for (int i = 0; i < subPages.Length; i++)
            {
                JSONObject subPageJson = subPages[i].GetPagesJson();
                foreach (var pageId in subPageJson.Keys)
                {
                    json[pageId] = subPageJson[pageId];
                }
                //json.Add(subPageJson);  // Will this work to combine json objects???
            }

            return json;
        }

        public override bool SetFromJson(JSONObject hashes, JSONObject values)
        {
            bool success = true;

            foreach (var page in subPages)
            {
                try
                {
                    if (!page.SetFromJson(hashes, values))
                        success = false;
                }
                catch (Exception e)
                {
                    Debug.Log("Failed to set page (" + id + "." + page.id + ")" + e.Message);
                    success = false;
                }
            }

            return success;
        }
    }
}