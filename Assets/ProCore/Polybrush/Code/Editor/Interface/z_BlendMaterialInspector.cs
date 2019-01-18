using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Polybrush;
using System.Reflection;

// Can't namespace material editors.
// namespace Polybrush
// {
	public class z_BlendMaterialInspector : MaterialEditor
	{
		System.Type sf_editor;

		public override void OnEnable()
		{
			base.OnEnable();
			sf_editor = z_ReflectionUtil.GetType("ShaderForge.SF_Editor", "ShaderForge");
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();

			if(sf_editor != null && GUILayout.Button("Open in ShaderForge"))
			{
				try
				{
					SerializedProperty shader_property = serializedObject.FindProperty ("m_Shader");
					Shader shader = (Shader) shader_property.objectReferenceValue;

					string path = AssetDatabase.GetAssetPath(shader);
					string non_modified_path = path.Replace(".shader", z_PostProcessTextureBlend.BLEND_SRC_SUFFIX + ".shader");
					Shader source = AssetDatabase.LoadAssetAtPath<Shader>(non_modified_path);

					z_ReflectionUtil.Invoke(null,
											sf_editor,
											"Init",
											new System.Type[] { typeof(Shader) },
											BindingFlags.Public | BindingFlags.Static,
											new object[] { source }) ;
				}
				catch(System.Exception e)
				{
					Debug.LogWarning("Could not find source for blend shader, or something else went wrong.\n" + e.ToString());
				}
			}

			base.OnInspectorGUI();
		}
	}
// }
