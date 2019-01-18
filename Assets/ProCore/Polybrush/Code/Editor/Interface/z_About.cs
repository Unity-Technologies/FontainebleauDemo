using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Polybrush
{
	class z_About : EditorWindow
	{
		static string CHANGELOG_PATH { get { return z_EditorUtility.RootFolder + "Documentation/changelog.md"; } }
		const string VERSION_NUMBER_PATTERN = "(?<=#\\sPolybrush\\s)([0-9]{1,2}\\.[0-9]{1,2}\\.[0-9]{1,2})";
		string versionNumber = "Major.Minor.Patch";

		void OnEnable()
		{
			changelog = System.IO.File.ReadAllText(CHANGELOG_PATH);

			Match versionMatch = Regex.Match(changelog, VERSION_NUMBER_PATTERN);
			if(versionMatch.Success) versionNumber = versionMatch.Value;

			// Match vcsMatch = Regex.Match(changelog, GIT_REVISION_PATTERN);
			// if(vcsMatch.Success) revisionNumber = vcsMatch.Value;
		}

		string changelog;
		GUIStyle centeredLargeLabel = null, centeredExtraLargeLabel = null;
		bool initialized = false;
		Vector2 scroll = Vector2.zero;

		void BeginHorizontalCenter()
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
		}

		void EndHorizontalCenter()
		{
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		void OnGUI()
		{
			if(!initialized)
			{
				centeredLargeLabel = new GUIStyle( EditorStyles.largeLabel );
				centeredLargeLabel.alignment = TextAnchor.MiddleCenter;
				centeredExtraLargeLabel = new GUIStyle( EditorStyles.largeLabel );
				centeredExtraLargeLabel.fontSize += 18;
				centeredExtraLargeLabel.alignment = TextAnchor.MiddleCenter;
				EditorStyles.largeLabel.richText = true;
			}

			GUILayout.Space(12);

			GUILayout.Label("Polybrush " + versionNumber, centeredExtraLargeLabel);

			GUILayout.Space(12);

			BeginHorizontalCenter();

			if(GUILayout.Button(" Documentation "))
				Application.OpenURL(z_Pref.DocumentationLink);

			if(GUILayout.Button(" Website "))
				Application.OpenURL(z_Pref.WebsiteLink);

			EndHorizontalCenter();

			GUILayout.Space(12);

			GUILayout.Label("<b>Changelog</b>", EditorStyles.largeLabel);

			scroll = GUILayout.BeginScrollView(scroll);

			GUILayout.Label( changelog, EditorStyles.wordWrappedLabel );

			GUILayout.EndScrollView();
		}
	}
}
