using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;
using System.Collections;
using System.Linq;

public class MultiSceneSetup : ScriptableObject
{
    public SceneSetup[] Setups;
}

public static class MultiSceneSetupMenu
{
    [MenuItem("Assets/Multi Scene Setup/Create")]
    public static void CreateNewSceneSetup()
    {
        var folderPath = TryGetSelectedFolderPathInProjectsTab();

        var assetPath = ConvertFullAbsolutePathToAssetPath(
            Path.Combine(folderPath, "SceneSetup.asset"));

        SaveCurrentSceneSetup(assetPath);
    }

    [MenuItem("Assets/Multi Scene Setup/Create", true)]
    public static bool CreateNewSceneSetupValidate()
    {
        return TryGetSelectedFolderPathInProjectsTab() != null;
    }

    [MenuItem("Assets/Multi Scene Setup/Overwrite")]
    public static void SaveSceneSetup()
    {
        var assetPath = ConvertFullAbsolutePathToAssetPath(
            TryGetSelectedFilePathInProjectsTab());

        SaveCurrentSceneSetup(assetPath);
    }

    [MenuItem("Load levels/Day",false,1)]
    public static void LoadDemoDay()
    {
        var SceneSetup = (MultiSceneSetup)AssetDatabase.LoadAssetAtPath("Assets/Demo/MSE/Day.asset", typeof(MultiSceneSetup));

        EditorSceneManager.RestoreSceneManagerSetup(SceneSetup.Setups);

        Debug.Log(string.Format("Scene setup '{0}' restored", Path.GetFileNameWithoutExtension(AssetDatabase.LoadAssetAtPath("Assets/Demo/MSE/Day.asset", typeof(MultiSceneSetup)).name)));

        GameObject.FindObjectOfType<LevelLightmapData>().LoadLightingScenario(0);
    }

    [MenuItem("Load levels/Sunset", false, 2)]
    public static void LoadDemoSunset()
    {
        var SceneSetup = (MultiSceneSetup)AssetDatabase.LoadAssetAtPath("Assets/Demo/MSE/Sunset.asset", typeof(MultiSceneSetup));

        EditorSceneManager.RestoreSceneManagerSetup(SceneSetup.Setups);

        Debug.Log(string.Format("Scene setup '{0}' restored", Path.GetFileNameWithoutExtension(AssetDatabase.LoadAssetAtPath("Assets/Demo/MSE/Sunset.asset", typeof(MultiSceneSetup)).name)));

        GameObject.FindObjectOfType<LevelLightmapData>().LoadLightingScenario(2);
    }

    [MenuItem("Load levels/Night", false, 3)]
    public static void LoadDemoNight()
    {
        var SceneSetup = (MultiSceneSetup)AssetDatabase.LoadAssetAtPath("Assets/Demo/MSE/Night.asset", typeof(MultiSceneSetup));

        EditorSceneManager.RestoreSceneManagerSetup(SceneSetup.Setups);

        Debug.Log(string.Format("Scene setup '{0}' restored", Path.GetFileNameWithoutExtension(AssetDatabase.LoadAssetAtPath("Assets/Demo/MSE/Night.asset", typeof(MultiSceneSetup)).name)));

        GameObject.FindObjectOfType<LevelLightmapData>().LoadLightingScenario(1);
    }

    [MenuItem("Load levels/Loader", false, 14)]
    public static void LoadVideoDay()
    {
        var SceneSetup = (MultiSceneSetup)AssetDatabase.LoadAssetAtPath("Assets/Demo/MSE/Loader.asset", typeof(MultiSceneSetup));

        EditorSceneManager.RestoreSceneManagerSetup(SceneSetup.Setups);

        Debug.Log(string.Format("Scene setup '{0}' restored", Path.GetFileNameWithoutExtension(AssetDatabase.LoadAssetAtPath("Assets/Demo/MSE/Loader.asset", typeof(MultiSceneSetup)).name)));
    }

