using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using System.IO;

public class ImageScriptAssetFactory
{
    [MenuItem("Assets/Create/Demo Photogrammetry/Audio Cue List", priority = 301)]
    private static void MenuCreateCueListAsset()
    {
        var icon = (Texture2D)null;
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateCueListAsset>(), "New CueList.asset", icon, null);
    }

    internal static CueList CreateCueListAtPath(string path)
    {
        CueList asset = ScriptableObject.CreateInstance<CueList>();
        asset.name = Path.GetFileName(path);
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

}

internal class DoCreateCueListAsset : EndNameEditAction
{
    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        CueList asset = ImageScriptAssetFactory.CreateCueListAtPath(pathName);
        ProjectWindowUtil.ShowCreatedAsset(asset);
    }
}
