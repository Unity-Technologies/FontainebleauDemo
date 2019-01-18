using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Polybrush
{
	/**
	 *	Provides a quick and dirty path to access a uv channels as Vector4 in ShaderForge
	 */
	public class z_PostProcessTextureBlend : AssetPostprocessor
	{
		// any shader file with this suffix will be run through the post-processor
		public const string BLEND_SRC_SUFFIX_OLD = "_SfTexBlendSrc.shader";
		public const string BLEND_SRC_SUFFIX_FILE = "_SfSrc.shader";
		public const string BLEND_SRC_SUFFIX = "_SfSrc";

		public const string TEXTURE_CHANNEL_DEF = "define Z_TEXTURE_CHANNELS ";

		public const string MESH_ATTRIBS_DEF = "define Z_MESH_ATTRIBUTES ";

		static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			string blendShaderPath = importedAssets.FirstOrDefault(x => x.EndsWith(BLEND_SRC_SUFFIX_FILE) || x.EndsWith(BLEND_SRC_SUFFIX_OLD));

			if(!string.IsNullOrEmpty(blendShaderPath))
				ProcessShader(blendShaderPath);
		}

		static void ProcessShader(string path)
		{
			string source = File.ReadAllText( path );

			// remove sf metadata
			string[] split = source.Split(Environment.NewLine.ToCharArray());

			int index = Array.FindIndex(split, x => x.StartsWith("/*SF_DATA"));

			if(index < 0)
				return;

			int channelCount = z_ShaderUtil.GetTextureChannelCount(source);
			
			if(channelCount < 1)
				Debug.LogWarning("ShaderForge created shader does not contain a \"" + TEXTURE_CHANNEL_DEF + "\" comment.");

			z_MeshChannel[] attribs = z_ShaderUtil.GetUsedMeshAttributes(source);

			if(attribs == null || attribs.Length < 1)
				Debug.LogWarning("ShaderForge created shader does not contain a \"" + MESH_ATTRIBS_DEF + "\" comment.");

			source = string.Join(Environment.NewLine, split.Skip(index + 1).ToArray());

			string header = string.Format("// {1}{0}// {2}{0}// Important!  This is a generated file, any changes will be overwritten{0}// when the _SfSrc suffixed version of this shader is modified.{0}{0}",
				System.Environment.NewLine,
				TEXTURE_CHANNEL_DEF + channelCount,
				MESH_ATTRIBS_DEF + attribs.ToString(" "));
			
			source = source.Insert(0, header);
			source = Regex.Replace(source, "Shader\\s\"Hidden/", "Shader \"");
			source = Regex.Replace(source, "CustomEditor\\s\\\"ShaderForgeMaterialInspector\\\"", "CustomEditor \"z_BlendMaterialInspector\"");

			// enable vec4 uv channels
			source = Regex.Replace(source, "uniform float4 _TEMP_CHANNEL_UV[0-9];", "");
			source = Regex.Replace(source, "_TEMP_CHANNEL_UV[0-9]\\s\\(\"TEMP_CHANNEL_UV[0-9]\", Vector\\)\\s=\\s\\([\\d*\\.?\\d*,?]*\\)", "");
			source = Regex.Replace(source, "_TEMP_CHANNEL_UV(?=[0-9])", "i.uv");
			source = Regex.Replace(source, "float[0-9](?=\\suv[0-9])", "float4");
			source = Regex.Replace(source, "float[0-9](?=\\stexcoord[0-9])", "float4");

			string regexed = path.Replace(BLEND_SRC_SUFFIX, "");

			File.WriteAllText(regexed, source);

			AssetDatabase.Refresh();

			EditorGUIUtility.PingObject( AssetDatabase.LoadAssetAtPath<Shader>(regexed) );
		}
	}
}
