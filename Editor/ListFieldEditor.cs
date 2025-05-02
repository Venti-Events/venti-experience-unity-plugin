using UnityEditor;
using UnityEngine;

namespace Venti.Experience
{
    [CustomEditor(typeof(ListField))]
    public class ListFieldEditor : Editor
    {
        ListField script;
        GameObject scriptObject;
        
        private void OnEnable()
        {
            script = (ListField)target;
            scriptObject = script.gameObject;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Populate List"))
            {
                script.FetchChildFields();
            }

            if (GUILayout.Button("Clear List"))
            {
                script.ClearFields();
            }

            if (GUILayout.Button("Check Layout"))
            {
                script.CheckLayout();
            }

            if (GUILayout.Button("Add Default Row"))
            {
                script.AddDefaultRow();
            }
        }
    }
}
