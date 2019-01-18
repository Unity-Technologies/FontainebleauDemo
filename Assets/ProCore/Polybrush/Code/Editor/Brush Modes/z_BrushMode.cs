// #define Z_DEBUG

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Polybrush
{
	/**
	 *	Base class for brush modes.
	 */
	[System.Serializable]
	public abstract class z_BrushMode : ScriptableObject
	{
		// The message that will accompany Undo commands for this brush.  Undo/Redo is handled by z_Editor.
		public virtual string UndoMessage { get { return "Apply Brush"; } }

		// A temporary component attached to the currently editing object.  Use this to (by default) override the
		// scene zoom functionality, or optionally extend (see z_OverlayRenderer).
		[SerializeField] protected z_ZoomOverride tempComponent;

		// The title to be displayed in the settings header.
		protected abstract string ModeSettingsHeader { get; }

		// The link to the documentation page for this mode.
		protected abstract string DocsLink { get; }

		protected Color innerColor, outerColor;

		protected virtual void CreateTempComponent(z_EditableObject target, z_BrushSettings settings)
		{
			if(!z_Util.IsValid(target))
				return;

			tempComponent = target.gameObject.AddComponent<z_ZoomOverride>();
			tempComponent.hideFlags = HideFlags.HideAndDontSave;
			tempComponent.SetWeights(null, 0f);
		}

		protected virtual void UpdateTempComponent(z_BrushTarget target, z_BrushSettings settings)
		{
			if(!z_Util.IsValid(target))
				return;

			tempComponent.SetWeights(target.GetAllWeights(), settings.strength);
		}

		protected virtual void DestroyTempComponent()
		{
			if(tempComponent != null)
				GameObject.DestroyImmediate(tempComponent);
		}

		// Called on instantiation.  Base implementation sets HideFlags.
		public virtual void OnEnable()
		{
			this.hideFlags = HideFlags.HideAndDontSave;

			innerColor = z_Pref.GetColor(z_Pref.brushColor);
			outerColor = z_Pref.GetGradient(z_Pref.brushGradient).Evaluate(1f);

			innerColor.a = .9f;
			outerColor.a = .35f;
		}

		// Called when mode is disabled.
		public virtual void OnDisable()
		{
			DestroyTempComponent();
		}

		// Called by z_Editor when brush settings have been modified.
		public virtual void OnBrushSettingsChanged(z_BrushTarget target, z_BrushSettings settings)
		{
			UpdateTempComponent(target, settings);
		}

		// Inspector GUI shown in the Editor window.
		public virtual void DrawGUI(z_BrushSettings brushSettings)
		{
			if( z_GUILayout.HeaderWithDocsLink( z_GUI.TempContent(ModeSettingsHeader, "")) )
				Application.OpenURL(DocsLink);
		}

		// Called when the mouse begins hovering an editable object.
		public virtual void OnBrushEnter(z_EditableObject target, z_BrushSettings settings)
		{
			if(z_Pref.GetBool(z_Pref.hideWireframe) && target.renderer != null)
			{
				// disable wirefame
				z_EditorUtility.SetSelectionRenderState(target.renderer, z_EditorUtility.GetSelectionRenderState() & z_SelectionRenderState.Outline);
			}

			CreateTempComponent(target, settings);
		}

		// Called whenever the brush is moved.  Note that @target may have a null editableObject.
		public virtual void OnBrushMove(z_BrushTarget target, z_BrushSettings settings)
		{
			UpdateTempComponent(target, settings);
		}

		// Called when the mouse exits hovering an editable object.
		public virtual void OnBrushExit(z_EditableObject target)
		{
			if(target.renderer != null)
				z_EditorUtility.SetSelectionRenderState(target.renderer, z_EditorUtility.GetSelectionRenderState());

			DestroyTempComponent();
		}

		// Called when the mouse begins a drag across a valid target.
		public virtual void OnBrushBeginApply(z_BrushTarget target, z_BrushSettings settings) {}

		// Called every time the brush should apply itself to a valid target.  Default is on mouse move.
		public abstract void OnBrushApply(z_BrushTarget target, z_BrushSettings settings);

		// Called when a brush application has finished.  Use this to clean up temporary resources or apply
		// deferred actions to a mesh (rebuild UV2, tangents, whatever).
		public virtual void OnBrushFinishApply(z_BrushTarget target, z_BrushSettings settings)
		{
			DestroyTempComponent();
		}

		// Draw scene gizmos.  Base implementation draws the brush preview.
		public virtual void DrawGizmos(z_BrushTarget target, z_BrushSettings settings)
		{
			foreach(z_RaycastHit hit in target.raycastHits)
				z_Handles.DrawBrush(hit.position, hit.normal, settings, target.localToWorldMatrix, innerColor, outerColor);

#if Z_DEBUG

#if Z_DRAW_WEIGHTS || DRAW_PER_VERTEX_ATTRIBUTES
			float[] w = target.GetAllWeights();
#endif

#if Z_DRAW_WEIGHTS
			Mesh m = target.mesh;
			Vector3[] v = m.vertices;
			GUIContent content = new GUIContent("","");

			Handles.BeginGUI();
			for(int i = 0; i < v.Length; i++)
			{
				if(w[i] < .0001f)
					continue;

				content.text = w[i].ToString("F2");
				GUI.Label(HandleUtility.WorldPointToSizedRect(target.transform.TransformPoint(v[i]), content, EditorStyles.label), content);
			}
			Handles.EndGUI();
#endif

#if DRAW_PER_VERTEX_ATTRIBUTES

			z_Mesh m = target.editableObject.editMesh;
			Color32[] colors = m.colors;
			Vector4[] tangents = m.tangents;
			List<Vector4> uv0 = m.uv0;
			List<Vector4> uv1 = m.uv1;
			List<Vector4> uv2 = m.uv2;
			List<Vector4> uv3 = m.uv3;

			int vertexCount = m.vertexCount;

			Vector3[] verts = m.vertices;
			GUIContent gc = new GUIContent("");

			List<List<int>> common = z_MeshUtility.GetCommonVertices(m);
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			Handles.BeginGUI();
			foreach(List<int> l in common)
			{
				if( w[l[0]] < .001 )
					continue;

				Vector3 v = target.transform.TransformPoint(verts[l[0]]);

				if(colors != null) sb.AppendLine("color: " + colors[l[0]].ToString("F2"));
				if(tangents != null) sb.AppendLine("tangent: " + tangents[l[0]].ToString("F2"));
				if(uv0 != null && uv0.Count == vertexCount) sb.AppendLine("uv0: " + uv0[l[0]].ToString("F2"));
				if(uv1 != null && uv1.Count == vertexCount) sb.AppendLine("uv1: " + uv1[l[0]].ToString("F2"));
				if(uv2 != null && uv2.Count == vertexCount) sb.AppendLine("uv2: " + uv2[l[0]].ToString("F2"));
				if(uv3 != null && uv3.Count == vertexCount) sb.AppendLine("uv3: " + uv3[l[0]].ToString("F2"));

				gc.text = sb.ToString();
				sb.Remove(0, sb.Length);	// @todo .NET 4.0
				GUI.Label(HandleUtility.WorldPointToSizedRect(v, gc, EditorStyles.label), gc);
			}
			Handles.EndGUI();
#endif

#endif
		}

		public abstract void RegisterUndo(z_BrushTarget brushTarget);

		public virtual void UndoRedoPerformed(List<GameObject> modified)
		{
			DestroyTempComponent();
		}
	}
}
