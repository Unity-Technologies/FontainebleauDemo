using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Polybrush
{
	/// <summary>
	/// Static helper methods for working with reflection.  Mostly used for ProBuilder compatibility.
	/// </summary>
	static class z_ReflectionUtil
	{
		static EditorWindow m_PbEditor = null;
		public static bool enableWarnings = true;
		const BindingFlags k_AllFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		static MethodInfo m_ProBuilderOptimize = null;
		static MethodInfo m_ProBuilderRefreshNoArgs = null;
		static MethodInfo m_ProBuilderRefresh = null;
		static MethodInfo m_ProBuilderToMeshNoArgs = null;
		static Type m_ProBuilderObjectType = null;
		static Type m_ProBuilderEditorType = null;
		static object[] m_RefreshArgs = new object[1];
		static Dictionary<string, Type> m_CachedTypes = new Dictionary<string, Type>();

		static readonly string[] k_EditorMeshUtilityTypeNames = new string[]
		{
			"ProBuilder.EditorCore.pb_EditorMeshUtility",
			"ProBuilder2.EditorCommon.pb_EditorMeshUtility",
			"ProBuilder2.EditorCommon.pb_Editor_Mesh_Utility",
		};

		static readonly string[] k_EditorTypeNames = new string[]
		{
			"ProBuilder.EditorCore.pb_Editor",
			"ProBuilder2.EditorCommon.pb_Editor",
		};

		static readonly string[] k_PbObjectNames = new string[]
		{
			"pb_Object",
			"ProBuilder.Core.pb_Object"
		};

		internal static Type ProBuilderObjectType
		{
			get
			{
				for (int i = 0, c = k_PbObjectNames.Length; i < c && m_ProBuilderObjectType == null; i++)
					m_ProBuilderObjectType = GetTypeCached(k_PbObjectNames[i]);

				return m_ProBuilderObjectType;
			}
		}

		internal static Type ProBuilderEditorType
		{
			get
			{
				for (int i = 0, c = k_EditorTypeNames.Length; i < c && m_ProBuilderEditorType == null; i++)
					m_ProBuilderEditorType = GetTypeCached(k_EditorTypeNames[i]);

				return m_ProBuilderEditorType;
			}
		}

		static void Warning(string text)
		{
			if(enableWarnings)
				Debug.LogWarning(text);
		}

		/// <summary>
		/// Reference to the ProBuilder Editor window if it is avaiable.
		/// </summary>
		internal static EditorWindow ProBuilderEditorWindow
		{
			get
			{
				if(m_PbEditor == null)
				{
					EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
					m_PbEditor = windows.FirstOrDefault(x => x.GetType().ToString().Contains("pb_Editor"));
				}
				return m_PbEditor;
			}
		}

		/// <summary>
		/// Tests if ProBuilder is available in the project.
		/// </summary>
		/// <returns></returns>
		internal static bool ProBuilderExists()
		{
			return ProBuilderObjectType != null;
		}

		/// <summary>
		/// Tests if a GameObject is a ProBuilder mesh or not.
		/// </summary>
		/// <param name="gameObject"></param>
		/// <returns></returns>
		internal static bool IsProBuilderObject(GameObject gameObject)
		{
			return gameObject != null && gameObject.GetComponent("pb_Object") != null;
		}

		/// <summary>
		/// Get a component with type name.
		/// </summary>
		/// <param name="gameObject"></param>
		/// <param name="componentTypeName"></param>
		/// <returns></returns>
		internal static object GetComponent(this GameObject gameObject, string componentTypeName)
		{
			return gameObject.GetComponent(componentTypeName);
		}

		/// <summary>
		/// Fetch a type with name and optional assembly name.  `type` should include namespace.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="assembly"></param>
		/// <returns></returns>
		public static Type GetType(string type, string assembly = null)
		{
			Type t = Type.GetType(type);

			if(t == null)
			{
				IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();

				if(assembly != null)
					assemblies = assemblies.Where(x => x.FullName.Contains(assembly));

				foreach(Assembly ass in assemblies)
				{
					t = ass.GetType(type);

					if(t != null)
						return t;
				}
			}

			return t;
		}

		/// <summary>
		/// Same as GetType except this function caches the result for quick lookups.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="assembly"></param>
		/// <returns></returns>
		static Type GetTypeCached(string type, string assembly = null)
		{
			Type res = null;

			if( m_CachedTypes.TryGetValue(type, out res) )
				return res;

			res = GetType(type);
			m_CachedTypes.Add(type, res);

			return res;
		}

		/// <summary>
		/// Call a method with args.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="method"></param>
		/// <param name="flags"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static object Invoke(object target,
									string method,
									BindingFlags flags = k_AllFlags,
									params object[] args)
		{
			if(target == null)
			{
				Warning("Invoke failed, target is null and no type was provided.");
				return null;
			}

			return Invoke(target, target.GetType(), method, null, flags, args);
		}

		public static object Invoke(object target,
									string type,
									string method,
									BindingFlags flags = k_AllFlags,
									string assembly = null,
									params object[] args)
		{
			Type t = GetType(type, assembly);

			if(t == null && target != null)
				t = target.GetType();

			if(t != null)
				return Invoke(target, t, method, null, flags, args);
			else
				Warning("Invoke failed, type is null: " + type);

			return null;
		}

		public static object Invoke(object target,
									Type type,
									string method,
									Type[] methodParams = null,
									BindingFlags flags = k_AllFlags,
									params object[] args)
		{
			MethodInfo mi = null;

			if(methodParams == null)
				mi = type.GetMethod(method, flags);
			else
				mi = type.GetMethod(method, flags, null, methodParams, null);

			if(mi == null)
			{
				Warning("Failed to find method " + method + " in type " + type);
				return null;
			}

			return mi.Invoke(target, args);
		}

		/// <summary>
		/// Fetch a value using GetProperty or GetField.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <param name="member"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static object GetValue(object target, string type, string member, BindingFlags flags = k_AllFlags)
		{
			Type t = GetType(type);

			if(t == null)
			{
				Warning(string.Format("Could not find type \"{0}\"!", type));
				return null;
			}
			else
				return GetValue(target, t, member, flags);
		}

		public static object GetValue(object target, Type type, string member, BindingFlags flags = k_AllFlags)
		{
			PropertyInfo pi = type.GetProperty(member, flags);

			if(pi != null)
				return pi.GetValue(target, null);

			FieldInfo fi = type.GetField(member, flags);

			if(fi != null)
				return fi.GetValue(target);

			Warning(string.Format("Could not find member \"{0}\" matching type {1}!", member, type));

			return null;
		}

		public static bool SetValue(object target, string member, object value, BindingFlags flags = k_AllFlags)
		{
			if(target == null)
				return false;

			PropertyInfo pi = target.GetType().GetProperty(member, flags);

			if(pi != null)
				pi.SetValue(target, value, flags, null, null, null);

			FieldInfo fi = target.GetType().GetField(member, flags);

			if(fi != null)
				fi.SetValue(target, value);

			return pi != null || fi != null;
		}

		public static MethodInfo ProBuilder_OptimizeMethodInfo()
		{
			if(m_ProBuilderOptimize == null)
			{
				Type editorMeshUtilityType = null;

				for(int i = 0, c = k_EditorMeshUtilityTypeNames.Length; i < c && editorMeshUtilityType == null; i++)
					editorMeshUtilityType = z_ReflectionUtil.GetType(k_EditorMeshUtilityTypeNames[i]);

				if(editorMeshUtilityType != null)
				{
					// 2.5.1
					m_ProBuilderOptimize = editorMeshUtilityType.GetMethod("Optimize",
						BindingFlags.Public | BindingFlags.Static,
						null,
						new Type[] { ProBuilderObjectType, typeof(bool) },
						null );

					if(m_ProBuilderOptimize == null)
					{
						m_ProBuilderOptimize = editorMeshUtilityType.GetMethod("Optimize",
							BindingFlags.Public | BindingFlags.Static,
							null,
							new Type[] { ProBuilderObjectType },
							null );
					}
				}
			}

			return m_ProBuilderOptimize;
		}

		/// <summary>
		/// Fallback for ProBuilder 2.6.1 and lower (Refresh() with no params).
		/// </summary>
		/// <returns></returns>
		static MethodInfo ProBuilder_RefreshMethodInfo()
		{
			if(m_ProBuilderRefreshNoArgs == null)
			{
				m_ProBuilderRefreshNoArgs = ProBuilderObjectType.GetMethod(
					"Refresh",
					BindingFlags.Public | BindingFlags.Instance);
			}

			return m_ProBuilderRefreshNoArgs;
		}

		static MethodInfo ProBuilder_RefreshWithMaskMethodInfo()
		{
			if(m_ProBuilderRefresh == null)
			{
				Type refreshMaskType = GetTypeCached("ProBuilder.Core.RefreshMask");

				if(refreshMaskType == null)
					refreshMaskType = GetTypeCached("ProBuilder2.Common.RefreshMask");

				if(refreshMaskType == null)
					return null;

				m_ProBuilderRefresh = ProBuilderObjectType.GetMethod(
					"Refresh",
					BindingFlags.Public | BindingFlags.Instance,
					null,
					new Type[] { refreshMaskType },
					null);
			}

			return m_ProBuilderRefresh;
		}

		/// <summary>
		/// Calls pb_EditorUtility.Optimize
		/// </summary>
		/// <param name="pb"></param>
		public static void ProBuilder_Optimize(object pb)
		{
			MethodInfo mi = ProBuilder_OptimizeMethodInfo();

			if(mi == null)
				return;

			ParameterInfo[] pi = mi.GetParameters();

			if(pi == null)
				return;

			object[] args = new object[pi.Length];

			args[0] = pb;

			// HasDefaultValue not available until .NET 4.5
			for(int i = 1; i < pi.Length; i++)
				args[i] = pi[i].DefaultValue;

			mi.Invoke(null, args);
		}

		public static void ProBuilder_Refresh(object pb, ushort mask = 0xFF)
		{
			MethodInfo refresh = ProBuilder_RefreshWithMaskMethodInfo();

			if(refresh != null)
			{
				m_RefreshArgs[0] = mask;
				refresh.Invoke(pb, m_RefreshArgs);
			}
			else
			{
				refresh = ProBuilder_RefreshMethodInfo();

				if(refresh != null)
					refresh.Invoke(pb, null);
				else
					Debug.LogWarning("ProBuilder_Refresh failed to find an appropriate `Refresh` method on `pb_Object` type");
			}
		}

		static MethodInfo ProBuilderToMeshNoArgsMethodInfo
		{
			get
			{
				if (m_ProBuilderToMeshNoArgs == null)
				{
					m_ProBuilderToMeshNoArgs = ProBuilderObjectType.GetMethod(
						"ToMesh",
						BindingFlags.Public | BindingFlags.Instance,
						null,
						Type.EmptyTypes,
						null);
				}

				return m_ProBuilderToMeshNoArgs;
			}
		}

		public static void ProBuilder_ToMesh(object pb, MeshTopology topology = MeshTopology.Quads)
		{
			if (ProBuilderToMeshNoArgsMethodInfo == null)
			{
				Debug.LogWarning("ProBuilder_ToMesh failed to find an appropriate `ToMesh` method on `pb_Object` type");
				return;
			}

			ProBuilderToMeshNoArgsMethodInfo.Invoke(pb, null);
		}
	}
}
