using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Polybrush
{
	internal class z_GlobalSettingsEditor : Editor
	{
		private static bool initialized = false;

		private static readonly GUIContent gc_rebuildNormals = new GUIContent("Rebuild Normals", "After a mesh modification the normals will be recalculated.");
		private static readonly GUIContent gc_rebuildCollisions = new GUIContent("Rebuild MeshCollider", "After a mesh modification the mesh collider will be recalculated.");
		public static readonly GUIContent gc_lockBrushToFirst = new GUIContent("Lock Brush to First", "When applying a brush this prevents any other mesh from intercepting the stroke.  Disable this if you want to apply across multiple meshes.");
		// private static readonly GUIContent gc_ignoreUnselected = new GUIContent("Ignore Unselected", "When a Polybrush tool is engaged, the mouse will always interact first with any selected GameObjects.");
		private static readonly GUIContent gc_lockBrushSettings = new GUIContent("Anchor Brush Settings", "Locks the Brush Settings to the top of the window.");
		private static readonly GUIContent gc_hideWireframe = new GUIContent("Hide Wireframe", "Hides the object wireframe when a brush is hovering.");
		private static readonly GUIContent gc_fullStrengthColor = new GUIContent("Brush Handle Color", "The color that the brush handle will render.");
		private static readonly GUIContent gc_BrushGradient = new GUIContent("Brush Gradient", "The color gradient used to mark a brush's strength through the falloff.");
		public static readonly GUIContent gc_vertexBillboardSize = new GUIContent("Vertex Render Size", "The size at which selected vertices will be rendered.");
		private static readonly GUIContent gc_additionalVertexStreams = new GUIContent("Addl. Vertex Streams", "Instead of applying changes directly to the mesh, modifications will be stored in an additionalVertexStreams mesh.  This option can be more performance friendly in some cases.");

		private static bool rebuildNormals { get { return z_Pref.GetBool(z_Pref.rebuildNormals); } set { z_Pref.SetBool(z_Pref.rebuildNormals, value); } }
		private static bool rebuildCollisions { get { return z_Pref.GetBool(z_Pref.rebuildCollisions); } set { z_Pref.SetBool(z_Pref.rebuildCollisions, value); } }
		private static bool hideWireframe { get { return z_Pref.GetBool(z_Pref.hideWireframe); } set { z_Pref.SetBool(z_Pref.hideWireframe, value); } }
		private static bool lockBrushSettings { get { return z_Pref.GetBool(z_Pref.lockBrushSettings); } set { z_Pref.SetBool(z_Pref.lockBrushSettings, value); } }
		private static bool additionalVertexStreams { get { return z_Pref.GetBool(z_Pref.additionalVertexStreams); } set { z_Pref.SetBool(z_Pref.additionalVertexStreams, value); } }

		public static bool lockBrushToFirst
		{
			get { return z_Pref.GetBool(z_Pref.lockBrushToFirst); }
			set  {
				z_Pref.SetBool(z_Pref.lockBrushToFirst, value);
				z_Editor.instance.lockBrushToFirst = value;
			}
		}

		public static bool ignoreUnselected
		{
			get { return z_Pref.GetBool(z_Pref.ignoreUnselected); }
			set  {
				z_Pref.SetBool(z_Pref.ignoreUnselected, value);
				z_Editor.instance.ignoreUnselected = value;
			}
		}


		private static Color fullStrengthColor { get { return z_Pref.GetColor(z_Pref.brushColor); } set { z_Pref.SetColor(z_Pref.brushColor, value); } }
		private static Color brushGradient { get { return z_Pref.GetColor(z_Pref.brushGradient); } set { z_Pref.SetColor(z_Pref.brushGradient, value); } }
		// vertexBillboardSize uses EditorPrefs because z_OverlayRenderer needs access, and it's not possible to query the z_PreferenceDictionary from
		// runtime code
		private static float vertexBillboardSize { get { return EditorPrefs.GetFloat(z_Pref.vertexBillboardSize, 1.4f); } set { EditorPrefs.SetFloat(z_Pref.vertexBillboardSize, value); } }

		private static Gradient gradient;

		static void GetPreferences()
		{
			gradient = z_Pref.GetGradient(z_Pref.brushGradient);
		}

		static void SetPreferences()
		{
			z_Pref.SetGradient(z_Pref.brushGradient, gradient);
		}

		internal static void OnGUI()
		{
			if(!initialized)
				GetPreferences();

			if( z_GUILayout.HeaderWithDocsLink(z_GUI.TempContent("General Settings", "")) )
				Application.OpenURL("http://procore3d.github.io/polybrush/settings/");

			rebuildNormals 		= z_GUILayout.Toggle(gc_rebuildNormals, rebuildNormals);
			rebuildCollisions 	= z_GUILayout.Toggle(gc_rebuildCollisions, rebuildCollisions);

			lockBrushToFirst 	= z_GUILayout.Toggle(gc_lockBrushToFirst, lockBrushToFirst);
			lockBrushSettings 	= z_GUILayout.Toggle(gc_lockBrushSettings, lockBrushSettings);

			additionalVertexStreams	= z_GUILayout.Toggle(gc_additionalVertexStreams, additionalVertexStreams);
			hideWireframe 		= z_GUILayout.Toggle(gc_hideWireframe, hideWireframe);

			GUILayout.Label(gc_fullStrengthColor);

			z_GUI.PushUnitySkin();
			fullStrengthColor 	= z_GUILayout.ColorField(GUIContent.none, fullStrengthColor);
			z_GUI.PopGUISkin();

			GUILayout.BeginHorizontal();
				GUILayout.Label(gc_vertexBillboardSize);
				vertexBillboardSize = GUILayout.HorizontalSlider(vertexBillboardSize, 0f, 4f);
				vertexBillboardSize = EditorGUILayout.FloatField(vertexBillboardSize, "textfield", GUILayout.MaxWidth(64));
			GUILayout.EndHorizontal();

			try
			{
				EditorGUI.BeginChangeCheck();

				gradient = z_GUILayout.GradientField(gc_BrushGradient, gradient);

				if(EditorGUI.EndChangeCheck())
					SetPreferences();
			}
			catch
			{
				// internal editor gripe about something unimportant
			}

			if(GUILayout.Button("Reset Defaults"))
				if(EditorUtility.DisplayDialog("Reset Polybrush Preferences", "This will clear any saved Polybrush preference items.  Are you sure you want to continue?", "Yes", "No"))
					z_Pref.ClearPrefs();
		}
	}
}
