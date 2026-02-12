using Hostage.Core;
using UnityEditor;

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
                if (property.name == "personMasterGraph")
                    continue;

                EditorGUILayout.PropertyField(property, true);
            }

            var flagProp = serializedObject.FindProperty("defaultFlag");
            if (((PersonFlag)flagProp.intValue & PersonFlag.Assistant) == 0)
            {
                var graphProp = serializedObject.FindProperty("personMasterGraph");
                EditorGUILayout.PropertyField(graphProp, true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
