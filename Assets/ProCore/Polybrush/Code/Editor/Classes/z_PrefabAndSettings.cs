using UnityEngine;

namespace Polybrush
{
	[System.Serializable]
	public class z_PlacementSettings
	{
		public Vector2 rotationRange;
		public Vector2 scaleRange;

		public z_PlacementSettings()
		{
			rotationRange = new Vector2(0f, 360f);
			scaleRange = new Vector2(.7f, 1.3f);
		}
	}

	[System.Serializable]
	public class z_PrefabAndSettings
	{
		public GameObject gameObject;
		public z_PlacementSettings settings;

		public z_PrefabAndSettings(GameObject go)
		{
			gameObject = go;
			settings = new z_PlacementSettings();
		}
	}
}
