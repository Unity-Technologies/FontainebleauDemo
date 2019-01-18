using UnityEngine;

namespace Polybrush
{
	[System.Flags]
	public enum z_BrushMirror
	{
		None = 0x0,
		X = 0x1,
		Y = 0x2,
		Z = 0x4
	}

	/**
	 *	Helper functions for working with Mirror enum.
	 */
	public static class z_BrushMirrorUtility
	{
		/**
		 *	Convert a mirror enum to it's corresponding vector value.
		 */
		public static Vector3 ToVector3(this z_BrushMirror mirror)
		{
			uint m = (uint) mirror;

			Vector3 reflection = Vector3.one;

			if( (m & (uint) z_BrushMirror.X) > 0 )
				reflection.x = -1f;

			if( (m & (uint) z_BrushMirror.Y) > 0 )
				reflection.y = -1f;

			if( (m & (uint) z_BrushMirror.Z) > 0 )
				reflection.z = -1f;

			return reflection;
		}
	}
}
