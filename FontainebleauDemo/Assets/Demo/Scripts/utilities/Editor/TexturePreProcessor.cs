using UnityEditor;

// Automatically convert any texture file with "_bumpmap"
// in its file name into a normal map.

class MyTexturePostprocessor : AssetPostprocessor
{
    /*
    private TextureImporterPlatformSettings[] heightMap_PlateformSettings = new TextureImporterPlatformSettings[] {
        new TextureImporterPlatformSettings() {
            name = "Standalone",
            overridden = true,
            format = TextureImporterFormat.BC5},
        new TextureImporterPlatformSettings() {
            name = "PS4",
            overridden = true,
            format = TextureImporterFormat.BC5},
    };
    */


    void OnPreprocessTexture()
    {
        TextureImporter textureImporter = (TextureImporter)assetImporter;


        // Game Assets --------------------------------------------------------------------
        if (assetPath.Contains("_BC.") || assetPath.Contains("_A."))
        {
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.sRGBTexture = true;
            textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
        }
        else if (assetPath.Contains("_MSK.") || assetPath.Contains("_M."))
        {
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.sRGBTexture = false;
            textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
        }
        else if (assetPath.Contains("_AO."))
        {
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.sRGBTexture = true;
            textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
        }
        else if (assetPath.Contains("_Layers."))
        {
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.sRGBTexture = false;
            textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
        }
        
        else if (assetPath.Contains("_N.") || assetPath.Contains("_n."))
        {
            textureImporter.textureType = TextureImporterType.NormalMap;
            textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;

            var StandalonePlatformSettings = textureImporter.GetPlatformTextureSettings("Standalone"); 
            StandalonePlatformSettings.overridden = true;
            StandalonePlatformSettings.format = TextureImporterFormat.BC5;
            textureImporter.SetPlatformTextureSettings(StandalonePlatformSettings);

            var PS4PlatformSettings = textureImporter.GetPlatformTextureSettings("PS4");
            PS4PlatformSettings.overridden = true;
            PS4PlatformSettings.format = TextureImporterFormat.BC5;
            textureImporter.SetPlatformTextureSettings(PS4PlatformSettings);
        }
        
        else if (assetPath.Contains("_H."))
        {
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.sRGBTexture = false;
            textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;

            var StandalonePlatformSettings = textureImporter.GetPlatformTextureSettings("Standalone");
            StandalonePlatformSettings.overridden = true;
            StandalonePlatformSettings.format = TextureImporterFormat.BC5;
            textureImporter.SetPlatformTextureSettings(StandalonePlatformSettings);

            var PS4PlatformSettings = textureImporter.GetPlatformTextureSettings("PS4");
            PS4PlatformSettings.overridden = true;
            PS4PlatformSettings.format = TextureImporterFormat.BC5;
            textureImporter.SetPlatformTextureSettings(PS4PlatformSettings);
            
        }
        
        // Temporary Assets --------------------------------------------------------------------
        else if (
            (assetPath.Contains("_LRBC.")) ||
            (assetPath.Contains("_NoLight."))
            )
        {
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.sRGBTexture = true;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.maxTextureSize = 8192;
        }
        else if (
            (assetPath.Contains("_LRN.")) || 
            (assetPath.Contains("_LRNB.")) || 
            (assetPath.Contains("_LRAO.")) 
            )
        {
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.sRGBTexture = false;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.maxTextureSize = 8192;
        }




    }
}
