using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Polybrush
{
	public class z_ImportIcons : AssetPostprocessor
	{
		// Path to icons relative to ProCore root folder.
		const string ICON_PATH = "Polybrush/Icons";

		public void OnPreprocessTexture()
		{
			if( assetPath.IndexOf(ICON_PATH) < 0 )
				return;

			TextureImporter ti = (TextureImporter) assetImporter;

			ti.textureType = TextureImporterType.Default;
			ti.textureCompression = TextureImporterCompression.Uncompressed;
			ti.sRGBTexture = true;
			ti.npotScale = TextureImporterNPOTScale.None;
			ti.filterMode = FilterMode.Point;
			ti.wrapMode = TextureWrapMode.Clamp;
			ti.mipmapEnabled = false;
			ti.maxTextureSize = 64;
		}

		public void OnPostprocessTexture(Texture2D texture)
		{
			/**
			 *	GUIStyle background generators
			 */
			if(	assetPath.IndexOf("Style/") > 0 &&
				assetPath.IndexOf("Generated/") < 0 &&
				assetPath.IndexOf("Style/Special") < 0)
			{
				string dir = System.IO.Path.GetDirectoryName(assetPath);
				string name = System.IO.Path.GetFileNameWithoutExtension (assetPath);
				string gen_dir_light = string.Format("{0}/Generated/Light/", dir);
				string gen_dir_dark = string.Format("{0}/Generated/Dark/", dir);

				if(!System.IO.Directory.Exists(string.Format(gen_dir_light)))
					System.IO.Directory.CreateDirectory(gen_dir_light);

				if(!System.IO.Directory.Exists(string.Format(gen_dir_dark)))
					System.IO.Directory.CreateDirectory(gen_dir_dark);

				// Normal is the default state
				// OnNormal is when the toggle state is 'On' (ex, a toolbar)
				// Active is the button down state (ex, mouse clicking)

				// PRO SKIN COLORS
				TintAndReimport(texture, string.Format("{0}{1}_Normal.png", gen_dir_dark, name), ColorFromHex(0x4e4e4e));
				TintAndReimport(texture, string.Format("{0}{1}_Active.png", gen_dir_dark, name), ColorFromHex(0x636363));
				TintAndReimport(texture, string.Format("{0}{1}_OnNormal.png", gen_dir_dark, name), ColorFromHex(0x252525));
				// TintAndReimport(texture, string.Format("{0}{1}_Active.png", gen_dir_dark, name), ColorFromHex(0x3F3F3F));

				// LIGHT SKIN COLORS
				TintAndReimport(texture, string.Format("{0}{1}_Normal.png", gen_dir_light, name), ColorFromHex(0xE7E7E7));
				TintAndReimport(texture, string.Format("{0}{1}_OnNormal.png", gen_dir_light, name), ColorFromHex(0xACACAC));
				TintAndReimport(texture, string.Format("{0}{1}_Active.png", gen_dir_light, name), ColorFromHex(0x929292));
			}
		}

		private void TintAndReimport(Texture2D source, string path, Color tint)
		{
			Texture2D tinted = new Texture2D(source.width, source.height);

			Color[] pix = source.GetPixels();

			for(int i = 0; i < pix.Length; i++)
				pix[i] *= tint;

			tinted.SetPixels(pix);
			tinted.Apply();

			byte[] bytes = tinted.EncodeToPNG();
			System.IO.File.WriteAllBytes(path, bytes);
		}

		private Color ColorFromHex(uint hex)
		{
			return new Color32(
				(byte) ((hex >> 16) & 0xFF),
				(byte) ((hex >> 8) & 0xFF),
				(byte) (hex & 0xFF),
				(byte) 0xFF);
		}
	}
}
