using UnityEngine;
using SimpleJSON;

namespace Venti.Experience
{
    public class ListDefaultField : MonoBehaviour
    {
        [field: SerializeField] public ListRowField[] value { get; private set; }

        public void FetchChildFields(bool searchForInactive = false)
        {
            // Fetch all child fields and populate their child fields
            value = Utils.FetchChildFields<ListRowField>(gameObject, searchForInactive);

            foreach (var field in value)
            {
                field.FetchChildFields(searchForInactive);
            }
        }

        public void ClearFields()
        {
            Utils.ClearChildFields(value);
            value = null;
        }

        public void AddRow(ListHeaderField header)
        {
            if (header == null || header.value == null)
                return;

            GameObject row = new GameObject();
            row.transform.SetParent(this.transform);

            ListRowField rowField = row.AddComponent<ListRowField>();
            rowField.GenerateRandomId();
            rowField.GenerateGameobjectName();

            // Duplicate the header fields in the row
            rowField.GenerateFields(header.value);
        }

        public JSONArray GetJson()
        {
            JSONArray json = new JSONArray();
            for (int i = 0; i < value.Length; i++)
                json[i] = value[i].GetJson();
            return json;
        }
    }
}