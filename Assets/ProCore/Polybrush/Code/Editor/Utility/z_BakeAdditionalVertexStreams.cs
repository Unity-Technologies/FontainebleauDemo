using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Polybrush
{
	/**
	 *	Utility for applying vertex stream data directly to a mesh.  Can either override the existing
	 *	mesh arrays or create a new mesh from the composite.
	 */
	public class z_BakeAdditionalVertexStreams : EditorWindow
	{
		[MenuItem("Tools/Polybrush/Bake Vertex Streams")]
		static void Init()
		{
			EditorWindow.GetWindow<z_BakeAdditionalVertexStreams>(true, "Bake Vertex Streams", true);
		}

		void OnEnable()
		{
			Undo.undoRedoPerformed += UndoRedoPerformed;
			autoRepaintOnSceneChange = true;
			OnSelectionChange();
		}

		void OnFocus()
		{
			OnSelectionChange();
		}

		void OnDisable()
		{
			Undo.undoRedoPerformed -= UndoRedoPerformed;
		}

		private List<z_AdditionalVertexStreams> m_VertexStreams = new List<z_AdditionalVertexStreams>();
		private Vector2 scroll = Vector2.zero;

		private GUIContent m_BatchNewMesh = new GUIContent("Create New\nComposite Mesh", "Create a new mesh for each selected mesh, automatically prefixing the built meshes with z_ and index.  This is useful in situations where you have used Additional Vertex Streams to paint a single mesh source many times and would like to ensure that all meshes remain unique.");

		void OnGUI()
		{
			GUILayout.BeginHorizontal();

				GUILayout.BeginVertical();
					GUILayout.Label("Selected", EditorStyles.boldLabel);

					scroll = EditorGUILayout.BeginScrollView(scroll, false, true);

					foreach(z_AdditionalVertexStreams vertexStream in m_VertexStreams)
					{
						if(vertexStream != null)
							GUILayout.Label(string.Format("{0} ({1})", vertexStream.gameObject.name, vertexStream.m_AdditionalVertexStreamMesh == null ? "null" : vertexStream.m_AdditionalVertexStreamMesh.name));
					}

					EditorGUILayout.EndScrollView();
				GUILayout.EndVertical();

				GUILayout.BeginVertical();
					GUILayout.Label("Bake Options", EditorStyles.boldLabel);

					GUI.enabled = m_VertexStreams.Count == 1;
					
					if(GUILayout.Button("Apply to\nCurrent Mesh"))
					{
						if( EditorUtility.DisplayDialog("Apply Vertex Streams to Mesh", "This action is not undo-able, are you sure you want to continue?", "Yes", "Cancel") )
						{
							foreach(var stream in m_VertexStreams)
								CreateComposite(stream, true);
						}

						m_VertexStreams.Clear();
					}

					if(GUILayout.Button("Create New\nComposite Mesh"))
					{
						foreach(var stream in m_VertexStreams)
							CreateComposite(stream, false);

						m_VertexStreams.Clear();
					}

					GUI.enabled = m_VertexStreams.Count > 0;

					GUILayout.Label("Batch Options", EditorStyles.boldLabel);

					if(GUILayout.Button(m_BatchNewMesh))
					{
						string path = EditorUtility.OpenFolderPanel("Save Vertex Stream Meshes", "Assets", "");

						for(int i = 0; i < m_VertexStreams.Count; i++)
						{
							path = path.Replace(Application.dataPath, "Assets");

							if(m_VertexStreams[i] == null || m_VertexStreams[i].m_AdditionalVertexStreamMesh == null)
								continue;

							CreateComposite(m_VertexStreams[i], false, string.Format("{0}/{1}.asset", path, m_VertexStreams[i].m_AdditionalVertexStreamMesh.name));
						}

						m_VertexStreams.Clear();
					}

				GUILayout.EndVertical();

			GUILayout.EndHorizontal();
		}

		void OnSelectionChange()
		{
			m_VertexStreams = Selection.transforms.SelectMany(x => x.GetComponentsInChildren<z_AdditionalVertexStreams>()).ToList();
			Repaint();
		}

		void UndoRedoPerformed()
		{
			foreach(Mesh m in Selection.transforms.SelectMany(x => x.GetComponentsInChildren<MeshFilter>()).Select(y => y.sharedMesh))
			{
				if(m != null)
					m.UploadMeshData(false);
			}
		}

		void CreateComposite(z_AdditionalVertexStreams vertexStream, bool applyToCurrent, string path = null)
		{
			GameObject go = vertexStream.gameObject;

			Mesh source = go.GetMesh();
			Mesh stream = vertexStream.m_AdditionalVertexStreamMesh;

			if(source == null || stream == null)
			{
				Debug.LogWarning("Mesh filter or vertex stream mesh is null, cannot continue.");
				return;
			}

			if(applyToCurrent)
			{
				CreateCompositeMesh(source, stream, source);

				MeshRenderer renderer = go.GetComponent<MeshRenderer>();

				if(renderer != null)
					renderer.additionalVertexStreams = null;

				GameObject.DestroyImmediate(vertexStream);
			}
			else
			{
				Mesh composite = new Mesh();
				CreateCompositeMesh(source, stream, composite);

				if( string.IsNullOrEmpty(path) )
				{
					z_EditorUtility.SaveMeshAsset(composite, go.GetComponent<MeshFilter>(), go.GetComponent<SkinnedMeshRenderer>());
				}
				else
				{
					AssetDatabase.CreateAsset(composite, path);

					MeshFilter mf = go.GetComponent<MeshFilter>();
					
					SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();

					if(mf != null)
						mf.sharedMesh = composite;
					else if(smr != null)
						smr.sharedMesh = composite;
				}


				Undo.DestroyObjectImmediate(vertexStream);
			}
		}

		void CreateCompositeMesh(Mesh source, Mesh vertexStream, Mesh composite)
		{
			int vertexCount = source.vertexCount;
			bool isNewMesh = composite.vertexCount != vertexCount;

			composite.name = source.name;

			composite.vertices = vertexStream.vertices != null && vertexStream.vertices.Length == vertexCount ?
				vertexStream.vertices :
				source.vertices;

			composite.normals = vertexStream.normals != null  && vertexStream.normals.Length == vertexCount ?
				vertexStream.normals :
				source.normals;

			composite.tangents = vertexStream.tangents != null && vertexStream.tangents.Length == vertexCount ?
				vertexStream.tangents :
				source.tangents;

			composite.boneWeights = vertexStream.boneWeights != null && vertexStream.boneWeights.Length == vertexCount ?
				vertexStream.boneWeights :
				source.boneWeights;

			composite.colors32 = vertexStream.colors32 != null && vertexStream.colors32.Length == vertexCount ?
				vertexStream.colors32 :
				source.colors32;

			composite.bindposes = vertexStream.bindposes != null && vertexStream.bindposes.Length == vertexCount ?
				vertexStream.bindposes :
				source.bindposes;
	
			List<Vector4> uvs = new List<Vector4>();

			vertexStream.GetUVs(0, uvs);
			if(uvs == null || uvs.Count != vertexCount)
				source.GetUVs(0, uvs);
			composite.SetUVs(0, uvs);

			vertexStream.GetUVs(1, uvs);
			if(uvs == null || uvs.Count != vertexCount)
				source.GetUVs(1, uvs);
			composite.SetUVs(1, uvs);

			vertexStream.GetUVs(2, uvs);
			if(uvs == null || uvs.Count != vertexCount)
				source.GetUVs(2, uvs);
			composite.SetUVs(2, uvs);

			vertexStream.GetUVs(3, uvs);
			if(uvs == null || uvs.Count != vertexCount)
				source.GetUVs(3, uvs);
			composite.SetUVs(3, uvs);

			if(isNewMesh)
			{
				composite.subMeshCount = source.subMeshCount;

				for(int i = 0; i < source.subMeshCount; i++)
					composite.SetIndices(source.GetIndices(i), source.GetTopology(i), i);
			}
		}
	}
}
