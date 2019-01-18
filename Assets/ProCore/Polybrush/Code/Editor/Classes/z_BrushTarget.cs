using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Polybrush
{
	/**
	 *	Stores information about the object a brush is currently hovering.
	 */
	public class z_BrushTarget : z_IValid
	{
		// List of hit locations on this target mesh.
		public List<z_RaycastHit> raycastHits = new List<z_RaycastHit>();

		private float[] _weights = null;

		// The GameObject the brush is currently hovering.
		[SerializeField] z_EditableObject _editableObject = null;

		// Getter for editableObject target
		public z_EditableObject editableObject { get { return _editableObject; } }

		// Convenience getter for editableObject.gameObject
		public GameObject gameObject { get { return editableObject == null ? null : editableObject.gameObject; } }

		// Convenience getter for editableObject.gameObject.transform
		public Transform transform { get { return editableObject == null ? null : editableObject.gameObject.transform; } }

		// Convenience getter for gameObject.transform.localToWorldMatrix
		public Matrix4x4 localToWorldMatrix { get { return editableObject == null ? Matrix4x4.identity : editableObject.gameObject.transform.localToWorldMatrix; } }

		// Convenience getter for editableObject.editMesh.vertexCount
		public int vertexCount { get { return _editableObject.editMesh.vertexCount; } }

		/**
		 * Constructor.
		 */
		public z_BrushTarget(z_EditableObject editableObject) : this(editableObject, new List<z_RaycastHit>()) {}

		/**
		 *	Explicit constructor.
		 */
		public z_BrushTarget(z_EditableObject editableObject, List<z_RaycastHit> hits)
		{
			this.raycastHits = hits;
			this._editableObject = editableObject;
			this._weights = new float[this._editableObject.editMesh.vertexCount];
		}

		~z_BrushTarget()
		{}

		public void ClearRaycasts()
		{
			foreach(z_RaycastHit hit in raycastHits)
				hit.ReleaseWeights();

			raycastHits.Clear();
		}

		/**
		 *	Returns an array of weights where each index is the max of all raycast hits.
		 */
		public float[] GetAllWeights(bool rebuildCache = false)
		{
			z_Mesh mesh = editableObject.editMesh;
			int vertexCount = mesh.vertexCount;

			if(mesh == null)
				return null;

			if(!rebuildCache)
				return _weights;

			for(int i = 0; i < vertexCount; i++)
				_weights[i] = 0f;

			for(int i = 0; i < raycastHits.Count; i++)
			{
				if(raycastHits[i].weights != null)
				{
					float[] w = raycastHits[i].weights;

					for(int n = 0; n < vertexCount; n++)
						if(w[n] > _weights[n])
							_weights[n] = w[n];
				}
			}

			return _weights;
		}

		public bool IsValid { get { return editableObject.IsValid(); } }

		public override string ToString()
		{
			return string.Format("valid: {0}\nvertices: {1}", IsValid, IsValid ? editableObject.vertexCount : 0);
		}
	}
}
