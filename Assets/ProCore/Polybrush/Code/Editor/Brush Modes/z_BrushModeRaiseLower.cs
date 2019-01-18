using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Polybrush
{
	/**
	 *	Brush mode for moving vertices in a direction.
	 */
	public class z_BrushModeRaiseLower : z_BrushModeSculpt
	{
		protected override string brushDirectionPref { get{ return z_Pref.raiseLowerDirection; } }
		protected override string brushNormalIsStickyPref { get{ return "pushpull_brush_sticky"; } }
		protected override string ignoreNonManifoldIndicesPref { get { return "pushpull_ignoreNonManifoldIndices"; } }

		Vector3[] vertices = null;
		Dictionary<int, Vector3> normalLookup = null;
		List<List<int>> commonVertices = null;
		int commonVertexCount;

		[SerializeField] float brushStrength = 1f;

		public override string UndoMessage { get { return "Push / Pull Vertices"; } }
		protected override string DocsLink { get { return "http://procore3d.github.io/polybrush/modes/sculpt/"; } }

		protected override string ModeSettingsHeader { get { return "Push / Pull Settings"; } }

		private GUIContent gc_BrushEffect = new GUIContent("Brush Effect", "Defines the baseline distance that vertices will be moved when a brush is applied at full strength.");

		public override void OnEnable()
		{
			base.OnEnable();
			brushStrength = z_Pref.GetFloat(z_Pref.pushPullEffect);
		}

		public override void DrawGUI(z_BrushSettings settings)
		{
			base.DrawGUI(settings);

			EditorGUI.BeginChangeCheck();
			brushStrength = z_GUILayout.FloatField(gc_BrushEffect, brushStrength);
			if(EditorGUI.EndChangeCheck())
				z_Pref.SetFloat(z_Pref.pushPullEffect, brushStrength);
		}

		public override void OnBrushEnter(z_EditableObject target, z_BrushSettings settings)
		{
			base.OnBrushEnter(target, settings);
			vertices = target.editMesh.vertices;
			normalLookup = z_MeshUtility.GetSmoothNormalLookup(target.editMesh);
			commonVertices = z_MeshUtility.GetCommonVertices(target.editMesh);
			commonVertexCount = commonVertices.Count;
		}

		public override void OnBrushApply(z_BrushTarget target, z_BrushSettings settings)
		{
			int rayCount = target.raycastHits.Count;

			if(rayCount < 1)
				return;

			Vector3 n = direction.ToVector3();

			float scale = 1f / ( Vector3.Scale(target.transform.lossyScale, n).magnitude );
			float sign = Event.current.control ? -1f : 1f;

			float maxMoveDistance = settings.strength * STRENGTH_MODIFIER * sign * brushStrength;
			int vertexCount = target.editableObject.vertexCount;

			z_Mesh mesh = target.editableObject.editMesh;

			for(int ri = 0; ri < rayCount; ri++)
			{
				z_RaycastHit hit = target.raycastHits[ri];

				if(hit.weights == null || hit.weights.Length < vertexCount)
					continue;

				if( direction == z_Direction.BrushNormal )
				{
					if(brushNormalIsSticky)
						n = brushNormalOnBeginApply[ri];
					else
						n = target.raycastHits[ri].normal;

					scale = 1f / ( Vector3.Scale(target.transform.lossyScale, n).magnitude );
				}

				for(int i = 0; i < commonVertexCount; i++)
				{
					int index = commonVertices[i][0];

					if(hit.weights[index] < .0001f || (ignoreNonManifoldIndices && nonManifoldIndices.Contains(index)))
						continue;

					if(direction == z_Direction.VertexNormal)
					{
						n = normalLookup[index];
						scale = 1f / ( Vector3.Scale(target.transform.lossyScale, n).magnitude );
					}

					Vector3 pos = vertices[index] + n * (hit.weights[index] * maxMoveDistance * scale);

					List<int> indices = commonVertices[i];

					for(int it = 0; it < indices.Count; it++)
						vertices[indices[it]] = pos;
				}

			}

			mesh.vertices = vertices;

			// different than setting weights on temp component,
			// which is what z_BrushModeMesh.OnBrushApply does.
			if(tempComponent != null)
				tempComponent.OnVerticesMoved(mesh);

			base.OnBrushApply(target, settings);
		}
	}
}
