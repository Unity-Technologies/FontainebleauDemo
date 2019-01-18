using UnityEngine;

namespace Polybrush
{
	/**
	 *	RGBA / XYZW / 0123
	 */
	public enum z_ComponentIndex
	{
		R = 0,
		X = 0,

		G = 1,
		Y = 1,

		B = 2,
		Z = 2,

		A = 3,
		W = 3
	};

	public enum z_ComponentIndexType
	{
		Vector = 0,
		Color = 1,
		Index = 2
	};

	public static class z_ComponentIndexUtility
	{
		public static float GetValueAtIndex(this Vector3 value, z_ComponentIndex index)
		{
			switch(index)
			{
				case z_ComponentIndex.X:
					return value.x;
				case z_ComponentIndex.Y:
					return value.y;
				case z_ComponentIndex.Z:
					return value.z;
				default:
					return 0f;
			}
		}

		public static float GetValueAtIndex(this Vector4 value, z_ComponentIndex index)
		{
			switch(index)
			{
				case z_ComponentIndex.X:
					return value.x;
				case z_ComponentIndex.Y:
					return value.y;
				case z_ComponentIndex.Z:
					return value.z;
				case z_ComponentIndex.W:
					return value.w;
				default:
					return 0f;
			}
		}

		public static float GetValueAtIndex(this Color value, z_ComponentIndex index)
		{
			switch(index)
			{
				case z_ComponentIndex.R:
					return value.r;
				case z_ComponentIndex.G:
					return value.g;
				case z_ComponentIndex.B:
					return value.b;
				case z_ComponentIndex.A:
					return value.a;
				default:
					return 0f;
			}
		}

		public static uint ToFlag(this z_ComponentIndex e)
		{
			int i = ((int)e) + 1;
			return (uint)(i < 3 ? i : i == 3 ? 4 : 8);
		}

		public static string GetString(this z_ComponentIndex component, z_ComponentIndexType type = z_ComponentIndexType.Vector)
		{
			int ind = ((int)component);

			if(type == z_ComponentIndexType.Vector)
				return ind == 0 ? "X" : (ind == 1 ? "Y" : (ind == 2 ? "Z" : "W"));
			else if(type == z_ComponentIndexType.Color)
				return ind == 0 ? "R" : (ind == 1 ? "G" : (ind == 2 ? "B" : "A"));
			else
				return ind.ToString();
		}

		public static readonly GUIContent[] ComponentIndexPopupDescriptions = new GUIContent[]
		{
			new GUIContent("X (R)"),
			new GUIContent("Y (G)"),
			new GUIContent("Z (B)"),
			new GUIContent("W (A)")
		};
		
		public static readonly int[] ComponentIndexPopupValues = new int[]
		{
			0,
			1,
			2,
			3
		};


	}
}
