using UnityEditor;
using UnityEngine;

namespace Venti.Experience
{
    [CustomEditor(typeof(ExperienceManager))]
    public class ExperienceManagerEditor : Editor
    {
        ExperienceManager script;
        GameObject scriptObject;

        private void OnEnable()
        {
            script = (ExperienceManager)target;
            scriptObject = script.gameObject;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Populate Fields"))
            {
                if (script != null)
                    script.FetchChildFields();
                else
                    Debug.LogWarning("DataManager script is null.");
            }

            if (GUILayout.Button("Clear Fields"))
            {
                if (script != null)
                    script.ClearFields();
                else
                    Debug.LogWarning("DataManager script is null.");
            }

            if (GUILayout.Button("Generate JSON"))
            {
                if (script != null)
                    script.ExportJson();
                else
                    Debug.LogWarning("DataManager script is null.");
            }

            // if (GUILayout.Button("Deserialize"))
            // {
            //     DataManager.deserialize.Invoke();
            // }

        }
    }
}
