using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Polybrush
{
	/**
	 *	Workaround for bug in `MeshRenderer.additionalVertexStreams`.
	 *
	 *	Namely, the mesh is not persistent in the editor and needs to be "refreshed" constantly.
	 *
	 *		- https://issuetracker.unity3d.com/issues/meshrenderer-dot-additionalvertexstreams-collapse-static-meshes
	 *		- https://issuetracker.unity3d.com/issues/api-mesh-cannot-change-vertex-colors-using-meshrender-dot-additionalvertexstreams
	 *		- https://issuetracker.unity3d.com/issues/meshrenderer-dot-additionalvertexstreams-discards-data-if-set-in-awake
	 *		- https://issuetracker.unity3d.com/issues/meshrenderer-dot-additionalvertexstreams-looses-color-fast-in-editor
	 */
	[ExecuteInEditMode]
	public class z_AdditionalVertexStreams : MonoBehaviour
	{
		public Mesh m_AdditionalVertexStreamMesh = null;

		MeshRenderer _meshRenderer;

		MeshRenderer meshRenderer
		{
			get {
				if(_meshRenderer == null)
					_meshRenderer = gameObject.GetComponent<MeshRenderer>();
				return _meshRenderer;
			}
		}

		void Start()
		{
			SetAdditionalVertexStreamsMesh(m_AdditionalVertexStreamMesh);
		}

		public void SetAdditionalVertexStreamsMesh(Mesh mesh)
		{
			this.m_AdditionalVertexStreamMesh = mesh;
			meshRenderer.additionalVertexStreams = mesh;
		}

#if UNITY_EDITOR
		void Update()
		{
			if(meshRenderer == null || m_AdditionalVertexStreamMesh == null || EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			meshRenderer.additionalVertexStreams = m_AdditionalVertexStreamMesh;
		}
#endif
	}
}
