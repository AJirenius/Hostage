using UnityEditor;
using UnityEngine;
using Hostage.SO;
using System.Reflection;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(Verb), true)]
public class VerbDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Draw a horizontal line separator above each verb
        float lineY = position.y;
        Rect lineRect = new Rect(position.x, lineY, position.width, 1);
        EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 1f));
        float y = position.y + 2 + EditorGUIUtility.standardVerticalSpacing;
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        EditorGUI.BeginProperty(position, label, property);
        var isAvailableProp = property.FindPropertyRelative("isAvailable");
        var actionTypeProp = property.FindPropertyRelative("<actionType>k__BackingField");

        // Get the ActionType name for the label
        string actionTypeName = "Action";
        if (actionTypeProp != null)
        {
            actionTypeName = actionTypeProp.enumDisplayNames[actionTypeProp.enumValueIndex];
        }
        else
        {
            // fallback: try to get from the object
            var targetObj = property.serializedObject.targetObject;
            var type = targetObj.GetType();
            var field = type.GetField(property.propertyPath);
            if (field != null)
            {
                var verbObj = field.GetValue(targetObj) as Verb;
                if (verbObj != null)
                    actionTypeName = verbObj.actionType.ToString();
            }
        }

        // Draw isAvailable with ActionType name, bold and colored
        var boldLabelStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.2f, 0.5f, 1f, 1f) } // blueish color for visibility
        };
        Rect isAvailableRect = new Rect(position.x, y, position.width, lineHeight);
        // Draw the checkbox manually to allow custom label style
        isAvailableProp.boolValue = EditorGUI.ToggleLeft(isAvailableRect, new GUIContent(actionTypeName), isAvailableProp.boolValue, boldLabelStyle);
        y += lineHeight + spacing;

        if (isAvailableProp.boolValue)
        {
            // Draw all serialized fields except isAvailable
            var iterator = property.Copy();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.propertyPath == property.propertyPath + ".isAvailable")
                    continue;
                if (iterator.depth != property.depth + 1)
                    break;
                Rect fieldRect = new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(iterator, true));
                EditorGUI.PropertyField(fieldRect, iterator, true);
                y += EditorGUI.GetPropertyHeight(iterator, true) + spacing;
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        var isAvailableProp = property.FindPropertyRelative("isAvailable");
        if (isAvailableProp.boolValue)
        {
            // Add heights for all serialized fields except isAvailable
            var iterator = property.Copy();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.propertyPath == property.propertyPath + ".isAvailable")
                    continue;
                if (iterator.depth != property.depth + 1)
                    break;
                height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
            }
        }
        return height;
    }
}
