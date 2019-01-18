using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Polybrush
{
	/**
	 *	Brush mode for moving vertices in a direction.
	 */
	public class z_BrushModeSmooth : z_BrushModeSculpt
	{
		const float SMOOTH_STRENGTH_MODIFIER = .1f;

		protected override string brushDirectionPref { get { return z_Pref.smoothDirection; } }
		protected override string brushNormalIsStickyPref { get { return "smooth_brush_sticky"; } }
		protected override string ignoreNonManifoldIndicesPref { get { return "smooth_ignoreNonManifoldIndices"; } }

		Vector3[] vertices = null;
		Dictionary<int, List<int>> neighborLookup = new Dictionary<int, List<int>>();
		List<List<int>> commonVertices = null;
		int commonVertexCount;

		public override string UndoMessage { get { return "Smooth Vertices"; } }
		protected override string ModeSettingsHeader { get { return "Smooth Settings"; } }
		protected override string DocsLink { get { return "http://procore3d.github.io/polybrush/modes/smooth/"; } }

		public override void OnEnable()
		{
			base.OnEnable();
		}

		public override void DrawGUI(z_BrushSettings settings)
		{
			base.DrawGUI(settings);
		}

		public override void OnBrushEnter(z_EditableObject target, z_BrushSettings settings)
		{
			base.OnBrushEnter(target, settings);

			vertices = target.editMesh.vertices;
			neighborLookup = z_MeshUtility.GetAdjacentVertices(target.editMesh);
			commonVertices = z_MeshUtility.GetCommonVertices(target.editMesh);
			commonVertexCount = commonVertices.Count;
		}

		public override void OnBrushApply(z_BrushTarget target, z_BrushSettings settings)
		{
			int rayCount = target.raycastHits.Count;

			Vector3[] normals = (direction == z_Direction.BrushNormal) ? target.editableObject.editMesh.normals : null;

			Vector3 v, t, avg, dirVec = direction.ToVector3();
			Plane plane = new Plane(Vector3.up, Vector3.zero);
			z_Mesh mesh = target.editableObject.editMesh;
			int vertexCount = mesh.vertexCount;

			// don't use target.GetAllWeights because brush normal needs
			// to know which ray to use for normal
			for(int ri = 0; ri < rayCount; ri++)
			{
				z_RaycastHit hit = target.raycastHits[ri];

				if(hit.weights == null || hit.weights.Length < vertexCount)
					continue;

				for(int i = 0; i < commonVertexCount; i++)
				{
					int index = commonVertices[i][0];

					if(hit.weights[index] < .0001f || (ignoreNonManifoldIndices && nonManifoldIndices.Contains(index)))
						continue;

					v = vertices[index];

					if(direction == z_Direction.VertexNormal)
					{
						avg = z_Math.Average(vertices, neighborLookup[index]);
					}
					else
					{
						avg = z_Math.WeightedAverage(vertices, neighborLookup[index], hit.weights);

						if(direction == z_Direction.BrushNormal)
						{
							if(brushNormalIsSticky)
								dirVec = brushNormalOnBeginApply[ri];
							else
								dirVec = z_Math.WeightedAverage(normals, neighborLookup[index], hit.weights).normalized;
						}

						plane.SetNormalAndPosition(dirVec, avg);
						avg = v - dirVec * plane.GetDistanceToPoint(v);
					}

					t = Vector3.Lerp(v, avg, hit.weights[index]);
					List<int> indices = commonVertices[i];

					Vector3 pos = v + (t-v) * settings.strength * SMOOTH_STRENGTH_MODIFIER;

					for(int n = 0; n < indices.Count; n++)
						vertices[indices[n]] = pos;
				}
			}

			mesh.vertices = vertices;

			if(tempComponent != null)
				tempComponent.OnVerticesMoved(mesh);

			base.OnBrushApply(target, settings);
		}
	}
}
