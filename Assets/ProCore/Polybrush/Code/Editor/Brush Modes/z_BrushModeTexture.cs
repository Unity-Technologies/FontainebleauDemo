// #define POLYBRUSH_DEBUG

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Polybrush
{
	/**
	 *	Vertex texture painter brush mode.
	 * 	Similar to z_BrushModePaint, except it packs blend information into both the color32 and UV3/4 channels.
	 */
	public class z_BrushModeTexture : z_BrushModeMesh
	{
		// how many applications it should take to reach the full strength
		const float STRENGTH_MODIFIER = 1f/8f;

		z_PaintMode paintMode = z_PaintMode.Brush;
		bool likelySupportsTextureBlending = true;

		[System.NonSerialized]
		z_SplatSet 	splat_cache = null,
					splat_target = null,
					splat_erase = null,
					splat_current = null;

		[System.NonSerialized] z_SplatWeight brushColor = null;
		[System.NonSerialized] z_SplatWeight minWeights = null;
		[SerializeField] int selectedAttributeIndex = -1;

		private z_AttributeLayoutContainer meshAttributesContainer = null;

		private z_AttributeLayout[] meshAttributes
		{
			get
			{
				return meshAttributesContainer != null ? meshAttributesContainer.attributes : null;
			}
		}

		// temp vars
		private z_Edge[] _fillModeEdges = new z_Edge[3];
		private List<int> _fillModeAdjacentTris = null;

		[SerializeField] int vertexCount = 0;
		Dictionary<z_Edge, List<int>> triangleLookup = null;

		public GUIContent[] modeIcons = new GUIContent[]
		{
			new GUIContent("Brush", "Brush" ),
			new GUIContent("Fill", "Fill" ),
			new GUIContent("Flood", "Flood" )
		};

		// The message that will accompany Undo commands for this brush.  Undo/Redo is handled by z_Editor.
		public override string UndoMessage { get { return "Paint Brush"; } }
		protected override string ModeSettingsHeader { get { return "Texture Paint Settings"; } }
		protected override string DocsLink { get { return "http://procore3d.github.io/polybrush/modes/texture/"; } }

		public override void OnEnable()
		{
			base.OnEnable();

			//modeIcons[0].image = z_IconUtility.GetIcon("Icon/Brush");
			//modeIcons[1].image = z_IconUtility.GetIcon("Icon/Roller");
			//modeIcons[2].image = z_IconUtility.GetIcon("Icon/Flood");

			likelySupportsTextureBlending = false;
			meshAttributesContainer = null;
			brushColor = null;

			foreach(GameObject go in Selection.gameObjects)
			{
				likelySupportsTextureBlending = CheckForTextureBlendSupport(go);

				if(likelySupportsTextureBlending)
					break;
			}
		}

		// Inspector GUI shown in the Editor window.  Base class shows z_BrushSettings by default
		public override void DrawGUI(z_BrushSettings brushSettings)
		{
			base.DrawGUI(brushSettings);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			paintMode = (z_PaintMode) GUILayout.Toolbar( (int) paintMode, modeIcons, "Command", GUILayout.Width(120));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(4);

			if(!likelySupportsTextureBlending)
			{
				EditorGUILayout.HelpBox("It doesn't look like any of the materials on this object support texture blending!\n\nSee the readme for information on creating custom texture blend shaders.", MessageType.Warning);
			}

 			if(meshAttributes != null)
			{
				int prevSelectedAttributeIndex = selectedAttributeIndex;
				selectedAttributeIndex = z_SplatWeightEditor.OnInspectorGUI(selectedAttributeIndex, ref brushColor, meshAttributes);
				if(prevSelectedAttributeIndex != selectedAttributeIndex)
					SetBrushColorWithAttributeIndex(selectedAttributeIndex);

#if POLYBRUSH_DEBUG
				GUILayout.BeginHorizontal();

				GUILayout.FlexibleSpace();

				if(GUILayout.Button("MetaData", EditorStyles.miniButton))
				{
					Debug.Log(meshAttributes.ToString("\n"));

					string str = z_EditorUtility.FindPolybrushMetaDataForShader(meshAttributesContainer.shader);

					if(!string.IsNullOrEmpty(str))
					{
						TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(str);

						if(asset != null)
							EditorGUIUtility.PingObject(asset);
						else
							Debug.LogWarning("No MetaData found for Shader \"" + meshAttributesContainer.shader.name + "\"");
					}
					else
					{
						Debug.LogWarning("No MetaData found for Shader \"" + meshAttributesContainer.shader.name + "\"");
					}
				}

				GUILayout.EndHorizontal();

				GUILayout.Space(4);

				if(GUILayout.Button("rebuild  targets"))
					RebuildColorTargets(brushColor, brushSettings.strength);


				GUILayout.Label(brushColor != null ? brushColor.ToString() : "brush color: null\n");
#endif
			}
		}

		public override void OnBrushSettingsChanged(z_BrushTarget target, z_BrushSettings settings)
		{
			base.OnBrushSettingsChanged(target, settings);
			RebuildColorTargets(brushColor, settings.strength);
		}

		/**
		 *	Test a gameObject and it's mesh renderers for compatible shaders, and if one is found
		 *	load it's attribute data into meshAttributes.
		 */
		private bool CheckForTextureBlendSupport(GameObject go)
		{
			z_AttributeLayoutContainer detectedMeshAttributes;

			foreach(Material mat in z_Util.GetMaterials(go))
			{
				if(z_ShaderUtil.GetMeshAttributes(mat, out detectedMeshAttributes))
				{
					meshAttributesContainer = detectedMeshAttributes;
					return true;
				}
			}

			return false;
		}

		private void SetBrushColorWithAttributeIndex(int index)
		{
			if(	brushColor == null ||
				meshAttributes == null ||
				index < 0 ||
				index >= meshAttributes.Length)
				return;

			selectedAttributeIndex = index;

			if(meshAttributes[index].mask > -1)
			{
				for(int i = 0; i < meshAttributes.Length; i++)
				{
					if(meshAttributes[i].mask == meshAttributes[index].mask)
						brushColor.SetAttributeValue(meshAttributes[i], meshAttributes[i].min);
				}
			}

			brushColor.SetAttributeValue(meshAttributes[index], meshAttributes[index].max);
		}

		private void RebuildColorTargets(z_SplatWeight blend, float strength)
		{
			if(blend == null || splat_cache == null || splat_target == null)
				return;

			minWeights = splat_target.GetMinWeights();

			splat_target.LerpWeights(splat_cache, blend, strength);
			splat_erase.LerpWeights(splat_cache, minWeights, strength);
		}

		private void RebuildCaches(z_Mesh m, float strength)
		{
			vertexCount = m.vertexCount;
			triangleLookup = z_MeshUtility.GetAdjacentTriangles(m);

			if(meshAttributes == null)
			{
				// clear caches
				splat_cache = null;
				splat_current = null;
				splat_target = null;
				splat_erase = null;
				return;
			}

			splat_cache 	= new z_SplatSet(m, meshAttributes);
			splat_current 	= new z_SplatSet(splat_cache);
			splat_target 	= new z_SplatSet(vertexCount, meshAttributes);
			splat_erase 	= new z_SplatSet(vertexCount, meshAttributes);
		}

		// Called when the mouse begins hovering an editable object.
		public override void OnBrushEnter(z_EditableObject target, z_BrushSettings settings)
		{
			base.OnBrushEnter(target, settings);

			if(target.editMesh == null)
				return;

			likelySupportsTextureBlending = CheckForTextureBlendSupport(target.gameObject);

			if(likelySupportsTextureBlending && (brushColor == null || !brushColor.MatchesAttributes(meshAttributes)))
			{
				brushColor = new z_SplatWeight( z_SplatWeight.GetChannelMap(meshAttributes) );
				SetBrushColorWithAttributeIndex( z_Math.Clamp(selectedAttributeIndex, 0, meshAttributes.Length - 1) );
			}

			RebuildCaches(target.editMesh, settings.strength);
			RebuildColorTargets(brushColor, settings.strength);
		}

		// Called whenever the brush is moved.  Note that @target may have a null editableObject.
		public override void OnBrushMove(z_BrushTarget target, z_BrushSettings settings)
		{
			base.OnBrushMove(target, settings);

			if(!z_Util.IsValid(target) || !likelySupportsTextureBlending)
				return;

			bool shift = Event.current.shift;
			float[] weights;

			if(paintMode == z_PaintMode.Brush)
			{
				weights = target.GetAllWeights();
			}
			else if(paintMode == z_PaintMode.Flood)
			{
				weights = new float[vertexCount];

				for(int i = 0; i < vertexCount; i++)
					weights[i] = 1f;
			}
			else
			{
				weights = new float[vertexCount];
				int[] indices = target.editableObject.editMesh.GetTriangles();
				int index = 0;
				float weightTarget = shift ? 0f : 1f;

				foreach(z_RaycastHit hit in target.raycastHits)
				{
					if(hit.triangle > -1)
					{
						index = hit.triangle * 3;

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

									weights[indices[index  ]] = weightTarget;
									weights[indices[index+1]] = weightTarget;
									weights[indices[index+2]] = weightTarget;
								}
							}
						}
					}
				}
			}

			if(selectedAttributeIndex < 0 || selectedAttributeIndex >= meshAttributes.Length)
				SetBrushColorWithAttributeIndex(0);

			int mask = meshAttributes[selectedAttributeIndex].mask;

			splat_current.LerpWeights(splat_cache, shift ? splat_erase : splat_target, mask, weights);
			splat_current.Apply(target.editableObject.editMesh);
			target.editableObject.ApplyMeshAttributes();
		}

		// Called when the mouse exits hovering an editable object.
		public override void OnBrushExit(z_EditableObject target)
		{
			base.OnBrushExit(target);

			if(splat_cache != null)
			{
				splat_cache.Apply(target.editMesh);
				target.ApplyMeshAttributes();
				target.graphicsMesh.UploadMeshData(false);
			}

			likelySupportsTextureBlending = true;
		}

		// Called every time the brush should apply itself to a valid target.  Default is on mouse move.
		public override void OnBrushApply(z_BrushTarget target, z_BrushSettings settings)
		{
			if(!likelySupportsTextureBlending)
				return;

			splat_current.CopyTo(splat_cache);
			splat_cache.Apply(target.editableObject.editMesh);
			base.OnBrushApply(target, settings);
		}

		// set mesh splat_current back to their original state before registering for undo
		public override void RegisterUndo(z_BrushTarget brushTarget)
		{
			if(splat_cache != null)
			{
				splat_cache.Apply(brushTarget.editableObject.editMesh);
				brushTarget.editableObject.ApplyMeshAttributes();
			}

			base.RegisterUndo(brushTarget);
		}

		public override void DrawGizmos(z_BrushTarget target, z_BrushSettings settings)
		{
			z_Mesh mesh = target.editableObject.editMesh;

			if(z_Util.IsValid(target) && paintMode == z_PaintMode.Fill)
			{
				Vector3[] vertices = mesh.vertices;
				int[] indices = mesh.GetTriangles();

				z_Handles.PushMatrix();
				z_Handles.PushHandleColor();

				Handles.matrix = target.transform.localToWorldMatrix;

				int index = 0;

				foreach(z_RaycastHit hit in target.raycastHits)
				{
					if(hit.triangle > -1)
					{
						Handles.color = Color.green;

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
