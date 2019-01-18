using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace Polybrush
{
	[InitializeOnLoad]
	public static class z_IconUtility
	{
		const string ICON_FOLDER_PATH = "Polybrush/Icons";
		private static string iconFolderPath = "Assets/ProCore/Polybrush/Icons/";
		private static Dictionary<string, Texture2D> m_icons = new Dictionary<string, Texture2D>();

		static z_IconUtility()
		{
			if(!Directory.Exists(iconFolderPath))
			{
				string folder = FindFolder(ICON_FOLDER_PATH);

				if(Directory.Exists(folder))
					iconFolderPath = folder;
			}
		}

		private static string FindFolder(string folder)
		{
			string single = folder.Replace("\\", "/").Substring(folder.LastIndexOf('/') + 1);

			string[] matches = Directory.GetDirectories("Assets/", single, SearchOption.AllDirectories);

			foreach(string str in matches)
			{
				string path = str.Replace("\\", "/");

				if(path.Contains(folder))
				{
					if(!path.EndsWith("/"))
						path += "/";

					return path;
				}
			}

			Debug.LogError(string.Format("Could not locate \"{0}\" folder.  The Polybrush folder can be moved, but the contents of this folder must remain unmodified.", folder));

			return null;
		}

		public static Texture2D GetIcon(string iconName)
		{
			return GetTextureInFolder(iconFolderPath, iconName);
		}

		public static Texture2D GetTextureInFolder(string folder, string name)
		{
			int ext = name.LastIndexOf('.');
			string nameWithoutExtension = ext < 0 ? name : name.Substring(0, ext);
			Texture2D icon = null;

			if(!m_icons.TryGetValue(nameWithoutExtension, out icon))
			{
				string fullPath = string.Format("{0}{1}.png", folder, nameWithoutExtension);

				icon = (Texture2D) AssetDatabase.LoadAssetAtPath(fullPath, typeof(Texture2D));

				if(icon == null)
				{
// #if Z_DEBUG
					Debug.LogWarning("failed to find icon: " + fullPath);
// #endif
					m_icons.Add(nameWithoutExtension, null);
					return null;
				}

				m_icons.Add(nameWithoutExtension, icon);
			}

			return icon;
		}
	}
}
