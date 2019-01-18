using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Polybrush
{
	/**
	 *	Prefab painter brush mode.
	 */
	public class z_BrushModePrefab : z_BrushMode
	{
		const string PREFAB_PALETTE_PATH = "Prefab Palettes/Default.asset";
		private double lastBrushApplication = 0.0;

		private z_LocalPref<bool> hitSurfaceIsParent;
		private z_LocalPref<bool> avoidOverlappingGameObjects;
		private z_LocalPref<int> previewThumbSize;

		// preferences
		private bool placeWithPivot = false;

		// The current prefab palette
		[SerializeField] z_PrefabPalette _prefabPalette = null;

		z_PrefabPalette[] availablePalettes = null;
		string[] availablePalettes_str = null;
		int currentPaletteIndex = -1;

		private z_PrefabPalette prefabPalette
		{
			get
			{
				if(_prefabPalette == null)
					prefabPalette = z_EditorUtility.GetDefaultAsset<z_PrefabPalette>(PREFAB_PALETTE_PATH);

				return _prefabPalette;
			}
			set
			{
				if(prefabPaletteEditor != null)
					GameObject.DestroyImmediate(prefabPaletteEditor);

				_prefabPalette = value;

				prefabPaletteEditor = (z_PrefabPaletteEditor) Editor.CreateEditor(_prefabPalette);
				prefabPaletteEditor.onSelectionChanged = SelectedPrefab;
			}
		}

		// An Editor for the prefabPalette.
		z_PrefabPaletteEditor prefabPaletteEditor = null;

		// all instances of prefabs in the current palette in this scene.
		private List<GameObject> instances = null;
		private List<z_PrefabAndSettings> queued = null;

		public override string UndoMessage { get { return "Paint Prefabs"; } }
		protected override string ModeSettingsHeader { get { return "Placement Settings"; } }
		protected override string DocsLink { get { return "http://procore3d.github.io/polybrush/modes/place/"; } }
		private GUIStyle paletteStyle;

		static GUIContent gc_usePrefabPivot = new GUIContent("Use Pivot", "By default Polybrush will position placed objects entirely on top of the target plane.  When 'Use Pivot' is enabled objects will instead be placed by their assigned mesh origin.");
		static GUIContent gc_hitSurfaceIsParent = new GUIContent("Hit Surface is Parent", "When enabled any instantiated prefab from this mode will be automatically made a child of the surface it was placed on.");
		static GUIContent gc_avoidOverlappingGameObjects = new GUIContent("Avoid Overlap", "If enabled Polybrush will attempt to avoid placing prefabs where they may overlap with another placed GameObject.");

		static string FormatInstanceName(GameObject go)
		{
			return string.Format("{0}(Clone)", go.name);
		}

		public override void OnEnable()
		{
			base.OnEnable();

			RefreshAvailablePalettes();

			if(_prefabPalette == null)
				prefabPalette = z_EditorUtility.GetDefaultAsset<z_PrefabPalette>(PREFAB_PALETTE_PATH);

			if(prefabPaletteEditor != null)
			{
				Object.DestroyImmediate(prefabPaletteEditor);
				prefabPaletteEditor = null;
			}

			prefabPaletteEditor = (z_PrefabPaletteEditor) Editor.CreateEditor(_prefabPalette);

			// unity won't serialize delegates, so even if prefabPalette isn't null and the editor remains valid
			// the delegate could still be null after a script reload.
			prefabPaletteEditor.onSelectionChanged = SelectedPrefab;

			paletteStyle = new GUIStyle();
			paletteStyle.padding = new RectOffset(8, 8, 8, 8);

			hitSurfaceIsParent = new z_LocalPref<bool>("prefab_hitSurfaceIsParent", true);
			avoidOverlappingGameObjects = new z_LocalPref<bool>("prefab_avoidOverlappingGameObjects");
			previewThumbSize = new z_LocalPref<int>("prefab_previewThumbSize", 64);
		}

		public override void OnDisable()
		{
			base.OnDisable();

			if(prefabPaletteEditor != null)
			{
				GameObject.DestroyImmediate(prefabPaletteEditor);
				prefabPaletteEditor = null;
			}
		}

		// Inspector GUI shown in the Editor window.  Base class shows z_BrushSettings by default
		public override void DrawGUI(z_BrushSettings brushSettings)
		{
			base.DrawGUI(brushSettings);

			placeWithPivot = z_GUILayout.Toggle(gc_usePrefabPivot, placeWithPivot);

			z_GlobalSettingsEditor.lockBrushToFirst = z_GUILayout.Toggle(z_GlobalSettingsEditor.gc_lockBrushToFirst, z_GlobalSettingsEditor.lockBrushToFirst);

			hitSurfaceIsParent.prefValue = z_GUILayout.Toggle(gc_hitSurfaceIsParent, hitSurfaceIsParent);
			avoidOverlappingGameObjects.prefValue = z_GUILayout.Toggle(gc_avoidOverlappingGameObjects, avoidOverlappingGameObjects);

			GUILayout.Space(4);

			if(prefabPalette == null)
				RefreshAvailablePalettes();

			EditorGUI.BeginChangeCheck();
			currentPaletteIndex = EditorGUILayout.Popup(currentPaletteIndex, availablePalettes_str, "popup");
			if(EditorGUI.EndChangeCheck())
			{
				if(currentPaletteIndex >= availablePalettes.Length)
					SetPrefabPalette( z_PrefabPaletteEditor.AddNew() );
				else
					SetPrefabPalette(availablePalettes[currentPaletteIndex]);
			}

			GUILayout.Space(4);

			GUILayout.BeginHorizontal();
				GUILayout.Label("Prefabs");
				previewThumbSize.prefValue = (int) GUILayout.HorizontalSlider((float)previewThumbSize, 16f, 128f);
			GUILayout.EndHorizontal();

			GUILayout.BeginVertical( paletteStyle );
			if(prefabPaletteEditor != null)
				prefabPaletteEditor.OnInspectorGUI_Internal(previewThumbSize);
			GUILayout.EndVertical();
		}

		private void SetPrefabPalette(z_PrefabPalette palette)
		{
			prefabPalette = palette;
			RefreshAvailablePalettes();
		}

		private void RefreshAvailablePalettes()
		{
			availablePalettes = Resources.FindObjectsOfTypeAll<z_PrefabPalette>().Where(x => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(x))).ToArray();
			availablePalettes_str = availablePalettes.Select(x => x.name).ToArray();
			ArrayUtility.Add<string>(ref availablePalettes_str, string.Empty);
			ArrayUtility.Add<string>(ref availablePalettes_str, "Add Palette...");
			currentPaletteIndex = System.Array.IndexOf(availablePalettes, _prefabPalette);
		}

		private void SelectedPrefab(IEnumerable<int> selected)
		{
			if(selected == null)
				queued = null;
			else
				queued = prefabPalette.prefabs.Where( (x, i) => { return selected.Contains(i); } ).ToList();
		}

		public override void OnBrushSettingsChanged(z_BrushTarget target, z_BrushSettings settings)
		{
			base.OnBrushSettingsChanged(target, settings);
		}

		// Called when the mouse begins hovering an editable object.
		public override void OnBrushEnter(z_EditableObject target, z_BrushSettings settings)
		{
			base.OnBrushEnter(target, settings);
		}

		// Called whenever the brush is moved.  Note that @target may have a null editableObject.
		public override void OnBrushMove(z_BrushTarget target, z_BrushSettings settings)
		{
			base.OnBrushMove(target, settings);
		}

		// Called when the mouse exits hovering an editable object.
		public override void OnBrushExit(z_EditableObject target)
		{
			base.OnBrushExit(target);
		}

		public override void OnBrushBeginApply(z_BrushTarget target, z_BrushSettings settings)
		{
			base.OnBrushBeginApply(target, settings);
			instances = z_SceneUtility.FindInstancesInScene(prefabPalette.prefabs.Select(x => x.gameObject), FormatInstanceName).ToList();
		}

		// Called every time the brush should apply itself to a valid target.  Default is on mouse move.
		public override void OnBrushApply(z_BrushTarget target, z_BrushSettings settings)
		{
			bool shift = Event.current.shift && Event.current.type != EventType.ScrollWheel;

			if( (EditorApplication.timeSinceStartup - lastBrushApplication) > Mathf.Max(.06f, (1f - settings.strength)) )
			{
				lastBrushApplication = EditorApplication.timeSinceStartup;

				if(shift)
				{
					foreach(z_RaycastHit hit in target.raycastHits)
						RemoveGameObjects(hit, target, settings);
				}
				else
				{
					foreach(z_RaycastHit hit in target.raycastHits)
						PlaceGameObject(hit, GetPrefab().gameObject, target, settings);
				}
			}
		}

		// Handle Undo locally since it doesn't follow the same pattern as mesh modifications.
		public override void RegisterUndo(z_BrushTarget brushTarget) {}

		private void PlaceGameObject(z_RaycastHit hit, GameObject prefab, z_BrushTarget target, z_BrushSettings settings)
		{
			if(prefab == null)
				return;

			Ray ray = RandomRay(hit.position, hit.normal, settings.radius, settings.falloff, settings.falloffCurve);

			z_RaycastHit rand_hit;

			Vector3[] vertices = target.editableObject.editMesh.vertices;
			int[] triangles = target.editableObject.editMesh.GetTriangles();

			if( z_SceneUtility.MeshRaycast(ray, vertices, triangles, out rand_hit) )
			{
				float pivotOffset = placeWithPivot ? 0f : GetPivotOffset(prefab);

				Quaternion rotation = Quaternion.FromToRotation(Vector3.up, target.transform.TransformDirection(rand_hit.normal));
				Quaternion random = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);

				GameObject inst = PrefabUtility.ConnectGameObjectToPrefab(Instantiate(prefab), prefab);

				inst.transform.localPosition = target.transform.TransformPoint(rand_hit.position);
				inst.transform.localRotation = rotation * random;
				inst.name = FormatInstanceName(prefab);
				inst.transform.position = inst.transform.position + inst.transform.up * pivotOffset;

				if( avoidOverlappingGameObjects && TestIntersection(inst) )
				{
					Object.DestroyImmediate(inst);
					return;
				}

				if( hitSurfaceIsParent )
					inst.transform.SetParent(target.transform);

				PrefabUtility.RecordPrefabInstancePropertyModifications(inst);

				instances.Add(inst);

				Undo.RegisterCreatedObjectUndo(inst, UndoMessage);
			}
		}

		private void RemoveGameObjects(z_RaycastHit hit, z_BrushTarget target, z_BrushSettings settings)
		{
			Vector3 worldHitPosition = target.editableObject.transform.TransformPoint(hit.position);

			int count = instances.Count;

			for(int i = 0; i < count; i++)
			{
				if( instances[i] != null && Vector3.Distance(worldHitPosition, instances[i].transform.position) < settings.radius )
				{
					GameObject go = instances[i];
					instances.RemoveAt(i);
					count--;
					Undo.DestroyObjectImmediate(go);
				}
			}
		}

		private Ray RandomRay(Vector3 position, Vector3 normal, float radius, float falloff, AnimationCurve curve)
		{
			Vector3 a = Vector3.zero;
			Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);

			a.x = Mathf.Cos(Random.Range(0f, 360f));
			a.y = Mathf.Sin(Random.Range(0f, 360f));

			float r = Mathf.Sqrt(Random.Range(0f, 1f));

			while(true)
			{
				// this isn't great
				if(r < falloff || Random.Range(0f, 1f) > Mathf.Clamp(curve.Evaluate( 1f - ((r - falloff) / (1f - falloff))), 0f, 1f))
				{
					a = position + (rotation * (a.normalized * r * radius));
					return new Ray(a + normal * 10f, -normal);
				}
				else
				{
					r = Mathf.Sqrt(Random.Range(0f, 1f));
				}
			}
		}

		private z_PrefabAndSettings GetPrefab()
		{
			List<z_PrefabAndSettings> pool = queued != null && queued.Count > 0 ? queued : prefabPalette.prefabs;
			int count = pool == null ? 0 : pool.Count;
			return count > 0 ? pool[(int) Random.Range(0, count)] : null;
		}

		private float GetPivotOffset(GameObject go)
		{
			MeshFilter mf = go.GetComponent<MeshFilter>();

			// probuilder meshes that are prefabs might not have a mesh
			// associated with them, so make sure they do before querying
			// for bounds
			object pb = go.GetComponent("pb_Object");

			if(pb != null)
				z_ReflectionUtil.Invoke(pb, "Awake");

			if(mf == null || mf.sharedMesh == null)
				return 0f;

			Bounds bounds = mf.sharedMesh.bounds;

			return (-bounds.center.y + bounds.extents.y) * go.transform.localScale.y;
		}

		private struct SphereBounds
		{
			public Vector3 position;
			public float radius;

			public SphereBounds(Vector3 p, float r)
			{
				position = p;
				radius = r;
			}

			public bool Intersects(SphereBounds other)
			{
				return Vector3.Distance(position, other.position) < (radius + other.radius);
			}
		}

		private static bool GetSphereBounds(GameObject go, out SphereBounds bounds)
		{
			bounds = new SphereBounds();

			if(go == null)
				return false;

			Mesh mesh = null;

			MeshFilter mf = go.GetComponentInChildren<MeshFilter>();

			if(mf != null && mf.sharedMesh != null)
			{
				mesh = mf.sharedMesh;
			}
			else
			{
				SkinnedMeshRenderer smr = go.GetComponentInChildren<SkinnedMeshRenderer>();

				if(smr != null && smr.sharedMesh != null)
					mesh = smr.sharedMesh;
			}

			if(mesh != null)
			{
				Bounds meshBounds = mf.sharedMesh.bounds;
				bounds.position = mf.transform.TransformPoint(meshBounds.center - (Vector3.up * meshBounds.extents.y));
				bounds.radius = Mathf.Max(meshBounds.extents.x, meshBounds.extents.z);

				return true;
			}

			return false;
		}

		/**
		 *	Tests if go intersects with any painted objects.
		 */
		private bool TestIntersection(GameObject go)
		{
			SphereBounds bounds, it_bounds;

			if(!GetSphereBounds(go, out bounds))
				return false;

			int c = instances == null ? 0 : instances.Count;

			for(int i = 0; i < c; i++)
			{
				if(GetSphereBounds(instances[i], out it_bounds) && bounds.Intersects(it_bounds))
					return true;
			}

			return false;
		}

		public override void DrawGizmos(z_BrushTarget target, z_BrushSettings settings)
		{
			base.DrawGizmos(target, settings);

			// SphereBounds bounds;

			// foreach(GameObject go in instances)
			// {
			// 	if(!GetSphereBounds(go, out bounds))
			// 		continue;

			// 	Handles.CircleCap(-1, bounds.position, Quaternion.Euler(Vector3.up * 90f), bounds.radius);
			// 	Handles.CircleCap(-1, bounds.position, Quaternion.Euler(Vector3.right * 90f), bounds.radius);
			// 	Handles.CircleCap(-1, bounds.position, Quaternion.Euler(Vector3.forward * 90f), bounds.radius);
			// }
		}

	}
}
