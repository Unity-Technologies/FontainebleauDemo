using UnityEditor;
using UnityEngine;

namespace Polybrush
{
	[CustomEditor(typeof(z_AttributeLayoutContainer), true)]
	public class z_AttributeLayoutContainerEditor : Editor
	{
		private static readonly Color LIGHT_GRAY = new Color(.13f, .13f, .13f, .3f);
		private static readonly Color DARK_GRAY = new Color(.3f, .3f, .3f, .3f);

		SerializedProperty 	p_shader,
							p_attributes;

		void OnEnable()
		{
			if(target == null)
			{
				GameObject.DestroyImmediate(this);
				return;
			}

			p_shader = serializedObject.FindProperty("shader");
			p_attributes = serializedObject.FindProperty("attributes");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(p_shader);

			for(int i = 0; i < p_attributes.arraySize; i++)
			{
				SerializedProperty attrib = p_attributes.GetArrayElementAtIndex(i);
				
				GUI.backgroundColor = i % 2 == 0 ? LIGHT_GRAY : DARK_GRAY;
				GUILayout.BeginVertical(z_GUI.backgroundColorStyle);
				GUI.backgroundColor = Color.white;

				SerializedProperty target = attrib.FindPropertyRelative("propertyTarget");
				SerializedProperty channel = attrib.FindPropertyRelative("channel");
				SerializedProperty index = attrib.FindPropertyRelative("index");
				SerializedProperty range = attrib.FindPropertyRelative("range");
				SerializedProperty mask = attrib.FindPropertyRelative("mask");

				EditorGUILayout.PropertyField(target);
				EditorGUILayout.PropertyField(channel);
				EditorGUILayout.IntPopup(index, z_ComponentIndexUtility.ComponentIndexPopupDescriptions, z_ComponentIndexUtility.ComponentIndexPopupValues);
				
				bool old = EditorGUIUtility.wideMode;
				EditorGUIUtility.wideMode = true;
				EditorGUILayout.PropertyField(range);
				EditorGUIUtility.wideMode = old;

				EditorGUILayout.IntPopup(mask, z_AttributeLayout.DefaultMaskDescriptions, z_AttributeLayout.DefaultMaskValues, z_GUI.TempContent("Group"));

				GUILayout.BeginHorizontal();

					GUILayout.FlexibleSpace();

					if(GUILayout.Button("Delete", EditorStyles.miniButton))
						p_attributes.DeleteArrayElementAtIndex(i);

				GUILayout.EndHorizontal();

				GUILayout.EndVertical();
			}

			if(GUILayout.Button("Add Attribute"))
				p_attributes.arraySize++;

			serializedObject.ApplyModifiedProperties();
		}
	}
}
