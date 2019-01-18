// #define POLYBRUSH_DEBUG

using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Polybrush
{
	/**
	 *	Interface and settings for Polybrush
	 */
	public class z_Editor : EditorWindow
	{
		private static z_Editor _instance = null;
		public static z_Editor instance { get { return _instance; } }

		const double EDITOR_TARGET_FRAMERATE_LOW = .016667;
		const double EDITOR_TARGET_FRAMERATE_HIGH = .03;
		private double lastBrushUpdate = 0.0;

		private System.Action<int> pbEditLevelChangedDelegate;

		// Path to EditorWindow icon.
		const string ICON_PATH = "icon";

		// The current editing mode (RaiseLower, Smooth, Color, etc).
		private z_BrushMode mode
		{
			get
			{
				return modes.Count > 0 ? modes[0] : null;
			}
			set
			{
				if(modes.Contains(value))
					modes.Remove(value);
				modes.Insert(0, value);
			}
		}

		[SerializeField] List<z_BrushMode> modes = new List<z_BrushMode>();

		// The current editing mode (RaiseLower, Smooth, Color, etc).
		public z_BrushTool tool = z_BrushTool.None;

		// Editor for the current brush settings.
		private z_BrushSettingsEditor _brushEditor = null;

		// gameobjects that are temporarily ignored by HandleUtility.PickGameObject.
		private List<GameObject> m_IgnoreDrag = new List<GameObject>(8);

		public z_BrushSettingsEditor brushEditor
		{
			get
			{
				if(_brushEditor == null && brushSettings != null)
				{
					_brushEditor = (z_BrushSettingsEditor) Editor.CreateEditor(brushSettings);
				}
				else if ( _brushEditor.target != brushSettings )
				{
					GameObject.DestroyImmediate(_brushEditor);

					if(brushSettings != null)
						_brushEditor = (z_BrushSettingsEditor) Editor.CreateEditor(brushSettings);
				}

				return _brushEditor;
			}
		}

		// All objects that have been hovered by the mouse
		private Dictionary<GameObject, z_BrushTarget> hovering = new Dictionary<GameObject, z_BrushTarget>();

		// The current brush status
		public z_BrushTarget brushTarget = null;

		// The current brush settings
		public z_BrushSettings brushSettings;

		// A reference to the saved preset brush settings.
		[SerializeField] private z_BrushSettings brushSettingsAsset;

		// Mirror settings for this brush.
		public z_BrushMirror brushMirror = z_BrushMirror.None;

		// In which coordinate space the brush ray is flipped.
		public z_MirrorCoordinateSpace mirrorSpace = z_MirrorCoordinateSpace.World;

		// When dragging brush only test the first object selected for rays
		public bool lockBrushToFirst = false;

		// If true the mouse will always try to raycast against selection first, even if a mesh is in the way.
		public bool ignoreUnselected = true;

		int currentBrushIndex = 0;
		List<z_BrushSettings> availableBrushes = null;
		string[] availableBrushes_str = null;

		private GUIContent[] mirrorGuiContent = null;
		private GUIContent[] mirrorSpaceGuiContent = null;
		private GUIContent[] modeIcons = null;
		private GUIContent gc_SaveBrushSettings = null;

		// Keep track of the objects that have been registered for undo, allowing the editor to
		// restrict undo calls to only the necessary meshes when applying a brush swath.
		private List<GameObject> undoQueue = new List<GameObject>();
		private bool wantsRepaint = false;

		private static List<Ray> rays = new List<Ray>();
		private GameObject lastHoveredGameObject = null;
		private bool applyingBrush = false;
		private Vector2 scroll = Vector2.zero;

		[MenuItem("Tools/Polybrush/About", false, 0)]
		public static void MenuOpenAbout()
		{
			EditorWindow.GetWindow<z_About>(true, "Polybrush About", true).Show();
		}

		[MenuItem("Tools/Polybrush/Documentation", false, 1)]
		public static void MenuOpenDocumentation()
		{
			Application.OpenURL( z_Pref.DocumentationLink );
		}

		[MenuItem("Tools/Polybrush/Polybrush Window %#v", false, 20)]
		public static void MenuInitEditorWindow()
		{
			EditorWindow.GetWindow<z_Editor>(z_Pref.GetBool(z_Pref.floatingEditorWindow)).Show();
		}

		[MenuItem("Tools/Polybrush/Next Brush", true, 100)]
		static bool VerifyCycleBrush()
		{
			return instance != null;
		}

		[MenuItem("Tools/Polybrush/Next Brush", false, 100)]
		static void MenuCycleBrush()
		{
			if(instance != null)
			{
				z_BrushTool tool = (z_BrushTool) instance.tool.Next();
				instance.SetTool( (z_BrushTool) System.Math.Max((int)tool, 1) );
			}
		}

		void SetWindowFloating(bool floating)
		{
			z_Pref.SetBool(z_Pref.floatingEditorWindow, floating);
			EditorWindow.GetWindow<z_Editor>().Close();
			MenuInitEditorWindow();
		}

		void OnEnable()
		{
			if( !z_Pref.VersionCheck() )
				z_Pref.ClearPrefs();

			z_Editor._instance = this;

			this.wantsMouseMove = true;

			if( z_ReflectionUtil.ProBuilderExists() )
			{
				pbEditLevelChangedDelegate = new System.Action<int>( OnProBuilderEditLevelChanged );

				z_ReflectionUtil.enableWarnings = false;

				z_ReflectionUtil.Invoke(null,
										z_ReflectionUtil.ProBuilderEditorType,
										"AddOnEditLevelChangedListener",
										new Type[] { typeof(System.Action<int>) },
										BindingFlags.Public | BindingFlags.Static,
										pbEditLevelChangedDelegate);

				z_ReflectionUtil.enableWarnings = true;
			}

			this.titleContent = new GUIContent("Polybrush", z_IconUtility.GetIcon("Icon/window_icon"));

			if(modeIcons == null)
			{
				modeIcons = new GUIContent[]
				{
					new GUIContent(z_IconUtility.GetIcon("Icon/Sculpt"), "Push and pull vertex positions"),
					new GUIContent(z_IconUtility.GetIcon("Icon/Smooth"), "Smooth vertex positions"),
					new GUIContent(z_IconUtility.GetIcon("Icon/Palette"), "Paint vertex colors"),
					new GUIContent(z_IconUtility.GetIcon("Icon/FlowerAndGrass"), "Paint Prefabs"),
					new GUIContent(z_IconUtility.GetIcon("Icon/Bricks"), "Paint textures on meshes"),
					new GUIContent(z_IconUtility.GetIcon("Icon/Gear"), "Polybrush general settings")
				};
			}

			if(mirrorGuiContent == null)
			{
				 mirrorGuiContent = new GUIContent[]
				{
					new GUIContent("None", "No Mirroring"),
					new GUIContent("X", "Mirror across the X axis, with Y up"),
					new GUIContent("Y", "Mirror the brush up/down."),
					new GUIContent("Z", "Mirror across the Z axis, with Y up")
				};
			}

			if(mirrorSpaceGuiContent == null)
			{
				mirrorSpaceGuiContent = new GUIContent[]
				{
					new GUIContent("World", "Mirror rays in world space"),
					new GUIContent("Camera", "Mirror rays in camera space")
				};
			}

			if(gc_SaveBrushSettings == null)
				gc_SaveBrushSettings = new GUIContent("Save",
					"Save the brush settings as a preset");

			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			SceneView.onSceneGUIDelegate += OnSceneGUI;

			Undo.undoRedoPerformed -= UndoRedoPerformed;
			Undo.undoRedoPerformed += UndoRedoPerformed;

			// force update the preview
			lastHoveredGameObject = null;

			if(brushSettings == null)
				SetBrushSettings(z_EditorUtility.GetDefaultAsset<z_BrushSettings>("Brush Settings/Default.asset"));

			lockBrushToFirst = z_Pref.GetBool(z_Pref.lockBrushToFirst);
			ignoreUnselected = z_Pref.GetBool(z_Pref.ignoreUnselected);

			RefreshAvailableBrushes();

			SetTool(tool == z_BrushTool.None ? z_BrushTool.RaiseLower : tool);
		}

		void OnDisable()
		{
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			Undo.undoRedoPerformed -= UndoRedoPerformed;

			if( z_ReflectionUtil.ProBuilderExists() )
			{
				z_ReflectionUtil.enableWarnings = false;
				z_ReflectionUtil.Invoke(null,
										z_ReflectionUtil.ProBuilderEditorType,
										"RemoveOnEditLevelChangedListener",
										new Type[] { typeof(System.Action<int>) },
										BindingFlags.Public | BindingFlags.Static,
										pbEditLevelChangedDelegate );
				z_ReflectionUtil.enableWarnings = true;
			}

			// don't iterate here!  FinalizeAndReset does that
			OnBrushExit( lastHoveredGameObject );
			FinalizeAndResetHovering();
		}

		void OnDestroy()
		{
			SetTool(z_BrushTool.None);

			if(z_ReflectionUtil.ProBuilderEditorWindow != null)
				z_ReflectionUtil.Invoke(z_ReflectionUtil.ProBuilderEditorWindow, "SetEditLevel", BindingFlags.Public | BindingFlags.Instance, 4);

			foreach(z_BrushMode m in modes)
				GameObject.DestroyImmediate(m);

			if(brushSettings != null)
				GameObject.DestroyImmediate(brushSettings);

			if(_brushEditor != null)
				GameObject.DestroyImmediate(_brushEditor);
		}

		public static void DoRepaint()
		{
			if(z_Editor.instance != null)
				z_Editor.instance.wantsRepaint = true;
		}

		void OnGUI()
		{
			Event e = Event.current;
			z_GUI.PushGUISkin(z_GUI.PolybrushSkin);

			if(e.type == EventType.ContextClick)
				OpenContextMenu();

			GUILayout.Space(8);

			EditorGUI.BeginChangeCheck();

			int toolbarIndex = (int) tool - 1;
			toolbarIndex = GUILayout.Toolbar(toolbarIndex, modeIcons, "Mode");

			if(EditorGUI.EndChangeCheck())
			{
				z_BrushTool newTool = (z_BrushTool) (toolbarIndex + 1);
				SetTool( newTool == tool ? z_BrushTool.None : (z_BrushTool)toolbarIndex + 1 );
			}

			// Call current mode GUI
			if(mode != null && tool != z_BrushTool.Settings)
			{
				if(!z_Pref.GetBool(z_Pref.lockBrushSettings))
				{
					z_GUI.PopGUISkin();
					scroll = EditorGUILayout.BeginScrollView(scroll);
					z_GUI.PushGUISkin(z_GUI.PolybrushSkin);
				}

				// Show the settings header in z_Editor so that the preset selector can be included in the block.
				// Can't move preset selector to z_BrushSettingsEditor because it's a CustomEditor for z_BrushSettings,
				// along with other issues.
				if(z_GUILayout.HeaderWithDocsLink(z_GUI.TempContent("Brush Settings")))
					Application.OpenURL("http://procore3d.github.io/polybrush/brushSettings/");

				/**
				 * Brush preset selector
				 */
				GUILayout.BeginHorizontal();
					EditorGUI.BeginChangeCheck();

					currentBrushIndex = EditorGUILayout.Popup(currentBrushIndex, availableBrushes_str, "Popup");

					if(EditorGUI.EndChangeCheck())
					{
						if(currentBrushIndex >= availableBrushes.Count)
							SetBrushSettings(z_BrushSettingsEditor.AddNew());
						else
							SetBrushSettings(availableBrushes[currentBrushIndex]);
					}

					if(GUILayout.Button( gc_SaveBrushSettings, GUILayout.Width(40) ))
					{
						if(brushSettings != null && brushSettingsAsset != null)
						{
							// integer 0, 1 or 2 corresponding to ok, cancel and alt buttons
							int res = EditorUtility.DisplayDialogComplex("Save Brush Settings", "Overwrite brush preset or save as a new preset? ", "Save", "Save As", "Cancel");

							if(res == 0)
							{
								brushSettings.CopyTo(brushSettingsAsset);
								EditorGUIUtility.PingObject(brushSettingsAsset);
							}
							else if(res == 1)
							{
								z_BrushSettings dup = z_BrushSettingsEditor.AddNew();
								string name = dup.name;
								brushSettings.CopyTo(dup);
								dup.name = name;	// want to retain the unique name generated by AddNew()
								SetBrushSettings(dup);
								EditorGUIUtility.PingObject(brushSettingsAsset);
							}
						}
						else
						{
							Debug.LogWarning("Something went wrong saving brush settings.");
						}
					}
				GUILayout.EndHorizontal();

				EditorGUI.BeginChangeCheck();

					brushEditor.OnInspectorGUI();

					if(z_Pref.GetBool(z_Pref.lockBrushSettings))
					{
						z_GUI.PopGUISkin();
						scroll = EditorGUILayout.BeginScrollView(scroll);
						z_GUI.PushGUISkin(z_GUI.PolybrushSkin);
					}

					/**
					 * Mirroring
					 */
					if(z_GUILayout.HeaderWithDocsLink(z_GUI.TempContent("Brush Mirroring")))
						Application.OpenURL("http://procore3d.github.io/polybrush/brushMirroring/");

					GUILayout.BeginHorizontal();
						brushMirror = (z_BrushMirror) z_GUILayout.BitMaskField((uint)brushMirror, System.Enum.GetNames(typeof(z_BrushMirror)), "Set Brush Mirroring");
						mirrorSpace = (z_MirrorCoordinateSpace) GUILayout.Toolbar((int) mirrorSpace, mirrorSpaceGuiContent, "Command");
					GUILayout.EndHorizontal();

					mode.DrawGUI(brushSettings);

					// When using non-conforming heights in a GUIStyle the GUI will sometimes
					// clip the content too early - this pads the size so that doesn't happen.
					GUILayout.Space(16);

				if(EditorGUI.EndChangeCheck())
					mode.OnBrushSettingsChanged(brushTarget, brushSettings);

				EditorGUILayout.EndScrollView();
			}
			else
			{
				if(tool == z_BrushTool.Settings)
				{
					z_GlobalSettingsEditor.OnGUI();
				}
				else
				{
					// ...yo dawg, heard you like FlexibleSpace
					GUILayout.BeginVertical();
						GUILayout.FlexibleSpace();
							GUILayout.BeginHorizontal();
								GUILayout.FlexibleSpace();
									GUILayout.Label("Select an Edit Mode", z_GUI.headerTextStyle);
								GUILayout.FlexibleSpace();
							GUILayout.EndHorizontal();
						GUILayout.FlexibleSpace();
					GUILayout.EndVertical();
				}
			}

#if POLYBRUSH_DEBUG
			z_GUI.PushUnitySkin();
			GUILayout.Label("DEBUG", EditorStyles.boldLabel);

			GUILayout.Label("target: " + (z_Util.IsValid(brushTarget) ? brushTarget.editableObject.gameObject.name : "null"));
			GUILayout.Label("vertex: " + (z_Util.IsValid(brushTarget) ? brushTarget.editableObject.vertexCount : 0));
			GUILayout.Label("applying: " + applyingBrush);
			GUILayout.Label("lockBrushToFirst: " + lockBrushToFirst);
			GUILayout.Label("lastHoveredGameObject: " + lastHoveredGameObject);

			GUILayout.Space(6);

			foreach(var kvp in hovering)
			{
				z_BrushTarget t = kvp.Value;
				z_EditableObject dbg_editable = t.editableObject;
				GUILayout.Label("Vertex Streams: " + dbg_editable.usingVertexStreams);
				GUILayout.Label("Original: " + (dbg_editable.originalMesh == null ? "null" : dbg_editable.originalMesh.name));
				GUILayout.Label("Active: " + (dbg_editable.editMesh == null ? "null" : dbg_editable.editMesh.name));
				GUILayout.Label("Graphics: " + (dbg_editable.graphicsMesh == null ? "null" : dbg_editable.graphicsMesh.name));
			}
			z_GUI.PopGUISkin();
#endif

			z_GUI.PopGUISkin();

			if(wantsRepaint)
			{
				wantsRepaint = false;
				Repaint();
			}
		}

		void OpenContextMenu()
		{
			GenericMenu menu = new GenericMenu();

			menu.AddItem (new GUIContent("Open as Floating Window", ""), z_Pref.GetBool(z_Pref.floatingEditorWindow, false), () => { SetWindowFloating(true); } );
			menu.AddItem (new GUIContent("Open as Dockable Window", ""), !z_Pref.GetBool(z_Pref.floatingEditorWindow, false), () => { SetWindowFloating(false); } );

			menu.ShowAsContext ();
		}

		void OnProBuilderEditLevelChanged(int i)
		{
			// Top = 0,
			// Geometry = 1,
			// Texture = 2,
			// Plugin = 4

			if(i > 0 && tool != z_BrushTool.None)
				SetTool(z_BrushTool.None);
		}

		void SetTool(z_BrushTool brushTool)
		{
			if(brushTool == tool && mode != null)
				return;

			if(z_ReflectionUtil.ProBuilderEditorWindow != null && brushTool != z_BrushTool.None)
				z_ReflectionUtil.Invoke(z_ReflectionUtil.ProBuilderEditorWindow, "SetEditLevel", BindingFlags.Public | BindingFlags.Instance, 0);

			if(mode != null)
			{
				// Exiting edit mode
				if(lastHoveredGameObject != null)
				{
					OnBrushExit( lastHoveredGameObject );
					FinalizeAndResetHovering();
				}

				mode.OnDisable();
			}
			else
			{
				if(z_ReflectionUtil.ProBuilderEditorWindow != null && brushTool != z_BrushTool.None)
					z_ReflectionUtil.Invoke(z_ReflectionUtil.ProBuilderEditorWindow, "SetEditLevel", BindingFlags.Public | BindingFlags.Instance, 0);
			}

			lastHoveredGameObject = null;

			System.Type modeType = brushTool.GetModeType();

			if(modeType != null)
			{
				mode = modes.FirstOrDefault(x => x != null && x.GetType() == modeType);

				if(mode == null)
					mode = (z_BrushMode) ScriptableObject.CreateInstance( modeType );
			}

			tool = brushTool;

			if(tool != z_BrushTool.None)
			{
				Tools.current = Tool.None;
				mode.OnEnable();
			}

			Repaint();
		}

		private void RefreshAvailableBrushes()
		{
			availableBrushes = Resources.FindObjectsOfTypeAll<z_BrushSettings>().Where(x => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(x))).ToList();

			if(availableBrushes.Count < 1)
				availableBrushes.Add(z_EditorUtility.GetDefaultAsset<z_BrushSettings>("Brush Settings/Default.asset"));

			currentBrushIndex = System.Math.Max(availableBrushes.FindIndex(x => x.name.Equals(brushSettings.name)), 0);

			availableBrushes_str = availableBrushes.Select(x => x.name).ToArray();

			ArrayUtility.Add<string>(ref availableBrushes_str, string.Empty);
			ArrayUtility.Add<string>(ref availableBrushes_str, "Add Brush...");
		}

		public void SetBrushSettings(z_BrushSettings settings)
		{
			if(settings == null)
				return;

			if(brushSettings != null && brushSettings != settings)
				GameObject.DestroyImmediate(brushSettings);

			brushSettingsAsset = settings;
			brushSettings = settings.DeepCopy();
			brushSettings.hideFlags = HideFlags.HideAndDontSave;
			RefreshAvailableBrushes();

			Repaint();
		}

		void OnSceneGUI(SceneView sceneView)
		{
			if( tool == z_BrushTool.Settings || mode == null)
				return;

			Event e = Event.current;

			if(Tools.current != Tool.None)
				SetTool(z_BrushTool.None);

			if(brushSettings == null)
				SetBrushSettings(z_EditorUtility.GetDefaultAsset<z_BrushSettings>("Brush Settings/Default.asset"));

			if(z_SceneUtility.SceneViewInUse(e) || tool == z_BrushTool.None)
				return;

			int controlID = GUIUtility.GetControlID(FocusType.Passive);

			if( z_Util.IsValid(brushTarget) )
				HandleUtility.AddDefaultControl(controlID);

			switch( e.GetTypeForControl(controlID) )
			{
				case EventType.MouseMove:
					// Handles:
					//		OnBrushEnter
					//		OnBrushExit
					//		OnBrushMove
					if( EditorApplication.timeSinceStartup - lastBrushUpdate > GetTargetFramerate(brushTarget) )
					{
						lastBrushUpdate = EditorApplication.timeSinceStartup;
						UpdateBrush(e.mousePosition);
					}
					break;

				case EventType.MouseDown:
				case EventType.MouseDrag:
					// Handles:
					//		OnBrushBeginApply
					//		OnBrushApply
					//		OnBrushFinishApply
					if( EditorApplication.timeSinceStartup - lastBrushUpdate > GetTargetFramerate(brushTarget) )
					{
						lastBrushUpdate = EditorApplication.timeSinceStartup;
						UpdateBrush(e.mousePosition, true);
						ApplyBrush();
					}
					break;

				case EventType.MouseUp:
					if(applyingBrush)
					{
						OnFinishApplyingBrush();
					}
					break;

				case EventType.ScrollWheel:
					ScrollBrushSettings(e);
					break;
			}

			if( z_Util.IsValid(brushTarget) )
				mode.DrawGizmos(brushTarget, brushSettings);
		}

		double GetTargetFramerate(z_BrushTarget target)
		{
			if(z_Util.IsValid(target) && target.vertexCount > 24000)
				return EDITOR_TARGET_FRAMERATE_LOW;

			return EDITOR_TARGET_FRAMERATE_HIGH;
		}

		/**
		 *	Get a z_EditableObject matching the GameObject go or create a new one.
		 */
		z_BrushTarget GetBrushTarget(GameObject go)
		{
			z_BrushTarget target = null;

			if( !hovering.TryGetValue(go, out target) )
			{
				target = new z_BrushTarget(z_EditableObject.Create(go));
				hovering.Add(go, target);
			}
			else if( !z_Util.IsValid(target) )
			{
				hovering[go] = new z_BrushTarget(z_EditableObject.Create(go));
			}

			return target;
		}

		/**
		 * Update the current brush object and weights with the current mouse position.
		 */
		void UpdateBrush(Vector2 mousePosition, bool isDrag = false)
		{
			// Must check HandleUtility.PickGameObject only during MouseMoveEvents or errors will rain.
			GameObject go = null;
			brushTarget = null;

			if( isDrag && lockBrushToFirst && lastHoveredGameObject != null )
			{
				go = lastHoveredGameObject;
				brushTarget = GetBrushTarget(go);
			}
			else if(ignoreUnselected || isDrag)
			{
				GameObject cur = null;
				int max = 0;	// safeguard against unforeseen while loop errors crashing unity

				do
				{
					int tmp;
					// overloaded PickGameObject ignores array of GameObjects, this is used
					// when there are non-selected gameObjects between the mouse and selected
					// gameObjects.
					cur = HandleUtility.PickGameObject(mousePosition, m_IgnoreDrag.ToArray(), out tmp);

					if(cur != null)
					{
						if( !z_EditorUtility.InSelection(cur.transform) )
						{
							if(!m_IgnoreDrag.Contains(cur))
							{
								m_IgnoreDrag.Add(cur);
							}
						}
						else
						{
							brushTarget = GetBrushTarget(cur);

							if(brushTarget != null)
							{
								go = cur;
							}
							else
							{
								m_IgnoreDrag.Add(cur);
							}
						}
					}
				} while( go == null && cur != null && max++ < 128);
			}
			else
			{
				go = HandleUtility.PickGameObject(mousePosition, false);

				if( go != null && z_EditorUtility.InSelection(go) )
					brushTarget = GetBrushTarget(go);
				else
					go = null;
			}

			bool mouseHoverTargetChanged = false;

			Ray mouseRay = HandleUtility.GUIPointToWorldRay(mousePosition);

			// if the mouse hover picked up a valid editable, raycast against that.  otherwise
			// raycast all meshes in selection
			if(go == null)
			{
				foreach(var kvp in hovering)
				{
					z_BrushTarget t = kvp.Value;

					if( z_Util.IsValid(t) && DoMeshRaycast(mouseRay, t) )
					{
						brushTarget = t;
						go = t.gameObject;
						break;
					}
				}

			}
			else
			{
				if(!DoMeshRaycast(mouseRay, brushTarget))
				{
					if(!isDrag || !lockBrushToFirst)
					{
						go = null;
						brushTarget = null;
					}

					return;
				}
			}

			// if hovering off another gameobject, call OnBrushExit on that last one and mark the
			// target as having been changed
			if( go != lastHoveredGameObject )
			{
				OnBrushExit(lastHoveredGameObject);
				mouseHoverTargetChanged = true;
				lastHoveredGameObject = go;
			}

			if(brushTarget == null)
				return;

			if(mouseHoverTargetChanged)
			{
				OnBrushEnter(brushTarget, brushSettings);

				// brush is in use, adding a new object to the undo
				if(applyingBrush && !undoQueue.Contains(go))
				{
					int curGroup = Undo.GetCurrentGroup();
					brushTarget.editableObject.isDirty = true;
					OnBrushBeginApply(brushTarget, brushSettings);
					Undo.CollapseUndoOperations(curGroup);
				}
			}

			OnBrushMove();

			SceneView.RepaintAll();
			this.Repaint();
		}

		/**
		 * Calculate the weights for this ray.
		 */
		bool DoMeshRaycast(Ray mouseRay, z_BrushTarget target)
		{
			if( !z_Util.IsValid(target) )
				return false;

			target.ClearRaycasts();

			z_EditableObject editable = target.editableObject;

			rays.Clear();
			rays.Add(mouseRay);

			if(brushMirror != z_BrushMirror.None)
			{
				for(int i = 0; i < 3; i++)
				{
					if( ((uint)brushMirror & (1u << i)) < 1 )
						continue;

					int len = rays.Count;

					for(int n = 0; n < len; n++)
					{
						Vector3 flipVec = ((z_BrushMirror)(1u << i)).ToVector3();

						if(mirrorSpace == z_MirrorCoordinateSpace.World)
						{
							Vector3 cen = editable.gameObject.GetComponent<Renderer>().bounds.center;
							rays.Add( new Ray(	Vector3.Scale(rays[n].origin - cen, flipVec) + cen,
												Vector3.Scale(rays[n].direction, flipVec)));
						}
						else
						{
							Transform t = SceneView.lastActiveSceneView.camera.transform;
							Vector3 o = t.InverseTransformPoint(rays[n].origin);
							Vector3 d = t.InverseTransformDirection(rays[n].direction);
							rays.Add(new Ray( 	t.TransformPoint(Vector3.Scale(o, flipVec)),
												t.TransformDirection(Vector3.Scale(d, flipVec))));
						}
					}
				}
			}

			bool hitMesh = false;

			int[] triangles = editable.editMesh.GetTriangles();

			foreach(Ray ray in rays)
			{
				z_RaycastHit hit;

				if( z_SceneUtility.WorldRaycast(ray, editable.transform, editable.editMesh.vertices, triangles, out hit) )
				{
					target.raycastHits.Add(hit);
					hitMesh = true;
				}
			}

			z_SceneUtility.CalculateWeightedVertices(target, brushSettings);

			return hitMesh;
		}

		void ApplyBrush()
		{
			if(!z_Util.IsValid(brushTarget))
				return;

			if(!applyingBrush)
			{
				undoQueue.Clear();
				applyingBrush = true;
				OnBrushBeginApply(brushTarget, brushSettings);
			}

			mode.OnBrushApply(brushTarget, brushSettings);

			SceneView.RepaintAll();
		}

		void OnBrushBeginApply(z_BrushTarget brushTarget, z_BrushSettings settings)
		{
			z_SceneUtility.PushGIWorkflowMode();
			mode.RegisterUndo(brushTarget);
			undoQueue.Add(brushTarget.gameObject);
			mode.OnBrushBeginApply(brushTarget, brushSettings);
		}

		void ScrollBrushSettings(Event e)
		{
			float nrm = 1f;

			switch(e.modifiers)
			{
				case EventModifiers.Control:
					nrm = Mathf.Sin(Mathf.Max(.001f, brushSettings.normalizedRadius)) * .03f * (brushSettings.brushRadiusMax - brushSettings.brushRadiusMin);
					brushSettings.radius = brushSettings.radius - (e.delta.y * nrm);
					break;

				case EventModifiers.Shift:
					nrm = Mathf.Sin(Mathf.Max(.001f, brushSettings.falloff)) * .03f;
					brushSettings.falloff = brushSettings.falloff - e.delta.y * nrm;
					break;

				case EventModifiers.Control | EventModifiers.Shift:
					nrm = Mathf.Sin(Mathf.Max(.001f, brushSettings.strength)) * .03f;
					brushSettings.strength = brushSettings.strength - e.delta.y * nrm;
					break;

				default:
					return;
			}

			EditorUtility.SetDirty(brushSettings);

			if(mode != null)
			{
				UpdateBrush(Event.current.mousePosition);
				mode.OnBrushSettingsChanged(brushTarget, brushSettings);
			}

			e.Use();
			Repaint();
			SceneView.RepaintAll();
		}

		void OnSelectionChange()
		{
			m_IgnoreDrag.Clear();
		}

		void OnBrushEnter(z_BrushTarget target, z_BrushSettings settings)
		{
			mode.OnBrushEnter(target.editableObject, settings);
		}

		void OnBrushMove()
		{
			if( z_Util.IsValid(brushTarget) )
				mode.OnBrushMove( brushTarget, brushSettings );
		}

		void OnBrushExit(GameObject go)
		{
			z_BrushTarget target;

			if(go == null || !hovering.TryGetValue(go, out target) || !z_Util.IsValid(target))
				return;

			mode.OnBrushExit(target.editableObject);

			if(!applyingBrush)
				target.editableObject.Revert();
		}

		void OnFinishApplyingBrush()
		{
			z_SceneUtility.PopGIWorkflowMode();

			applyingBrush = false;
			mode.OnBrushFinishApply(brushTarget, brushSettings);
			FinalizeAndResetHovering();
			m_IgnoreDrag.Clear();
		}

		void FinalizeAndResetHovering()
		{
			foreach(var kvp in hovering)
			{
				z_BrushTarget target = kvp.Value;

				if(!z_Util.IsValid(target))
					continue;

				// if mesh hasn't been modified, revert it back
				// to the original mesh so that unnecessary assets
				// aren't allocated.  if it has been modified, let
				// the editableObject apply those changes to the
				// pb_Object if necessary.
				if(!target.editableObject.isDirty)
					target.editableObject.Revert();
				else
					target.editableObject.Apply(true, true);
			}

			hovering.Clear();
			brushTarget = null;
			lastHoveredGameObject = null;

			EditorWindow pbEditor = z_ReflectionUtil.ProBuilderEditorWindow;

			if(pbEditor != null)
				z_ReflectionUtil.Invoke(	pbEditor,
											pbEditor.GetType(),
											"UpdateSelection",
											new System.Type[] { typeof(bool) },
											BindingFlags.Instance | BindingFlags.Public,
											new object[] { true });

			Repaint();
		}

		void UndoRedoPerformed()
		{
			if( z_Pref.GetBool(z_Pref.rebuildCollisions) )
			{
				foreach(GameObject go in undoQueue.Where(x => x != null))
				{
					MeshCollider mc = go.GetComponent<MeshCollider>();
					MeshFilter mf = go.GetComponent<MeshFilter>();

					if(mc == null || mf == null || mf.sharedMesh == null)
						continue;

					mc.sharedMesh = null;
					mc.sharedMesh = mf.sharedMesh;
				}
			}

			hovering.Clear();
			brushTarget = null;
			lastHoveredGameObject = null;

			mode.UndoRedoPerformed(undoQueue);
			undoQueue.Clear();

			SceneView.RepaintAll();
			DoRepaint();
		}
	}
}
