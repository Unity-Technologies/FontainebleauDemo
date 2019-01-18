using UnityEngine;
using UnityEditor;

namespace Polybrush
{
	/**
	 *	The default editor for z_BrushSettings.
	 */
	[CustomEditor(typeof(z_BrushSettings))]
	public class z_BrushSettingsEditor : Editor
	{
		public bool showSettingsBounds = false;
		private GUIStyle 	settingsButtonStyle,
							settingsBackgroundStyle,
							settingsBackgroundBorderStyle;

		private GUIContent gc_Radius;
		private GUIContent gc_Falloff;
		private GUIContent gc_FalloffCurve;
		private GUIContent gc_Strength;

		private GUIContent gc_RadiusMin, gc_RadiusMax;
		private GUIContent gc_AllowUnclampedFalloff;
		private GUIContent gc_BrushSettingsMinMax;

		private static Color settings_background_color;
		private static Color settings_border_color;

		private static readonly Rect RECT_ONE = new Rect(0,0,1,1);

		SerializedProperty 	radius,
							falloff,
							strength,
							brushRadiusMin,
							brushRadiusMax,
							brushStrengthMin,
							brushStrengthMax,
							curve,
							allowNonNormalizedFalloff;

		public void OnEnable()
		{
			settings_background_color = EditorGUIUtility.isProSkin ? z_GUI.BOX_BACKGROUND_DARK : z_GUI.BOX_BACKGROUND_LIGHT;
			settings_border_color = EditorGUIUtility.isProSkin ? z_GUI.BOX_OUTLINE_DARK : z_GUI.BOX_OUTLINE_LIGHT;

			if(serializedObject == null)
				GameObject.DestroyImmediate(this);

			settingsButtonStyle = new GUIStyle();
			settingsButtonStyle.imagePosition = ImagePosition.ImageOnly;
			const int PAD = 2, MARGIN_HORIZONTAL = 4, MARGIN_VERTICAL = 0;
			settingsButtonStyle.alignment = TextAnchor.MiddleCenter;
			settingsButtonStyle.margin = new RectOffset(MARGIN_HORIZONTAL, MARGIN_HORIZONTAL, MARGIN_VERTICAL, MARGIN_VERTICAL);
			settingsButtonStyle.padding = new RectOffset(PAD, PAD, 4, PAD);

			settingsBackgroundStyle = new GUIStyle();
			settingsBackgroundStyle.normal.background = EditorGUIUtility.whiteTexture;
			settingsBackgroundStyle.margin = new RectOffset(0,0,0,0);
			settingsBackgroundStyle.padding = new RectOffset(2,2,4,4);

			settingsBackgroundBorderStyle = new GUIStyle();
			settingsBackgroundBorderStyle.normal.background = EditorGUIUtility.whiteTexture;
			settingsBackgroundBorderStyle.margin = new RectOffset(4,4,0,6);
			settingsBackgroundBorderStyle.padding = new RectOffset(1,1,1,1);

			/// User settable
			radius = serializedObject.FindProperty("_radius");
			falloff = serializedObject.FindProperty("_falloff");
			curve = serializedObject.FindProperty("_curve");
			strength = serializedObject.FindProperty("_strength");

			/// Bounds
			brushRadiusMin = serializedObject.FindProperty("brushRadiusMin");
			brushRadiusMax = serializedObject.FindProperty("brushRadiusMax");
			allowNonNormalizedFalloff = serializedObject.FindProperty("allowNonNormalizedFalloff");

			gc_Radius = new GUIContent("Outer Radius", "Radius: The distance from the center of a brush to it's outer edge.\n\nShortcut: 'Ctrl + Mouse Wheel'");
			gc_Falloff = new GUIContent("Inner Radius", "Inner Radius: The distance from the center of a brush at which the strength begins to linearly taper to 0.  This value is normalized, 1 means the entire brush gets full strength, 0 means the very center point of a brush is full strength and the edges are 0.\n\nShortcut: 'Shift + Mouse Wheel'");
			gc_FalloffCurve = new GUIContent("Falloff Curve", "Falloff: Sets the Falloff Curve.");
			gc_Strength = new GUIContent("Strength", "Strength: The effectiveness of this brush.  The actual applied strength also depends on the Falloff setting.\n\nShortcut: 'Ctrl + Shift + Mouse Wheel'");
			gc_RadiusMin = new GUIContent("Brush Radius Min", "The minimum value the brush radius slider can access");
			gc_RadiusMax = new GUIContent("Brush Radius Max", "The maximum value the brush radius slider can access");
			gc_AllowUnclampedFalloff = new GUIContent("Unclamped Falloff", "If enabled, the falloff curve will not be limited to values between 0 and 1.");
			gc_BrushSettingsMinMax = new GUIContent("Brush Radius Min / Max", "Set the minimum and maximum brush radius values");
		}

