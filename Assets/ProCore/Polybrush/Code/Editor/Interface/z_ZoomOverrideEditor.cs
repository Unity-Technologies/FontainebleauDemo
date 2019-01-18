using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Polybrush
{
	[CustomEditor(typeof(z_ZoomOverride), true)]
	public class z_ZoomOverrideEditor : Editor
	{
		void OnEnable()
		{
			if(z_Editor.instance == null)
				GameObject.DestroyImmediate(this.target);
		}

		public override void OnInspectorGUI() {}

		bool HasFrameBounds()
		{
			z_ZoomOverride ren = (z_ZoomOverride) target;
			return 	ren.mesh != null && ren.GetWeights().Length == ren.mesh.vertexCount;
		}

		Bounds OnGetFrameBounds()
		{
			z_ZoomOverride ren = (z_ZoomOverride) target;

			Mesh m = ren.mesh;

			Vector3[] vertices = m.vertices;
			float[] weights = ren.GetWeights();

			Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
			int appliedWeights = 0;

			Transform transform = ((z_ZoomOverride)target).transform;

			for(int i = 0; i < m.vertexCount; i++)
			{
				if(weights[i] > 0.0001f)
				{
					if(appliedWeights > 0)
						bounds.Encapsulate( transform.TransformPoint(vertices[i]));
					else
						bounds.center = transform.TransformPoint(vertices[i]);

					appliedWeights++;
				}
			}

			if(appliedWeights < 1)
				bounds = ren.transform.GetComponent<MeshRenderer>().bounds;
			else if(appliedWeights == 1 || bounds.size.magnitude < .1f)
				bounds.size = Vector3.one * .5f;

			return bounds;
		}
	}
}
