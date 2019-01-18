using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace Polybrush
{
	/**
	 * Helper class for generating asset preview textures.  Necessary
	 * because AssetPreview.GetAssetPreview only works with assets
	 * and not prefabs.
	 */
	public static class z_AssetPreview
	{
		private static Editor cachedEditor = null;
		public static double cachedTextureLifetime = 3.0;

		private class CachedTexture
		{
			public double lastAccessed;
			public Texture2D texture;
		}

		static Dictionary<Object, CachedTexture> cache = new Dictionary<Object, CachedTexture>();

		private static void PurgeCache()
		{
			double time = EditorApplication.timeSinceStartup;

			List<Object> undesirables = cache.Keys.Where(x => (time - cache[x].lastAccessed) > cachedTextureLifetime).ToList();

			if(undesirables.Count < 1)
				return;

			for(int i = 0; i < undesirables.Count; i++)
			{
				Texture2D t = cache[undesirables[i]].texture;
				cache.Remove(undesirables[i]);
				Object.DestroyImmediate(t);
			}
		}

		/**
		 * Attempt to retrieve a new Texture2D asset preview.
		 */
		public static Texture2D GetAssetPreview(Object o, int size = 128)
		{
			CachedTexture cached;

			if(cache.TryGetValue(o, out cached))
			{
				cached.lastAccessed = EditorApplication.timeSinceStartup;
				PurgeCache();
				return cached.texture;
			}

			cached = new CachedTexture();
			cached.lastAccessed = EditorApplication.timeSinceStartup;

			// Unity crashes in this case
			if(o != null && o.GetInstanceID() != 0)
			{
				// cached.texture = AssetPreview.GetAssetPreview(o);

				if(cached.texture == null)
				{
					Editor.CreateCachedEditor(o, null, ref cachedEditor);

					if(cachedEditor != null)
						cached.texture = cachedEditor.RenderStaticPreview(AssetDatabase.GetAssetPath(o.GetInstanceID()), null, size, size);
				}
			}

			PurgeCache();

			cache.Add(o, cached);

			return cached.texture;
		}
	}
}
