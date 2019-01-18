// #define Z_DEBUG_INFO
// #define Z_DEBUG_WARNING
// #define Z_DEBUG_ERROR
// #define BUGGER

using UnityEngine;

namespace Polybrush
{
	public static class z_Debug
	{
		[System.Diagnostics.Conditional("Z_DEBUG_INFO")]
		public static void Log(string message, string color = "000000FF")
		{
#if BUGGER
			Bugger.Log( string.Format("<color=\"{0}\">{1}</color>", color, message), 3 );
#else
			Debug.Log( string.Format("<color=\"{0}\">{1}</color>", color, message) );
#endif
		}

		[System.Diagnostics.Conditional("Z_DEBUG_WARNING")]
		public static void LogWarning(string message, string color = "FF00FFFF")
		{
			Debug.LogWarning( string.Format("<color=\"{0}\">{1}</color>", color, message) );
		}

		[System.Diagnostics.Conditional("Z_DEBUG_ERROR")]
		public static void LogError(string message, string color = "FF0000FF")
		{
			Debug.LogError( string.Format("<color=\"{0}\">{1}</color>", color, message) );
		}
	}
}