    /*
    [MenuItem("Demo Photogrammetry/Load levels/Video Day", false, 14)]
    public static void LoadVideoDay()
    {
        var SceneSetup = (MultiSceneSetup)AssetDatabase.LoadAssetAtPath("Assets/Demo/MSE/VideoDay.asset", typeof(MultiSceneSetup));

        EditorSceneManager.RestoreSceneManagerSetup(SceneSetup.Setups);

        Debug.Log(string.Format("Scene setup '{0}' restored", Path.GetFileNameWithoutExtension(AssetDatabase.LoadAssetAtPath("Assets/Demo/MSE/VideoDay.asset", typeof(MultiSceneSetup)).name)));

        GameObject.FindObjectOfType<LevelLightmapData>().LoadLightingScenario(0);
    }
    [MenuItem("Demo Photogrammetry/Load levels/Video Sunset", false, 15)]
    public static void LoadVideoSunset()
    {
        var SceneSetup = (MultiSceneSetup)AssetDatabase.LoadAssetAtPath("Assets/Demo/MSE/VideoSunset.asset", typeof(MultiSceneSetup));

        EditorSceneManager.RestoreSceneManagerSetup(SceneSetup.Setups);

        Debug.Log(string.Format("Scene setup '{0}' restored", Path.GetFileNameWithoutExtension(AssetDatabase.LoadAssetAtPath("Assets/Demo/MSE/VideoSunset.asset", typeof(MultiSceneSetup)).name)));

        GameObject.FindObjectOfType<LevelLightmapData>().LoadLightingScenario(2);
    }
    [MenuItem("Demo Photogrammetry/Load levels/Video Night", false, 16)]
    public static void LoadVideoNight()
    {
        var SceneSetup = (MultiSceneSetup)AssetDatabase.LoadAssetAtPath("Assets/Demo/MSE/VideoNight.asset", typeof(MultiSceneSetup));

        EditorSceneManager.RestoreSceneManagerSetup(SceneSetup.Setups);

        Debug.Log(string.Format("Scene setup '{0}' restored", Path.GetFileNameWithoutExtension(AssetDatabase.LoadAssetAtPath("Assets/Demo/MSE/VideoNight.asset", typeof(MultiSceneSetup)).name)));

        GameObject.FindObjectOfType<LevelLightmapData>().LoadLightingScenario(1);
    }
    */
    static void SaveCurrentSceneSetup(string assetPath)
    {
        var loader = ScriptableObject.CreateInstance<MultiSceneSetup>();

        loader.Setups = EditorSceneManager.GetSceneManagerSetup();

        AssetDatabase.CreateAsset(loader, assetPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(string.Format("Scene setup '{0}' saved", Path.GetFileNameWithoutExtension(assetPath)));
    }

    [MenuItem("Assets/Multi Scene Setup/Load")]
    public static void RestoreSceneSetup()
    {
        var assetPath = ConvertFullAbsolutePathToAssetPath(
            TryGetSelectedFilePathInProjectsTab());

        var loader = AssetDatabase.LoadAssetAtPath<MultiSceneSetup>(assetPath);

        EditorSceneManager.RestoreSceneManagerSetup(loader.Setups);

        Debug.Log(string.Format("Scene setup '{0}' restored", Path.GetFileNameWithoutExtension(assetPath)));
    }

    [MenuItem("Assets/Multi Scene Setup", true)]
    public static bool SceneSetupRootValidate()
    {
        return HasSceneSetupFileSelected();
    }

    [MenuItem("Assets/Multi Scene Setup/Overwrite", true)]
    public static bool SaveSceneSetupValidate()
    {
        return HasSceneSetupFileSelected();
    }

    [MenuItem("Assets/Multi Scene Setup/Load", true)]
    public static bool RestoreSceneSetupValidate()
    {
        return HasSceneSetupFileSelected();
    }

    static bool HasSceneSetupFileSelected()
    {
        return TryGetSelectedFilePathInProjectsTab() != null;
    }

    static List<string> GetSelectedFilePathsInProjectsTab()
    {
        return GetSelectedPathsInProjectsTab()
            .Where(x => File.Exists(x)).ToList();
    }

    static string TryGetSelectedFilePathInProjectsTab()
    {
        var selectedPaths = GetSelectedFilePathsInProjectsTab();

        if (selectedPaths.Count == 1)
        {
            return selectedPaths[0];
        }

        return null;
    }

    // Returns the best guess directory in projects pane
    // Useful when adding to Assets -> Create context menu
    // Returns null if it can't find one
    // Note that the path is relative to the Assets folder for use in AssetDatabase.GenerateUniqueAssetPath etc.
    static string TryGetSelectedFolderPathInProjectsTab()
    {
        var selectedPaths = GetSelectedFolderPathsInProjectsTab();

        if (selectedPaths.Count == 1)
        {
            return selectedPaths[0];
        }

        return null;
    }

    // Note that the path is relative to the Assets folder
    static List<string> GetSelectedFolderPathsInProjectsTab()
    {
        return GetSelectedPathsInProjectsTab()
            .Where(x => Directory.Exists(x)).ToList();
    }

    static List<string> GetSelectedPathsInProjectsTab()
    {
        var paths = new List<string>();

        UnityEngine.Object[] selectedAssets = Selection.GetFiltered(
            typeof(UnityEngine.Object), SelectionMode.Assets);

        foreach (var item in selectedAssets)
        {
            var relativePath = AssetDatabase.GetAssetPath(item);

            if (!string.IsNullOrEmpty(relativePath))
            {
                var fullPath = Path.GetFullPath(Path.Combine(
                    Application.dataPath, Path.Combine("..", relativePath)));

                paths.Add(fullPath);
            }
        }

        return paths;
    }

    static string ConvertFullAbsolutePathToAssetPath(string fullPath)
    {
        return "Assets/" + Path.GetFullPath(fullPath)
            .Remove(0, Path.GetFullPath(Application.dataPath).Length + 1)
            .Replace("\\", "/");
    }
}