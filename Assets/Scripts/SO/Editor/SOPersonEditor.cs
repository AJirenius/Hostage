using Hostage.Core;
using Hostage.Graphs;
using Hostage.Graphs.Editor;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace Hostage.SO.Editor
{
    [CustomEditor(typeof(SOPerson))]
    public class SOPersonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty property = serializedObject.GetIterator();
            property.NextVisible(true); // skip m_Script

            while (property.NextVisible(false))
            {
                if (property.name == "npcGraph")
                    continue;

                EditorGUILayout.PropertyField(property, true);
            }

            var flagProp = serializedObject.FindProperty("defaultFlag");
            if (((PersonFlag)flagProp.intValue & PersonFlag.Assistant) == 0)
            {
                var graphProp = serializedObject.FindProperty("npcGraph");
                EditorGUILayout.PropertyField(graphProp, true);

                if (graphProp.objectReferenceValue == null)
                {
                    if (GUILayout.Button("Create Graph"))
                    {
                        CreateGraphForNpc((SOPerson)target);
                        GUIUtility.ExitGUI();
                    }
                }
            }

            var intelProp = serializedObject.FindProperty("personIntel");
            if (intelProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Person Intel is required for this person to work.", MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void CreateGraphForNpc(SOPerson person)
        {
            string personPath = AssetDatabase.GetAssetPath(person);
            if (string.IsNullOrEmpty(personPath)) return;

            // e.g. Assets/Content/Persons/Chapter1/1_PersonWife.asset
            string personDir = System.IO.Path.GetDirectoryName(personPath).Replace('\\', '/');
            string chapterFolder = System.IO.Path.GetFileName(personDir); // Chapter1
            string personFileName = System.IO.Path.GetFileNameWithoutExtension(personPath); // 1_PersonWife
            string graphFileName = personFileName.Replace("Person", "GraphNpc"); // 1_GraphNpcWife

            string graphsRoot = "Assets/Content/Graphs";
            string chapterDir = $"{graphsRoot}/{chapterFolder}";
            string graphDir = $"{chapterDir}/Npc";
            string graphPath = $"{graphDir}/{graphFileName}.eventgraph";

            if (!AssetDatabase.IsValidFolder(graphsRoot))
                AssetDatabase.CreateFolder("Assets/Content", "Graphs");
            if (!AssetDatabase.IsValidFolder(chapterDir))
                AssetDatabase.CreateFolder(graphsRoot, chapterFolder);
            if (!AssetDatabase.IsValidFolder(graphDir))
                AssetDatabase.CreateFolder(chapterDir, "Npc");

            GraphDatabase.CreateGraph<EditorEventGraph>(graphPath);
            AssetDatabase.ImportAsset(graphPath, ImportAssetOptions.ForceUpdate);

            var eventGraph = AssetDatabase.LoadAssetAtPath<EventGraph>(graphPath);
            if (eventGraph == null)
            {
                Debug.LogWarning($"[SOPersonEditor] Created graph at {graphPath} but could not load it. Open it and add a StartNode.");
                return;
            }

            var graphProp = serializedObject.FindProperty("npcGraph");
            graphProp.objectReferenceValue = eventGraph;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(person);
            AssetDatabase.SaveAssets();
        }
    }
}
