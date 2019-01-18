#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;

namespace Polybrush
{
	/**
	 *	Overrides the default scene zoom with the current values.
	 */
	public class z_ZoomOverride : MonoBehaviour
	{
		// The current weights applied to this mesh
		protected float[] weights;

		// Normalized brush strength
		protected float normalizedStrength;

		public virtual void SetWeights(float[] weights, float normalizedStrength)
		{
			this.weights = weights;
			this.normalizedStrength = normalizedStrength;
		}

		public virtual float[] GetWeights()
		{
			return weights;
		}

		private MeshFilter _meshFilter;
		private SkinnedMeshRenderer _skinnedMeshRenderer;
		private z_AdditionalVertexStreams _additionalVertexStreams;

		public Mesh mesh
		{
			get
			{
				if(_additionalVertexStreams != null && _additionalVertexStreams.m_AdditionalVertexStreamMesh != null)
					return _additionalVertexStreams.m_AdditionalVertexStreamMesh;

				if(_meshFilter == null)
					_meshFilter = gameObject.GetComponent<MeshFilter>();

				if(_meshFilter != null && _meshFilter.sharedMesh != null)
					return _meshFilter.sharedMesh;

				if(_skinnedMeshRenderer == null)
					_skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

				if(_skinnedMeshRenderer != null && _skinnedMeshRenderer.sharedMesh != null)
					return _skinnedMeshRenderer.sharedMesh;
				else
					return null;
			}
		}

		/**
		 *	Let the temp mesh know that vertex positions have changed.
		 */
		public virtual void OnVerticesMoved(z_Mesh mesh) {}

		protected virtual void OnEnable()
		{
			this.hideFlags = HideFlags.HideAndDontSave;

			Component[] other = GetComponents<z_ZoomOverride>();

			foreach(Component c in other)
				if(c != this)
					GameObject.DestroyImmediate(c);

			_additionalVertexStreams = gameObject.GetComponent<z_AdditionalVertexStreams>();
		}
	}
}
#endif
