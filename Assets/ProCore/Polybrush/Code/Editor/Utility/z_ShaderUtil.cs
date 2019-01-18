using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Polybrush
{
	/**
	 *	Utility methods for working with shaders.
	 */
	public static class z_ShaderUtil
	{
		/**
		 *	Attempt to read the shader source code to a string.  If source can't be found (built-in shaders are in binary bundles)
		 * 	an empty string is returned.
		 */
		public static string GetSource(Material material)
		{
			if(material == null || material.shader == null)
				return null;

			return GetSource(material.shader);
		}

		public static string GetSource(Shader shader)
		{
			string path = AssetDatabase.GetAssetPath(shader);

			// built-in shaders don't have a valid path.
			if(File.Exists(path))
				return File.ReadAllText( path );
			else
				return string.Empty;
		}

		/**
		 *	Returns true if shader has a COLOR attribute.
		 */
		public static bool SupportsVertexColors(Shader source)
		{
			return SupportsVertexColors(GetSource(source));
		}

		public static bool SupportsVertexColors(string source)
		{
			return Regex.Match(source, "float4\\s.*\\s:\\sCOLOR;").Success;
		}

		/**
		 *	Parse the shader source for a Z_SHADER_METADATA line with the path
		 * 	to the shader's polybrush metadata.  Path should be relative to the
		 *	directory of the shader.
		 */
		public static string GetMetaDataPath(Shader shader)
		{
			string src = z_ShaderUtil.GetSource(shader);

			if(!string.IsNullOrEmpty(src))
			{
				Match match = Regex.Match(src, "(?<=Z_SHADER_METADATA).*");

				if(match.Success)
				{
					string res = match.Value.Trim();
					res = res.Replace(".pbs", "");
					res = res.Replace(".shader", "");
					return res;
				}
			}

			return null;
		}

		/**
		 *	Loads z_AttributeLayout data from a shader.  Checks for both legacy (define Z_TEXTURE_CHANNELS) and
		 *	.pbs.json metadata.
		 */
		public static bool GetMeshAttributes(Material material, out z_AttributeLayoutContainer attribContainer)
		{
			attribContainer = null;

			if(material == null)
				return false;

			// first search for json, then fall back on legacy
			if( z_EditorUtility.FindMeshAttributesForShader(material.shader, out attribContainer) )
			{
				Dictionary<string, int> shaderProperties = new Dictionary<string, int>();

				for(int i = 0; i < ShaderUtil.GetPropertyCount(material.shader); i++)
					shaderProperties.Add(ShaderUtil.GetPropertyName(material.shader, i), i);

				foreach(z_AttributeLayout a in attribContainer.attributes)
				{
					int index = -1;

					if(shaderProperties.TryGetValue(a.propertyTarget, out index))
					{
						if(ShaderUtil.GetPropertyType(material.shader, index) == ShaderUtil.ShaderPropertyType.TexEnv)
							a.previewTexture = (Texture2D) material.GetTexture(a.propertyTarget);
					}
				}

				return true;
			}

			z_AttributeLayout[] attribs;

			if( GetMeshAttributes_Legacy(material, out attribs) )
			{
				attribContainer = z_AttributeLayoutContainer.Create(material.shader, attribs);
				return true;
			}

			return false;
		}

		// legacy route (define Z_TEXTURE_CHANNELS X)
		// doesn't support masks or different ranges.
		public static bool GetMeshAttributes_Legacy(Material material, out z_AttributeLayout[] attributes)
		{
			attributes = null;

			string src = GetSource(material);

			if(string.IsNullOrEmpty(src))
				return false;

			int expectedTextureCount = GetTextureChannelCount(src);

			if(expectedTextureCount < 1)
				return false;

			z_MeshChannel[] channels = GetUsedMeshAttributes(src);

			if(channels == null)
				return false;

			string[] textureProperties;

			Texture2D[] textures;

			GetBlendTextures(material, src, expectedTextureCount, out textures, out textureProperties);

			attributes = new z_AttributeLayout[expectedTextureCount];

			for(int i = 0; i < expectedTextureCount; i++)
				attributes[i] = new z_AttributeLayout(
					channels[i / 4],
					(z_ComponentIndex)(i % 4),
					z_AttributeLayout.NormalizedRange,
					z_AttributeLayout.DefaultMask,
					textureProperties[i],
					textures[i]);

			return true;
		}

		/**
		 *	True if the shader likely supports texture blending.
		 */
		public static bool SupportsTextureBlending(Shader shader)
		{
			string src = GetSource(shader);
			return GetTextureChannelCount(src) > 0;
		}

		/**
		 *	Get projected number of texture channels from Z_MESH_CHANNELS define.
		 */
		public static int GetTextureChannelCount(string src)
		{
			// old method: define Z_TEXTURE_CHANNELS 4
			string pattern = "(?<=" + z_PostProcessTextureBlend.TEXTURE_CHANNEL_DEF + ")[0-9]{1,2}";

			Match match = Regex.Match(src, pattern);

			int val = -1;

			if(match.Success)
				int.TryParse(match.Value, out val);

			return val;
		}

		/**
		 *	Returns the shader defined mesh attributes used.  Null if no attributes are defined.
		 */
		public static z_MeshChannel[] GetUsedMeshAttributes(Material material)
		{
			try
			{
				return GetUsedMeshAttributes( GetSource(material.shader) );
			}
			catch(System.Exception e)
			{
				Debug.LogWarning("Failed reading mesh attributes.\n" + e.ToString());
				return null;
			}
		}

		public static z_MeshChannel[] GetUsedMeshAttributes(string src)
		{
			// old method
			string pattern = "(?<=" + z_PostProcessTextureBlend.MESH_ATTRIBS_DEF + ").*?(?=,|\\r\\n|\\n)";

			Match match = Regex.Match(src, pattern);

			if(match.Success)
			{
				string[] attribs = match.Value.Trim().Split(' ');
				z_MeshChannel[] channels = attribs.Select(x => z_MeshChannelUtility.StringToEnum(x))
													.Where(y => y != z_MeshChannel.Null).ToArray();
				return channels;
			}

			return null;
		}

		/**
		 *	Tries to extract texture channels from a blend material, ignoring bump/normal images.
		 *	This function always returns an array of size expectedTextureCount, filling in NULL
		 *	where an image is not loaded.
		 */
		private static int GetBlendTextures(Material material, string src, int expectedTextureCount, out Texture2D[] textures, out string[] textureProperties)
		{
			MatchCollection non_bump_textures = Regex.Matches(src, "_.*?\\s\\(\".*?\", 2D\\)\\s=\\s\"[^(bump)|(gray)]*?\"");

			textureProperties = new string[ expectedTextureCount ];
			textures = new Texture2D[ expectedTextureCount ];

			int i = 0, found = 0;

			foreach(Match m in non_bump_textures)
			{
				int space = m.Value.IndexOf(" ");

				if(space < 0)
				{
					if(i < expectedTextureCount)
					{
						textures[i] = null;
						textureProperties[i] = m.Value;
					}
				}
				else
				{
					string prop = m.Value.Substring(0, space);

					if( material.HasProperty(prop) )
					{
						found++;

						if(i < expectedTextureCount)
						{
							textures[i] = material.GetTexture(prop) as Texture2D;
							textureProperties[i] = prop;
						}
					}
					else
					{
						if(i < expectedTextureCount)
						{
							textures[i] = null;
							textureProperties[i] = prop;
						}
					}
				}

				i++;
			}

			return found;
		}
	}
}
