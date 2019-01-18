#define DO_RENDER_OVERLAY_MESH
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Polybrush
{
	/**
	 *	An editor-only script that renders a mesh and material list in the scene view only.
	 */
	[ExecuteInEditMode]
	public class z_OverlayRenderer : z_ZoomOverride
	{
		// HideFlags.DontSaveInEditor isn't exposed for whatever reason, so do the bit math on ints
		// and just cast to HideFlags.
		// HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable
		HideFlags SceneCameraHideFlags = (HideFlags) (1 | 4 | 8);

		[SerializeField] private Material _overlayMaterial;
		[SerializeField] private Material _billboardMaterial;

		public Color fullColor = Color.green;
		public Gradient gradient = new Gradient();

		List<List<int>> common = null;
		Color[] w_colors;
		Color[] v_colors;

		bool drawVertexBillboards = true;

		protected override void OnEnable()
		{
			base.OnEnable();
			drawVertexBillboards = EditorPrefs.GetFloat("z_pref_vertexBillboardSize", 2f) > 0f;
		}

		private Material OverlayMaterial
		{
			get
			{
				if(_overlayMaterial == null)
				{
					_overlayMaterial = new Material(Shader.Find("Hidden/Polybrush/Overlay"));
					_overlayMaterial.hideFlags = HideFlags.HideAndDontSave;
				}
				return _overlayMaterial;
			}
		}

		public Material VertexBillboardMaterial
		{
			get
			{
				if(_billboardMaterial == null)
				{
					_billboardMaterial = new Material(Shader.Find("Hidden/Polybrush/z_VertexBillboard"));
					// would be nice to not have magic strings here
					_billboardMaterial.SetFloat("_Scale", EditorPrefs.GetFloat("z_pref_vertexBillboardSize", 2f));
					_billboardMaterial.hideFlags = HideFlags.HideAndDontSave;
				}
				return _billboardMaterial;
			}
		}

		private void OnDestroy()
		{
			if(wireframeMesh != null) GameObject.DestroyImmediate(wireframeMesh);
			if(vertexMesh != null) GameObject.DestroyImmediate(vertexMesh);
			if(_overlayMaterial != null) GameObject.DestroyImmediate(_overlayMaterial);
			if(_billboardMaterial != null) GameObject.DestroyImmediate(_billboardMaterial);
		}

		private Mesh 	wireframeMesh,
						vertexMesh;

		public void SetMesh(z_Mesh m)
		{
#if DO_RENDER_OVERLAY_MESH
			wireframeMesh = z_MeshUtility.CreateOverlayMesh(m);

			wireframeMesh.hideFlags = HideFlags.HideAndDontSave;

			w_colors = new Color[wireframeMesh.vertexCount];

			if(drawVertexBillboards)
			{
				common = z_MeshUtility.GetCommonVertices(m);
				vertexMesh 				= z_MeshUtility.CreateVertexBillboardMesh(m, common);
				vertexMesh.hideFlags 	= HideFlags.HideAndDontSave;
				v_colors = new Color[vertexMesh.vertexCount];
			}
#endif
		}

		public override void OnVerticesMoved(z_Mesh mesh)
		{
#if DO_RENDER_OVERLAY_MESH
			Vector3[] v = mesh.vertices;

			if( wireframeMesh != null )
				wireframeMesh.vertices = v;

			if(drawVertexBillboards)
			{
				int len = System.Math.Min(ushort.MaxValue / 4, common.Count);

				if( vertexMesh != null )
				{
					Vector3[] v2 = new Vector3[ len * 4 ];

					for(int i = 0; i < len; i++)
					{
						int ind = common[i][0];

						v2[i * 4 + 0] = v[ind];
						v2[i * 4 + 1] = v[ind];
						v2[i * 4 + 2] = v[ind];
						v2[i * 4 + 3] = v[ind];
					}

					vertexMesh.vertices = v2;
				}
			}
#endif
		}

		/**
		 *	Set the vertex colors to match the brush weights.
		 */
		public override void SetWeights(float[] weights, float normalizedStrength)
		{
#if DO_RENDER_OVERLAY_MESH
			this.weights = weights;
			int vertexCount = w_colors.Length;

			const float MIN_ALPHA = .5f;
			const float MAX_ALPHA = 1f;
			const float ALPHA_RANGE = MAX_ALPHA - MIN_ALPHA;

			if(weights == null || weights.Length < vertexCount)
				return;

			for(int ind = 0; ind < vertexCount; ind++)
			{
				if(weights[ind] < 0.0001f)
				{
					w_colors[ind].a = 0f;
					continue;
				}

				float strength = 1f - weights[ind];

				if(strength < .001f)
				{
					w_colors[ind] = fullColor;
				}
				else
				{
					w_colors[ind] = gradient.Evaluate(strength);
					w_colors[ind].a *= MIN_ALPHA + (ALPHA_RANGE * (1f-strength));
				}
			}

			wireframeMesh.colors = w_colors;
			wireframeMesh.UploadMeshData(false);

			if(drawVertexBillboards)
			{
				for(int i = 0; i < vertexMesh.vertexCount; i+=4)
				{
					int ind = i / 4;
					v_colors[i+0] = w_colors[common[ind][0]];
					v_colors[i+1] = w_colors[common[ind][0]];
					v_colors[i+2] = w_colors[common[ind][0]];
					v_colors[i+3] = w_colors[common[ind][0]];
				}

				vertexMesh.colors = v_colors;
				vertexMesh.UploadMeshData(false);
			}
#endif
		}

		void OnRenderObject()
		{
#if DO_RENDER_OVERLAY_MESH
			// instead of relying on 'SceneCamera' string comparison, check if the hideflags match.
			// this could probably even just check for one bit match, since chances are that any
			// game view camera isn't going to have hideflags set.
			if((Camera.current.gameObject.hideFlags & SceneCameraHideFlags) != SceneCameraHideFlags || Camera.current.name != "SceneCamera" )
				return;

			if(wireframeMesh != null)
			{
				OverlayMaterial.SetFloat("_Alpha", .3f);
				OverlayMaterial.SetPass(0);
				Graphics.DrawMeshNow(wireframeMesh, transform.localToWorldMatrix);
			}

			if(vertexMesh != null)
			{
				VertexBillboardMaterial.SetPass(0);
				Graphics.DrawMeshNow(vertexMesh, transform.localToWorldMatrix);
			}
#endif
		}
	}
}

#endif
