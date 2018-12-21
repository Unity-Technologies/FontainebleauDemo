using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

/*  VertexPainterWindow
 *    - Jason Booth
 * 
 *    Uses Unity 5.0+ MeshRenderer.additionalVertexStream so that you can paint per-instance vertex colors on your meshes.
 * A component is added to your mesh to serialize this data and set it at load time. This is more effecient than making
 * duplicate meshes, and certainly less painful than saving them as separate asset files on disk. However, if you only have
 * one copy of the vertex information in your game and want to burn it into the original mesh, you can use the save feature
 * to save a new version of your mesh with the data burned into the verticies, avoiding the need for the runtime component. 
 * 
 * In other words, bake it if you need to instance the paint job - however, if you want tons of the same instances painted
 * uniquely in your scene, keep the component version and skip the baking..
 * 
 * One possible optimization is to have the component free the array after updating the mesh when in play mode..
 * 
 * Also supports burning data into the UV channels, in case you want some additional channels to work with, which also
 * happen to be full 32bit floats. You can set a viewable range; so if your floats go from 0-120, it will remap it to
 * 0-1 for display in the shader. That way you can always see your values, even when they go out of color ranges.
 * 
 * Note that as of this writing Unity has a bug in the additionalVertexStream function. The docs claim the data applied here
 * will supply or overwrite the data in the mesh, however, this is not true. Rather, it will only replace the data that's 
 * there - if your mesh has no color information, it will not upload the color data in additionalVertexStream, which is sad
 * because the original mesh doesn't need this data. As a workaround, if your mesh does not have color channels on the verts,
 * they will be created for you. 
 * 
 * There is another bug in additionalVertexStream, in that the mesh keeps disapearing in edit mode. So the component
 * which holds the data caches the mesh and keeps assigning it in the Update call, but only when running in the editor
 * and not in play mode. 
 * 
 * Really, the additionalVertexStream mesh should be owned by the MeshRenderer and saved as part of the objects instance
 * data. That's essentially what the VertexInstaceStream component does, but it's annoying and wasteful of memory to do
 * it this way since it doesn't need to be on the CPU at all. Enlighten somehow does this with the UVs it generates
 * this way, but appears to be handled specially. Oh, Unity..
*/



namespace JBooth.VertexPainterPro
{
   public partial class VertexPainterWindow : EditorWindow 
   {
      enum Tab
      {
         Paint = 0,
         Deform,
         Flow,
         Utility,
         Custom
      }

      string[] tabNames =
      {
         "Paint",
         "Deform",
         "Flow",
         "Utility",
         "Custom"
      };


      static string sSwatchKey = "VertexPainter_Swatches";

      ColorSwatches swatches = null;

      #if __MEGASPLAT__
      Tab tab = Tab.Custom;
      #else
      Tab tab = Tab.Paint;
      #endif
      bool hideMeshWireframe = false;
      
      bool DrawClearButton(string label)
      {
         if (GUILayout.Button(label, GUILayout.Width(46)))
         {
            return (EditorUtility.DisplayDialog("Confirm", "Clear " + label + " data?", "ok", "cancel"));
         }
         return false;
      }

      static Dictionary<string, bool> rolloutStates = new Dictionary<string, bool>();
      static GUIStyle rolloutStyle;
      public static bool DrawRollup(string text, bool defaultState = true, bool inset = false)
      {
         if (rolloutStyle == null)
         {
            rolloutStyle = GUI.skin.box;
            rolloutStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         }
         GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         if (inset == true)
         {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Width(40));
         }

