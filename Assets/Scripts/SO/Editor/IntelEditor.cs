using UnityEngine;
using UnityEditor;
using Hostage.Core;
using Hostage.Graphs;

namespace Hostage.SO.Editor
{
    [CustomEditor(typeof(SOIntel))]
    public class IntelEditor : UnityEditor.Editor
    {
        static readonly string[] VerbFieldNames = { "investigate", "interview", "surveillance", "analyze" };
        static readonly string[] VerbDisplayNames = { "Investigate", "Interview", "Surveillance", "Analyze" };

        SerializedProperty graphProp;
        EventGraph previousGraph;

        void OnEnable()
        {
            graphProp = serializedObject.FindProperty("graph");
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
