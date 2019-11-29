using UnityEditor;
using GameplayIngredients.Editor;

static class DiscoverFontainebleau
{
    [MenuItem("Help/Discover Fontainebleau", priority = 0)]
    static void ShowSpaceshipDiscover()
    {
        var asset = AssetDatabase.LoadAssetAtPath<DiscoverAsset>("Assets/DiscoverFontainebleau.asset");
        DiscoverWindow.ShowDiscoverWindow(asset);
    }
}