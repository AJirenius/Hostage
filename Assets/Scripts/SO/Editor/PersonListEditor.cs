using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Hostage.SO.Editor
{
    [CustomEditor(typeof(SOActionPersonList))]
    public class PersonListEditor : UnityEditor.Editor
    {
        private string _folderPath = "Assets/Content/Persons";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            SOActionPersonList soActionPersonList = (SOActionPersonList)target;

            EditorGUILayout.Space();
            _folderPath = EditorGUILayout.TextField("Folder Path", _folderPath);

            if (GUILayout.Button("Scan Folder and Populate List"))
            {
                PopulatePersonList(soActionPersonList, _folderPath);
            }
        }

        private void PopulatePersonList(SOActionPersonList soActionPersonList, string folder)
        {
            string[] guids = AssetDatabase.FindAssets("t:Hostage.SO.SOActionPerson", new[] { folder });
            var foundPersons = new List<SOActionPerson>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SOActionPerson soPerson = AssetDatabase.LoadAssetAtPath<SOActionPerson>(path);
                if (soPerson != null)
                    foundPersons.Add(soPerson);
            }
            Undo.RecordObject(soActionPersonList, "Populate Person List");
            soActionPersonList.SetPersons(foundPersons);
            EditorUtility.SetDirty(soActionPersonList);
        }
    }
}
