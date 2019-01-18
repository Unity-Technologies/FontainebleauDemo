using UnityEngine;

namespace Polybrush
{
	public static class z_Vector
	{
		/**
		 *	Non-allocating cross product.
		 *	`ref` does not box with primitive types (https://msdn.microsoft.com/en-us/library/14akc2c7.aspx)
		 */
		public static void Cross(Vector3 a, Vector3 b, ref float x, ref float y, ref float z)
		{
			x = a.y * b.z - a.z * b.y;
			y = a.z * b.x - a.x * b.z;
			z = a.x * b.y - a.y * b.x;
		}

		public static void Cross(Vector3 a, Vector3 b, ref Vector3 res)
		{
			res.x = a.y * b.z - a.z * b.y;
			res.y = a.z * b.x - a.x * b.z;
			res.z = a.x * b.y - a.y * b.x;
		}

		public static void Cross(float ax, float ay, float az, float bx, float by, float bz, ref float x, ref float y, ref float z)
		{
			x = ay * bz - az * by;
			y = az * bx - ax * bz;
			z = ax * by - ay * bx;
		}

		/**
		 * res = b - a
		 */
		public static void Subtract(Vector3 a, Vector3 b, ref Vector3 res)
		{
			res.x = b.x - a.x;
			res.y = b.y - a.y;
			res.z = b.z - a.z;
		}

	}
}
