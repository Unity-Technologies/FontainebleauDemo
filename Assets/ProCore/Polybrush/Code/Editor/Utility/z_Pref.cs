using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Polybrush
{
	/**
	 *	Editor preferences and defaults.
	 */
	public static class z_Pref
	{
		public const string ProductName = "Polybrush";
		public static string DocumentationLink { get { return "http://procore3d.github.io/polybrush"; } }
		public const string ContactLink = "mailto:contact@procore3d.com";
		public const string WebsiteLink = "http://www.procore3d.com";

		public const string POLYBRUSH_VERSION = "0.9.9b2";

		public const string floatingEditorWindow 			= "z_pref_floatingEditorWindow";
		public const string lockBrushToFirst 				= "z_pref_lockBrushToFirst";
		public const string rebuildNormals 					= "z_pref_rebuildNormals";
		public const string rebuildCollisions 				= "z_pref_rebuildCollisions";
		public const string hideWireframe 					= "z_pref_hideWireframe";
		public const string lockBrushSettings 				= "z_pref_lockBrushSettings";
		public const string ignoreUnselected 				= "z_Pref_ignoreUnselected";

		public const string brushColor 						= "z_pref_brushColor";
		public const string brushGradient 					= "z_pref_brushGradient";
		public const string vertexBillboardSize				= "z_pref_vertexBillboardSize";
		public const string brushNormalIsSticky 			= "z_pref_brushNormalIsSticky";

		public const string sculptDirection 				= "z_pref_sculptDirection";
		public const string raiseLowerDirection 			= "pushpull_brush_dir";
		public const string smoothDirection		 			= "smooth_brush_dir";
		public const string additionalVertexStreams 		= "z_pref_additionalVertexStreams";
		public const string pushPullEffect 					= "push_pull_effect";

		private static z_PreferenceDictionary _preferences = null;

		public static z_PreferenceDictionary preferences
		{
			get
			{
				if(_preferences == null)
					_preferences = z_EditorUtility.GetDefaultAsset<z_PreferenceDictionary>("Settings.asset");
				return _preferences;
			}
		}

		/**
		 *	Check if the last opened version of Polybrush matches this one.  Returns false if it doesn't.

		 */
		public static bool VersionCheck()
		{
			if( !EditorPrefs.GetString("z_pref_version", "null").Equals(z_Pref.POLYBRUSH_VERSION) )
			{
				EditorPrefs.SetString("z_pref_version", z_Pref.POLYBRUSH_VERSION);
				return false;
			}
			return true;
		}

		// [MenuItem("Tools/Clear Preferences")]
		public static void ClearPrefs()
		{
			EditorPrefs.DeleteKey(floatingEditorWindow);
			EditorPrefs.DeleteKey(lockBrushToFirst);
			EditorPrefs.DeleteKey(rebuildNormals);
			EditorPrefs.DeleteKey(rebuildCollisions);
			EditorPrefs.DeleteKey(hideWireframe);
			EditorPrefs.DeleteKey(lockBrushSettings);
			EditorPrefs.DeleteKey(ignoreUnselected);
			EditorPrefs.DeleteKey(brushColor);
			EditorPrefs.DeleteKey(brushGradient);
			EditorPrefs.DeleteKey(sculptDirection);
			EditorPrefs.DeleteKey(raiseLowerDirection);
			EditorPrefs.DeleteKey(smoothDirection);
			EditorPrefs.DeleteKey(vertexBillboardSize);
			EditorPrefs.DeleteKey(additionalVertexStreams);

			preferences.SetDefaultValues();
		}

		static readonly Dictionary<string, float> FloatDefaults = new Dictionary<string, float>()
		{
			{ vertexBillboardSize, 2f },
			{ pushPullEffect, 5f }
		};

		static readonly Dictionary<string, bool> BoolDefaults = new Dictionary<string, bool>()
		{
			{ floatingEditorWindow, false },
			{ lockBrushToFirst, true },
			{ rebuildNormals, true },
			{ rebuildCollisions, true },
			{ hideWireframe, true },
			{ lockBrushSettings, false },
			{ ignoreUnselected, false },
			{ brushNormalIsSticky, true },
			{ additionalVertexStreams, true }
		};

		static readonly Dictionary<string, Color> ColorDefaults = new Dictionary<string, Color>()
		{
			{ brushColor, new Color(0f, .8f, 1f, 1f) }
		};

		static readonly Dictionary<string, int> EnumDefaults = new Dictionary<string, int>()
		{
			{ sculptDirection, (int) z_Direction.VertexNormal },
			{ smoothDirection, (int) z_Direction.VertexNormal },
			{ raiseLowerDirection, (int) z_Direction.VertexNormal }
		};

		public static bool HasKey(string key)
		{
			return preferences.HasKey(key);
		}

		public static bool GetBool(string key, bool fallback = true)
		{
			if(preferences.HasKey(key))
				return preferences.GetBool(key);
			else if( BoolDefaults.ContainsKey(key) )
				return BoolDefaults[key];
			else
				return fallback;
		}

		public static void SetBool(string key, bool value)
		{
			preferences.SetBool(key, value);
		}

		public static Color GetColor(string key, Color fallback = default(Color))
		{
			return preferences.GetColor(key, ColorDefaults.ContainsKey(key) ? ColorDefaults[key] : fallback);
		}

		public static void SetColor(string key, Color value)
		{
			preferences.SetColor(key, value);
		}

		public static int GetInt(string key, int fallback = 0)
		{
			return preferences.GetInt(key, EnumDefaults.ContainsKey(key) ? EnumDefaults[key] : fallback);
		}

		public static void SetInt(string key, int value)
		{
			preferences.SetInt(key, value);
		}

		public static float GetFloat(string key, float fallback = 1f)
		{
			return preferences.GetFloat(key, FloatDefaults.ContainsKey(key) ? FloatDefaults[key] : fallback);
		}

		public static void SetFloat(string key, float value)
		{
			preferences.SetFloat(key, value);
		}

		const string DEFAULT_GRADIENT = "RGBA(0.227, 1.000, 0.227, 255.000)&0.000|RGBA(1.000, 1.000, 1.000, 255.000)&1.000|\n1.0000&0.000|0.2588&1.000|";

		public static Gradient GetGradient(string key)
		{
			Gradient gradient;

			if( z_GradientSerializer.Deserialize(preferences.GetString(key, DEFAULT_GRADIENT), out gradient) )
			{
				return gradient;
			}
			else
			{
				gradient.SetKeys(
					new GradientColorKey[] {
						new GradientColorKey(new Color(.1f, 0f, 1f, 1f), 0f),
						new GradientColorKey(Color.black, 1f)
						},
					new GradientAlphaKey[] {
						new GradientAlphaKey(1f, 0f),
						new GradientAlphaKey(1f, 1f),
						});
			}

			return gradient;
		}

		public static void SetGradient(string key, Gradient gradient)
		{
			preferences.SetString(key, z_GradientSerializer.Serialize(gradient));
		}
	}
}
