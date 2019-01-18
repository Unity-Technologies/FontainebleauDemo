using UnityEngine;
using UnityEditor;

namespace Polybrush
{
	[CustomEditor(typeof(MeshFilter)), CanEditMultipleObjects]
	public class z_MeshFilterEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			SerializedProperty mesh = serializedObject.FindProperty("m_Mesh");

			if(mesh != null)
				EditorGUILayout.PropertyField(mesh);

			Mesh m = (Mesh) mesh.objectReferenceValue;

			if(m != null)
			{
				string dontcare = null;
				z_ModelSource source = z_EditorUtility.GetMeshGUID(m, ref dontcare);

				if(	source == z_ModelSource.Scene &&
					!(z_ReflectionUtil.IsProBuilderObject(((MeshFilter)serializedObject.targetObject).gameObject)) )
				{
					if(GUILayout.Button(new GUIContent("Save to Asset", "Save this instance mesh to an Asset so that you can use it as a prefab.")))
					{
						z_EditorUtility.SaveMeshAsset(m);
					}
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
