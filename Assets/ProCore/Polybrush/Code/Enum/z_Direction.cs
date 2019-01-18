using UnityEngine;

namespace Polybrush
{
	/**
	 *	Describes the different directions in which the brush tool can move vertices.
	 */
	public enum z_Direction
	{
		BrushNormal,
		VertexNormal,
		Up,
		Right,
		Forward
	}

	/**
	 *	Helper methods for working with Direction.
	 */
	public static class z_DirectionUtil
	{
		/**
		 *	Convert a direction to a vector.  If dir is Normal, 0 is returned.
		 */
		public static Vector3 ToVector3(this z_Direction dir)
		{
			switch(dir)
			{
				case z_Direction.Up:
					return Vector3.up;
				case z_Direction.Right:
					return Vector3.right;
				case z_Direction.Forward:
					return Vector3.forward;
				default:
					return Vector3.zero;
			}
		}
	}
}
