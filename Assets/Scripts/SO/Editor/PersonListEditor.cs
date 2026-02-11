using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Hostage.SO.Editor
{
    [CustomEditor(typeof(SOPersonList))]
    public class PersonListEditor : UnityEditor.Editor
    {
        private string _folderPath = "Assets/Content/Persons";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SOPersonList soPersonList = (SOPersonList)target;

            EditorGUILayout.Space();
            _folderPath = EditorGUILayout.TextField("Folder Path", _folderPath);

            if (GUILayout.Button("Scan Folder and Populate List"))
            {
                PopulatePersonList(soPersonList, _folderPath);
            }
        }

        private void PopulatePersonList(SOPersonList soPersonList, string folder)
        {
            string[] guids = AssetDatabase.FindAssets("t:Hostage.SO.SOPerson", new[] { folder });
            var foundPersons = new List<SOPerson>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SOPerson soPerson = AssetDatabase.LoadAssetAtPath<SOPerson>(path);
                if (soPerson != null)
                    foundPersons.Add(soPerson);
            }
            Undo.RecordObject(soPersonList, "Populate Person List");
            soPersonList.SetPersons(foundPersons);
            EditorUtility.SetDirty(soPersonList);
        }
    }
}
