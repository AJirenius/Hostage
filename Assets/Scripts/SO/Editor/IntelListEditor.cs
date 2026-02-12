using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Hostage.SO.Editor
{
    [CustomEditor(typeof(IntelList))]
    public class IntelListEditor : UnityEditor.Editor
    {
        private string _folderPath = "Assets/";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            IntelList intelList = (IntelList)target;

            EditorGUILayout.Space();
            _folderPath = EditorGUILayout.TextField("Folder Path", _folderPath);

            if (GUILayout.Button("Scan Folder and Populate List"))
            {
                PopulateIntelList(intelList, _folderPath);
            }
        }

        private void PopulateIntelList(IntelList intelList, string folder)
        {
            string[] guids = AssetDatabase.FindAssets("t:SOIntel", new[] { folder });
            var foundIntels = new List<SOIntel>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SOIntel soIntel = AssetDatabase.LoadAssetAtPath<SOIntel>(path);
                if (soIntel != null)
                    foundIntels.Add(soIntel);
            }
            Undo.RecordObject(intelList, "Populate Intel List");
            intelList.SetIntels(foundIntels);
            EditorUtility.SetDirty(intelList);
        }
    }
}
