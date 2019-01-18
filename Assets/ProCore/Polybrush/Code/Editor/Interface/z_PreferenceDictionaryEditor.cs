using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Polybrush
{
	[CustomEditor(typeof(z_PreferenceDictionary))]
	public class z_PreferenceDictionaryEditor : Editor
	{
		static Color RowEven = new Color(.40f, .40f, .40f, .3f);
		static Color RowOdd  = new Color(.60f, .60f, .60f, .3f);

		bool	showBool = true,
				showInt = true,
				showFloat = true,
				showString = true,
				showColor = true;

		Vector2 scroll = Vector2.zero;

		public override void OnInspectorGUI()
		{
			if(target == null)
				return;

			z_PreferenceDictionary dic = target as z_PreferenceDictionary;

			if(dic == null)
				return;

			Dictionary<string, bool> 		m_bool		= (Dictionary<string, bool>) 		z_ReflectionUtil.GetValue(dic, typeof(z_PreferenceDictionary), "m_bool");
			Dictionary<string, int> 		m_int		= (Dictionary<string, int>)	 		z_ReflectionUtil.GetValue(dic, typeof(z_PreferenceDictionary), "m_int");
			Dictionary<string, float> 		m_float		= (Dictionary<string, float>)		z_ReflectionUtil.GetValue(dic, typeof(z_PreferenceDictionary), "m_float");
			Dictionary<string, string> 		m_string	= (Dictionary<string, string>)		z_ReflectionUtil.GetValue(dic, typeof(z_PreferenceDictionary), "m_string");
			Dictionary<string, Color> 		m_Color		= (Dictionary<string, Color>)		z_ReflectionUtil.GetValue(dic, typeof(z_PreferenceDictionary), "m_Color");

			scroll = EditorGUILayout.BeginScrollView(scroll);

			GUILayout.Label("Bool Values", EditorStyles.boldLabel);

			int i = 0;

			if(showBool)
			{
				foreach(var kvp in m_bool)
				{
					GUI.backgroundColor = i++ % 2 == 0 ? RowEven : RowOdd;
					GUILayout.BeginHorizontal(z_GUI.backgroundColorStyle);
					GUILayout.Label(kvp.Key);
					GUILayout.FlexibleSpace();
					GUILayout.Label(kvp.Value.ToString());
					GUILayout.EndHorizontal();
				}
				GUI.backgroundColor = Color.white;
			}

			GUILayout.Label("Int Values", EditorStyles.boldLabel);

			if(showInt)
			{
				foreach(var kvp in m_int)
				{
					GUI.backgroundColor = i++ % 2 == 0 ? RowEven : RowOdd;
					GUILayout.BeginHorizontal(z_GUI.backgroundColorStyle);
					GUILayout.Label(kvp.Key);
					GUILayout.FlexibleSpace();
					GUILayout.Label(kvp.Value.ToString());
					GUILayout.EndHorizontal();
				}
				GUI.backgroundColor = Color.white;
			}

			GUILayout.Label("Float Values", EditorStyles.boldLabel);

			if(showFloat)
			{
				foreach(var kvp in m_float)
				{
					GUI.backgroundColor = i++ % 2 == 0 ? RowEven : RowOdd;
					GUILayout.BeginHorizontal(z_GUI.backgroundColorStyle);
					GUILayout.Label(kvp.Key);
					GUILayout.FlexibleSpace();
					GUILayout.Label(kvp.Value.ToString());
					GUILayout.EndHorizontal();
				}
				GUI.backgroundColor = Color.white;
			}

			GUILayout.Label("String Values", EditorStyles.boldLabel);

			if(showString)
			{
				foreach(var kvp in m_string)
				{
					GUI.backgroundColor = i++ % 2 == 0 ? RowEven : RowOdd;
					GUILayout.BeginHorizontal(z_GUI.backgroundColorStyle);
					GUILayout.Label(kvp.Key);
					GUILayout.FlexibleSpace();
					GUILayout.Label(kvp.Value.ToString());
					GUILayout.EndHorizontal();
				}
				GUI.backgroundColor = Color.white;
			}

			GUILayout.Label("Color Values", EditorStyles.boldLabel);

			if(showColor)
			{
				foreach(var kvp in m_Color)
				{
					GUI.backgroundColor = i++ % 2 == 0 ? RowEven : RowOdd;
					GUILayout.BeginHorizontal(z_GUI.backgroundColorStyle);
					GUILayout.Label(kvp.Key);
					GUILayout.FlexibleSpace();
					GUILayout.Label(kvp.Value.ToString());
					GUILayout.EndHorizontal();
				}
				GUI.backgroundColor = Color.white;
			}

			EditorGUILayout.EndScrollView();
		}
	}
}