         if (!rolloutStates.ContainsKey(text))
         {
            rolloutStates[text] = defaultState;
         }
         if (GUILayout.Button(text, rolloutStyle, new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)}))
         {
            rolloutStates[text] = !rolloutStates[text];
         }
         if (inset == true)
         {
            EditorGUILayout.GetControlRect(GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
         }
         return rolloutStates[text];
      }

      Vector2 scroll;
      void OnGUI()
      {
         
         if (Selection.activeGameObject == null)
         {
            EditorGUILayout.LabelField("No objects selected. Please select an object with a MeshFilter and Renderer");
            return;
         }

         if (swatches == null)
         {
            swatches = ColorSwatches.CreateInstance<ColorSwatches>();
            if (EditorPrefs.HasKey(sSwatchKey))
            {
               JsonUtility.FromJsonOverwrite(EditorPrefs.GetString(sSwatchKey), swatches);
            }
            if (swatches == null)
            {
               swatches = ColorSwatches.CreateInstance<ColorSwatches>();
               EditorPrefs.SetString(sSwatchKey, JsonUtility.ToJson(swatches, false));
            }
         }

         DrawChannelGUI();

         var ot = tab;
         tab = (Tab)GUILayout.Toolbar((int)tab, tabNames);
         if (ot != tab)
         {
            UpdateDisplayMode();
         }

         if (tab == Tab.Paint)
         {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawPaintGUI();
         }
         else if (tab == Tab.Deform)
         {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawDeformGUI();
         }
         else if (tab == Tab.Flow)
         {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawFlowGUI();
         }
         else if (tab == Tab.Utility)
         {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawUtilityGUI();
         }
         else if (tab == Tab.Custom)
         {
            DrawCustomGUI();
         }
         EditorGUILayout.EndScrollView();
      }


      void DrawChannelGUI()
      {
         EditorGUILayout.Separator();
         GUI.skin.box.normal.textColor = Color.white;
         if (DrawRollup("Vertex Painter"))
         {
            bool oldEnabled = enabled;
            enabled = GUILayout.Toggle(enabled, "Active (ESC)");
            if (enabled != oldEnabled)
            {
               InitMeshes();
               UpdateDisplayMode();
            }
            var oldShow = showVertexShader;
            EditorGUILayout.BeginHorizontal();
            showVertexShader = GUILayout.Toggle(showVertexShader, "Show Vertex Data (ctrl-V)");
            if (oldShow != showVertexShader)
            {
               UpdateDisplayMode();
            }
            bool emptyStreams = false;
            for (int i = 0; i < jobs.Length; ++i)
            {
               if (!jobs[i].HasStream())
                  emptyStreams = true;
            }
            EditorGUILayout.EndHorizontal();
            if (emptyStreams)
            {
               if (GUILayout.Button("Add Streams"))
               {
                  for (int i = 0; i < jobs.Length; ++i)
                  {
                     jobs[i].EnforceStream();
                  }
                  UpdateDisplayMode();
               }
            }


            brushVisualization = (BrushVisualization)EditorGUILayout.EnumPopup("Brush Visualization", brushVisualization);
            EditorGUILayout.BeginHorizontal();
            showVertexPoints = GUILayout.Toggle(showVertexPoints, "Show Brush Influence");
            showVertexSize = EditorGUILayout.Slider(showVertexSize, 0.2f, 10);
            showVertexColor = EditorGUILayout.ColorField(showVertexColor, GUILayout.Width(40));
            showNormals = GUILayout.Toggle(showNormals, "N");
            showTangents = GUILayout.Toggle(showTangents, "T");
            EditorGUILayout.EndHorizontal();
            bool oldHideMeshWireframe = hideMeshWireframe;
            hideMeshWireframe = !GUILayout.Toggle(!hideMeshWireframe, "Show Wireframe (ctrl-W)");

            if (hideMeshWireframe != oldHideMeshWireframe)
            {
               for (int i = 0; i < jobs.Length; ++i)
               {
                  SetWireframeDisplay(jobs[i].renderer, hideMeshWireframe);
               }
            }
               
            bool hasColors = false;
            bool hasUV0 = false;
            bool hasUV1 = false;
            bool hasUV2 = false;
            bool hasUV3 = false;
            bool hasPositions = false;
            bool hasNormals = false;
            bool hasStream = false;
            for (int i = 0; i < jobs.Length; ++i)
            {
               var stream = jobs[i]._stream;
               if (stream != null)
               {
                  int vertexCount = jobs[i].verts.Length;
                  hasStream = true;
                  hasColors = (stream.colors != null && stream.colors.Length == vertexCount);
                  hasUV0 = (stream.uv0 != null && stream.uv0.Count == vertexCount);
                  hasUV1 = (stream.uv1 != null && stream.uv1.Count == vertexCount);
                  hasUV2 = (stream.uv2 != null && stream.uv2.Count == vertexCount);
                  hasUV3 = (stream.uv3 != null && stream.uv3.Count == vertexCount);
                  hasPositions = (stream.positions != null && stream.positions.Length == vertexCount);
                  hasNormals = (stream.normals != null && stream.normals.Length == vertexCount);
               }
            }

            if (hasStream && (hasColors || hasUV0 || hasUV1 || hasUV2 || hasUV3 || hasPositions || hasNormals))
            {
               EditorGUILayout.BeginHorizontal();
               EditorGUILayout.PrefixLabel("Clear Channel:");
               if (hasColors && DrawClearButton("Colors"))
               {
                  for (int i = 0; i < jobs.Length; ++i)
                  {
                     Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
                     var stream = jobs[i].stream;
                     stream.colors = null;
                     stream.Apply();
                  }
                  Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
               }
               if (hasColors && DrawClearButton("RGB"))
               {
                  for (int i = 0; i < jobs.Length; ++i)
                  {
                     Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
                     var stream = jobs[i].stream;
                     Color[] src = jobs[i].meshFilter.sharedMesh.colors;
                     int count = jobs[i].meshFilter.sharedMesh.colors.Length;
                     for (int j = 0; j < count; ++j)
                     {
                        stream.colors[j].r = src[j].r;
                        stream.colors[j].g = src[j].g;
                        stream.colors[j].b = src[j].b;
                     }
                     stream.Apply();
                  }
                  Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
               }
               if (hasUV0 && DrawClearButton("UV0"))
               {
                  for (int i = 0; i < jobs.Length; ++i)
                  {
                     Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
                     var stream = jobs[i].stream;
                     stream.uv0 = null;
                     stream.Apply();
                  }
                  Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
               }
               if (hasUV1 && DrawClearButton("UV1"))
               {
                  for (int i = 0; i < jobs.Length; ++i)
                  {
                     Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
                     var stream = jobs[i].stream;
                     stream.uv1 = null;
                     stream.Apply();
                  }
                  Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
               }
               if (hasUV2 && DrawClearButton("UV2"))
               {
                  for (int i = 0; i < jobs.Length; ++i)
                  {
                     Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
                     var stream = jobs[i].stream;
                     stream.uv2 = null;
                     stream.Apply();
                  }
                  Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
               }
               if (hasUV3 && DrawClearButton("UV3"))
               {
                  for (int i = 0; i < jobs.Length; ++i)
                  {
                     Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
                     var stream = jobs[i].stream;
                     stream.uv3 = null;
                     stream.Apply();
                  }
                  Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
               }
               if (hasPositions && DrawClearButton("Pos"))
               {
                  for (int i = 0; i < jobs.Length; ++i)
                  {
                     Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
                     jobs[i].stream.positions = null;
                     Mesh m = jobs[i].stream.GetModifierMesh();
                     if (m != null)
                        m.vertices = jobs[i].meshFilter.sharedMesh.vertices;
                     jobs[i].stream.Apply();
                  }
                  Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
               }
               if (hasNormals && DrawClearButton("Norm"))
               {
                  for (int i = 0; i < jobs.Length; ++i)
                  {
                     Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
                     jobs[i].stream.normals = null;
                     jobs[i].stream.tangents = null;
                     jobs[i].stream.Apply();
                  }
                  Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
               }

               EditorGUILayout.EndHorizontal();
            }
            else if (hasStream)
            {
               if (GUILayout.Button("Remove Unused Stream Components"))
               {
                  RevertMat();
                  for (int i = 0; i < jobs.Length; ++i)
                  {
                     if (jobs[i].HasStream())
                     {
                        DestroyImmediate(jobs[i].stream);
                     }
                  }
                  UpdateDisplayMode();
               }
            }

         }
         EditorGUILayout.Separator();
         GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
         EditorGUILayout.Separator();
 
      }

      void DrawBrushSettingsGUI()
      {
         brushSize      = EditorGUILayout.Slider("Brush Size", brushSize, 0.01f, 30.0f);
         brushFlow      = EditorGUILayout.Slider("Brush Flow", brushFlow, 0.1f, 128.0f);
         brushFalloff   = EditorGUILayout.Slider("Brush Falloff", brushFalloff, 0.1f, 3.5f);

         if (tab == Tab.Paint && flowTarget != FlowTarget.ColorBA && flowTarget != FlowTarget.ColorRG)
         {
            flowRemap01 = EditorGUILayout.Toggle("use 0->1 mapping", flowRemap01);
         }
         EditorGUILayout.Separator();
         GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
         EditorGUILayout.Separator();

      }


      void DrawCustomGUI()
      {
         if (DrawRollup("Brush Settings"))
         {
            customBrush = EditorGUILayout.ObjectField("Brush", customBrush, typeof(VertexPainterCustomBrush), false) as VertexPainterCustomBrush;

            DrawBrushSettingsGUI();
         }
         scroll = EditorGUILayout.BeginScrollView(scroll);
         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Fill"))
         {
            if (OnBeginStroke != null)
            {
               OnBeginStroke(jobs);
            }
            for (int i = 0; i < jobs.Length; ++i)
            {
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Fill");
               FillMesh(jobs[i]);
            }
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            if (OnEndStroke != null)
            {
               OnEndStroke();
            }
         }

         EditorGUILayout.EndHorizontal();

         if (customBrush != null)
         {
            customBrush.DrawGUI();
         }


      }

      void DrawPaintGUI()
      {

         GUILayout.Box("Brush Settings", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)});
         var oldBM = brushMode;
         brushMode = (BrushTarget)EditorGUILayout.EnumPopup("Target Channel", brushMode);
         if (oldBM != brushMode)
         {
            UpdateDisplayMode();
         }
         if (brushMode == BrushTarget.Color || brushMode == BrushTarget.UV0_AsColor || brushMode == BrushTarget.UV1_AsColor
            || brushMode == BrushTarget.UV2_AsColor || brushMode == BrushTarget.UV3_AsColor)
         {
            brushColorMode = (BrushColorMode)EditorGUILayout.EnumPopup("Blend Mode", (System.Enum)brushColorMode);

            if (brushColorMode == BrushColorMode.Overlay || brushColorMode == BrushColorMode.Normal)
            {
               brushColor = EditorGUILayout.ColorField("Brush Color", brushColor);

               if (GUILayout.Button("Reset Palette", EditorStyles.miniButton, GUILayout.Width(80), GUILayout.Height(16)))
               {
                  if (swatches != null)
                  {
                     DestroyImmediate(swatches);
                  }
                  swatches = ColorSwatches.CreateInstance<ColorSwatches>();
                  EditorPrefs.SetString(sSwatchKey, JsonUtility.ToJson(swatches, false));
               }
            
               GUILayout.BeginHorizontal();

               for (int i = 0; i < swatches.colors.Length; ++i)
               {
                  if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(16), GUILayout.Height(16)))
                  {
                     brushColor = swatches.colors[i];
                  }
                  EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + 1, GUILayoutUtility.GetLastRect().y + 1, 14, 14), swatches.colors[i]);
               }
               GUILayout.EndHorizontal();
               GUILayout.BeginHorizontal();
               for (int i = 0; i < swatches.colors.Length; i++)
               {
                  if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(16), GUILayout.Height(12)))
                  {
                     swatches.colors[i] = brushColor;
                     EditorPrefs.SetString(sSwatchKey, JsonUtility.ToJson(swatches, false));
                  }
               }
               GUILayout.EndHorizontal();
            }
         }
         else if (brushMode == BrushTarget.ValueR || brushMode == BrushTarget.ValueG || brushMode == BrushTarget.ValueB || brushMode == BrushTarget.ValueA)
         {
            brushValue = (int)EditorGUILayout.Slider("Brush Value", (float)brushValue, 0.0f, 256.0f);
         }
         else
         {
            floatBrushValue = EditorGUILayout.FloatField("Brush Value", floatBrushValue);
            var oldUVRange = uvVisualizationRange;
            uvVisualizationRange = EditorGUILayout.Vector2Field("Visualize Range", uvVisualizationRange);
            if (oldUVRange != uvVisualizationRange)
            {
               UpdateDisplayMode();
            }
         }
 
         DrawBrushSettingsGUI();
 
         //GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Fill"))
         {
            if (OnBeginStroke != null)
            {
               OnBeginStroke(jobs);
            }
            for (int i = 0; i < jobs.Length; ++i)
            {
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Fill");
               FillMesh(jobs[i]);
            }
            if (OnEndStroke != null)
            {
               OnEndStroke();
            }
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
         }
         if (GUILayout.Button("Random"))
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Fill");
               RandomMesh(jobs[i]);
            }
         }
         EditorGUILayout.EndHorizontal();

      }

      void DrawDeformGUI()
      {
         GUILayout.Box("Brush Settings", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)});
         pull = (Event.current.shift);

         vertexMode = (VertexMode)EditorGUILayout.EnumPopup("Vertex Mode", vertexMode);
         vertexContraint = (VertexContraint)EditorGUILayout.EnumPopup("Vertex Constraint", vertexContraint);

         DrawBrushSettingsGUI();

         EditorGUILayout.LabelField(pull ? "Pull (shift)" : "Push (shift)");

      }

      void DrawFlowGUI()
      {
         GUILayout.Box("Brush Settings", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)});
         var oldV = flowVisualization;
         flowVisualization = (FlowVisualization)EditorGUILayout.EnumPopup("Visualize", flowVisualization);
         if (flowVisualization != oldV)
         {
            UpdateDisplayMode();
         }
         var ft = flowTarget;
         flowTarget = (FlowTarget)EditorGUILayout.EnumPopup("Target", flowTarget);
         if (flowTarget != ft)
         {
            UpdateDisplayMode();
         }
         flowBrushType = (FlowBrushType)EditorGUILayout.EnumPopup("Mode", flowBrushType);

         DrawBrushSettingsGUI();
         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.Space();
         
         
         
         if (GUILayout.Button("Reset"))
         {
            Vector2 norm = new Vector2(0.5f, 0.5f);
            
            foreach (PaintJob job in jobs)
            {
               PrepBrushMode(job);
               switch (flowTarget)
               {
                  case FlowTarget.ColorRG:
                     job.stream.SetColorRG(norm, job.verts.Length); break;
                  case FlowTarget.ColorBA:
                     job.stream.SetColorBA(norm, job.verts.Length); break;
                  case FlowTarget.UV0_XY:
                     job.stream.SetUV0_XY(norm, job.verts.Length); break;
                  case FlowTarget.UV0_ZW:
                     job.stream.SetUV0_ZW(norm, job.verts.Length); break;
                  case FlowTarget.UV1_XY:
                     job.stream.SetUV1_XY(norm, job.verts.Length); break;
                  case FlowTarget.UV1_ZW:
                     job.stream.SetUV1_ZW(norm, job.verts.Length); break;
                  case FlowTarget.UV2_XY:
                     job.stream.SetUV2_XY(norm, job.verts.Length); break;
                  case FlowTarget.UV2_ZW:
                     job.stream.SetUV2_ZW(norm, job.verts.Length); break;
                  case FlowTarget.UV3_XY:
                     job.stream.SetUV3_XY(norm, job.verts.Length); break;
                  case FlowTarget.UV3_ZW:
                     job.stream.SetUV3_ZW(norm, job.verts.Length); break;
               }
            }
         }
         EditorGUILayout.Space();
         EditorGUILayout.EndHorizontal();
         
      }

      List<IVertexPainterUtility> utilities = new List<IVertexPainterUtility>();
      void InitPluginUtilities()
      {
         if (utilities == null || utilities.Count == 0)
         {
            var interfaceType = typeof(IVertexPainterUtility);
            var all = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            .Select(x => System.Activator.CreateInstance(x));


            foreach (var o in all)
            {
               IVertexPainterUtility u = o as IVertexPainterUtility;
               if (u != null)
               {
                  utilities.Add(u);
               }
            }
            utilities = utilities.OrderBy(o=>o.GetName()).ToList();
         }
      }

      void DrawUtilityGUI()
      {
         InitPluginUtilities();
         for (int i = 0; i < utilities.Count; ++i)
         {
            var u = utilities[i];
            if (DrawRollup(u.GetName(), false))
            {
               u.OnGUI(jobs);
            }
         }
      }


      void OnFocus() 
      {
         if (painting)
         {
            EndStroke();
         }

         SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
         SceneView.onSceneGUIDelegate += this.OnSceneGUI;

         Undo.undoRedoPerformed -= this.OnUndo;
         Undo.undoRedoPerformed += this.OnUndo;
         this.titleContent = new GUIContent("Vertex Paint");
         Repaint();

      }
      
      void OnInspectorUpdate()
      {
         // unfortunate...
         Repaint ();
      }
      
      void OnSelectionChange()
      {
         InitMeshes();
         this.Repaint();
      }
      
      void OnDestroy() 
      {
         bool show = showVertexShader;
         showVertexShader = false;
         UpdateDisplayMode();
         showVertexShader = show;
         DestroyImmediate(VertexInstanceStream.vertexShaderMat);
         SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
      }
   }
}