// C# example.
using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(NamedColor))]
public class NamedColorDrawer : PropertyDrawer {

	// Draw the property inside the given rect
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		// Using BeginProperty / EndProperty on the parent property means that
		// prefab override logic works on the entire property.
		EditorGUI.BeginProperty(position, label, property);

		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		// Calculate rects
		Rect nameRect = new Rect(position.x, position.y, position.width/2-10, position.height);
		Rect colorRect = new Rect(position.width/2+10, position.y, position.width/2-10, position.height);

		// Draw fields - passs GUIContent.none to each so they are drawn without labels
		property.FindPropertyRelative ("name").stringValue = EditorGUI.TextField (nameRect, GUIContent.none, property.FindPropertyRelative ("name").stringValue);
		property.FindPropertyRelative ("color").colorValue = EditorGUI.ColorField (colorRect, GUIContent.none, property.FindPropertyRelative ("color").colorValue);
		// Set indent back to what it was
		EditorGUI.indentLevel = indent;

		EditorGUI.EndProperty();
	}
}
