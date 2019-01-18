namespace Polybrush
{
	/**
	 *	Tool enum for brush modes.
	 */
	public enum z_BrushTool
	{
		None = 0,
		RaiseLower = 1,
		Smooth = 2,
		Paint = 3,
		Prefab = 4,
		Texture = 5,
		Settings = 6
	}

	public static class z_BrushToolUtility
	{
		public static System.Type GetModeType(this z_BrushTool tool)
		{
			switch(tool)
			{
				case z_BrushTool.RaiseLower:
					return typeof(z_BrushModeRaiseLower);

				case z_BrushTool.Smooth:
					return typeof(z_BrushModeSmooth);

				case z_BrushTool.Paint:
					return typeof(z_BrushModePaint);

				case z_BrushTool.Prefab:
					return typeof(z_BrushModePrefab);

				case z_BrushTool.Texture:
					return typeof(z_BrushModeTexture);
			}

			return null;
		}
	}
}
