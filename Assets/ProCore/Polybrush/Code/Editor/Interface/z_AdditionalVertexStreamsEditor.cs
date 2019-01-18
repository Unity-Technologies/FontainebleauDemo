using UnityEngine;
using UnityEditor;

namespace Polybrush
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(z_AdditionalVertexStreams))]
	public class z_AdditionalVertexStreamsEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var addlVertexStreamsMesh = target as z_AdditionalVertexStreams;

			if(addlVertexStreamsMesh == null)
				return;

			MeshRenderer mr = addlVertexStreamsMesh.gameObject.GetComponent<MeshRenderer>();

			GUILayout.Label("Additional Vertex Streams");

			if(targets.Length > 1)
				EditorGUI.showMixedValue = true;

			EditorGUILayout.ObjectField(mr.additionalVertexStreams, typeof(Mesh), true);

			EditorGUI.showMixedValue = false;

			if(GUILayout.Button("Delete"))
			{
				foreach(z_AdditionalVertexStreams addlVertStreamMesh in targets)
				{
					if(addlVertStreamMesh == null)
						continue;

					mr = addlVertStreamMesh.GetComponent<MeshRenderer>();

					if(mr != null)
						mr.additionalVertexStreams = null;

					if(addlVertStreamMesh.m_AdditionalVertexStreamMesh != null)
					{
						Undo.DestroyObjectImmediate(addlVertStreamMesh);
						Undo.RecordObject(mr, "Delete AdditionalVertexStreams");
					}
				}
			}
		}
	}
}
