using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Polybrush
{
	/**
	 *	Vertex painter brush mode.
	 */
	public class z_BrushModePaint : z_BrushModeMesh
	{
		// how many applications it should take to reach the full strength
		const float STRENGTH_MODIFIER = 1f/8f;
		private static readonly Color32 WHITE = new Color32(255, 255, 255, 255);

		[SerializeField] z_PaintMode paintMode = z_PaintMode.Brush;
		[SerializeField] bool likelySupportsVertexColors = false;

		// mesh vertex colors
		[SerializeField] Color32[] colors_cache = null, target_colors = null, erase_colors = null, colors = null;
		[SerializeField] Color32 brushColor = Color.green;

		z_ColorMask mask = new z_ColorMask(true, true, true, true);

		z_ColorPalette[] availablePalettes = null;
		string[] availablePalettes_str = null;
		int currentPaletteIndex = -1;

		// temp vars
		private z_Edge[] _fillModeEdges = new z_Edge[3];
		private List<int> _fillModeAdjacentTris = null;

		// used for fill mode
		Dictionary<z_Edge, List<int>> triangleLookup = null;

		public GUIContent[] modeIcons = new GUIContent[]
		{
			new GUIContent("Brush", "Brush" ),
			new GUIContent("Fill", "Fill" ),
			new GUIContent("Flood", "Flood" )
		};

		// The current color palette.
		[SerializeField] z_ColorPalette _colorPalette = null;

		private z_ColorPalette colorPalette
		{
			get
			{
				if(_colorPalette == null)
					colorPalette = z_EditorUtility.GetDefaultAsset<z_ColorPalette>("Color Palettes/Default.asset");
				return _colorPalette;
			}
			set
			{
				_colorPalette = value;
			}
		}

		// An Editor for the colorPalette.
		[SerializeField] z_ColorPaletteEditor _colorPaletteEditor = null;

		private z_ColorPaletteEditor colorPaletteEditor
		{
			get
			{
				if(_colorPaletteEditor == null || _colorPaletteEditor.target != colorPalette)
				{
					_colorPaletteEditor = (z_ColorPaletteEditor) Editor.CreateEditor(colorPalette);
					_colorPaletteEditor.hideFlags = HideFlags.HideAndDontSave;
				}

				return _colorPaletteEditor;
			}
		}

		// The message that will accompany Undo commands for this brush.  Undo/Redo is handled by z_Editor.
		public override string UndoMessage { get { return "Paint Brush"; } }
		protected override string ModeSettingsHeader { get { return "Paint Settings"; } }
		protected override string DocsLink { get { return "http://procore3d.github.io/polybrush/modes/color/"; } }

		public override void OnEnable()
		{
			base.OnEnable();

			//modeIcons[0].image = z_IconUtility.GetIcon("Icon/Brush");
			//modeIcons[1].image = z_IconUtility.GetIcon("Icon/Roller");
			//modeIcons[2].image = z_IconUtility.GetIcon("Icon/Flood");

			RefreshAvailablePalettes();
		}

		public override void OnDisable()
		{
			base.OnDisable();
			if(_colorPaletteEditor != null)
				Object.DestroyImmediate(_colorPaletteEditor);
		}


		// Inspector GUI shown in the Editor window.  Base class shows z_BrushSettings by default
		public override void DrawGUI(z_BrushSettings brushSettings)
		{
			base.DrawGUI(brushSettings);

			GUILayout.BeginHorizontal();

				if(colorPalette == null)
					RefreshAvailablePalettes();

				EditorGUI.BeginChangeCheck();
				currentPaletteIndex = EditorGUILayout.Popup(currentPaletteIndex, availablePalettes_str, "popup");
				if(EditorGUI.EndChangeCheck())
				{
					if(currentPaletteIndex >= availablePalettes.Length)
						SetColorPalette( z_ColorPaletteEditor.AddNew() );
					else
						SetColorPalette(availablePalettes[currentPaletteIndex]);
				}

				paintMode = (z_PaintMode) GUILayout.Toolbar( (int) paintMode, modeIcons, "Command", GUILayout.Width(120));

			GUILayout.EndHorizontal();

			if(!likelySupportsVertexColors)
				EditorGUILayout.HelpBox("It doesn't look like any of the materials on this object support vertex colors!", MessageType.Warning);

			colorPaletteEditor.onSelectIndex = (color) => { SetBrushColor(color, brushSettings.strength); };
			colorPaletteEditor.onSaveAs = SetColorPalette;

			mask = z_GUILayout.ColorMaskField("Color Mask", mask);

			colorPaletteEditor.OnInspectorGUI();
		}

		private void SetBrushColor(Color color, float strength)
		{
			brushColor = color;
			RebuildColorTargets(color, strength);
		}

		private void RefreshAvailablePalettes()
		{
			if(colorPalette == null)
				colorPalette = z_EditorUtility.GetDefaultAsset<z_ColorPalette>("Color Palettes/Default.asset");

			availablePalettes = Resources.FindObjectsOfTypeAll<z_ColorPalette>().Where(x => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(x))).ToArray();
			availablePalettes_str = availablePalettes.Select(x => x.name).ToArray();
			ArrayUtility.Add<string>(ref availablePalettes_str, string.Empty);
			ArrayUtility.Add<string>(ref availablePalettes_str, "Add Palette...");
			currentPaletteIndex = System.Array.IndexOf(availablePalettes, colorPalette);
		}

		private void SetColorPalette(z_ColorPalette palette)
		{
			colorPalette = palette;
			RefreshAvailablePalettes();
		}

		public override void OnBrushSettingsChanged(z_BrushTarget target, z_BrushSettings settings)
		{
			base.OnBrushSettingsChanged(target, settings);
			RebuildColorTargets(brushColor, settings.strength);
		}

		private void RebuildColorTargets(Color color, float strength)
		{
			if( colors_cache == null ||
				target_colors == null ||
				colors_cache.Length != target_colors.Length)
				return;

			for(int i = 0; i < colors_cache.Length; i++)
			{
				target_colors[i]	= z_Util.Lerp(colors_cache[i], color, mask, strength);
				erase_colors[i]		= z_Util.Lerp(colors_cache[i], WHITE, mask, strength);
			}
		}

		// Called when the mouse begins hovering an editable object.
		public override void OnBrushEnter(z_EditableObject target, z_BrushSettings settings)
		{
			base.OnBrushEnter(target, settings);

			if(target.graphicsMesh == null)
				return;

			RebuildCaches(target, settings);

			triangleLookup = z_MeshUtility.GetAdjacentTriangles(target.editMesh);

			MeshRenderer mr = target.gameObject.GetComponent<MeshRenderer>();

			if(mr != null && mr.sharedMaterials != null)
				likelySupportsVertexColors = mr.sharedMaterials.Any(x => x.shader != null && z_ShaderUtil.SupportsVertexColors(x.shader));
			else
				likelySupportsVertexColors = false;
		}

		private void RebuildCaches(z_EditableObject target, z_BrushSettings settings)
		{
			z_Mesh m = target.editMesh;
			int vertexCount = m.vertexCount;

			if(m.colors != null && m.colors.Length == vertexCount)
				colors_cache = z_Util.Duplicate(m.colors);
			else
				colors_cache = z_Util.Fill<Color32>( x => { return Color.white; }, vertexCount);

			colors 			= new Color32[vertexCount];
			target_colors 	= new Color32[vertexCount];
			erase_colors 	= new Color32[vertexCount];

			RebuildColorTargets(brushColor, settings.strength);
		}

		// Called whenever the brush is moved.  Note that @target may have a null editableObject.
		public override void OnBrushMove(z_BrushTarget target, z_BrushSettings settings)
		{
			base.OnBrushMove(target, settings);

			if(!z_Util.IsValid(target))
				return;

			bool shift = Event.current.shift && Event.current.type != EventType.ScrollWheel;

			z_Mesh mesh = target.editableObject.editMesh;
			int vertexCount = mesh.vertexCount;
			float[] weights = target.GetAllWeights();

			switch(paintMode)
			{
				case z_PaintMode.Flood:
					for(int i = 0; i < vertexCount; i++)
						colors[i] = target_colors[i];
					break;

				case z_PaintMode.Fill:

					System.Array.Copy(colors_cache, colors, vertexCount);
					int[] indices = target.editableObject.editMesh.GetTriangles();
					int index = 0;

					foreach(z_RaycastHit hit in target.raycastHits)
					{
						if(hit.triangle > -1)
						{
							index = hit.triangle * 3;

							colors[indices[index + 0]] = shift ? WHITE : target_colors[indices[index + 0]];
							colors[indices[index + 1]] = shift ? WHITE : target_colors[indices[index + 1]];
							colors[indices[index + 2]] = shift ? WHITE : target_colors[indices[index + 2]];

							_fillModeEdges[0].x = indices[index+0];
							_fillModeEdges[0].y = indices[index+1];

							_fillModeEdges[1].x = indices[index+1];
							_fillModeEdges[1].y = indices[index+2];

							_fillModeEdges[2].x = indices[index+2];
							_fillModeEdges[2].y = indices[index+0];

							for(int i = 0; i < 3; i++)
							{
								if(triangleLookup.TryGetValue(_fillModeEdges[i], out _fillModeAdjacentTris))
								{
									for(int n = 0; n < _fillModeAdjacentTris.Count; n++)
									{
										index = _fillModeAdjacentTris[n] * 3;

										colors[indices[index + 0]] = shift ? WHITE : target_colors[indices[index + 0]];
										colors[indices[index + 1]] = shift ? WHITE : target_colors[indices[index + 1]];
										colors[indices[index + 2]] = shift ? WHITE : target_colors[indices[index + 2]];
									}
								}
							}
						}
					}

					break;

				default:
				{
					for(int i = 0; i < vertexCount; i++)
					{
						colors[i] = z_Util.Lerp(colors_cache[i],
												shift ? erase_colors[i] : target_colors[i],
												mask,
												weights[i]);
					}
					break;
				}
			}

			target.editableObject.editMesh.colors = colors;
			target.editableObject.ApplyMeshAttributes(z_MeshChannel.Color);
		}

		// Called when the mouse exits hovering an editable object.
		public override void OnBrushExit(z_EditableObject target)
		{
			base.OnBrushExit(target);

			if(target.editMesh != null)
			{
				target.editMesh.colors = colors_cache;
				target.ApplyMeshAttributes(z_MeshChannel.Color);
			}

			likelySupportsVertexColors = true;
		}

		// Called every time the brush should apply itself to a valid target.  Default is on mouse move.
		public override void OnBrushApply(z_BrushTarget target, z_BrushSettings settings)
		{
			System.Array.Copy(colors, colors_cache, colors.Length);
			target.editableObject.editMesh.colors = colors_cache;
			base.OnBrushApply(target, settings);
		}

		// set mesh colors back to their original state before registering for undo
		public override void RegisterUndo(z_BrushTarget brushTarget)
		{
			brushTarget.editableObject.editMesh.colors = colors_cache;
			brushTarget.editableObject.ApplyMeshAttributes(z_MeshChannel.Color);

			base.RegisterUndo(brushTarget);
		}

		public override void DrawGizmos(z_BrushTarget target, z_BrushSettings settings)
		{
			if(z_Util.IsValid(target) && paintMode == z_PaintMode.Fill)
			{
				Vector3[] vertices = target.editableObject.editMesh.vertices;
				int[] indices = target.editableObject.editMesh.GetTriangles();

				z_Handles.PushMatrix();
				z_Handles.PushHandleColor();

				Handles.matrix = target.transform.localToWorldMatrix;

				int index = 0;

				foreach(z_RaycastHit hit in target.raycastHits)
				{
					if(hit.triangle > -1)
					{
						Handles.color = target_colors[indices[index]];

						index = hit.triangle * 3;

						Handles.DrawLine(vertices[indices[index+0]] + hit.normal * .1f, vertices[indices[index+1]] + hit.normal * .1f);
						Handles.DrawLine(vertices[indices[index+1]] + hit.normal * .1f, vertices[indices[index+2]] + hit.normal * .1f);
						Handles.DrawLine(vertices[indices[index+2]] + hit.normal * .1f, vertices[indices[index+0]] + hit.normal * .1f);

						_fillModeEdges[0].x = indices[index+0];
						_fillModeEdges[0].y = indices[index+1];

						_fillModeEdges[1].x = indices[index+1];
						_fillModeEdges[1].y = indices[index+2];

						_fillModeEdges[2].x = indices[index+2];
						_fillModeEdges[2].y = indices[index+0];

						for(int i = 0; i < 3; i++)
						{
							if(triangleLookup.TryGetValue(_fillModeEdges[i], out _fillModeAdjacentTris))
							{
								for(int n = 0; n < _fillModeAdjacentTris.Count; n++)
								{
									index = _fillModeAdjacentTris[n] * 3;

									Handles.DrawLine(vertices[indices[index+0]] + hit.normal * .1f, vertices[indices[index+1]] + hit.normal * .1f);
									Handles.DrawLine(vertices[indices[index+1]] + hit.normal * .1f, vertices[indices[index+2]] + hit.normal * .1f);
									Handles.DrawLine(vertices[indices[index+2]] + hit.normal * .1f, vertices[indices[index+0]] + hit.normal * .1f);
								}
							}
						}
					}
				}

				z_Handles.PopHandleColor();
				z_Handles.PopMatrix();
			}
			else
			{
				base.DrawGizmos(target, settings);
			}
		}
	}
}
