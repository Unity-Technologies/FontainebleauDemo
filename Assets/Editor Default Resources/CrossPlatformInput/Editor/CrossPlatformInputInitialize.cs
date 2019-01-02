using System;
using System.Collections.Generic;
using UnityEditor;


namespace UnityStandardAssets.CrossPlatformInput.Inspector
{
    [InitializeOnLoad]
    public class CrossPlatformInitialize
    {
        // Custom compiler defines:
        //
        // CROSS_PLATFORM_INPUT : denotes that cross platform input package exists, so that other packages can use their CrossPlatformInput functions.
        // EDITOR_MOBILE_INPUT : denotes that mobile input should be used in editor, if a mobile build target is selected. (i.e. using Unity Remote app).
        // MOBILE_INPUT : denotes that mobile input should be used right now!

        static CrossPlatformInitialize()
        {
            var defines = GetDefinesList(buildTargetGroups[0]);
            if (!defines.Contains("CROSS_PLATFORM_INPUT"))
            {
                SetEnabled("CROSS_PLATFORM_INPUT", true, false);
            }
        }


        private static BuildTargetGroup[] buildTargetGroups = new BuildTargetGroup[]
            {
                BuildTargetGroup.Standalone,
                BuildTargetGroup.Android,
                BuildTargetGroup.iOS
            };

        private static BuildTargetGroup[] mobileBuildTargetGroups = new BuildTargetGroup[]
            {
                BuildTargetGroup.Android,
                BuildTargetGroup.iOS,
				//BuildTargetGroup.PSM, 
				BuildTargetGroup.WSA 
            };


        private static void SetEnabled(string defineName, bool enable, bool mobile)
        {
            //Debug.Log("setting "+defineName+" to "+enable);
            foreach (var group in mobile ? mobileBuildTargetGroups : buildTargetGroups)
            {
                var defines = GetDefinesList(group);
                if (enable)
                {
                    if (defines.Contains(defineName))
                    {
                        return;
                    }
                    defines.Add(defineName);
                }
                else
                {
                    if (!defines.Contains(defineName))
                    {
                        return;
                    }
                    while (defines.Contains(defineName))
                    {
                        defines.Remove(defineName);
                    }
                }
                string definesString = string.Join(";", defines.ToArray());
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, definesString);
            }
        }


        private static List<string> GetDefinesList(BuildTargetGroup group)
        {
            return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
        }
    }
}