		private bool approx(float lhs, float rhs)
		{
			return Mathf.Abs(lhs-rhs) < .0001f;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			z_GUI.PushGUISkin(z_GUI.PolybrushSkin);

			// Manually show the settings header in z_Editor so that the preset selector can be included in the block
			// if(z_GUILayout.HeaderWithDocsLink(z_GUI.TempContent("Brush Settings")))
			// 	Application.OpenURL("http://procore3d.github.io/polybrush/brushSettings/");

			showSettingsBounds = z_GUILayout.Foldout(showSettingsBounds, gc_BrushSettingsMinMax);

			if(showSettingsBounds)
			{
				z_GUI.PushBackgroundColor(settings_border_color);
				GUILayout.BeginVertical(settingsBackgroundBorderStyle);
				z_GUI.PushBackgroundColor(settings_background_color);
				GUILayout.BeginVertical(settingsBackgroundStyle);
				z_GUI.PopBackgroundColor();
				z_GUI.PopBackgroundColor();

				brushRadiusMin.floatValue = z_GUILayout.FloatField(gc_RadiusMin, brushRadiusMin.floatValue);
				brushRadiusMin.floatValue = Mathf.Clamp(brushRadiusMin.floatValue, .0001f, Mathf.Infinity);

				brushRadiusMax.floatValue = z_GUILayout.FloatField(gc_RadiusMax, brushRadiusMax.floatValue);
				brushRadiusMax.floatValue = Mathf.Clamp(brushRadiusMax.floatValue, brushRadiusMin.floatValue + .001f, Mathf.Infinity);

				allowNonNormalizedFalloff.boolValue = z_GUILayout.Toggle(gc_AllowUnclampedFalloff, allowNonNormalizedFalloff.boolValue);

				GUILayout.EndVertical();
				GUILayout.EndVertical();
			}

			GUILayout.BeginHorizontal();
				GUILayout.Label(gc_Radius, "IconLabel");
				radius.floatValue = GUILayout.HorizontalSlider(radius.floatValue, brushRadiusMin.floatValue, brushRadiusMax.floatValue);
				radius.floatValue = EditorGUILayout.FloatField(radius.floatValue, "textfield", GUILayout.MaxWidth(64));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label(gc_Strength, "IconLabel");
				strength.floatValue = GUILayout.HorizontalSlider(strength.floatValue, 0f, 1f);
				strength.floatValue = EditorGUILayout.FloatField(strength.floatValue, "textfield", GUILayout.MaxWidth(64));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label(gc_Falloff, "IconLabel");
				falloff.floatValue = GUILayout.HorizontalSlider(falloff.floatValue, 0f, 1f);
				falloff.floatValue = EditorGUILayout.FloatField(falloff.floatValue, "textfield", GUILayout.MaxWidth(64));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

				GUILayout.Label(gc_FalloffCurve, "IconLabel");

				if(allowNonNormalizedFalloff.boolValue)
					curve.animationCurveValue = EditorGUILayout.CurveField(curve.animationCurveValue, GUILayout.MinHeight(22));
				else
					curve.animationCurveValue = EditorGUILayout.CurveField(curve.animationCurveValue, Color.green, RECT_ONE, GUILayout.MinHeight(22));
			GUILayout.EndHorizontal();

			Keyframe[] keys = curve.animationCurveValue.keys;

			if( (approx(keys[0].time, 0f) && approx(keys[0].value, 0f) && approx(keys[1].time, 1f) && approx(keys[1].value, 1f)) )
			{
				Keyframe[] rev = new Keyframe[keys.Length];

				for(int i = 0 ; i < keys.Length; i++)
					rev[keys.Length - i -1] = new Keyframe(1f - keys[i].time, keys[i].value, -keys[i].outTangent, -keys[i].inTangent);

				curve.animationCurveValue = new AnimationCurve(rev);
			}

			serializedObject.ApplyModifiedProperties();

			z_GUI.PopGUISkin();

			SceneView.RepaintAll();
		}

		public static z_BrushSettings AddNew()
		{
			string path = z_EditorUtility.FindFolder(z_Pref.ProductName + "/" + "Brush Settings");

			if(string.IsNullOrEmpty(path))
				path = "Assets";

			path = AssetDatabase.GenerateUniqueAssetPath(path + "/New Brush.asset");

			if(!string.IsNullOrEmpty(path))
			{
				z_BrushSettings settings = ScriptableObject.CreateInstance<z_BrushSettings>();
				settings.SetDefaultValues();

				AssetDatabase.CreateAsset(settings, path);
				AssetDatabase.Refresh();

				EditorGUIUtility.PingObject(settings);

				return settings;
			}

			return null;
		}
	}
}
