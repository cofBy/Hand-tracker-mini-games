using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ReadOnlyAttribute readOnly = (ReadOnlyAttribute)attribute;

        SerializedProperty boolProp = property.serializedObject.FindProperty(readOnly.boolName);

        bool oldState = GUI.enabled;

        if (boolProp != null)
        {
            GUI.enabled = boolProp.boolValue;
        }

        EditorGUI.PropertyField(position, property, label, true);

        GUI.enabled = oldState;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}