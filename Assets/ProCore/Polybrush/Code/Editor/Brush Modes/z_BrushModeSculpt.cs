using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Polybrush
{
	/**
	 *	Base class for brush modes that move vertices around.  Implements an overlay preview.
	 */
	public abstract class z_BrushModeSculpt : z_BrushModeMesh
	{
		// Modifier to apply on top of strength.  Translates to brush applications per second roughly.
		public const float STRENGTH_MODIFIER = .01f;

		// Allow child classes to store independent preference values.
		protected virtual string brushDirectionPref { get { return z_Pref.sculptDirection; } }

		// Allow child classes to store independent preference values.
		protected virtual string brushNormalIsStickyPref { get { return z_Pref.brushNormalIsSticky; } }

		// Allow child classes to store independent preference values.
		protected virtual string ignoreNonManifoldIndicesPref { get { return "ignoreNonManifoldIndices"; } }

		public Color[] gradient = new Color[3]
		{
			Color.green,
			Color.yellow,
			Color.black
		};

		// What direction to push vertices in.
		public z_Direction direction = z_Direction.Up;
		protected List<Vector3> brushNormalOnBeginApply = new List<Vector3>();

		protected Vector3[] cached_normals;

		public GUIContent gc_direction = new GUIContent("Direction", "How vertices are moved when the brush is applied.  You can explicitly set an axis, or use the vertex normal.");
		public GUIContent gc_ignoreOpenEdges = new GUIContent("Ignore Open Edges", "When on, edges that are not connected on both sides will be ignored by brush strokes.");

		protected bool brushNormalIsSticky = false;
		private GUIContent gc_brushNormalIsSticky = new GUIContent("Brush Normal is Sticky", "If enabled, vertices will be moved only on the direction of the brush normal at the time of first application.");

		public override string UndoMessage { get { return "Sculpt Vertices"; } }
		protected override string ModeSettingsHeader { get { return "Sculpt Settings"; } }

		// If true vertices on the edge of a mesh will not be affected by brush strokes.  It is up to inheriting
		// classes to implement this preference (use `nonManifoldIndices` HashSet to check if a vertex index is
		// non-manifold).
		protected bool ignoreNonManifoldIndices = true;

		protected HashSet<int> nonManifoldIndices = null;

		public override void OnEnable()
		{
			base.OnEnable();
			brushNormalIsSticky = z_Pref.GetBool( brushNormalIsStickyPref );
			ignoreNonManifoldIndices = z_Pref.GetBool( ignoreNonManifoldIndicesPref );
			direction = (z_Direction) z_Pref.GetInt( brushDirectionPref );
		}

		public override void OnBrushEnter(z_EditableObject target, z_BrushSettings settings)
		{
			base.OnBrushEnter(target, settings);
			nonManifoldIndices = z_MeshUtility.GetNonManifoldIndices(target.editMesh);
		}

		public override void DrawGUI(z_BrushSettings settings)
		{
			base.DrawGUI(settings);

			EditorGUI.BeginChangeCheck();
			ignoreNonManifoldIndices = z_GUILayout.Toggle(gc_ignoreOpenEdges, ignoreNonManifoldIndices);
			if(EditorGUI.EndChangeCheck())
				z_Pref.SetBool(ignoreNonManifoldIndicesPref, ignoreNonManifoldIndices);

			if(direction == z_Direction.BrushNormal)
			{
				EditorGUI.BeginChangeCheck();
				brushNormalIsSticky = z_GUILayout.Toggle(gc_brushNormalIsSticky, brushNormalIsSticky);
				if(EditorGUI.EndChangeCheck())
					z_Pref.SetBool(brushNormalIsStickyPref, brushNormalIsSticky);
			}

			EditorGUI.BeginChangeCheck();
			direction = (z_Direction) z_GUILayout.EnumPopup(gc_direction, direction);
			if(EditorGUI.EndChangeCheck())
				z_Pref.SetInt(brushDirectionPref, (int) direction);
		}

		protected void CacheBrushNormals(z_BrushTarget target)
		{
			brushNormalOnBeginApply.Clear();

			for(int i = 0; i < target.raycastHits.Count; i++)
				brushNormalOnBeginApply.Add(target.raycastHits[i].normal);

			z_Mesh mesh = target.editableObject.editMesh;

			cached_normals = new Vector3[mesh.vertexCount];
			
			if(mesh.normals != null && mesh.normals.Length == mesh.vertexCount)
				System.Array.Copy(mesh.normals, 0, cached_normals, 0, mesh.vertexCount);
		}

		public override void OnBrushBeginApply(z_BrushTarget target, z_BrushSettings settings)
		{
			CacheBrushNormals(target);
			base.OnBrushBeginApply(target, settings);
		}

		protected override void CreateTempComponent(z_EditableObject target, z_BrushSettings settings)
		{
			z_OverlayRenderer ren = target.gameObject.AddComponent<z_OverlayRenderer>();
			ren.SetMesh(target.editMesh);

			ren.fullColor = z_Pref.GetColor(z_Pref.brushColor);
			ren.gradient = z_Pref.GetGradient(z_Pref.brushGradient);

			tempComponent = ren;
		}

		protected override void UpdateTempComponent(z_BrushTarget target, z_BrushSettings settings)
		{
			if(tempComponent != null)
				((z_OverlayRenderer)tempComponent).SetWeights(target.GetAllWeights(), settings.strength);
		}
	}
}
