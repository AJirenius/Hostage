using UnityEngine;
using UnityEditor;
using Hostage.Core;
using Hostage.Graphs;
using Hostage.Graphs.Editor;
using Unity.GraphToolkit.Editor;

namespace Hostage.SO.Editor
{
    [CustomEditor(typeof(SOIntel))]
    public class IntelEditor : UnityEditor.Editor
    {
        static readonly string[] VerbFieldNames = { "investigate", "interview", "surveillance", "analyze" };
        static readonly string[] VerbDisplayNames = { "Investigate", "Interview", "Surveillance", "Analyze" };

        SerializedProperty graphProp;
        SerializedProperty personProp;
        EventGraph previousGraph;

        void OnEnable()
        {
            graphProp = serializedObject.FindProperty("graph");
            personProp = serializedObject.FindProperty("person");
            previousGraph = graphProp.objectReferenceValue as EventGraph;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("intelName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));

            var categoryProp = serializedObject.FindProperty("category");
            EditorGUILayout.PropertyField(categoryProp);

            if ((IntelCategory)categoryProp.enumValueIndex == IntelCategory.Person)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("person"));
            }

            EditorGUILayout.PropertyField(graphProp);

            bool isPersonCategory = (IntelCategory)categoryProp.enumValueIndex == IntelCategory.Person;
            bool missingPerson = isPersonCategory && personProp.objectReferenceValue == null;

            if (graphProp.objectReferenceValue == null)
            {
                if (GUILayout.Button("Create Graph"))
                {
                    CreateGraphForIntel((SOIntel)target, missingPerson);
                    GUIUtility.ExitGUI();
                }
            }

            for (int i = 0; i < VerbFieldNames.Length; i++)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(VerbFieldNames[i]), true);
            }

            var currentGraph = graphProp.objectReferenceValue as EventGraph;

            // Auto-sync verbs when masterGraph is assigned or changed
            if (currentGraph != previousGraph)
            {
                if (currentGraph != null && currentGraph.StartNodeOutputCount > 1)
                {
                    SyncVerbsToGraph(currentGraph);
                }
                previousGraph = currentGraph;
            }

            // Show mismatch warnings
            if (currentGraph != null && currentGraph.StartNodeOutputCount > 1)
            {
                DrawMismatchWarnings(currentGraph);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void SyncVerbsToGraph(EventGraph graph)
        {
            for (int i = 0; i < VerbFieldNames.Length; i++)
            {
                var verbProp = serializedObject.FindProperty(VerbFieldNames[i]);
                if (verbProp == null) continue;

                var isAvailableProp = verbProp.FindPropertyRelative("isAvailable");
                if (isAvailableProp == null) continue;

                isAvailableProp.boolValue = graph.IsOutputConnected(i);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void CreateGraphForIntel(SOIntel intel, bool createPerson)
        {
            string intelPath = AssetDatabase.GetAssetPath(intel);
            if (string.IsNullOrEmpty(intelPath)) return;

            // e.g. Assets/Content/Intel/Chapter1/1_IntelWife.asset
            string intelDir = System.IO.Path.GetDirectoryName(intelPath).Replace('\\', '/');
            string chapterFolder = System.IO.Path.GetFileName(intelDir); // Chapter1
            string intelFileName = System.IO.Path.GetFileNameWithoutExtension(intelPath); // 1_IntelWife
            string graphFileName = intelFileName.Replace("Intel", "Graph"); // 1_GraphWife

            string graphsRoot = "Assets/Content/Graphs";
            string chapterDir = $"{graphsRoot}/{chapterFolder}";
            string graphDir = $"{chapterDir}/Intel";
            string graphPath = $"{graphDir}/{graphFileName}.eventgraph";

            // Create folder hierarchy if needed
            if (!AssetDatabase.IsValidFolder(graphsRoot))
                AssetDatabase.CreateFolder("Assets/Content", "Graphs");
            if (!AssetDatabase.IsValidFolder(chapterDir))
                AssetDatabase.CreateFolder(graphsRoot, chapterFolder);
            if (!AssetDatabase.IsValidFolder(graphDir))
                AssetDatabase.CreateFolder(chapterDir, "Intel");

            GraphDatabase.CreateGraph<EditorEventGraph>(graphPath);
            AssetDatabase.ImportAsset(graphPath, ImportAssetOptions.ForceUpdate);

            var eventGraph = AssetDatabase.LoadAssetAtPath<EventGraph>(graphPath);
            if (eventGraph == null)
            {
                Debug.LogWarning($"[IntelEditor] Created graph at {graphPath} but could not load it. Open it and add a StartNode.");
                return;
            }

            graphProp.objectReferenceValue = eventGraph;

            if (createPerson)
            {
                string personFileName = intelFileName.Replace("Intel", "Person"); // 1_PersonWife
                string personsRoot = "Assets/Content/Persons";
                string personsChapterDir = $"{personsRoot}/{chapterFolder}";
                string personPath = $"{personsChapterDir}/{personFileName}.asset";

                if (!AssetDatabase.IsValidFolder(personsRoot))
                    AssetDatabase.CreateFolder("Assets/Content", "Persons");
                if (!AssetDatabase.IsValidFolder(personsChapterDir))
                    AssetDatabase.CreateFolder(personsRoot, chapterFolder);

                var person = ScriptableObject.CreateInstance<SOPerson>();
                person.personIntel = intel;
                AssetDatabase.CreateAsset(person, personPath);

                var loadedPerson = AssetDatabase.LoadAssetAtPath<SOPerson>(personPath);
                personProp.objectReferenceValue = loadedPerson;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(intel);
            AssetDatabase.SaveAssets();
        }

        void DrawMismatchWarnings(EventGraph graph)
        {
            for (int i = 0; i < VerbFieldNames.Length; i++)
            {
                var verbProp = serializedObject.FindProperty(VerbFieldNames[i]);
                if (verbProp == null) continue;

                var isAvailableProp = verbProp.FindPropertyRelative("isAvailable");
                if (isAvailableProp == null) continue;

                bool verbEnabled = isAvailableProp.boolValue;
                bool graphConnected = graph.IsOutputConnected(i);

                if (verbEnabled && !graphConnected)
                {
                    EditorGUILayout.HelpBox(
                        $"{VerbDisplayNames[i]} is enabled but has no connection in the graph.",
                        MessageType.Warning);
                }
                else if (!verbEnabled && graphConnected)
                {
                    EditorGUILayout.HelpBox(
                        $"{VerbDisplayNames[i]} is disabled but has a connection in the graph.",
                        MessageType.Warning);
                }
            }
        }
    }
}
