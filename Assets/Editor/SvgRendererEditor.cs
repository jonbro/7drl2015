using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SvgRenderer))]
[CanEditMultipleObjects]
public class CustomInspector : Editor {
	SerializedProperty colorProp;

	void OnEnable () {
		// Setup the SerializedProperties
		colorProp = serializedObject.FindProperty ("color");
	}
	public override void OnInspectorGUI () 
	{
		serializedObject.Update ();
		base.OnInspectorGUI();
		foreach (object sr in targets) {
			SvgRenderer getterSetter = (SvgRenderer)sr;
			getterSetter.colorProperty = colorProp.colorValue;
		}
		serializedObject.ApplyModifiedProperties ();
	}
}

