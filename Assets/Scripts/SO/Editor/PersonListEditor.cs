using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Hostage.SO.Editor
{
    [CustomEditor(typeof(ActionPersonList))]
    public class PersonListEditor : UnityEditor.Editor
    {
        private string _folderPath = "Assets/Content/Persons";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            ActionPersonList actionPersonList = (ActionPersonList)target;

            EditorGUILayout.Space();
            _folderPath = EditorGUILayout.TextField("Folder Path", _folderPath);

            if (GUILayout.Button("Scan Folder and Populate List"))
            {
                PopulatePersonList(actionPersonList, _folderPath);
            }
        }

        private void PopulatePersonList(ActionPersonList actionPersonList, string folder)
        {
            string[] guids = AssetDatabase.FindAssets("t:Hostage.SO.ActionPerson", new[] { folder });
            var foundPersons = new List<ActionPerson>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ActionPerson person = AssetDatabase.LoadAssetAtPath<ActionPerson>(path);
                if (person != null)
                    foundPersons.Add(person);
            }
            Undo.RecordObject(actionPersonList, "Populate Person List");
            actionPersonList.SetPersons(foundPersons);
            EditorUtility.SetDirty(actionPersonList);
        }
    }
}
